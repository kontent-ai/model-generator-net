using Kontent.Ai.ModelGenerator.Core.Configuration;
using Kontent.Ai.ModelGenerator.Core.Helpers;

namespace Kontent.Ai.ModelGenerator.Core.Tests.Helpers;

public class DeliveryElementHelperTests
{
    [Fact]
    public void GetElementType_OptionsIsNull_ThrowsException()
    {
        var call = () => DeliveryElementHelper.GetElementType(null, "type");

        call.Should().ThrowExactly<ArgumentNullException>();
    }

    [Fact]
    public void GetElementType_ElementTypeIsNull_ThrowsException()
    {
        var call = () => DeliveryElementHelper.GetElementType(new CodeGeneratorOptions { ManagementApi = false }, null);

        call.Should().ThrowExactly<ArgumentNullException>();
    }

    [Fact]
    public void GetElementType_ManagementApiIsTrue_ThrowsException()
    {
        var call = () => DeliveryElementHelper.GetElementType(new CodeGeneratorOptions { ManagementApi = true }, "type");

        call.Should().ThrowExactly<InvalidOperationException>();
    }

    [Theory]
    [InlineData(StructuredModelFlags.DateTime, "date_time", "date_time(structured)")]
    [InlineData(StructuredModelFlags.RichText, "rich_text", "rich_text(structured)")]
    [InlineData(StructuredModelFlags.True, "rich_text", "rich_text(structured)")]
    [InlineData(StructuredModelFlags.RichText | StructuredModelFlags.DateTime, "rich_text", "rich_text(structured)")]
    [InlineData(StructuredModelFlags.RichText | StructuredModelFlags.DateTime, "date_time", "date_time(structured)")]
    [InlineData(StructuredModelFlags.ModularContent | StructuredModelFlags.DateTime, "modular_content", "modular_content(structured)")]
    [InlineData(StructuredModelFlags.RichText | StructuredModelFlags.True | StructuredModelFlags.DateTime | StructuredModelFlags.ModularContent, "date_time", "date_time(structured)")]
    public void GetElementType_StructuredModel_ReturnsStructuredElementType(StructuredModelFlags structuredModel, string elementType, string expected)
    {
        var result = DeliveryElementHelper.GetElementType(new CodeGeneratorOptions
        {
            ManagementApi = false,
            StructuredModel = structuredModel.ToString()
        }, elementType);

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(StructuredModelFlags.RichText, "date_time", "date_time")]
    [InlineData(StructuredModelFlags.True, "date_time", "date_time")]
    [InlineData(StructuredModelFlags.DateTime, "rich_text", "rich_text")]
    [InlineData(StructuredModelFlags.ModularContent, "rich_text", "rich_text")]
    public void GetElementType_StructuredModelForDifferentElement_ReturnsElementType(StructuredModelFlags structuredModel, string elementType, string expected)
    {
        var result = DeliveryElementHelper.GetElementType(new CodeGeneratorOptions
        {
            ManagementApi = false,
            StructuredModel = structuredModel.ToString()
        }, elementType);

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(StructuredModelFlags.DateTime, "text", "text")]
    [InlineData(StructuredModelFlags.RichText, "text", "text")]
    [InlineData(StructuredModelFlags.True, "text", "text")]
    [InlineData(StructuredModelFlags.ModularContent, "text", "text")]
    [InlineData(StructuredModelFlags.RichText | StructuredModelFlags.DateTime | StructuredModelFlags.ModularContent, "text", "text")]
    [InlineData(StructuredModelFlags.RichText | StructuredModelFlags.True | StructuredModelFlags.DateTime | StructuredModelFlags.ModularContent, "text", "text")]
    [InlineData(StructuredModelFlags.True | StructuredModelFlags.DateTime, "text", "text")]
    [InlineData(StructuredModelFlags.NotSet, "text", "text")]
    public void GetElementType_Returns(StructuredModelFlags structuredModel, string elementType, string expected)
    {
        var result = DeliveryElementHelper.GetElementType(new CodeGeneratorOptions
        {
            ManagementApi = false,
            StructuredModel = structuredModel.ToString()
        }, elementType);

        result.Should().Be(expected);
    }
}
