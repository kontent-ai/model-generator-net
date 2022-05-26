using System;
using System.IO;
using System.Reflection;
using Kentico.Kontent.ModelGenerator.Core.Generators.Class;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace Kentico.Kontent.ModelGenerator.Tests.Generators.Class
{
    public class ContentItemClassCodeGeneratorTests : ClassCodeGeneratorTestsBase
    {
        [Fact]
        public void Constructor_CreatesInstance()
        {
            var classCodeGenerator = new ContentItemClassCodeGenerator();

            Assert.NotNull(classCodeGenerator);
            Assert.True(classCodeGenerator.OverwriteExisting);
        }

        [Fact]
        public void Build_CreatesClassWithSystemProperty()
        {
            var classCodeGenerator = new ContentItemClassCodeGenerator();

            var compiledCode = classCodeGenerator.GenerateCode();

            var executingPath = AppContext.BaseDirectory;
            var expectedCode = File.ReadAllText(executingPath + "/Assets/DefaultContentItem_CompiledCode.txt");

            Assert.Equal(expectedCode, compiledCode, ignoreWhiteSpaceDifferences: true, ignoreLineEndingDifferences: true);
        }

        [Fact]
        public void IntegrationTest_GeneratedCodeCompilesWithoutErrors()
        {
            var classCodeGenerator = new ContentItemClassCodeGenerator();

            var compiledCode = classCodeGenerator.GenerateCode();

            var compilation = CSharpCompilation.Create(
                assemblyName: Path.GetRandomFileName(),
                syntaxTrees: new[] { CSharpSyntaxTree.ParseText(compiledCode) },
                references: new[] {
                    MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(Delivery.Abstractions.IApiResponse).GetTypeInfo().Assembly.Location)
                },
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            AssertCompiledCode(compilation);
        }
    }
}
