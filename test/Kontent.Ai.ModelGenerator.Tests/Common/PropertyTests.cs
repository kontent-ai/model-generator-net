using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Kontent.Ai.Management.Models.Types.Elements;
using Kontent.Ai.ModelGenerator.Core.Common;
using Kontent.Ai.ModelGenerator.Tests.TestHelpers;
using Xunit;

namespace Kontent.Ai.ModelGenerator.Tests.Common;

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
    [InlineData("rich_text" + Property.StructuredSuffix, "IRichTextContent")]
    [InlineData("number", "decimal?")]
    [InlineData("multiple_choice", "IEnumerable<IMultipleChoiceOption>")]
    [InlineData("date_time", "DateTime?")]
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

        element.Identifier.Should().Be(expectedCodename);
        element.TypeName.Should().Be(expectedTypeName);
    }

    [Theory, MemberData(nameof(ManagementElements))]
    public void FromContentTypeElement_ManagementApiModel_Returns(string expectedTypeName, string expectedCodename, ElementMetadataBase element)
    {
        var property = Property.FromContentTypeElement(element);

        property.Identifier.Should().Be(expectedCodename);
        property.TypeName.Should().Be(expectedTypeName);
        property.Id.Should().Be(element.Id.ToString());
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

        fromContentTypeElementCall.Should().ThrowExactly<UnsupportedTypeException>();
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
}
