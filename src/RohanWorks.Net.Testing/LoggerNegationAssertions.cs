using System;
using Microsoft.Extensions.Logging;
using Moq;

namespace RohanWorks.Net.Testing;

public sealed class LoggerNegationAssertions<T>
{
    private readonly Mock<ILogger<T>> _instance;

    internal LoggerNegationAssertions(Mock<ILogger<T>> instance) => _instance = instance;

    #region LogDebug
    public void LogDebug(string message, params object[] args) => Verify(LogLevel.Debug, 0, null, message, args);
    public void LogDebug(EventId eventId, string message, params object[] args) => Verify(LogLevel.Debug, eventId, null, message, args);
    public void LogDebug(Exception exception, string message, params object[] args) => Verify(LogLevel.Debug, 0, exception, message, args);
    public void LogDebug(EventId eventId, Exception? exception, string message, params object[] args) => Verify(LogLevel.Debug, eventId, exception, message, args);
    #endregion

    #region LogTrace
    public void LogTrace(string message, params object[] args) => Verify(LogLevel.Trace, 0, null, message, args);
    public void LogTrace(EventId eventId, string message, params object[] args) => Verify(LogLevel.Trace, eventId, null, message, args);
    public void LogTrace(Exception exception, string message, params object[] args) => Verify(LogLevel.Trace, 0, exception, message, args);
    public void LogTrace(EventId eventId, Exception? exception, string message, params object[] args) => Verify(LogLevel.Trace, eventId, exception, message, args);
    #endregion

    #region LogInformation
    public void LogInformation(string message, params object[] args) => Verify(LogLevel.Information, 0, null, message, args);
    public void LogInformation(EventId eventId, string message, params object[] args) => Verify(LogLevel.Information, eventId, null, message, args);
    public void LogInformation(Exception exception, string message, params object[] args) => Verify(LogLevel.Information, 0, exception, message, args);
    public void LogInformation(EventId eventId, Exception? exception, string message, params object[] args) => Verify(LogLevel.Information, eventId, exception, message, args);
    #endregion

    #region LogWarning
    public void LogWarning(string message, params object[] args) => Verify(LogLevel.Warning, 0, null, message, args);
    public void LogWarning(EventId eventId, string message, params object[] args) => Verify(LogLevel.Warning, eventId, null, message, args);
    public void LogWarning(Exception exception, string message, params object[] args) => Verify(LogLevel.Warning, 0, exception, message, args);
    public void LogWarning(EventId eventId, Exception? exception, string message, params object[] args) => Verify(LogLevel.Warning, eventId, exception, message, args);
    #endregion

    #region LogError
    public void LogError(string message, params object[] args) => Verify(LogLevel.Error, 0, null, message, args);
    public void LogError(EventId eventId, string message, params object[] args) => Verify(LogLevel.Error, eventId, null, message, args);
    public void LogError(Exception exception, string message, params object[] args) => Verify(LogLevel.Error, 0, exception, message, args);
    public void LogError(EventId eventId, Exception? exception, string message, params object[] args) => Verify(LogLevel.Error, eventId, exception, message, args);
    #endregion

    #region LogCritical
    public void LogCritical(string message, params object[] args) => Verify(LogLevel.Critical, 0, null, message, args);
    public void LogCritical(EventId eventId, string message, params object[] args) => Verify(LogLevel.Critical, eventId, null, message, args);
    public void LogCritical(Exception exception, string message, params object[] args) => Verify(LogLevel.Critical, 0, exception, message, args);
    public void LogCritical(EventId eventId, Exception? exception, string message, params object[] args) => Verify(LogLevel.Critical, eventId, exception, message, args);
    #endregion

    #region Log
    public void Log(LogLevel logLevel, string message, params object[] args) => Verify(logLevel, 0, null, message, args);
    public void Log(LogLevel logLevel, EventId eventId, string message, params object[] args) => Verify(logLevel, eventId, null, message, args);
    public void Log(LogLevel logLevel, Exception exception, string message, params object[] args) => Verify(logLevel, 0, exception, message, args);
    public void Log(LogLevel logLevel, EventId eventId, Exception? exception, string message, params object[] args) => Verify(logLevel, eventId, exception, message, args);
    #endregion

    public void HaveScope<TState>(TState state) where TState : notnull
        => _instance.Verify(x => x.BeginScope(state), Times.Never(), "Unexpected scope was begun");

    private void Verify(LogLevel logLevel, EventId eventId, Exception? exception, string message, object[] args)
    {
        var formattedMessage = new FormattedLogValues(message, args).ToString();

        _instance.Verify(
            x => x.Log(
                logLevel,
                eventId,
                It.Is<It.IsAnyType>((v, _) => v.ToString() == formattedMessage),
                exception,
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, _) => true)),
            Times.Never(),
            $"Expected no log: Log(LogLevel.{logLevel}, {eventId}, {formattedMessage}, {exception?.Message ?? "null"})");
    }
}
