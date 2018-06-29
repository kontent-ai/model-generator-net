using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Xunit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace CloudModelGenerator.Tests
{
    public class BaseClassCodeGeneratorTests
    {
        private const string _baseClassName = "ContentBase";
        [Fact]
        public void BaseClassCodeTests()
        {
            var codeGenerator = new BaseClassCodeGenerator(_baseClassName);
            codeGenerator.AddClassNameToExtend("Article");
            codeGenerator.AddClassNameToExtend("Office");

            string actualCompiled_BaseClass = codeGenerator.GenerateBaseClassCode();
            string actualCompiled_ExtenderClass = codeGenerator.GenereateExtenderCode();

            string executingPath = AppContext.BaseDirectory;

            string expected_BaseClassCode = File.ReadAllText(executingPath + "/Assets/BaseClass_CompiledCode.txt");
            string expected_ExtenderCode = File.ReadAllText(executingPath + "/Assets/BaseClassExtender_CompiledCode.txt");

            // Ignore white space
            actualCompiled_BaseClass = Regex.Replace(actualCompiled_BaseClass, @"\s+", "");
            expected_BaseClassCode = Regex.Replace(expected_BaseClassCode, @"\s+", "");

            // Test base class
            Assert.Equal(actualCompiled_BaseClass, expected_BaseClassCode);


            actualCompiled_ExtenderClass = Regex.Replace(actualCompiled_ExtenderClass, @"\s+", "");
            expected_ExtenderCode = Regex.Replace(expected_ExtenderCode, @"\s+", "");

            // Test extender class
            Assert.Equal(actualCompiled_ExtenderClass, expected_ExtenderCode);
        }
        
      
    }
}
