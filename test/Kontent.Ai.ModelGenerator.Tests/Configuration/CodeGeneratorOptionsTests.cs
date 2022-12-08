using System;
using System.Linq;
using Kontent.Ai.ModelGenerator.Core.Configuration;
using Xunit;

namespace Kontent.Ai.ModelGenerator.Tests.Configuration;

public class CodeGeneratorOptionsTests
{
    [Theory]
    [InlineData(ElementReferenceType.Id)]
    [InlineData(ElementReferenceType.Id |
                ElementReferenceType.ExternalId |
                ElementReferenceType.Codename |
                ElementReferenceType.Empty |
                ElementReferenceType.Error)]
    public void ElementReferenceFlags_CorrectOptions(ElementReferenceType elementReference)
    {
        var stringElementReference = string.Join(',', Enum.GetValues<ElementReferenceType>()
            .Where(x => elementReference.HasFlag(x))
            .Select(x => x.ToString()));

        var codeGenerationOptions = new CodeGeneratorOptions
        {
            ElementReference = stringElementReference
        };

        Assert.Equal(elementReference, codeGenerationOptions.ElementReferenceFlags);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void ElementReferenceFlags_NullOrWhiteSpace_ReturnsEmpty(string elementReference)
    {
        var codeGenerationOptions = new CodeGeneratorOptions
        {
            ElementReference = elementReference
        };

        Assert.Equal(ElementReferenceType.Empty, codeGenerationOptions.ElementReferenceFlags);
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("invalid,invalid")]
    [InlineData("invalid,")]
    [InlineData(",invalid")]
    [InlineData(",")]
    public void ElementReferenceFlags_InvalidValue_ReturnsError(string elementReference)
    {
        var codeGenerationOptions = new CodeGeneratorOptions
        {
            ElementReference = elementReference
        };

        Assert.Equal(ElementReferenceType.Error, codeGenerationOptions.ElementReferenceFlags);
    }

    [Theory]
    [InlineData("invalid,Codename", ElementReferenceType.Error | ElementReferenceType.Codename)]
    [InlineData("Codename,invalid", ElementReferenceType.Codename | ElementReferenceType.Error)]
    public void ElementReferenceFlags_InvalidAndValidValues_ReturnsErrorWithValid(string elementReference, ElementReferenceType result)
    {
        var codeGenerationOptions = new CodeGeneratorOptions
        {
            ElementReference = elementReference
        };

        Assert.Equal(result, codeGenerationOptions.ElementReferenceFlags);
    }
}
