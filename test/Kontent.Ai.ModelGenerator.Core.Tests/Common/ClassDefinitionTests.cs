using Kontent.Ai.ModelGenerator.Core.Common;

namespace Kontent.Ai.ModelGenerator.Core.Tests.Common;

public class ClassDefinitionTests
{
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

    [Fact]
    public void AddProperty_ContentTypeCodenameCollision_RenamesProperty()
    {
        var classDefinition = new ClassDefinition("Class name");
        classDefinition.AddProperty(Property.FromContentTypeElement("content_type_codename", "text"));

        classDefinition.Properties.Should().ContainSingle(p => p.Identifier == "_ContentTypeCodename");
    }

    [Fact]
    public void AddProperty_NoCollision_IdentifierUnchanged()
    {
        var classDefinition = new ClassDefinition("Class name");
        classDefinition.AddProperty(Property.FromContentTypeElement("text", "text"));

        classDefinition.Properties.Should().ContainSingle(p => p.Identifier == "Text");
    }

    [Fact]
    public void AddPropertyCodenameConstant_ContentTypeCodenameCollision_TracksRenamedConstant()
    {
        var classDefinition = new ClassDefinition("Class name");
        classDefinition.AddPropertyCodenameConstant("content_type");

        classDefinition.RenamedCodenameConstants.Should().Contain("content_type");
    }

    [Fact]
    public void AddPropertyCodenameConstant_NoCollision_NotTrackedAsRenamed()
    {
        var classDefinition = new ClassDefinition("Class name");
        classDefinition.AddPropertyCodenameConstant("text");

        classDefinition.RenamedCodenameConstants.Should().BeEmpty();
    }
}
