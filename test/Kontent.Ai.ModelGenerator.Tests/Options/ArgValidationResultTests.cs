using Kontent.Ai.ModelGenerator.Options;

namespace Kontent.Ai.ModelGenerator.Tests.Options;

public class ArgValidationResultTests
{
    [Fact]
    public void Constructor_NoUnsupportedArgs_Returns()
    {
        var argValidationResult = new ArgValidationResult(Array.Empty<string>());

        argValidationResult.HasUnsupportedParams.Should().BeFalse();
        argValidationResult.UnsupportedArgs.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_UnsupportedArgs_Returns()
    {
        var unsupportedArgs = new List<string> { "some", "params" };

        var argValidationResult = new ArgValidationResult(unsupportedArgs);

        argValidationResult.HasUnsupportedParams.Should().BeTrue();
        argValidationResult.UnsupportedArgs.Should().BeEquivalentTo(unsupportedArgs);
    }
}
