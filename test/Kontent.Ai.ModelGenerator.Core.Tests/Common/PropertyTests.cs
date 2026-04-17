using Kontent.Ai.ModelGenerator.Core.Common;

namespace Kontent.Ai.ModelGenerator.Core.Tests.Common;

public class PropertyTests
{
    [Fact]
    public void Constructor_MissingIdParam_ObjectIsInitializedWithCorrectValues()
    {
        var element = new Property("element_codename", "string");

        element.Identifier.Should().Be("ElementCodename");
        element.TypeName.Should().Be("string");
        element.Id.Should().BeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("id")]
    public void Constructor_IdParamPresent_ObjectIsInitializedWithCorrectValues(string id)
    {
        var element = new Property("element_codename", "string", id);

        element.Identifier.Should().Be("ElementCodename");
        element.TypeName.Should().Be("string");
        element.Id.Should().Be(id);
    }

    [Theory]
    [InlineData("text", "string?")]
    [InlineData("rich_text", "RichTextContent?")]
    [InlineData("number", "double?")]
    [InlineData("multiple_choice", "IEnumerable<MultipleChoiceOption>?")]
    [InlineData("date_time", "DateTimeContent?")]
    [InlineData("asset", "IEnumerable<Asset>?")]
    [InlineData("modular_content", "IEnumerable<IEmbeddedContent>?")]
    [InlineData("taxonomy", "IEnumerable<TaxonomyTerm>?")]
    [InlineData("url_slug", "string?")]
    [InlineData("custom", "string?")]
    public void FromContentTypeElement_DeliveryApiModel_Returns(string contentType, string expectedTypeName)
    {
        var codename = "element_codename";
        var expectedCodename = "ElementCodename";

        var element = Property.FromContentTypeElement(codename, contentType);

        element.Identifier.Should().Be(expectedCodename);
        element.TypeName.Should().Be(expectedTypeName);
    }

    [Fact]
    public void FromContentTypeElement_DeliveryApiModel_InvalidContentTypeElement_Throws()
    {
        var fromContentTypeElementCall = () => Property.FromContentTypeElement("codename", "unknown content type");

        fromContentTypeElementCall.Should().ThrowExactly<ArgumentException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void IsDateTimeElementType_NullOrWhiteSpace_ReturnsFalse(string elementType)
    {
        var result = Property.IsDateTimeElementType(elementType);

        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("text")]
    [InlineData("date_time(structured)")]
    public void IsDateTimeElementType_NotDateTimeElementType_ReturnsFalse(string elementType)
    {
        var result = Property.IsDateTimeElementType(elementType);

        result.Should().BeFalse();
    }

    [Fact]
    public void IsDateTimeElementType_ReturnsTrue()
    {
        var result = Property.IsDateTimeElementType("date_time");

        result.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void IsRichTextElementType_NullOrWhiteSpace_ReturnsFalse(string elementType)
    {
        var result = Property.IsRichTextElementType(elementType);

        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("text")]
    [InlineData("rich_text(structured)")]
    public void IsRichTextElementType_NotDateTimeElementType_ReturnsFalse(string elementType)
    {
        var result = Property.IsRichTextElementType(elementType);

        result.Should().BeFalse();
    }

    [Fact]
    public void IsRichTextElementType_ReturnsTrue()
    {
        var result = Property.IsRichTextElementType("rich_text");

        result.Should().BeTrue();
    }

    [Theory]
    [InlineData("text")]
    [InlineData("modular_content(structured)")]
    public void IsModularContentElementType_NotModularContentElementType_ReturnsFalse(string elementType)
    {
        var result = Property.IsModularContentElementType(elementType);

        result.Should().BeFalse();
    }

    [Fact]
    public void IsModularContentElementType_ReturnsTrue()
    {
        var result = Property.IsModularContentElementType("modular_content");

        result.Should().BeTrue();
    }
}
