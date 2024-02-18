# Legato - CQRS Architecture Application Blueprint

## Overview

Legato is a .NET framework designed to facilitate the development of scalable web applications employing the Command Query Responsibility Segregation (CQRS) architecture. It offers a robust foundation for building microservices or modular monoliths, integrating seamlessly with Entity Framework Core and providing support for both in-memory and Azure Service Bus messaging. Legato emphasizes a clear separation of concerns, advanced transaction management, and efficient message routing, making it ideal for complex business applications.

## Features

- **CQRS Abstractions**: Commands, Queries, and Events for a clear separation of operations.
- **Entity Framework Core Integration**: Efficient data access and manipulation.
- **Service Bus Support**: Flexible messaging with in-memory and Azure Service Bus options.
- **Event Streaming and Sourcing**: Facilitates event-driven architectures.
- **Transactional Support**: Ensures data consistency across distributed systems.

## Core Concepts and Examples

### Handling Domain Commands

Legato simplifies the process of command handling with defined interfaces. For instance, adding a blog post involves creating a specific command handler:

```csharp
class BlogAggregate : ICommandHandler<AddBlogPost>
{
    ...
    public async Task Handle(AddBlogPost command)
    {
        ...
        context.Store(command);
        context.Add(post);
        await context.PublishChanges(new BlogPostAdded(command));
    }
}
```

### Cross-Domain Interactions

Legato enables seamless cross-domain interactions, such as reacting to events across different bounded contexts:

```csharp
class CustomerUserCreator : IEventHandler<CustomerCreated>
{
    ...
    public Task Handle(CustomerCreated data) => 
        commands.Execute(new CreateCustomerUserCommand {...});
}
```

### Transaction Management

Ensuring transactional integrity is crucial, and Legato provides structured support for transaction management:

```csharp
[ApiController]
public class LegalEntityController : AbstractController
{
    ...
    [HttpPost]
    public async Task AddLegalEntity(EditLegalEntityDto dto)
    {
        await using var transaction = await transactions.BeginTransaction();
        ...
        await transaction.CommitAsync();
    }
}
```

### Message Routing

Legato supports dynamic message routing, allowing for flexible and scalable messaging architectures:

```csharp
[RoutedTo(Queues.Applications)]
public record ApproveCommand : DomainCommand {...}

[Handles(Queues.Applications)]
class ApplicationAggregate : ICommandHandler<ApproveCommand> {...}
```

### Query Extensions and EF Core Integration

Legato enhances query capabilities with extensions, and abstracts away Entity Framework Core complexities:

```csharp
public static class QueryExtensions
{
    public static IQueryable<Payment> ByExternalId(this IQueryable<Payment> payments, Guid externalId) => ...;
}

public static class Queries
{
    public static Task<Payment> GetByExternalId(this IQueryProvider<Payment> payments, Guid externalId) => ...;
}
```

### EF Core Integration Internals

Legato's integration with EF Core is encapsulated within the `StateContext`, simplifying data persistence operations:

```csharp
async Task InternalSaveChanges<TEvent>(TEvent? domainEvent) where TEvent : DomainEvent
{
    ...
    await context.SaveChangesAsync();
    context.ChangeTracker.Clear();
}
```

## License

Legato is distributed under the MIT license
