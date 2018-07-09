using Xunit;
using System;
using System.IO;

namespace CloudModelGenerator.Tests
{
    public class BaseClassCodeGeneratorTests
    {
        private const string BASE_CLASS_NAME = "ContentBase";

        [Fact]
        public void GenerateBaseClassCodeWithDefaultNamespace()
        {
            var codeGenerator = new BaseClassCodeGenerator(BASE_CLASS_NAME);
            codeGenerator.AddClassNameToExtend("Article");
            codeGenerator.AddClassNameToExtend("Office");

            var executingPath = AppContext.BaseDirectory;
            var expected_BaseClassCode = File.ReadAllText(executingPath + "/Assets/BaseClass_CompiledCode.txt");

            var actualCompiled_BaseClass = codeGenerator.GenerateBaseClassCode();

            Assert.Equal(expected_BaseClassCode, actualCompiled_BaseClass, ignoreLineEndingDifferences:true, ignoreWhiteSpaceDifferences: true);
        }

        [Fact]
        public void GenerateBaseClassCodeWithCustomNamespace()
        {
            var customNamespace = "CustomNamespace";
            var codeGenerator = new BaseClassCodeGenerator(BASE_CLASS_NAME, customNamespace);
            codeGenerator.AddClassNameToExtend("Article");
            codeGenerator.AddClassNameToExtend("Office");

            var executingPath = AppContext.BaseDirectory;
            var expected_BaseClassCode = File.ReadAllText(executingPath + "/Assets/BaseClass_CompiledCode.txt");
            expected_BaseClassCode = expected_BaseClassCode.Replace(ClassCodeGenerator.DEFAULT_NAMESPACE, customNamespace);

            var actualCompiled_BaseClass = codeGenerator.GenerateBaseClassCode();            

            Assert.Equal(expected_BaseClassCode, actualCompiled_BaseClass, ignoreLineEndingDifferences:true, ignoreWhiteSpaceDifferences: true);
        }

        [Fact]
        public void GenerateExtenderClassCode()
        {
            var codeGenerator = new BaseClassCodeGenerator(BASE_CLASS_NAME);
            codeGenerator.AddClassNameToExtend("Article");
            codeGenerator.AddClassNameToExtend("Office");

            var executingPath = AppContext.BaseDirectory;
            var expected_ExtenderCode = File.ReadAllText(executingPath + "/Assets/BaseClassExtender_CompiledCode.txt");

            var actualCompiled_ExtenderClass = codeGenerator.GenereateExtenderCode();

            Assert.Equal(expected_ExtenderCode, actualCompiled_ExtenderClass, ignoreLineEndingDifferences: true, ignoreWhiteSpaceDifferences: true);
        }
    }
}
