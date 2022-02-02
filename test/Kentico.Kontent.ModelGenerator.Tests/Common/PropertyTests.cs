using System;
using System.Collections.Generic;
using System.Linq;
using Kentico.Kontent.Management.Models.Types.Elements;
using Kentico.Kontent.ModelGenerator.Core.Common;
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
        public void DeliveryApiModel_FromContentTypeElement(string contentType, string expectedTypeName)
        {
            var codename = "element_codename";
            var expectedCodename = "ElementCodename";

            var element = Property.FromContentTypeElement(codename, contentType);

            Assert.Equal(expectedCodename, element.Identifier);
            Assert.Equal(expectedTypeName, element.TypeName);
        }

        [Theory, MemberData(nameof(ManagementElements))]
        public void ManagementApiModel_FromContentTypeElement(string expectedTypeName, string expectedCodename, ElementMetadataBase element)
        {
            var property = Property.FromContentTypeElement(element);

            Assert.Equal(expectedCodename, property.Identifier);
            Assert.Equal(expectedTypeName, property.TypeName);
            Assert.Equal(element.Id.ToString(), property.Id);
        }

        [Fact]
        public void FromContentTypeElement_ThrowsAnExceptionForInvalidContentType()
        {
            Assert.Throws<ArgumentException>(() => Property.FromContentTypeElement("codename", "unknown content type"));
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
}
