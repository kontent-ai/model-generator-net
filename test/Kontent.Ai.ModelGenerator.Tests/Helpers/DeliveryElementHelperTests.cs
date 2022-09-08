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

    [Fact]
    public void GetElementType_StructuredModel_ReturnsStructuredElementType()
    {
        var result = DeliveryElementHelper.GetElementType(new CodeGeneratorOptions
        {
            ManagementApi = false,
            StructuredModel = true
        }, "rich_text");

        Assert.Equal("rich_text(structured)", result);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void GetElementType_Returns(bool structuredModel)
    {
        var result = DeliveryElementHelper.GetElementType(new CodeGeneratorOptions
        {
            ManagementApi = false,
            StructuredModel = structuredModel
        }, "text");

        Assert.Equal("text", result);
    }
}
