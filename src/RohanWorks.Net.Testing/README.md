# RohanWorks.Net.Testing

Fluent assertion helpers for `ILogger<T>` in .NET tests. Wraps Moq with a FluentAssertions-style API so verifying log output reads naturally instead of fighting `It.Is<>` lambda chains.

## Installation

```
dotnet add package RohanWorks.Net.Testing
```

Requires `Moq` and `FluentAssertions` (both are dependencies — no separate install needed).

## Quick Start

```csharp
using Moq;
using RohanWorks.Net.Testing;

public class OrderServiceTests
{
    private readonly Mock<ILogger<OrderService>> _logger = new();

    [Fact]
    public async Task ProcessOrder_LogsSuccess()
    {
        var sut = new OrderService(_logger.Object);

        await sut.ProcessAsync(orderId: 42);

        _logger.Should().LogInformation("Order {OrderId} processed successfully", 42);
    }
}
```

---

## Verifying Log Output

Every log level has a matching assertion method:

```csharp
_logger.Should().LogDebug("Entering method");
_logger.Should().LogTrace("Value: {Value}", someValue);
_logger.Should().LogInformation("Order {Id} created", orderId);
_logger.Should().LogWarning("Retry attempt {Count}", retryCount);
_logger.Should().LogError("Failed to process order {Id}", orderId);
_logger.Should().LogCritical("Service unavailable");
```

## Verifying No Log Output

Each level also has a negation:

```csharp
_logger.Should().NotLogError("anything");
_logger.Should().NotLogWarning("Retry attempt {Count}", retryCount);
```

---

## EventId Overloads

```csharp
var eventId = new EventId(1001, "OrderProcessed");

_logger.Should().LogInformation(eventId, "Order {Id} processed", orderId);
```

## Exception Overloads

```csharp
var ex = new InvalidOperationException("payment failed");

_logger.Should().LogError(ex, "Order {Id} failed", orderId);
_logger.Should().LogError(eventId, ex, "Order {Id} failed", orderId);
```

---

## API Reference

All methods follow the same pattern across six log levels (`Debug`, `Trace`, `Information`, `Warning`, `Error`, `Critical`):

| Signature | Asserts |
|---|---|
| `Log{Level}(message, args)` | Message was logged at the level |
| `Log{Level}(eventId, message, args)` | Message was logged with a specific EventId |
| `Log{Level}(exception, message, args)` | Message was logged with a specific exception |
| `Log{Level}(eventId, exception, message, args)` | Message logged with both EventId and exception |
| `NotLog{Level}(...)` | Negation of any of the above |
