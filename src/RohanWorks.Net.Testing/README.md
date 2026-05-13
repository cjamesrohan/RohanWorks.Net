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

## Asserting Log Output

Every log level has a matching assertion method. The default verifies the message was logged **at least once**:

```csharp
_logger.Should().LogDebug("Entering method");
_logger.Should().LogTrace("Value: {Value}", someValue);
_logger.Should().LogInformation("Order {Id} created", orderId);
_logger.Should().LogWarning("Retry attempt {Count}", retryCount);
_logger.Should().LogError("Failed to process order {Id}", orderId);
_logger.Should().LogCritical("Service unavailable");
```

With an exception:

```csharp
var ex = new InvalidOperationException("payment failed");

_logger.Should().LogError(ex, "Order {Id} failed", orderId);
```

With an EventId:

```csharp
var eventId = new EventId(1001, "OrderProcessed");

_logger.Should().LogInformation(eventId, "Order {Id} processed", orderId);
_logger.Should().LogError(eventId, ex, "Order {Id} failed", orderId);
```

---

## Asserting No Log Output

Use `Should().Not` to assert a message was never logged:

```csharp
_logger.Should().Not.LogError("Order {Id} failed", orderId);
_logger.Should().Not.LogWarning("Retry attempt {Count}", retryCount);
```

All the same overloads (exception, EventId, etc.) are available on `Not`.

---

## Asserting Exact Counts

Chain `.WithCount(Times)` when you need to verify exactly how many times something was logged:

```csharp
// Logged exactly once
_logger.Should().LogInformation("Order {Id} created", orderId).WithCount(Times.Once());

// Logged a specific number of times
_logger.Should().LogWarning("Retry attempt {Count}", retryCount).WithCount(Times.Exactly(3));

// Logged at least once (explicit — same as the default)
_logger.Should().LogInformation("Order {Id} created", orderId).WithCount(Times.AtLeastOnce());
```

---

## Asserting Scopes

```csharp
var scope = new Dictionary<string, string> { { "CorrelationId", "abc-123" } };

_logger.Should().HaveScope(scope);       // scope was begun
_logger.Should().Not.HaveScope(scope);   // scope was NOT begun
```

---

## API Reference

| Method | Description |
|---|---|
| `Should().Log{Level}(message, args)` | Asserts message was logged at the given level (at least once) |
| `Should().Log{Level}(exception, message, args)` | Includes exception matching |
| `Should().Log{Level}(eventId, message, args)` | Includes EventId matching |
| `Should().Log{Level}(eventId, exception, message, args)` | Includes both |
| `Should().Not.Log{Level}(...)` | Asserts message was never logged — same overloads as above |
| `.WithCount(Times)` | Chains off any positive assertion to verify exact count |
| `Should().HaveScope(state)` | Asserts `BeginScope` was called with the given state |
| `Should().Not.HaveScope(state)` | Asserts `BeginScope` was NOT called with the given state |
