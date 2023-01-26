using Kontent.Ai.ModelGenerator.Core.Configuration;
using Xunit;

namespace Kontent.Ai.ModelGenerator.Tests.Configuration;

public class CodeGeneratorOptionsExtensionsTests
{
    [Theory]
    [InlineData(StructuredModelFlags.DateTime)]
    [InlineData(StructuredModelFlags.RichText)]
    [InlineData(StructuredModelFlags.True)]
    [InlineData(StructuredModelFlags.RichText | StructuredModelFlags.DateTime)]
    [InlineData(StructuredModelFlags.True | StructuredModelFlags.DateTime)]
    [InlineData(StructuredModelFlags.True | StructuredModelFlags.RichText)]
    [InlineData(StructuredModelFlags.True | StructuredModelFlags.RichText | StructuredModelFlags.DateTime)]
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

    [Theory]
    [InlineData(StructuredModelFlags.True)]
    [InlineData(StructuredModelFlags.RichText)]
    [InlineData(StructuredModelFlags.RichText | StructuredModelFlags.True)]
    public void IsStructuredModelRichText_Enabled_ReturnsTrue(StructuredModelFlags structuredModel)
    {
        var options = new CodeGeneratorOptions
        {
            StructuredModel = structuredModel.ToString()
        };

        var result = options.IsStructuredModelRichText();

        Assert.True(result);
    }

    [Theory]
    [InlineData(StructuredModelFlags.DateTime)]
    [InlineData(StructuredModelFlags.ValidationIssue)]
    [InlineData(StructuredModelFlags.NotSet)]
    public void IsStructuredModelRichText_Disabled_ReturnsTrue(StructuredModelFlags structuredModel)
    {
        var options = new CodeGeneratorOptions
        {
            StructuredModel = structuredModel.ToString()
        };

        var result = options.IsStructuredModelRichText();

        Assert.False(result);
    }

    [Fact]
    public void IsIsStructuredModelRichText_Disabled_InvalidString_ReturnsFalse()
    {
        var options = new CodeGeneratorOptions
        {
            StructuredModel = "StructuredModel"
        };

        var result = options.IsStructuredModelRichText();

        Assert.False(result);
    }
}
