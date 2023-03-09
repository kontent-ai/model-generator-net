using FluentAssertions;
using Kontent.Ai.ModelGenerator.Core.Configuration;

namespace Kontent.Ai.ModelGenerator.Core.Tests.Configuration;

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

        codeGenerationOptions.StructuredModelFlags.Should().Be(structuredModel);
    }

    [Fact]
    public void StructuredModelFlags_ObsoleteOption_CorrectOptions()
    {
        var codeGenerationOptions = new CodeGeneratorOptions
        {
            StructuredModel = "true"
        };

        codeGenerationOptions.StructuredModelFlags.Should().Be(StructuredModelFlags.True);
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

        codeGenerationOptions.StructuredModelFlags.Should().Be(StructuredModelFlags.NotSet);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("asdasd")]
    [InlineData("true")]
    [InlineData(null)]
    public void StructuredModel_Get_ReturnsNull(string structuredModel)
    {
        var codeGenerationOptions = new CodeGeneratorOptions
        {
            StructuredModel = structuredModel
        };

        codeGenerationOptions.StructuredModel.Should().BeNull();
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

        codeGenerationOptions.StructuredModelFlags.Should().Be(StructuredModelFlags.ValidationIssue);
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

        codeGenerationOptions.StructuredModelFlags.Should().Be(expected);
    }
}
