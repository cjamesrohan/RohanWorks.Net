using Microsoft.Extensions.Logging;
using Moq;
using RohanWorks.Net.Testing;

namespace RohanWorks.Net.Testing.Tests;

public class LoggerAssertionsTests
{
    private readonly Mock<ILogger<LoggerAssertionsTests>> _loggerMock = new();

    private ILogger<LoggerAssertionsTests> Logger => _loggerMock.Object;

    [Fact]
    public void Should_ReturnsLoggerAssertions()
    {
        var assertions = _loggerMock.Should();
        assertions.Should().BeOfType<LoggerAssertions<LoggerAssertionsTests>>();
    }

    [Fact]
    public void LogInformation_Passes_WhenLoggedWithSameMessage()
    {
        Logger.LogInformation("Hello {Name}", "World");

        _loggerMock.Should().LogInformation("Hello {Name}", "World");
    }

    [Fact]
    public void LogInformation_Fails_WhenMessageDoesNotMatch()
    {
        Logger.LogInformation("Hello World");

        var act = () => _loggerMock.Should().LogInformation("Different message");
        act.Should().Throw<Exception>();
    }

    [Fact]
    public void NotLogInformation_Passes_WhenNotLogged()
    {
        _loggerMock.Should().NotLogInformation("Never logged");
    }

    [Fact]
    public void NotLogInformation_Fails_WhenMessageWasLogged()
    {
        Logger.LogInformation("Was logged");

        var act = () => _loggerMock.Should().NotLogInformation("Was logged");
        act.Should().Throw<Exception>();
    }

    [Fact]
    public void LogError_WithException_Passes_WhenMatches()
    {
        var ex = new InvalidOperationException("boom");
        Logger.LogError(ex, "Error occurred: {Msg}", "boom");

        _loggerMock.Should().LogError(ex, "Error occurred: {Msg}", "boom");
    }

    [Fact]
    public void LogWarning_Passes_WhenLoggedAtCorrectLevel()
    {
        Logger.LogWarning("Watch out");

        _loggerMock.Should().LogWarning("Watch out");
        // Should NOT pass for a different level
        var act = () => _loggerMock.Should().LogError("Watch out");
        act.Should().Throw<Exception>();
    }

    [Fact]
    public void Log_GenericMethod_Passes_WhenLevelMatches()
    {
        Logger.LogDebug("Debug msg");

        _loggerMock.Should().Log(LogLevel.Debug, "Debug msg");
    }

    [Fact]
    public void HaveScope_Passes_WhenScopeMatches()
    {
        var scope = new Dictionary<string, string> { { "CorrelationId", "abc-123" } };
        using var _ = Logger.BeginScope(scope);

        _loggerMock.Should().HaveScope(scope);
    }
}
