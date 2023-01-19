using Kontent.Ai.ModelGenerator.Core.Configuration;
using Xunit;

namespace Kontent.Ai.ModelGenerator.Tests.Configuration;

public class CodeGeneratorOptionsExtensionsTests
{
    [Theory]
    [InlineData(StructuredModelFlags.DateTime)]
    [InlineData(StructuredModelFlags.RichText)]
    [InlineData(StructuredModelFlags.RichText | StructuredModelFlags.DateTime)]
    public void IsStructuredModelEnabled_Enabled_ReturnsTrue(StructuredModelFlags structuredModel)
    {
        var options = new CodeGeneratorOptions
        {
            StructuredModel = structuredModel.ToString()
        };

        var result = options.IsStructuredModelEnabled();

        Assert.True(result);
    }

    [Theory]
    [InlineData(StructuredModelFlags.NotSet)]
    [InlineData(StructuredModelFlags.ValidationIssue)]
    public void IsStructuredModelEnabled_Disabled_ReturnsFalse(StructuredModelFlags structuredModel)
    {
        var options = new CodeGeneratorOptions
        {
            StructuredModel = structuredModel.ToString()
        };

        var result = options.IsStructuredModelEnabled();

        Assert.False(result);
    }

    [Fact]
    public void IsStructuredModelEnabled_Disabled_InvalidString_ReturnsFalse()
    {
        var options = new CodeGeneratorOptions
        {
            StructuredModel = "StructuredModel"
        };

        var result = options.IsStructuredModelEnabled();

        Assert.False(result);
    }
}
