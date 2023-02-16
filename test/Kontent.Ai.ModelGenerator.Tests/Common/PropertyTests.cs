using System;
using System.Collections.Generic;
using System.Linq;
using Kontent.Ai.Management.Models.Types.Elements;
using Kontent.Ai.ModelGenerator.Core.Common;
using Xunit;

namespace Kontent.Ai.ModelGenerator.Tests.Common;

public class PropertyTests
{
    [Fact]
    public void Constructor_MissingIdParam_ObjectIsInitializedWithCorrectValues()
    {
        var element = new Property("element_codename", "string");

        Assert.Equal("ElementCodename", element.Identifier);
        Assert.Equal("string", element.TypeName);
        Assert.Null(element.Id);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("id")]
    public void Constructor_IdParamPresent_ObjectIsInitializedWithCorrectValues(string id)
    {
        var element = new Property("element_codename", "string", id);

        Assert.Equal("ElementCodename", element.Identifier);
        Assert.Equal("string", element.TypeName);
        Assert.Equal(id, element.Id);
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
    [InlineData("taxonomy", "IEnumerable<ITaxonomyTerm>")]
    [InlineData("url_slug", "string")]
    [InlineData("custom", "string")]
    public void FromContentTypeElement_DeliveryApiModel_Returns(string contentType, string expectedTypeName)
    {
        var codename = "element_codename";
        var expectedCodename = "ElementCodename";

        var element = Property.FromContentTypeElement(codename, contentType);

        Assert.Equal(expectedCodename, element.Identifier);
        Assert.Equal(expectedTypeName, element.TypeName);
    }

    [Theory, MemberData(nameof(ManagementElements))]
    public void FromContentTypeElement_ManagementApiModel_Returns(string expectedTypeName, string expectedCodename, ElementMetadataBase element)
    {
        var property = Property.FromContentTypeElement(element);

        Assert.Equal(expectedCodename, property.Identifier);
        Assert.Equal(expectedTypeName, property.TypeName);
        Assert.Equal(element.Id.ToString(), property.Id);
    }

    [Fact]
    public void FromContentTypeElement_DeliveryApiModel_InvalidContentTypeElement_Throws()
    {
        Assert.Throws<ArgumentException>(() => Property.FromContentTypeElement("codename", "unknown content type"));
    }

    [Fact]
    public void FromContentTypeElement_ManagementApiModel_GuidelinesElement_Throws()
    {
        Assert.Throws<UnsupportedTypeException>(() =>
            Property.FromContentTypeElement(TestHelper.GenerateGuidelinesElement(Guid.NewGuid(), "codename")));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void IsDateTimeElementType_NullOrWhiteSpace_ReturnsFalse(string elementType)
    {
        var result = Property.IsDateTimeElementType(elementType);

        Assert.False(result);
    }

    [Theory]
    [InlineData("text")]
    [InlineData("date_time(structured)")]
    public void IsDateTimeElementType_NotDateTimeElementType_ReturnsFalse(string elementType)
    {
        var result = Property.IsDateTimeElementType(elementType);

        Assert.False(result);
    }

    [Fact]
    public void IsDateTimeElementType_ReturnsTrue()
    {
        var result = Property.IsDateTimeElementType("date_time");

        Assert.True(result);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void IsRichTextElementType_NullOrWhiteSpace_ReturnsFalse(string elementType)
    {
        var result = Property.IsRichTextElementType(elementType);

        Assert.False(result);
    }

    [Theory]
    [InlineData("text")]
    [InlineData("rich_text(structured)")]
    public void IsRichTextElementType_NotDateTimeElementType_ReturnsFalse(string elementType)
    {
        var result = Property.IsRichTextElementType(elementType);

        Assert.False(result);
    }

    [Fact]
    public void IsRichTextElementType_ReturnsTrue()
    {
        var result = Property.IsRichTextElementType("rich_text");

        Assert.True(result);
    }

    public static IEnumerable<object[]> ManagementElements =>
        new List<(string, string, ElementMetadataBase)>
        {
            ("TextElement", "TextElement",
                TestHelper.GenerateElementMetadataBase(Guid.NewGuid(), "text_element")),
            ("RichTextElement","RichTextElement",
                TestHelper.GenerateElementMetadataBase(Guid.NewGuid(), "rich_text_element", ElementMetadataType.RichText)),
            ("NumberElement", "NumberElement",
                TestHelper.GenerateElementMetadataBase(Guid.NewGuid(), "number_element", ElementMetadataType.Number)),
            ("MultipleChoiceElement","MultipleChoiceElement",
                TestHelper.GenerateElementMetadataBase(Guid.NewGuid(), "multiple_choice_element", ElementMetadataType.MultipleChoice)),
            ("DateTimeElement","DateTimeElement",
                TestHelper.GenerateElementMetadataBase(Guid.NewGuid(), "date_time_element", ElementMetadataType.DateTime)),
            ("AssetElement", "AssetElement",
                TestHelper.GenerateElementMetadataBase(Guid.NewGuid(), "asset_element", ElementMetadataType.Asset)),
            ("LinkedItemsElement", "LinkedItemsElement",
                TestHelper.GenerateElementMetadataBase(Guid.NewGuid(), "linked_items_element", ElementMetadataType.LinkedItems)),
            ("SubpagesElement", "SubpagesElement",
                TestHelper.GenerateElementMetadataBase(Guid.NewGuid(), "subpages_element", ElementMetadataType.Subpages)),
            ("TaxonomyElement", "TaxonomyElement",
                TestHelper.GenerateElementMetadataBase(Guid.NewGuid(), "taxonomy_element", ElementMetadataType.Taxonomy)),
            ("UrlSlugElement", "UrlSlugElement",
                TestHelper.GenerateElementMetadataBase(Guid.NewGuid(), "url_slug_element", ElementMetadataType.UrlSlug)),
            ("CustomElement", "CustomElement",
                TestHelper.GenerateElementMetadataBase(Guid.NewGuid(), "custom_element", ElementMetadataType.Custom))
        }.Select(triple => new object[] { triple.Item1, triple.Item2, triple.Item3 });
}
