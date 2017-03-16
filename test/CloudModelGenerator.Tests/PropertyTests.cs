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
        [InlineData("element_codename", "text", "ElementCodename", "string")]
        public void FromContentType(string codename, string contentType, string expectedCodename, string expectedTypeName)
        {
            var element = Property.FromContentType(codename, contentType);

            Assert.Equal("ElementCodename", element.Identifier);
            Assert.Equal("string", element.TypeName);
        }

        [Fact]
        public void FromContentType_ThrowsAnExceptionForInvalidContentType()
        {
            Assert.Throws<ArgumentException>(() => Property.FromContentType("codename", "unknown content type"));
        }
    }
}
