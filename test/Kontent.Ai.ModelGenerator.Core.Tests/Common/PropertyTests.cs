using Kontent.Ai.Delivery.Abstractions;
using Kontent.Ai.Management.Models.Types.Elements;
using Kontent.Ai.ModelGenerator.Core.Common;
using Kontent.Ai.ModelGenerator.Core.Tests.TestHelpers;

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
    [InlineData("text", "string")]
    [InlineData("rich_text", "string")]
    [InlineData($"rich_text{Property.StructuredSuffix}", "IRichTextContent")]
    [InlineData("number", "decimal?")]
    [InlineData("multiple_choice", "IEnumerable<IMultipleChoiceOption>")]
    [InlineData("date_time", "DateTime?")]
    [InlineData($"date_time{Property.StructuredSuffix}", "IDateTimeContent")]
    [InlineData("asset", "IEnumerable<IAsset>")]
    [InlineData("modular_content", "IEnumerable<object>")]
    [InlineData($"modular_content{Property.StructuredSuffix}", "IEnumerable<IContentItem>")]
    [InlineData("taxonomy", "IEnumerable<ITaxonomyTerm>")]
    [InlineData("url_slug", "string")]
    [InlineData("custom", "string")]
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

    [Fact]
    public void FromContentTypeElement_ManagementApiModel_GuidelinesElement_Throws()
    {
        var fromContentTypeElementCall = () =>
            Property.FromContentTypeElement(TestDataGenerator.GenerateGuidelinesElement(Guid.NewGuid(), "codename"));

        fromContentTypeElementCall.Should().ThrowExactly<ArgumentException>();
    }

    [Theory]
    [MemberData(nameof(ManagementElements))]
    public void FromContentTypeElement_ManagementApiModel_Returns(string expectedTypeName, string expectedCodename, ElementMetadataBase element)
    {
        var property = Property.FromContentTypeElement(element);

        property.Identifier.Should().Be(expectedCodename);
        property.TypeName.Should().Be(expectedTypeName);
        property.Id.Should().Be(element.Id.ToString());
    }

    [Fact]
    public void FromContentTypeElement_ExtendedDeliveryModel_TypedProperty_GuidelinesElement_Throws()
    {
        var fromContentTypeElementCall = () =>
            Property.FromContentTypeElement(TestDataGenerator.GenerateGuidelinesElement(
                Guid.NewGuid(), "codename"),
                "guidelines",
                "guidelines");

        fromContentTypeElementCall.Should().ThrowExactly<ArgumentException>();
    }

    [Theory]
    [MemberData(nameof(ExtendedDeliveryElements))]
    public void FromContentTypeElement_ExtendedDeliveryModel_TypedProperty_Returns(string elementType, string expectedTypeName, string expectedIdentifier, ElementMetadataBase element)
    {
        var result = Property.FromContentTypeElement(element, elementType, element.Codename);

        result.Identifier.Should().Be(expectedIdentifier);
        result.TypeName.Should().Be(expectedTypeName);
        result.Id.Should().BeNull();
    }

    [Fact]
    public void FromContentTypeElement_ExtendedDeliveryModel_GuidelinesElement_Throws()
    {
        var fromContentTypeElementCall = () =>
            Property.FromContentTypeElement(
                TestDataGenerator.GenerateGuidelinesElement(Guid.NewGuid(), "codename"),
                "guidelines");

        fromContentTypeElementCall.Should().ThrowExactly<ArgumentException>();
    }

    [Theory]
    [MemberData(nameof(ExtendedDeliveryElements))]
    public void FromContentTypeElement_ExtendedDeliveryModel_Returns(string elementType, string expectedTypeName, string expectedIdentifier, ElementMetadataBase element)
    {
        var result = Property.FromContentTypeElement(element, elementType);

        result.Identifier.Should().Be(expectedIdentifier);
        result.TypeName.Should().Be(expectedTypeName);
        result.Id.Should().BeNull();
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

    public static IEnumerable<object[]> ManagementElements =>
        new List<(string, string, ElementMetadataBase)>
        {
            ("TextElement", "TextElement",
                TestDataGenerator.GenerateElementMetadataBase(Guid.NewGuid(), "text_element")),
            ("RichTextElement","RichTextElement",
                TestDataGenerator.GenerateElementMetadataBase(Guid.NewGuid(), "rich_text_element", ElementMetadataType.RichText)),
            ("NumberElement", "NumberElement",
                TestDataGenerator.GenerateElementMetadataBase(Guid.NewGuid(), "number_element", ElementMetadataType.Number)),
            ("MultipleChoiceElement","MultipleChoiceElement",
                TestDataGenerator.GenerateElementMetadataBase(Guid.NewGuid(), "multiple_choice_element", ElementMetadataType.MultipleChoice)),
            ("DateTimeElement","DateTimeElement",
                TestDataGenerator.GenerateElementMetadataBase(Guid.NewGuid(), "date_time_element", ElementMetadataType.DateTime)),
            ("AssetElement", "AssetElement",
                TestDataGenerator.GenerateElementMetadataBase(Guid.NewGuid(), "asset_element", ElementMetadataType.Asset)),
            ("LinkedItemsElement", "LinkedItemsElement",
                TestDataGenerator.GenerateElementMetadataBase(Guid.NewGuid(), "linked_items_element", ElementMetadataType.LinkedItems)),
            ("SubpagesElement", "SubpagesElement",
                TestDataGenerator.GenerateElementMetadataBase(Guid.NewGuid(), "subpages_element", ElementMetadataType.Subpages)),
            ("TaxonomyElement", "TaxonomyElement",
                TestDataGenerator.GenerateElementMetadataBase(Guid.NewGuid(), "taxonomy_element", ElementMetadataType.Taxonomy)),
            ("UrlSlugElement", "UrlSlugElement",
                TestDataGenerator.GenerateElementMetadataBase(Guid.NewGuid(), "url_slug_element", ElementMetadataType.UrlSlug)),
            ("CustomElement", "CustomElement",
                TestDataGenerator.GenerateElementMetadataBase(Guid.NewGuid(), "custom_element", ElementMetadataType.Custom))
        }.Select(triple => new object[] { triple.Item1, triple.Item2, triple.Item3 });

    public static IEnumerable<object[]> ExtendedDeliveryElements =>
        new List<(string, string, string, ElementMetadataBase)>
        {
            (
                ElementMetadataType.Text.ToString(),
                "string",
                "TextElement",
                TestDataGenerator.GenerateElementMetadataBase(Guid.NewGuid(), "text_element")),
            (
                ElementMetadataType.RichText.ToString(),
                "string",
                "RichTextElement",
                TestDataGenerator.GenerateElementMetadataBase(Guid.NewGuid(), "rich_text_element", ElementMetadataType.RichText)),
            (
                $"{ElementMetadataType.RichText}{Property.StructuredSuffix}",
                nameof(IRichTextContent),
                "RichTextElement",
                TestDataGenerator.GenerateElementMetadataBase(Guid.NewGuid(), "rich_text_element", ElementMetadataType.RichText)),
            (
                ElementMetadataType.Number.ToString(),
                "decimal?",
                "NumberElement",
                TestDataGenerator.GenerateElementMetadataBase(Guid.NewGuid(), "number_element", ElementMetadataType.Number)),
            (
                ElementMetadataType.MultipleChoice.ToString(),
                "IEnumerable<IMultipleChoiceOption>",
                "MultipleChoiceElement",
                TestDataGenerator.GenerateElementMetadataBase(Guid.NewGuid(), "multiple_choice_element", ElementMetadataType.MultipleChoice)),
            (
                ElementMetadataType.DateTime.ToString(),
                "DateTime?",
                "DateTimeElement",
                TestDataGenerator.GenerateElementMetadataBase(Guid.NewGuid(), "date_time_element", ElementMetadataType.DateTime)),
            (
                $"{ElementMetadataType.DateTime}{Property.StructuredSuffix}",
                nameof(IDateTimeContent),
                "DateTimeElement",
                TestDataGenerator.GenerateElementMetadataBase(Guid.NewGuid(), "date_time_element", ElementMetadataType.DateTime)),
            (
                ElementMetadataType.Asset.ToString(),
                "IEnumerable<IAsset>",
                "AssetElement",
                TestDataGenerator.GenerateElementMetadataBase(Guid.NewGuid(), "asset_element", ElementMetadataType.Asset)),
            (
                "CustomLinkedItemsType",
                "CustomLinkedItemsType",
                "LinkedItemsElement",
                TestDataGenerator.GenerateElementMetadataBase(Guid.NewGuid(), "linked_items_element", ElementMetadataType.LinkedItems)),
            (
                "CustomSubpagesType",
                "CustomSubpagesType",
                "SubpagesElement",
                TestDataGenerator.GenerateElementMetadataBase(Guid.NewGuid(), "subpages_element", ElementMetadataType.Subpages)),
            (
                ElementMetadataType.Taxonomy.ToString(),
                "IEnumerable<ITaxonomyTerm>",
                "TaxonomyElement",
                TestDataGenerator.GenerateElementMetadataBase(Guid.NewGuid(), "taxonomy_element", ElementMetadataType.Taxonomy)),
            (
                ElementMetadataType.UrlSlug.ToString(),
                "string",
                "UrlSlugElement",
                TestDataGenerator.GenerateElementMetadataBase(Guid.NewGuid(), "url_slug_element", ElementMetadataType.UrlSlug)),
            (
                ElementMetadataType.Custom.ToString(),
                "string",
                "CustomElement",
                TestDataGenerator.GenerateElementMetadataBase(Guid.NewGuid(), "custom_element", ElementMetadataType.Custom))
        }.Select(quadruple => new object[] { quadruple.Item1, quadruple.Item2, quadruple.Item3, quadruple.Item4 });
}
