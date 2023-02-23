using System;
using System.IO;
using FluentAssertions;
using Kontent.Ai.ModelGenerator.Core.Common;
using Kontent.Ai.ModelGenerator.Core.Generators.Class;
using Xunit;

namespace Kontent.Ai.ModelGenerator.Tests.Generators.Class;

public class PartialClassCodeGeneratorTests
{
    [Fact]
    public void Constructor_CreatesInstance()
    {
        var classDefinition = new ClassDefinition("Complete content type");

        var classCodeGenerator = new PartialClassCodeGenerator(classDefinition, classDefinition.ClassName);

        classCodeGenerator.Should().NotBeNull();
        classCodeGenerator.OverwriteExisting.Should().BeFalse();
    }

    [Fact]
    public void Build_CreatesCustomPartialContentType()
    {
        var classDefinition = new ClassDefinition("Complete content type");

        var classCodeGenerator = new PartialClassCodeGenerator(classDefinition, classDefinition.ClassName);

        var compiledCode = classCodeGenerator.GenerateCode();

        var executingPath = AppContext.BaseDirectory;
        var expectedCode = File.ReadAllText(executingPath + "/Assets/CompleteContentType_CompiledCode_CustomPartial.txt");

        compiledCode.Should().Be(expectedCode);
    }
}
