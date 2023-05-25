using Kontent.Ai.ModelGenerator.Core.Contract;
using Moq;

namespace Kontent.Ai.ModelGenerator.Tests;

public class ProgramTests
{
    private readonly Mock<IUserMessageLogger> _messageLoggerMock;

    public ProgramTests()
    {
        _messageLoggerMock = new Mock<IUserMessageLogger>();
    }

    [Fact]
    public async Task Main_NoProjectId_ReturnsError()
    {
        Program.Logger = _messageLoggerMock.Object;
        var result = await Program.Main(Array.Empty<string>());

        _messageLoggerMock.Verify(
            n => n.LogErrorAsync(It.Is<string>(m => m.Contains("You have to provide at least the 'ProjectId' argument. See http://bit.ly/k-params for more details on configuration."))),
            Times.Once);
        result.Should().Be(1);
    }

    [Fact]
    public async Task Main_ValidArgs_Returns()
    {
        Program.Logger = _messageLoggerMock.Object;
        var result = await Program.Main(new string[] { "-p", Guid.NewGuid().ToString() });

        _messageLoggerMock.Verify(
            n => n.LogInfo(It.Is<string>(m => m.Contains("Models were generated for"))),
            Times.Once);
        result.Should().Be(0);
    }

    [Fact]
    public async Task Main_InvalidArgs_Returns()
    {
        var unsupportedParam = "-pdasdadsas";

        Program.Logger = _messageLoggerMock.Object;
        _messageLoggerMock.Setup(n => n.LogErrorAsync(It.Is<string>(m => m == "Failed to run due to invalid configuration.")));
        _messageLoggerMock.Setup(n => n.LogErrorAsync(It.Is<string>(m => m.Contains(unsupportedParam))));

        var result = await Program.Main(new string[] { unsupportedParam, Guid.NewGuid().ToString() });

        _messageLoggerMock.VerifyAll();
        result.Should().Be(1);
    }
}
