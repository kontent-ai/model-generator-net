﻿using Kontent.Ai.ModelGenerator.Core.Common;
using Kontent.Ai.ModelGenerator.Core.Configuration;
using Kontent.Ai.ModelGenerator.Core.Contract;
using Kontent.Ai.ModelGenerator.Core.Generators.Class;

namespace Kontent.Ai.ModelGenerator.Core.Tests.Common;

public class ClassCodeGeneratorFactoryTests
{
    private readonly IClassCodeGeneratorFactory _classCodeGeneratorFactory;
    private readonly IUserMessageLogger _userMessageLogger;

    public ClassCodeGeneratorFactoryTests()
    {
        _userMessageLogger = new UserMessageLogger();
        _classCodeGeneratorFactory = new ClassCodeGeneratorFactory();
    }

    [Fact]
    public void CreateClassCodeGenerator_CodeGeneratorOptionsIsNull_ThrowsException()
    {
        var createClassCodeGeneratorCall = () =>
            _classCodeGeneratorFactory.CreateClassCodeGenerator(null, new ClassDefinition("codename"), "classFileName", _userMessageLogger);

        createClassCodeGeneratorCall.Should().ThrowExactly<ArgumentNullException>();
    }

    [Fact]
    public void CreateClassCodeGenerator_ClassDefinitionIsNull_ThrowsException()
    {
        var createClassCodeGeneratorCall = () =>
            _classCodeGeneratorFactory.CreateClassCodeGenerator(new CodeGeneratorOptions(), null, "classFileName", _userMessageLogger);

        createClassCodeGeneratorCall.Should().ThrowExactly<ArgumentNullException>();
    }

    [Fact]
    public void CreateClassCodeGenerator_ClassFilenameIsNull_ThrowsException()
    {
        var createClassCodeGeneratorCall = () =>
            _classCodeGeneratorFactory.CreateClassCodeGenerator(new CodeGeneratorOptions(), new ClassDefinition("codename"), null, _userMessageLogger);

        createClassCodeGeneratorCall.Should().ThrowExactly<ArgumentNullException>();
    }

    [Fact]
    public void CreateClassCodeGenerator_LoggerIsNull_ThrowsException()
    {
        var createClassCodeGeneratorCall = () =>
            _classCodeGeneratorFactory.CreateClassCodeGenerator(new CodeGeneratorOptions(), new ClassDefinition("codename"), "classFileName", null);

        createClassCodeGeneratorCall.Should().ThrowExactly<ArgumentNullException>();
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

        var result = _classCodeGeneratorFactory.CreateClassCodeGenerator(codeGeneratorOptions, new ClassDefinition(classDefinitionCodename), classFileName, _userMessageLogger, false);

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

        var result = _classCodeGeneratorFactory.CreateClassCodeGenerator(codeGeneratorOptions, new ClassDefinition(classDefinitionCodename), classFileName, _userMessageLogger);

        AssertClassCodeGenerator<DeliveryClassCodeGenerator>(result, classDefinitionCodename, classFileName, ClassCodeGenerator.DefaultNamespace);
    }

    [Fact]
    public void CreateClassCodeGenerator_ExtendedDeliveryClassCodeGenerator_NoCustomPartialProperty_Returns()
    {
        var classDefinitionCodename = "codename";
        var classFileName = "classFileName";
        var codeGeneratorOptions = new CodeGeneratorOptions
        {
            ManagementApi = false,
            ExtendedDeliveryModels = true
        };

        var result = _classCodeGeneratorFactory.CreateClassCodeGenerator(codeGeneratorOptions, new ClassDefinition(classDefinitionCodename), classFileName, _userMessageLogger);

        AssertClassCodeGenerator<ExtendedDeliveryClassCodeGenerator>(result, classDefinitionCodename, classFileName, ClassCodeGenerator.DefaultNamespace);
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

        var result = _classCodeGeneratorFactory.CreateClassCodeGenerator(codeGeneratorOptions, new ClassDefinition(classDefinitionCodename), classFileName, _userMessageLogger, true);

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

        var result = _classCodeGeneratorFactory.CreateClassCodeGenerator(codeGeneratorOptions, new ClassDefinition(classDefinitionCodename), classFileName, _userMessageLogger, true);

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

        var result = _classCodeGeneratorFactory.CreateClassCodeGenerator(codeGeneratorOptions, new ClassDefinition(classDefinitionCodename), classFileName, _userMessageLogger);

        AssertClassCodeGenerator<DeliveryClassCodeGenerator>(result, classDefinitionCodename, classFileName, customNamespace);
    }

    [Fact]
    public void CreateClassCodeGenerator_ExtendedDeliveryClassCodeGenerator_CustomNamespace_Returns()
    {
        var classDefinitionCodename = "codename";
        var classFileName = "classFileName";
        var customNamespace = "CustomNameSpace";
        var codeGeneratorOptions = new CodeGeneratorOptions
        {
            ManagementApi = false,
            Namespace = customNamespace,
            ExtendedDeliveryModels = true
        };

        var result = _classCodeGeneratorFactory.CreateClassCodeGenerator(codeGeneratorOptions, new ClassDefinition(classDefinitionCodename), classFileName, _userMessageLogger);

        AssertClassCodeGenerator<ExtendedDeliveryClassCodeGenerator>(result, classDefinitionCodename, classFileName, customNamespace);
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

        var result = _classCodeGeneratorFactory.CreateClassCodeGenerator(codeGeneratorOptions, new ClassDefinition(classDefinitionCodename), classFileName, _userMessageLogger);

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

        var result = _classCodeGeneratorFactory.CreateClassCodeGenerator(codeGeneratorOptions, new ClassDefinition(classDefinitionCodename), classFileName, _userMessageLogger);

        AssertClassCodeGenerator<ManagementClassCodeGenerator>(result, classDefinitionCodename, classFileName, customNamespace);
    }

    private static void AssertClassCodeGenerator<T>(ClassCodeGenerator result, string classDefinitionCodename, string classFileName, string @namespace)
    {
        result.Should().BeOfType<T>();
        result.ClassDefinition.Codename.Should().Be(classDefinitionCodename);
        result.ClassFilename.Should().Be(classFileName);
        result.Namespace.Should().Be(@namespace, result.Namespace);
    }
}
