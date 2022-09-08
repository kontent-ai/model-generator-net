using System;
using Kontent.Ai.ModelGenerator.Core.Common;
using Kontent.Ai.ModelGenerator.Core.Configuration;
using Kontent.Ai.ModelGenerator.Core.Generators.Class;
using Xunit;

namespace Kontent.Ai.ModelGenerator.Tests.Common;

public class ClassCodeGeneratorFactoryTests
{
    [Fact]
    public void CreateClassCodeGenerator_CodeGeneratorOptionsIsNull_ThrowsException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            ClassCodeGeneratorFactory.CreateClassCodeGenerator(null, new ClassDefinition("codename"), "classFileName"));
    }

    [Fact]
    public void CreateClassCodeGenerator_ClassDefinitionIsNull_ThrowsException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            ClassCodeGeneratorFactory.CreateClassCodeGenerator(new CodeGeneratorOptions(), null, "classFileName"));
    }

    [Fact]
    public void CreateClassCodeGenerator_ClassFilenameIsNull_ThrowsException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            ClassCodeGeneratorFactory.CreateClassCodeGenerator(new CodeGeneratorOptions(), new ClassDefinition("codename"), null));
    }

    [Fact]
    public void CreateClassCodeGenerator_DeliveryClassCodeGenerator_NoCustomPartialPropertyFalse_Returns()
    {
        var classDefinitionCodename = "codename";
        var classFileName = "classFileName";
        var codeGeneratorOptions = new CodeGeneratorOptions
        {
            ManagementApi = false
        };

        var result = ClassCodeGeneratorFactory.CreateClassCodeGenerator(codeGeneratorOptions, new ClassDefinition(classDefinitionCodename), classFileName, false);

        AssertClassCodeGenerator<DeliveryClassCodeGenerator>(result, classDefinitionCodename, classFileName, ClassCodeGenerator.DefaultNamespace);
    }

    [Fact]
    public void CreateClassCodeGenerator_DeliveryClassCodeGenerator_NoCustomPartialProperty_Returns()
    {
        var classDefinitionCodename = "codename";
        var classFileName = "classFileName";
        var codeGeneratorOptions = new CodeGeneratorOptions
        {
            ManagementApi = false
        };

        var result = ClassCodeGeneratorFactory.CreateClassCodeGenerator(codeGeneratorOptions, new ClassDefinition(classDefinitionCodename), classFileName);

        AssertClassCodeGenerator<DeliveryClassCodeGenerator>(result, classDefinitionCodename, classFileName, ClassCodeGenerator.DefaultNamespace);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void CreateClassCodeGenerator_PartialClassCodeGenerator_Returns(bool managementApi)
    {
        var classDefinitionCodename = "codename";
        var classFileName = "classFileName";
        var codeGeneratorOptions = new CodeGeneratorOptions
        {
            ManagementApi = managementApi
        };

        var result = ClassCodeGeneratorFactory.CreateClassCodeGenerator(codeGeneratorOptions, new ClassDefinition(classDefinitionCodename), classFileName, true);

        AssertClassCodeGenerator<PartialClassCodeGenerator>(result, classDefinitionCodename, classFileName, ClassCodeGenerator.DefaultNamespace);
    }

    [Fact]
    public void CreateClassCodeGenerator_PartialClassCodeGenerator_CustomNamespace_Returns()
    {
        var classDefinitionCodename = "codename";
        var classFileName = "classFileName";
        var customNamespace = "CustomNameSpace";
        var codeGeneratorOptions = new CodeGeneratorOptions
        {
            ManagementApi = false,
            Namespace = customNamespace
        };

        var result = ClassCodeGeneratorFactory.CreateClassCodeGenerator(codeGeneratorOptions, new ClassDefinition(classDefinitionCodename), classFileName, true);

        AssertClassCodeGenerator<PartialClassCodeGenerator>(result, classDefinitionCodename, classFileName, customNamespace);
    }

    [Fact]
    public void CreateClassCodeGenerator_DeliveryClassCodeGenerator_CustomNamespace_Returns()
    {
        var classDefinitionCodename = "codename";
        var classFileName = "classFileName";
        var customNamespace = "CustomNameSpace";
        var codeGeneratorOptions = new CodeGeneratorOptions
        {
            ManagementApi = false,
            Namespace = customNamespace
        };

        var result = ClassCodeGeneratorFactory.CreateClassCodeGenerator(codeGeneratorOptions, new ClassDefinition(classDefinitionCodename), classFileName);

        AssertClassCodeGenerator<DeliveryClassCodeGenerator>(result, classDefinitionCodename, classFileName, customNamespace);
    }

    [Fact]
    public void CreateClassCodeGenerator_ManagementClassCodeGenerator_NoCustomPartialProperty_Returns()
    {
        var classDefinitionCodename = "codename";
        var classFileName = "classFileName";
        var codeGeneratorOptions = new CodeGeneratorOptions
        {
            ManagementApi = true
        };

        var result = ClassCodeGeneratorFactory.CreateClassCodeGenerator(codeGeneratorOptions, new ClassDefinition(classDefinitionCodename), classFileName);

        AssertClassCodeGenerator<ManagementClassCodeGenerator>(result, classDefinitionCodename, classFileName, ClassCodeGenerator.DefaultNamespace);
    }

    [Fact]
    public void CreateClassCodeGenerator_ManagementClassCodeGenerator_CustomNamespace_Returns()
    {
        var classDefinitionCodename = "codename";
        var classFileName = "classFileName";
        var customNamespace = "CustomNameSpace";
        var codeGeneratorOptions = new CodeGeneratorOptions
        {
            ManagementApi = true,
            Namespace = customNamespace
        };

        var result = ClassCodeGeneratorFactory.CreateClassCodeGenerator(codeGeneratorOptions, new ClassDefinition(classDefinitionCodename), classFileName);

        AssertClassCodeGenerator<ManagementClassCodeGenerator>(result, classDefinitionCodename, classFileName, customNamespace);
    }

    private static void AssertClassCodeGenerator<T>(ClassCodeGenerator result, string classDefinitionCodename, string classFileName, string @namespace)
    {
        Assert.IsType<T>(result);
        Assert.Equal(classDefinitionCodename, result.ClassDefinition.Codename);
        Assert.Equal(classFileName, result.ClassFilename);
        Assert.Equal(@namespace, result.Namespace);
    }
}
