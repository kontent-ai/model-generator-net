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
    public void Log_MessageIsLoggedToConsole()
    {
        var message = "message";
        var expectedMessage = $"{message}{Environment.NewLine}";

        _userMessageLogger.Log(message);

        _stringWriter.ToString().Should().Be(expectedMessage);

        _stringWriter.Flush();
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void Log_MessageIsNullOrWhitespace_MessageIsNotLoggedToConsole(string message)
    {
        _userMessageLogger.Log(message);

        _stringWriter.ToString().Should().BeEmpty();

        _stringWriter.Flush();
    }
}
