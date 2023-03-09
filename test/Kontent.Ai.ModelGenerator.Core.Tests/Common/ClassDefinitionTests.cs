using FluentAssertions;
using Kontent.Ai.ModelGenerator.Core.Common;

namespace Kontent.Ai.ModelGenerator.Core.Tests.Common;

public class ClassDefinitionTests
{
    [Fact]
    public void Constructor_SetsClassNameIdentifier()
    {
        var definition = new ClassDefinition("Article type");

        definition.ClassName.Should().Be("ArticleType");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void Constructor_CodenameIsNullEmptyOrWhiteSpace_Throws(string codename)
    {
        var call = () => new ClassDefinition(codename);

        call.Should().ThrowExactly<ArgumentException>();
    }

    [Fact]
    public void AddProperty_AddCustomProperty_PropertyIsAdded()
    {
        var propertyCodename = "element_1";
        var classDefinition = new ClassDefinition("Class name");
        classDefinition.AddProperty(Property.FromContentTypeElement(propertyCodename, "text"));

        classDefinition.Properties.Should().ContainSingle(property => property.Codename == propertyCodename);
    }

    [Fact]
    public void AddProperty_CustomSystemField_SystemFieldIsReplaced()
    {
        var classDefinition = new ClassDefinition("Class name");

        var userDefinedSystemProperty = Property.FromContentTypeElement("system", "text");
        classDefinition.AddProperty(userDefinedSystemProperty);

        classDefinition.Properties.First().Should().Be(userDefinedSystemProperty);
    }

    [Fact]
    public void AddSystemProperty_SystemPropertyIsAdded()
    {
        var classDefinition = new ClassDefinition("Class name");
        classDefinition.AddSystemProperty();

        classDefinition.Properties.Should().ContainSingle(property => property.Codename == "system");
    }

    [Fact]
    public void AddPropertyCodenameConstant_PropertyIsAdded()
    {
        var elementCodename = "element_codename";

        var classDefinition = new ClassDefinition("Class name");
        classDefinition.AddPropertyCodenameConstant(elementCodename);

        classDefinition.PropertyCodenameConstants.Should().ContainSingle(property => property == elementCodename);
    }

    [Fact]
    public void AddPropertyCodenameConstant_DuplicatePropertyCodenameConstant_Throws()
    {
        var elementCodename = "element_codename";

        var classDefinition = new ClassDefinition("Class name");
        classDefinition.AddPropertyCodenameConstant(elementCodename);

        var call = () => classDefinition.AddPropertyCodenameConstant(elementCodename);

        call.Should().ThrowExactly<InvalidOperationException>();
        classDefinition.PropertyCodenameConstants.Should().ContainSingle(property => property == elementCodename);
    }

    [Fact]
    public void AddProperty_DuplicateElementCodenames_Throws()
    {
        var classDefinition = new ClassDefinition("Class name");
        classDefinition.AddProperty(Property.FromContentTypeElement("element", "text"));

        var call = () => classDefinition.AddProperty(Property.FromContentTypeElement("element", "text"));

        call.Should().ThrowExactly<InvalidOperationException>();
        classDefinition.Properties.Should().ContainSingle();
    }
}
