using System;
using System.Linq;
using Kentico.Kontent.ModelGenerator.Core;
using Xunit;

namespace Kentico.Kontent.ModelGenerator.Tests
{
    public class ClassDefinitionTests
    {
        [Fact]
        public void Constructor_SetsClassNameIdentifier()
        {
            var definition = new ClassDefinition("Article type");

            Assert.Equal("ArticleType", definition.ClassName);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("  ")]
        public void Constructor_CodenameIsNullEmptyOrWhiteSpace_Throws(string codename)
        {
            Assert.Throws<ArgumentException>(() => new ClassDefinition(codename));
        }

        [Fact]
        public void AddElement_AddCustomElement_PropertyIsAdded()
        {
            var classDefinition = new ClassDefinition("Class name");
            classDefinition.AddProperty(Property.FromContentType("element_1", "text"));

            Assert.Single(classDefinition.Properties);
        }

        [Fact]
        public void AddElement_CustomSystemField_SystemFieldIsReplaced()
        {
            var definition = new ClassDefinition("Class name");

            var userDefinedSystemProperty = Property.FromContentType("system", "text");
            definition.AddProperty(userDefinedSystemProperty);

            Assert.Equal(userDefinedSystemProperty, definition.Properties.First());
        }

        [Fact]
        public void AddElement_DuplicateElementCodenames_Throws()
        {
            var definition = new ClassDefinition("Class name");
            definition.AddProperty(Property.FromContentType("element", "text"));

            Assert.Throws<InvalidOperationException>(() => definition.AddProperty(Property.FromContentType("element", "text")));
            Assert.Single(definition.Properties);
        }
    }
}
