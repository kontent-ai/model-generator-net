using Kontent.Ai.ModelGenerator.Core.Configuration;
using Kontent.Ai.ModelGenerator.Core.Services;
using Microsoft.Extensions.Options;
using Moq;

namespace Kontent.Ai.ModelGenerator.Core.Tests.Services;

public class DeliveryElementServiceTests
{
    [Fact]
    public void Constructor_OptionsIsNull_ThrowsException()
    {
        var call = () => new DeliveryElementService(null);

        call.Should().ThrowExactly<NullReferenceException>();
    }

    [Fact]
    public void Constructor_ManagementApiIsTrue_ThrowsException()
    {
        var mockOptions = new Mock<IOptions<CodeGeneratorOptions>>();
        mockOptions.Setup(x => x.Value).Returns(new CodeGeneratorOptions
        {
            ManagementApi = true
        });

        var call = () => new DeliveryElementService(mockOptions.Object);

        call.Should().ThrowExactly<InvalidOperationException>();
    }

    [Fact]
    public void GetElementType_ElementTypeIsNull_ThrowsException()
    {
        var mockOptions = new Mock<IOptions<CodeGeneratorOptions>>();
        mockOptions.Setup(x => x.Value).Returns(new CodeGeneratorOptions
        {
            ManagementApi = false
        });

        var deliveryElementService = new DeliveryElementService(mockOptions.Object);

        var call = () => deliveryElementService.GetElementType(null);

        call.Should().ThrowExactly<ArgumentNullException>();
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
        var mockOptions = new Mock<IOptions<CodeGeneratorOptions>>();
        mockOptions.Setup(x => x.Value).Returns(new CodeGeneratorOptions
        {
            ManagementApi = false,
            StructuredModel = structuredModel.ToString()
        });

        var deliveryElementService = new DeliveryElementService(mockOptions.Object);

        var result = deliveryElementService.GetElementType(elementType);

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(StructuredModelFlags.RichText, "date_time", "date_time")]
    [InlineData(StructuredModelFlags.True, "date_time", "date_time")]
    [InlineData(StructuredModelFlags.DateTime, "rich_text", "rich_text")]
    [InlineData(StructuredModelFlags.ModularContent, "rich_text", "rich_text")]
    public void GetElementType_StructuredModelForDifferentElement_ReturnsElementType(StructuredModelFlags structuredModel, string elementType, string expected)
    {
        var mockOptions = new Mock<IOptions<CodeGeneratorOptions>>();
        mockOptions.Setup(x => x.Value).Returns(new CodeGeneratorOptions
        {
            ManagementApi = false,
            StructuredModel = structuredModel.ToString()
        });

        var deliveryElementService = new DeliveryElementService(mockOptions.Object);

        var result = deliveryElementService.GetElementType(elementType);

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
        var mockOptions = new Mock<IOptions<CodeGeneratorOptions>>();
        mockOptions.Setup(x => x.Value).Returns(new CodeGeneratorOptions
        {
            ManagementApi = false,
            StructuredModel = structuredModel.ToString()
        });

        var deliveryElementService = new DeliveryElementService(mockOptions.Object);

        var result = deliveryElementService.GetElementType(elementType);

        result.Should().Be(expected);
    }
}
