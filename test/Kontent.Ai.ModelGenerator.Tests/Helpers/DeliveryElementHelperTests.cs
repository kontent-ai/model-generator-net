using System;
using Kontent.Ai.ModelGenerator.Core.Configuration;
using Kontent.Ai.ModelGenerator.Core.Helpers;
using Xunit;

namespace Kontent.Ai.ModelGenerator.Tests.Helpers;

public class DeliveryElementHelperTests
{
    [Fact]
    public void GetElementType_OptionsIsNull_ThrowsException()
    {
        Assert.Throws<ArgumentNullException>(() => DeliveryElementHelper.GetElementType(null, "type"));
    }

    [Fact]
    public void GetElementType_ElementTypeIsNull_ThrowsException()
    {
        Assert.Throws<ArgumentNullException>(() => DeliveryElementHelper.GetElementType(new CodeGeneratorOptions { ManagementApi = false }, null));
    }

    [Fact]
    public void GetElementType_ManagementApiIsTrue_ThrowsException()
    {
        Assert.Throws<InvalidOperationException>(() => DeliveryElementHelper.GetElementType(new CodeGeneratorOptions { ManagementApi = true }, "type"));
    }

    [Theory]
    [InlineData(StructuredModelFlags.DateTime, "date_time", "date_time(structured)")]
    [InlineData(StructuredModelFlags.RichText, "rich_text", "rich_text(structured)")]
    [InlineData(StructuredModelFlags.True, "rich_text", "rich_text(structured)")]
    [InlineData(StructuredModelFlags.RichText | StructuredModelFlags.DateTime, "rich_text", "rich_text(structured)")]
    [InlineData(StructuredModelFlags.RichText | StructuredModelFlags.DateTime, "date_time", "date_time(structured)")]
    [InlineData(StructuredModelFlags.RichText | StructuredModelFlags.True | StructuredModelFlags.DateTime, "date_time", "date_time(structured)")]
    public void GetElementType_StructuredModel_ReturnsStructuredElementType(StructuredModelFlags structuredModel, string elementType, string expected)
    {
        var result = DeliveryElementHelper.GetElementType(new CodeGeneratorOptions
        {
            ManagementApi = false,
            StructuredModel = structuredModel.ToString()
        }, elementType);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(StructuredModelFlags.RichText, "date_time", "date_time")]
    [InlineData(StructuredModelFlags.True, "date_time", "date_time")]
    [InlineData(StructuredModelFlags.DateTime, "rich_text", "rich_text")]
    public void GetElementType_StructuredModelForDifferentElement_ReturnsElementType(StructuredModelFlags structuredModel, string elementType, string expected)
    {
        var result = DeliveryElementHelper.GetElementType(new CodeGeneratorOptions
        {
            ManagementApi = false,
            StructuredModel = structuredModel.ToString()
        }, elementType);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(StructuredModelFlags.DateTime, "text", "text")]
    [InlineData(StructuredModelFlags.RichText, "text", "text")]
    [InlineData(StructuredModelFlags.True, "text", "text")]
    [InlineData(StructuredModelFlags.RichText | StructuredModelFlags.DateTime, "text", "text")]
    [InlineData(StructuredModelFlags.RichText | StructuredModelFlags.True | StructuredModelFlags.DateTime, "text", "text")]
    [InlineData(StructuredModelFlags.True | StructuredModelFlags.DateTime, "text", "text")]
    [InlineData(StructuredModelFlags.NotSet, "text", "text")]
    public void GetElementType_Returns(StructuredModelFlags structuredModel, string elementType, string expected)
    {
        var result = DeliveryElementHelper.GetElementType(new CodeGeneratorOptions
        {
            ManagementApi = false,
            StructuredModel = structuredModel.ToString()
        }, elementType);

        Assert.Equal(expected, result);
    }
}
