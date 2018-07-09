using Xunit;
using System;
using System.IO;
using System.Text.RegularExpressions;

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

            string actualCompiled_BaseClass = codeGenerator.GenerateBaseClassCode();
            string executingPath = AppContext.BaseDirectory;
            string expected_BaseClassCode = File.ReadAllText(executingPath + "/Assets/BaseClass_CompiledCode.txt");

            // Line endings can vary due to git, so strip them out
            expected_BaseClassCode = Regex.Replace(expected_BaseClassCode, @"\s+", "");
            actualCompiled_BaseClass = Regex.Replace(actualCompiled_BaseClass, @"\s+", "");

            // Test base class
            Assert.Equal(actualCompiled_BaseClass, expected_BaseClassCode, ignoreLineEndingDifferences:true, ignoreWhiteSpaceDifferences: true);
        }

        [Fact]
        public void GenerateBaseClassCodeWithCustomNamespace()
        {
            string customNamespace = "CustomNamespace";
            var codeGenerator = new BaseClassCodeGenerator(BASE_CLASS_NAME, customNamespace);
            codeGenerator.AddClassNameToExtend("Article");
            codeGenerator.AddClassNameToExtend("Office");

            string actualCompiled_BaseClass = codeGenerator.GenerateBaseClassCode();
            string executingPath = AppContext.BaseDirectory;
            string expected_BaseClassCode = File.ReadAllText(executingPath + "/Assets/BaseClass_CompiledCode.txt");

            // Line endings can vary due to git, so strip them out
            actualCompiled_BaseClass = Regex.Replace(actualCompiled_BaseClass, @"\s+", "");
            expected_BaseClassCode = Regex.Replace(expected_BaseClassCode, @"\s+", "");
            
            // Replace default namespace in expected result
            expected_BaseClassCode = expected_BaseClassCode.Replace(ClassCodeGenerator.DEFAULT_NAMESPACE, customNamespace);

            // Test base class
            Assert.Equal(actualCompiled_BaseClass, expected_BaseClassCode, ignoreLineEndingDifferences:true, ignoreWhiteSpaceDifferences: true);
        }

        [Fact]
        public void GenerateExtenderClassCode()
        {
            var codeGenerator = new BaseClassCodeGenerator(BASE_CLASS_NAME);
            codeGenerator.AddClassNameToExtend("Article");
            codeGenerator.AddClassNameToExtend("Office");

            string actualCompiled_ExtenderClass = codeGenerator.GenereateExtenderCode();
            string executingPath = AppContext.BaseDirectory;
            string expected_ExtenderCode = File.ReadAllText(executingPath + "/Assets/BaseClassExtender_CompiledCode.txt");

            // Line endings can vary due to git, so strip them out
            actualCompiled_ExtenderClass = Regex.Replace(actualCompiled_ExtenderClass, @"\s+", "");
            expected_ExtenderCode = Regex.Replace(expected_ExtenderCode, @"\s+", "");

            // Test extender class
            Assert.Equal(actualCompiled_ExtenderClass, expected_ExtenderCode, ignoreLineEndingDifferences: true, ignoreWhiteSpaceDifferences: true);
        }
    }
}
