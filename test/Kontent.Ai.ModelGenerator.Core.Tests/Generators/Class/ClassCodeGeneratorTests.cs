using System.Reflection;
using Kontent.Ai.ModelGenerator.Core.Common;
using Kontent.Ai.ModelGenerator.Core.Generators.Class;

namespace Kontent.Ai.ModelGenerator.Core.Tests.Generators.Class;

public class ClassCodeGeneratorTests
{
    [Theory]
    [MemberData(nameof(GetTypes))]
    public void Constructor_ClassDefinitionIsNull_Throws(Type type)
    {
        var call = () => Activator.CreateInstance(type, ConstructorParams());


        call.Should().ThrowExactly<TargetInvocationException>()
            .And.InnerException.Message.Should().Contain("classDefinition");
    }

    [Theory]
    [MemberData(nameof(GetTypesWithEmptyStringParam))]
    public void Constructor_ClassFileNameIsNullOrEmptyOrWhiteSpace_Returns_ClassDefinitionCodename(Type type, string classFilename)
    {
        var classDefinitionCodename = "classdefinitioncodename";

        var expectedClassFilename = "Classdefinitioncodename";

        var classCodeGenerator = (ClassCodeGenerator)Activator.CreateInstance(type, ConstructorParams(classDefinitionCodename, classFilename));

        classCodeGenerator.Should().NotBeNull();
        classCodeGenerator.ClassFilename.Should().Be(expectedClassFilename);
    }

    [Theory]
    [MemberData(nameof(GetTypes))]
    public void Constructor_CustomClassFileName_Returns_CustomClassFileName(Type type)
    {
        var classDefinitionCodename = "codename";
        var classFilename = "CustomClassFileName";

        var expectedClassFilename = "CustomClassFileName";

        var classCodeGenerator = (ClassCodeGenerator)Activator.CreateInstance(type, ConstructorParams(classDefinitionCodename, classFilename));

        classCodeGenerator.Should().NotBeNull();
        classCodeGenerator.ClassFilename.Should().Be(expectedClassFilename);
    }

    [Theory]
    [MemberData(nameof(GetTypesWithEmptyStringParam))]
    public void Constructor_NamespaceIsNullOrEmptyOrWhiteSpace_Returns_DefaultNamespace(Type type, string @namespace)
    {
        var classDefinitionCodename = "classdefinitioncodename";

        var classCodeGenerator = (ClassCodeGenerator)Activator.CreateInstance(type, ConstructorParams(classDefinitionCodename, null, @namespace));

        classCodeGenerator.Should().NotBeNull();
        classCodeGenerator.Namespace.Should().Be(ClassCodeGenerator.DefaultNamespace);
    }

    [Theory]
    [MemberData(nameof(GetTypes))]
    public void Constructor_CustomNamespace_Returns_CustomNamespace(Type type)
    {
        var classDefinitionCodename = "classdefinitioncodename";
        var customNamespace = "CustomNamespace";

        var classCodeGenerator = (ClassCodeGenerator)Activator.CreateInstance(type, ConstructorParams(classDefinitionCodename, null, customNamespace));


        classCodeGenerator.Should().NotBeNull();
        classCodeGenerator.Namespace.Should().Be(customNamespace);
    }

    public static IEnumerable<object[]> GetTypes()
    {
        yield return new object[] { typeof(PartialClassCodeGenerator) };
        yield return new object[] { typeof(ManagementClassCodeGenerator) };
        yield return new object[] { typeof(DeliveryClassCodeGenerator) };
    }

    public static IEnumerable<object[]> GetTypesWithEmptyStringParam()
    {
        yield return new object[] { typeof(PartialClassCodeGenerator), "" };
        yield return new object[] { typeof(ManagementClassCodeGenerator), "  " };
        yield return new object[] { typeof(DeliveryClassCodeGenerator), null };
    }

    private static object[] ConstructorParams(string classDefinitionCodename = null, string classFileName = null, string @namespace = null)
        => new object[] { GetClassDefinition(classDefinitionCodename), classFileName, @namespace };

    private static ClassDefinition GetClassDefinition(string classDefinitionCodename) => classDefinitionCodename == null
        ? (ClassDefinition)null
        : new ClassDefinition(classDefinitionCodename);
}
