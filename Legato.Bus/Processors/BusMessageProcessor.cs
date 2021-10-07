using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;

namespace Legato.Bus.Azure.Processors
{
    abstract class BusMessageProcessor<TSubscription> : IAsyncDisposable where TSubscription : Subscription
    {
        Dictionary<Type, TSubscription> subscriptions;
        ServiceBusProcessor processor;
        ILogger logger;

        public TSubscription[] Subscriptions => subscriptions.Values.ToArray();

        protected BusMessageProcessor(IEnumerable<TSubscription> subscriptions, ServiceBusProcessor processor, ILogger logger)
        {
            this.subscriptions = subscriptions.ToDictionary(s => s.MessageType);
            this.processor = processor;
            this.processor.ProcessMessageAsync += OnMessage;
            this.processor.ProcessErrorAsync += OnError;
            this.logger = logger;
        }

        public async Task Start()
        {
            await processor.StartProcessingAsync();
            logger.LogInformation("Started");
        }

        public async Task Stop()
        {
            await processor.StopProcessingAsync();
            logger.LogInformation("Stopped");
        }

        async Task OnMessage(ProcessMessageEventArgs arg)
        {
            new Activity("ServiceBusProcessor.ProcessMessage").Start();

            try
            {
                var subject = arg.Message.Subject;
                if (subject is null)
                    throw new InvalidOperationException("Subject expected, but was empty");

                var messageType = Type.GetType(subject);
                if (messageType is null)
                    throw new InvalidOperationException($"Subject expected, but type was not found: '{subject}'");

                if (subscriptions.TryGetValue(messageType, out var subscription))
                {
                    await subscription.Handler.Invoke(arg.Message);
                    await arg.CompleteMessageAsync(arg.Message);
                }
            }
            catch (Exception ex)
            {
                var state = new Dictionary<string, object>
                {
                    {"ActivityId", Activity.Current?.Id},
                    {"CorrelationId", arg.Message.CorrelationId},
                    {"StackTrace", ex.ToString()}
                };

                using (logger.BeginScope(state))
                    logger.LogError(ex, "An error occurred on message processing");

                await arg.DeadLetterMessageAsync(arg.Message, MapToProperties(ex));
            }
        }

        Task OnError(ProcessErrorEventArgs arg)
        {
            if (arg.Exception is ObjectDisposedException {ObjectName: "FaultTolerantAmqpObject`1"})
            {
                logger.LogTrace(arg.Exception, "An unhandled error occurred on message processing");
                return Task.CompletedTask;
            }

            logger.LogError(arg.Exception, "An unhandled error occurred on message processing");
            return Task.CompletedTask;
        }

        public async ValueTask DisposeAsync()
        {
            await processor.DisposeAsync();
            processor.ProcessMessageAsync -= OnMessage;
            processor.ProcessErrorAsync -= OnError;
        }

        static Dictionary<string, object> MapToProperties(Exception ex) =>
            new()
            {
                { "message", ex.Message },
                { "exception", ex.ToString() }
            };
    }
}
