using System;
using Kentico.Kontent.ModelGenerator.Core.Common;
using Xunit;

namespace Kentico.Kontent.ModelGenerator.Tests.Common
{
    public class PropertyTests
    {
        [Fact]
        public void Constructor_ObjectIsInitializedWithCorrectValues()
        {
            var element = new Property("element_codename", "string");

            Assert.Equal("ElementCodename", element.Identifier);
            Assert.Equal("string", element.TypeName);
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
        public void DAPIModel_FromContentType(string contentType, string expectedTypeName)
        {
            var codename = "element_codename";
            var expectedCodename = "ElementCodename";

            var element = Property.FromContentType(codename, contentType, false);

            Assert.Equal(expectedCodename, element.Identifier);
            Assert.Equal(expectedTypeName, element.TypeName);
        }

        [Theory]
        [InlineData("text", "TextElement")]
        [InlineData("rich_text", "RichTextElement")]
        [InlineData("number", "NumberElement")]
        [InlineData("multiple_choice", "MultipleChoiceElement")]
        [InlineData("date_time", "DateTimeElement")]
        [InlineData("asset", "AssetElement")]
        [InlineData("modular_content", "LinkedItemsElement")]
        [InlineData("taxonomy", "TaxonomyElement")]
        [InlineData("url_slug", "UrlSlugElement")]
        [InlineData("custom", "CustomElement")]
        public void CMAPIModel_FromContentType(string contentType, string expectedTypeName)
        {
            var codename = "element_codename";
            var expectedCodename = "ElementCodename";

            var element = Property.FromContentType(codename, contentType, true);

            Assert.Equal(expectedCodename, element.Identifier);
            Assert.Equal(expectedTypeName, element.TypeName);
        }

        [Fact]
        public void FromContentType_ThrowsAnExceptionForInvalidContentType()
        {
            Assert.Throws<ArgumentException>(() => Property.FromContentType("codename", "unknown content type"));
        }
    }
}
