using System;
using System.Linq;
using Kentico.Kontent.Delivery.Abstractions;
using Kentico.Kontent.ModelGenerator.Core;
using Moq;
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
        public void AddProperty_AddCustomProperty_PropertyIsAdded()
        {
            var propertyCodename = "element_1";
            var classDefinition = new ClassDefinition("Class name");
            classDefinition.AddProperty(Property.FromContentType(propertyCodename, "text"));

            Assert.Single(classDefinition.Properties, property => property.Codename == propertyCodename);
        }

        [Fact]
        public void AddProperty_CustomSystemField_SystemFieldIsReplaced()
        {
            var classDefinition = new ClassDefinition("Class name");

            var userDefinedSystemProperty = Property.FromContentType("system", "text");
            classDefinition.AddProperty(userDefinedSystemProperty);

            Assert.Equal(userDefinedSystemProperty, classDefinition.Properties.First());
        }

        [Fact]
        public void AddSystemProperty_SystemPropertyIsAdded()
        {
            var classDefinition = new ClassDefinition("Class name");
            classDefinition.AddSystemProperty();

            Assert.Single(classDefinition.Properties, property => property.Codename == "system");
        }

        [Fact]
        public void AddPropertyCodenameConstant_PropertyIsAdded()
        {
            var elementCodename = "element_codename";
            var contentElementMock = new Mock<IContentElement>();
            contentElementMock.SetupGet(x => x.Codename).Returns(elementCodename);

            var classDefinition = new ClassDefinition("Class name");
            classDefinition.AddPropertyCodenameConstant(contentElementMock.Object);

            Assert.Single(classDefinition.PropertyCodenameConstants, property => property.Codename == elementCodename);
        }

        [Fact]
        public void AddPropertyCodenameConstant_DuplicatePropertyCodenameConstant_Throws()
        {
            var elementCodename = "element_codename";
            var contentElementMock = new Mock<IContentElement>();
            contentElementMock.SetupGet(x => x.Codename).Returns(elementCodename);

            var classDefinition = new ClassDefinition("Class name");
            classDefinition.AddPropertyCodenameConstant(contentElementMock.Object);

            Assert.Throws<InvalidOperationException>(() => classDefinition.AddPropertyCodenameConstant(contentElementMock.Object));
            Assert.Single(classDefinition.PropertyCodenameConstants, property => property.Codename == elementCodename);
        }

        [Fact]
        public void AddProperty_DuplicateElementCodenames_Throws()
        {
            var classDefinition = new ClassDefinition("Class name");
            classDefinition.AddProperty(Property.FromContentType("element", "text"));

            Assert.Throws<InvalidOperationException>(() => classDefinition.AddProperty(Property.FromContentType("element", "text")));
            Assert.Single(classDefinition.Properties);
        }
    }
}
