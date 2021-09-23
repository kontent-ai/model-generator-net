using System;
using System.IO;
using Kentico.Kontent.ModelGenerator.Core;
using Xunit;

namespace Kentico.Kontent.ModelGenerator.Tests
{
    public class PartialClassCodeGeneratorTests
    {
        [Fact]
        public void Build_CreatesCustomPartialContentType()
        {
            var classDefinition = new ClassDefinition("Complete content type");

            var classCodeGenerator = new PartialClassCodeGenerator(classDefinition, classDefinition.ClassName);

            var compiledCode = classCodeGenerator.GenerateCode(false);

            var executingPath = AppContext.BaseDirectory;
            var expectedCode = File.ReadAllText(executingPath + "/Assets/CompleteContentType_CompiledCode_CustomPartial.txt");

            Assert.Equal(expectedCode, compiledCode, ignoreWhiteSpaceDifferences: true, ignoreLineEndingDifferences: true);
        }
    }
}
