using Kontent.Ai.ModelGenerator.Core.Configuration;
using Xunit;

namespace Kontent.Ai.ModelGenerator.Tests.Configuration;

public class CodeGeneratorOptionsTests
{
    [Theory]
    [InlineData(StructuredModelFlags.DateTime)]
    [InlineData(StructuredModelFlags.DateTime | StructuredModelFlags.True | StructuredModelFlags.NotSet | StructuredModelFlags.RichText | StructuredModelFlags.ValidationIssue)]
    public void StructuredModelFlags_CorrectOptions(StructuredModelFlags structuredModel)
    {
        var codeGenerationOptions = new CodeGeneratorOptions
        {
            StructuredModel = structuredModel.ToString()
        };

        Assert.Equal(structuredModel, codeGenerationOptions.StructuredModelFlags);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void StructuredModelFlags_NullOrWhiteSpace_ReturnsNotSet(string structuredModel)
    {
        var codeGenerationOptions = new CodeGeneratorOptions
        {
            StructuredModel = structuredModel
        };

        Assert.Equal(StructuredModelFlags.NotSet, codeGenerationOptions.StructuredModelFlags);
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("invalid,invalid")]
    [InlineData("invalid,")]
    [InlineData(",invalid")]
    [InlineData(",")]
    public void StructuredModelFlags_InvalidEnumValue_ReturnsValidationIssue(string structuredModel)
    {
        var codeGenerationOptions = new CodeGeneratorOptions
        {
            StructuredModel = structuredModel
        };

        Assert.Equal(StructuredModelFlags.ValidationIssue, codeGenerationOptions.StructuredModelFlags);
    }

    [Theory]
    [InlineData("invalid,DateTime", StructuredModelFlags.ValidationIssue | StructuredModelFlags.DateTime)]
    [InlineData("DateTime,invalid", StructuredModelFlags.DateTime | StructuredModelFlags.ValidationIssue)]
    public void StructuredModelFlags_InvalidEnumValueWithValid_ReturnsValidationIssueAndValidEnumValue(string structuredModel, StructuredModelFlags expected)
    {
        var codeGenerationOptions = new CodeGeneratorOptions
        {
            StructuredModel = structuredModel
        };

        Assert.Equal(expected, codeGenerationOptions.StructuredModelFlags);
    }
}
