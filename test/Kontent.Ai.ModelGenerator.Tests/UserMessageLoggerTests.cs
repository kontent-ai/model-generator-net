using Kontent.Ai.ModelGenerator.Core.Contract;

namespace Kontent.Ai.ModelGenerator.Tests;

public class UserMessageLoggerTests
{
    private readonly IUserMessageLogger _userMessageLogger;
    private readonly StringWriter _stringWriter;

    public UserMessageLoggerTests()
    {
        _userMessageLogger = new UserMessageLogger();
        _stringWriter = new StringWriter();
        Console.SetOut(_stringWriter);
    }

    [Fact]
    public void LogInfo_MessageIsLoggedToConsole()
    {
        var message = "message";
        var expectedMessage = $"{message}{Environment.NewLine}";

        _userMessageLogger.LogInfo(message);

        _stringWriter.ToString().Should().Be(expectedMessage);

        _stringWriter.Flush();
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void LogInfo_MessageIsNullOrWhitespace_MessageIsNotLoggedToConsole(string message)
    {
        _userMessageLogger.LogInfo(message);

        _stringWriter.ToString().Should().BeEmpty();

        _stringWriter.Flush();
    }

    [Fact]
    public void LogWarning_MessageIsLoggedToConsole()
    {
        var message = "message";
        var expectedMessage = $"{message}{Environment.NewLine}";

        _userMessageLogger.LogWarning(message);

        _stringWriter.ToString().Should().Be($"Warning: {expectedMessage}");

        _stringWriter.Flush();
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void LogWarning_MessageIsNullOrWhitespace_MessageIsNotLoggedToConsole(string message)
    {
        _userMessageLogger.LogWarning(message);

        _stringWriter.ToString().Should().BeEmpty();

        _stringWriter.Flush();
    }
}
