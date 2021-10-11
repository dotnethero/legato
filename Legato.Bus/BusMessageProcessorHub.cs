using Autofac;
using Autofac.Core;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using Legato.Bus.Attributes;
using Legato.Bus.Azure.Extensions;
using Legato.Bus.Azure.Options;
using Legato.Bus.Azure.Processors;
using Legato.Common.Extensions;
using Legato.CQRS;
using Legato.Transactions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Legato.Bus.Azure
{
    public interface IBusMessageProcessorHub : IAsyncDisposable
    {
        Task EnsureIsConfigured();
        Task Start();
        Task Stop();
    }

    class BusMessageProcessorHub : IBusMessageProcessorHub
    {
        static ServiceBusProcessorOptions DefaultProcessorOptions => new() { AutoCompleteMessages = false };

        ILoggerFactory loggerFactory;
        ILifetimeScope scope;
        ServiceBusClient client;
        ServiceBusAdministrationClient admin;
        Lazy<TopicProcessor[]> topicProcessors;
        Lazy<QueueProcessor[]> queueProcessors;

        public BusMessageProcessorHub(
            IOptions<BusConfiguration> options,
            ILoggerFactory loggerFactory,
            ILifetimeScope scope)
        {
            this.loggerFactory = loggerFactory;
            this.scope = scope;
            this.client = new ServiceBusClient(options.Value.ConnectionString);
            this.admin = new ServiceBusAdministrationClient(options.Value.ConnectionString);
            this.topicProcessors = new Lazy<TopicProcessor[]>(() => CreateTopicProcessors().ToArray());
            this.queueProcessors = new Lazy<QueueProcessor[]>(() => CreateQueueProcessors().ToArray());
        }

        public async Task EnsureIsConfigured()
        {
            foreach (var processor in topicProcessors.Value) 
                await EnsureSubscriptionsAreCreated(processor);

            foreach (var processor in queueProcessors.Value) 
                await EnsureQueueCreated(processor);
        }

        public async Task Start()
        {
            foreach (var processor in topicProcessors.Value) 
                await processor.Start();

            foreach (var processor in queueProcessors.Value) 
                await processor.Start();
        }

        public async Task Stop()
        {
            foreach (var processor in topicProcessors.Value)
                await processor.Stop();

            foreach (var processor in queueProcessors.Value)
                await processor.Stop();
        }
        
        async Task EnsureQueueCreated(QueueProcessor processor)
        {
            if (!await admin.QueueExistsAsync(processor.QueueName)) 
                await admin.CreateQueueAsync(new CreateQueueOptions(processor.QueueName));
        }

        async Task EnsureSubscriptionsAreCreated(TopicProcessor processor)
        {
            if (!await admin.TopicExistsAsync(processor.Topic))
                await admin.CreateTopicAsync(new CreateTopicOptions(processor.Topic));

            foreach (var subscription in processor.Subscriptions) 
                await EnsureSubscriptionCreated(subscription);
        }

        async Task EnsureSubscriptionCreated(TopicSubscription subscription)
        {
            if (!await admin.SubscriptionExistsAsync(subscription.Topic, subscription.Queue))
                await admin.CreateSubscriptionAsync(new CreateSubscriptionOptions(subscription.Topic, subscription.Queue) {MaxDeliveryCount = 1});
        }

        IEnumerable<QueueProcessor> CreateQueueProcessors() =>
            from type in AppDomain.CurrentDomain.GetRoutedCommands()
            from subscription in GetQueueSubscriptions(type)
            group subscription by subscription.Queue into g
            select CreateQueueProcessor(g.Key, g);

        IEnumerable<TopicProcessor> CreateTopicProcessors() =>
            from topic in AppDomain.CurrentDomain.GetRoutedEvents()
            from subscription in GetTopicSubscriptions(topic)
            select CreateTopicProcessor(subscription);

        TopicProcessor CreateTopicProcessor(TopicSubscription subscription) =>
            new(subscription.Topic,
                subscription,
                client.CreateProcessor(
                    subscription.Topic,
                    subscription.Queue,
                    DefaultProcessorOptions),
                loggerFactory.CreateLogger($"TopicProcessor:{subscription.Topic}/{subscription.Queue}"));

        QueueProcessor CreateQueueProcessor(string queueName, IEnumerable<QueueSubscription> subscriptions) =>
            new(queueName,
                subscriptions,
                client.CreateProcessor(
                    queueName,
                    DefaultProcessorOptions),
                loggerFactory.CreateLogger($"QueueProcessor:{queueName}"));

        IEnumerable<QueueSubscription> GetQueueSubscriptions(Type commandType)
        {
            var handler = typeof(ICommandHandler<>).MakeGenericType(commandType);
            var handlerService = new TypedService(handler);
            var methodInfo = handler.GetMethod(nameof(ICommandHandler<DomainCommand>.Handle));
            var handlers = new HashSet<Type>();

            foreach (var registration in scope.ComponentRegistry.Registrations)
            {
                var serviceType = registration.Activator.LimitType;

                if (handlers.Contains(serviceType) ||
                    !registration.Services.Contains(handlerService)) continue;

                handlers.Add(serviceType);

                yield return new QueueSubscription(
                    commandType,
                    serviceType.GetCustomAttribute<HandlesAttribute>()!.Queue,
                    message => Handle(commandType, serviceType, methodInfo, message));
            }
        }

        IEnumerable<TopicSubscription> GetTopicSubscriptions(Type eventType)
        {
            var handlerType = typeof(IEventHandler<>).MakeGenericType(eventType);
            var handlerService = new TypedService(handlerType);
            var methodInfo = handlerType.GetMethod(nameof(IEventHandler<DomainEvent>.Handle));
            var handlers = new HashSet<Type>();

            foreach (var registration in scope.ComponentRegistry.Registrations)
            {
                var serviceType = registration.Activator.LimitType;

                if (handlers.Contains(serviceType) || !registration.Services.Contains(handlerService)) continue;

                handlers.Add(serviceType);

                yield return new TopicSubscription(
                    eventType,
                    serviceType,
                    message => Handle(eventType, serviceType, methodInfo, message));
            }
        }

        async Task Handle(Type consumedType, Type serviceType, MethodBase methodInfo, ServiceBusReceivedMessage message)
        {
            await using var innerScope = scope.BeginLifetimeScope();

            var json = message.Body.ToString();
            var obj = JsonExtensions.Deserialize(json, consumedType);

            var logger = loggerFactory.CreateLogger<BusMessageProcessorHub>();
            var state = new Dictionary<string, object>
            {
                {"ActivityId", Activity.Current?.Id},
                {"CorrelationId", message.CorrelationId},
                {"Request", json }
            };

            using (logger.BeginScope(state))
                logger.LogInformation($"ServiceBus message received: {consumedType}");

            var context = innerScope.ResolveOptional<ITransactionContext>();
            if (context is not null)
            {
                await using var transaction = await context.BeginTransaction();
                try
                {
                    var handler = innerScope.Resolve(serviceType);
                    var task = (Task) methodInfo.Invoke(handler, new[] {obj});
                    await task!;
                    await transaction.CommitAsync();
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            else
            {
                // if transactions are not supported

                var handler = innerScope.Resolve(serviceType);
                var task = (Task) methodInfo.Invoke(handler, new[] {obj});
                await task!;
            }
        }

        public async ValueTask DisposeAsync()
        {
            foreach (var processor in topicProcessors.Value)
                await processor.DisposeAsync();

            foreach (var processor in queueProcessors.Value)
                await processor.DisposeAsync();

            await client.DisposeAsync();
        }
    }
}
