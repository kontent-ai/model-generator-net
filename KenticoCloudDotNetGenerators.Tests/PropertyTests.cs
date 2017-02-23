using NUnit.Framework;
using System;

namespace KenticoCloudDotNetGenerators.Tests
{
    [TestFixture]
    public class PropertyTests
    {
        [TestCase]
        public void Constructor_ObjectIsInitializetWithCorrectValues()
        {
            var element = new Property("element_codename", "string");

            Assert.AreEqual("ElementCodename", element.Identifier);
            Assert.AreEqual("string", element.TypeName);
        }

        [TestCase("element_codename", "text", "ElementCodename", "string")]
        public void FromContentType(string codename, string contentType, string expectedCodename, string expectedTypeName)
        {
            var element = Property.FromContentType(codename, contentType);

            Assert.AreEqual("ElementCodename", element.Identifier);
            Assert.AreEqual("string", element.TypeName);
        }

        [TestCase]
        public void FromContentType_ThrowsAnExceptionForInvalidContentType()
        {
            Assert.Throws<ArgumentException>(() => Property.FromContentType("codename", "unknown content type"));
        }
    }
}
