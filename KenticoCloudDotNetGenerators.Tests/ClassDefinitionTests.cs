using NUnit.Framework;
using System;
using System.Linq;

namespace KenticoCloudDotNetGenerators.Tests
{
    [TestFixture]
    public class ClassDefinitionTests
    {
        [TestCase]
        public void Constructor_SetsClassNameIdentifier()
        {
            var definition = new ClassDefinition("Article type");

            Assert.AreEqual("ArticleType", definition.ClassName);
        }

        [TestCase]
        public void AddElement_AddsValidElement()
        {
            var classDefinition = new ClassDefinition("Class name");
            classDefinition.AddProperty(Property.FromContentType("element_1", "text"));

            Assert.AreEqual(1, classDefinition.Properties.Count());            
        }

        [TestCase]
        public void AddElement_RewritesSystemFieldWithUsersCustomOne()
        {
            var definition = new ClassDefinition("Class name");

            var userDefinedSystemProperty = Property.FromContentType("system", "text");
            definition.AddProperty(userDefinedSystemProperty);
            
            Assert.AreEqual(userDefinedSystemProperty, definition.Properties.First());
        }

        [TestCase]
        public void AddElement_ThrowsAnExceptionWhenAddingElementWithSameCodename()
        {
            var definition = new ClassDefinition("Class name");
            definition.AddProperty(Property.FromContentType("element", "text"));

            Assert.Throws<InvalidOperationException>(() => definition.AddProperty(Property.FromContentType("element", "text")));
            Assert.AreEqual(1, definition.Properties.Count);
        }
    }
}
