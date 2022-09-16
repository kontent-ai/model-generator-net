﻿using System;
using System.IO;
using Kontent.Ai.ModelGenerator.Core.Configuration;
using Kontent.Ai.ModelGenerator.Core.Generators.Class;
using Xunit;

namespace Kontent.Ai.ModelGenerator.Tests.Generators.Class;

public class BaseClassCodeGeneratorTests
{
    private readonly CodeGeneratorOptions CodeGeneratorOptions = new CodeGeneratorOptions
    {
        BaseClass = "ContentBase"
    };

    [Fact]
    public void GenerateBaseClassCodeWithDefaultNamespace()
    {
        var codeGenerator = new BaseClassCodeGenerator(CodeGeneratorOptions);

        var executingPath = AppContext.BaseDirectory;
        var expectedBaseClassCode = File.ReadAllText(executingPath + "/Assets/BaseClass_CompiledCode.txt");

        var actualCompiledBaseClass = codeGenerator.GenerateBaseClassCode();

        Assert.Equal(expectedBaseClassCode, actualCompiledBaseClass, ignoreLineEndingDifferences: true, ignoreWhiteSpaceDifferences: true);
    }

    [Fact]
    public void GenerateBaseClassCodeWithCustomNamespace()
    {
        var codeGenerationOptions = new CodeGeneratorOptions
        {
            BaseClass = CodeGeneratorOptions.BaseClass,
            Namespace = "CustomNamespace"
        };
        var codeGenerator = new BaseClassCodeGenerator(codeGenerationOptions);

        var executingPath = AppContext.BaseDirectory;
        var expectedBaseClassCode = File.ReadAllText(executingPath + "/Assets/BaseClass_CompiledCode.txt");
        expectedBaseClassCode = expectedBaseClassCode.Replace(ClassCodeGenerator.DefaultNamespace, CodeGeneratorOptions.Namespace);

        var actualCompiledBaseClass = codeGenerator.GenerateBaseClassCode();

        Assert.Equal(expectedBaseClassCode, actualCompiledBaseClass, ignoreLineEndingDifferences: true, ignoreWhiteSpaceDifferences: true);
    }

    [Fact]
    public void GenerateExtenderClassCode()
    {
        var codeGenerator = new BaseClassCodeGenerator(CodeGeneratorOptions);
        codeGenerator.AddClassNameToExtend("Article");
        codeGenerator.AddClassNameToExtend("Office");

        var executingPath = AppContext.BaseDirectory;
        var expectedExtenderCode = File.ReadAllText(executingPath + "/Assets/BaseClassExtender_CompiledCode.txt");

        var actualCompiledExtenderClass = codeGenerator.GenerateExtenderCode();

        Assert.Equal(expectedExtenderCode, actualCompiledExtenderClass, ignoreLineEndingDifferences: true, ignoreWhiteSpaceDifferences: true);
    }
}
