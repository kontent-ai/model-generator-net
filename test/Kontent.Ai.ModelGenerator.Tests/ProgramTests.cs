namespace Kontent.Ai.ModelGenerator.Tests;

public class ProgramTests
{
    [Fact]
    public async Task CreateCodeGeneratorOptions_NoProjectId_ReturnsError()
    {
        var result = await Program.Main(Array.Empty<string>());
        result.Should().Be(1);
    }
}
