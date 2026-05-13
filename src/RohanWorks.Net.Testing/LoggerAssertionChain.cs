using System;
using Microsoft.Extensions.Logging;
using Moq;

namespace RohanWorks.Net.Testing;

public sealed class LoggerAssertionChain<T>
{
    private readonly Mock<ILogger<T>> _mock;
    private readonly LogLevel _logLevel;
    private readonly EventId _eventId;
    private readonly Exception? _exception;
    private readonly string _formattedMessage;

    internal LoggerAssertionChain(
        Mock<ILogger<T>> mock,
        LogLevel logLevel,
        EventId eventId,
        Exception? exception,
        string formattedMessage)
    {
        _mock = mock;
        _logLevel = logLevel;
        _eventId = eventId;
        _exception = exception;
        _formattedMessage = formattedMessage;
    }

    public void WithCount(Times times)
    {
        _mock.Verify(
            x => x.Log(
                _logLevel,
                _eventId,
                It.Is<It.IsAnyType>((v, _) => v.ToString() == _formattedMessage),
                _exception,
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, _) => true)),
            times,
            $"Expected log {times}: Log(LogLevel.{_logLevel}, {_eventId}, {_formattedMessage}, {_exception?.Message ?? "null"})");
    }
}
