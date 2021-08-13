using Xunit;
using System;
using System.IO;
using Kentico.Kontent.ModelGenerator.Core;

namespace Kentico.Kontent.ModelGenerator.Tests
{
    public class GeneralClassCodeGeneratorTests
    {
        private const string BASE_CLASS_NAME = "ContentBase";

        [Fact]
        public void GenerateBaseClassCodeWithDefaultNamespace()
        {
            var codeGenerator = new GeneralClassCodeGenerator(BASE_CLASS_NAME);
            codeGenerator.AddClassNameToExtend("Article");
            codeGenerator.AddClassNameToExtend("Office");

            var executingPath = AppContext.BaseDirectory;
            var expectedBaseClassCode = File.ReadAllText(executingPath + "/Assets/BaseClass_CompiledCode.txt");

            var actualCompiledBaseClass = codeGenerator.GenerateBaseClassCode();

            Assert.Equal(expectedBaseClassCode, actualCompiledBaseClass, ignoreLineEndingDifferences: true, ignoreWhiteSpaceDifferences: true);
        }

        [Fact]
        public void GenerateBaseClassCodeWithCustomNamespace()
        {
            var customNamespace = "CustomNamespace";
            var codeGenerator = new GeneralClassCodeGenerator(BASE_CLASS_NAME, customNamespace);
            codeGenerator.AddClassNameToExtend("Article");
            codeGenerator.AddClassNameToExtend("Office");

            var executingPath = AppContext.BaseDirectory;
            var expectedBaseClassCode = File.ReadAllText(executingPath + "/Assets/BaseClass_CompiledCode.txt");
            expectedBaseClassCode = expectedBaseClassCode.Replace(ClassCodeGeneratorBase.DEFAULT_NAMESPACE, customNamespace);

            var actualCompiledBaseClass = codeGenerator.GenerateBaseClassCode();

            Assert.Equal(expectedBaseClassCode, actualCompiledBaseClass, ignoreLineEndingDifferences: true, ignoreWhiteSpaceDifferences: true);
        }

        [Fact]
        public void GenerateExtenderClassCode()
        {
            var codeGenerator = new GeneralClassCodeGenerator(BASE_CLASS_NAME);
            codeGenerator.AddClassNameToExtend("Article");
            codeGenerator.AddClassNameToExtend("Office");

            var executingPath = AppContext.BaseDirectory;
            var expectedExtenderCode = File.ReadAllText(executingPath + "/Assets/BaseClassExtender_CompiledCode.txt");

            var actualCompiledExtenderClass = codeGenerator.GenereateExtenderCode();

            Assert.Equal(expectedExtenderCode, actualCompiledExtenderClass, ignoreLineEndingDifferences: true, ignoreWhiteSpaceDifferences: true);
        }
    }
}
