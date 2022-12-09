using System;
using System.IO;
using Kontent.Ai.ModelGenerator.Core.Generators.Class;
using Xunit;

namespace Kontent.Ai.ModelGenerator.Tests.Generators.Class;

public class PartialClassCodeGeneratorTests : ClassCodeGeneratorTestsBase
{
    [Fact]
    public void Constructor_CreatesInstance()
    {
        var classCodeGenerator = new PartialClassCodeGenerator(ClassDefinition, ClassDefinition.ClassName);

        Assert.NotNull(classCodeGenerator);
        Assert.False(classCodeGenerator.OverwriteExisting);
    }

    [Fact]
    public void Build_CreatesCustomPartialContentType()
    {
        var classCodeGenerator = new PartialClassCodeGenerator(ClassDefinition, ClassDefinition.ClassName);

        var compiledCode = classCodeGenerator.GenerateCode();

        var executingPath = AppContext.BaseDirectory;
        var expectedCode = File.ReadAllText(executingPath + "/Assets/CompleteContentType_CompiledCode_CustomPartial.txt");

        Assert.Equal(expectedCode, compiledCode, ignoreWhiteSpaceDifferences: true, ignoreLineEndingDifferences: true);
    }
}
