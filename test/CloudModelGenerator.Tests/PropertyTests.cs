using System;
using Xunit;

namespace CloudModelGenerator.Tests
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
        [InlineData("element_codename", "text", false, "ElementCodename", "string")]
        [InlineData("element_codename", "text", true, "ElementCodename", "string")]
        [InlineData("element_codename", "asset", true, "ElementCodename", "IEnumerable<AssetIdentifier>")]
        public void FromContentType(string codename, string contentType, bool cmApi, string expectedCodename, string expectedTypeName)
        {
            var element = Property.FromContentType(codename, contentType, cmApi);

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
