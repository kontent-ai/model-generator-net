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
    public class TypeProviderCodeGeneratorTests
    {
        [Fact]
        public void GenerateCodeTests()
        {
            var codeGenerator = new TypeProviderCodeGenerator();
            codeGenerator.AddContentType("article", "Article");
            codeGenerator.AddContentType("office", "Office");

            string compiledCode = codeGenerator.GenerateCode();

            string executingPath = AppContext.BaseDirectory;
            string expectedCode = File.ReadAllText(executingPath + "/Assets/CustomTypeProvider_CompiledCode.txt");

            // Ignore white space
            expectedCode = Regex.Replace(expectedCode, @"\s+", "");
            compiledCode = Regex.Replace(compiledCode, @"\s+", "");

            Assert.Equal(expectedCode, compiledCode);
        }
        
        [Fact]
        public void IntegrationTest_GeneratedCodeCompilesWithoutErrors()
        {
            var codeGenerator = new TypeProviderCodeGenerator();
            codeGenerator.AddContentType("article", "Article");
            codeGenerator.AddContentType("office", "Office");

            string compiledCode = codeGenerator.GenerateCode();

            // Dummy implementation of Article and Office class to make compilation work
            string dummyClasses = "public class Article {} public class Office {}";

            CSharpCompilation compilation = CSharpCompilation.Create(
                assemblyName: Path.GetRandomFileName(),
                syntaxTrees: new[] {
                    CSharpSyntaxTree.ParseText(compiledCode),
                    CSharpSyntaxTree.ParseText(dummyClasses)
                },
                references: new[] {
                    MetadataReference.CreateFromFile(Assembly.Load(new AssemblyName("mscorlib")).Location),
                    MetadataReference.CreateFromFile(Assembly.Load(new AssemblyName("System.Runtime")).Location),
                    MetadataReference.CreateFromFile(typeof(Object).GetTypeInfo().Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(KenticoCloud.Delivery.DeliveryClient).GetTypeInfo().Assembly.Location)
                },
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            using (var ms = new MemoryStream())
            {
                EmitResult result = compilation.Emit(ms);
                string compilationErrors = "Compilation errors:\n";

                if (!result.Success)
                {
                    IEnumerable<Diagnostic> failures = result.Diagnostics.Where(diagnostic =>
                        diagnostic.IsWarningAsError ||
                        diagnostic.Severity == DiagnosticSeverity.Error);

                    foreach (Diagnostic diagnostic in failures)
                    {
                        compilationErrors += String.Format("{0}: {1}\n", diagnostic.Id, diagnostic.GetMessage());
                    }
                }

                Assert.True(result.Success, compilationErrors);
            }
        }
    }
}
