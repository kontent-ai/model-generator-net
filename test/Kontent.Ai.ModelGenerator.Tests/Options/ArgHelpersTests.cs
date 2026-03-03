using Kontent.Ai.ModelGenerator.Options;

namespace Kontent.Ai.ModelGenerator.Tests.Options;

public class ArgHelpersTests
{
    [Theory]
    [InlineData("--environmentid=123")]
    [InlineData("--environmentId=123")]
    [InlineData("--ENVIRONMENTID=123")]
    public void ContainsValidArgs_EnvironmentIdCasingVariants_ReturnsTrue(string argument)
    {
        var result = ArgHelpers.ContainsValidArgs([argument]);

        result.Should().BeTrue();
    }

    [Fact]
    public void GetSwitchMappings_EnvironmentIdMapping_IsCaseInsensitive()
    {
        var mappings = ArgHelpers.GetSwitchMappings([]);

        mappings.ContainsKey("--environmentid").Should().BeTrue();
        mappings.ContainsKey("--environmentId").Should().BeTrue();
        mappings.Keys.Count(key => key.Equals("--environmentid", StringComparison.OrdinalIgnoreCase)).Should().Be(1);
    }
}
