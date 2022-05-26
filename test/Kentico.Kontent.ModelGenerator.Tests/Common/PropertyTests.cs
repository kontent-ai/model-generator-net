using System;
using System.Collections.Generic;
using System.Linq;
using Kentico.Kontent.Management.Models.Types.Elements;
using Kentico.Kontent.ModelGenerator.Core.Common;
using Kentico.Kontent.ModelGenerator.Core.Generators.Class;
using Xunit;

namespace Kentico.Kontent.ModelGenerator.Tests.Common
{
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

            var property = Property.FromContentTypeElement(codename, contentType);

            Assert.Equal(expectedCodename, property.Identifier);
            Assert.Equal(expectedTypeName, property.TypeName);
        }

        [Theory, MemberData(nameof(ManagementElements))]
        public void FromContentTypeElement_ManagementApiModel_Returns(string expectedTypeName, string expectedCodename, ElementMetadataBase element)
        {
            var property = Property.FromContentTypeElement(element);

            Assert.Equal(expectedCodename, property.Identifier);
            Assert.Equal(expectedTypeName, property.TypeName);
            Assert.Equal(element.Id.ToString(), property.Id);
        }

        [Theory, MemberData(nameof(ManagementElementsForExtendedDeliveryModels))]
        public void FromContentTypeElement_ExtendedDeliverModels_Returns(string expectedTypeName, string elementType, ElementMetadataBase element)
        {
            var property = Property.FromContentTypeElement(element, elementType);

            Assert.Equal(expectedTypeName, property.TypeName);
            Assert.Equal(element.Codename, property.Codename);
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

        [Fact]
        public void FromContentTypeElement_ExtendedDeliverModels_GuidelinesElement_Throws()
        {
            Assert.Throws<UnsupportedTypeException>(() =>
                Property.FromContentTypeElement(TestHelper.GenerateGuidelinesElement(Guid.NewGuid(), "codename"), "type"));
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

        public static IEnumerable<object[]> ManagementElementsForExtendedDeliveryModels =>
            new List<(string, string, ElementMetadataBase)>
            {
                (
                    "string",
                    ElementMetadataType.Text.ToString(),
                    TestHelper.GenerateElementMetadataBase(Guid.NewGuid(), "text_element")
                ),
                (
                    "string",
                    ElementMetadataType.RichText.ToString(),
                    TestHelper.GenerateElementMetadataBase(Guid.NewGuid(), "rich_text_element", ElementMetadataType.RichText)
                ),
                (
                    "IRichTextContent",
                    ElementMetadataType.RichText + Property.StructuredSuffix,
                    TestHelper.GenerateElementMetadataBase(Guid.NewGuid(), "rich_text_element", ElementMetadataType.RichText)
                ),
                (
                    "decimal?",
                    ElementMetadataType.Number.ToString(),
                    TestHelper.GenerateElementMetadataBase(Guid.NewGuid(), "number_element", ElementMetadataType.Number)
                ),
                (
                    "IEnumerable<IMultipleChoiceOption>",
                    ElementMetadataType.MultipleChoice.ToString(),
                    TestHelper.GenerateElementMetadataBase(Guid.NewGuid(), "multiple_choice_element", ElementMetadataType.MultipleChoice)
                ),
                (
                    "DateTime?",
                    ElementMetadataType.DateTime.ToString(),
                    TestHelper.GenerateElementMetadataBase(Guid.NewGuid(), "date_time_element", ElementMetadataType.DateTime)
                ),
                (
                    "IEnumerable<IAsset>",
                    ElementMetadataType.Asset.ToString(),
                    TestHelper.GenerateElementMetadataBase(Guid.NewGuid(), "asset_element", ElementMetadataType.Asset)
                ),
                (
                    $"IEnumerable<{ContentItemClassCodeGenerator.DefaultContentItemClassName}>",
                    $"IEnumerable<{ContentItemClassCodeGenerator.DefaultContentItemClassName}>",
                    TestHelper.GenerateElementMetadataBase(Guid.NewGuid(), "linked_items_element", ElementMetadataType.LinkedItems)
                ),
                (
                    "IEnumerable<Hero>",
                    "IEnumerable<Hero>",
                    TestHelper.GenerateElementMetadataBase(Guid.NewGuid(), "linked_items_element", ElementMetadataType.LinkedItems)
                ),
                (
                    "IEnumerable<ITaxonomyTerm>",
                    ElementMetadataType.Taxonomy.ToString(),
                    TestHelper.GenerateElementMetadataBase(Guid.NewGuid(), "taxonomy_element", ElementMetadataType.Taxonomy)
                ),
                (
                    "string",
                    ElementMetadataType.UrlSlug.ToString(),
                    TestHelper.GenerateElementMetadataBase(Guid.NewGuid(), "url_slug_element", ElementMetadataType.UrlSlug)
                ),
                (
                    "string",
                    ElementMetadataType.Custom.ToString(),
                    TestHelper.GenerateElementMetadataBase(Guid.NewGuid(), "custom_element", ElementMetadataType.Custom)
                )
            }.Select(triple => new object[] { triple.Item1, triple.Item2, triple.Item3 });
    }
}
