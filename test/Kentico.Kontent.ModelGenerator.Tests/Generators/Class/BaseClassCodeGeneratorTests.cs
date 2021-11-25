using System;
using System.IO;
using Kentico.Kontent.ModelGenerator.Core.Generators.Class;
using Xunit;

namespace Kentico.Kontent.ModelGenerator.Tests.Generators.Class
{
    public class BaseClassCodeGeneratorTests
    {
        private const string BaseClassName = "ContentBase";

        [Fact]
        public void GenerateBaseClassCodeWithDefaultNamespace()
        {
            var codeGenerator = new BaseClassCodeGenerator(BaseClassName);
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
            var codeGenerator = new BaseClassCodeGenerator(BaseClassName, customNamespace);
            codeGenerator.AddClassNameToExtend("Article");
            codeGenerator.AddClassNameToExtend("Office");

            var executingPath = AppContext.BaseDirectory;
            var expectedBaseClassCode = File.ReadAllText(executingPath + "/Assets/BaseClass_CompiledCode.txt");
            expectedBaseClassCode = expectedBaseClassCode.Replace(ClassCodeGenerator.DefaultNamespace, customNamespace);

            var actualCompiledBaseClass = codeGenerator.GenerateBaseClassCode();

            Assert.Equal(expectedBaseClassCode, actualCompiledBaseClass, ignoreLineEndingDifferences: true, ignoreWhiteSpaceDifferences: true);
        }

        [Fact]
        public void GenerateExtenderClassCode()
        {
            var codeGenerator = new BaseClassCodeGenerator(BaseClassName);
            codeGenerator.AddClassNameToExtend("Article");
            codeGenerator.AddClassNameToExtend("Office");

            var executingPath = AppContext.BaseDirectory;
            var expectedExtenderCode = File.ReadAllText(executingPath + "/Assets/BaseClassExtender_CompiledCode.txt");

            var actualCompiledExtenderClass = codeGenerator.GenereateExtenderCode();

            Assert.Equal(expectedExtenderCode, actualCompiledExtenderClass, ignoreLineEndingDifferences: true, ignoreWhiteSpaceDifferences: true);
        }
    }
}
