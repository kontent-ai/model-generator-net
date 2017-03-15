using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using NUnit.Framework;

namespace CloudModelGenerator.Tests
{
    [TestFixture]
    public class ClassCodeGeneratorTests
    {
        [TestCase]
        public void Constructor_ThrowsAnExceptionForNullArgument()
        {
            Assert.Throws<ArgumentNullException>(() => new ClassCodeGenerator(null));
        }

        [TestCase]
        public void Constructor_ReplacesNullNamespaceWithDefault()
        {
            var classDefinition = new ClassDefinition("codename");
            var classCodeGenerator = new ClassCodeGenerator(classDefinition, null);

            Assert.AreEqual("KenticoCloudModels", classCodeGenerator.Namespace);
        }

        [TestCase]
        public void Build_CreatesClassWithCompleteContentType()
        {
            var classDefinition = new ClassDefinition("Complete content type");
            classDefinition.AddProperty(Property.FromContentType("text", "text"));
            classDefinition.AddProperty(Property.FromContentType("rich_text", "rich_text"));
            classDefinition.AddProperty(Property.FromContentType("number", "number"));
            classDefinition.AddProperty(Property.FromContentType("multiple_choice", "multiple_choice"));
            classDefinition.AddProperty(Property.FromContentType("date_time", "date_time"));
            classDefinition.AddProperty(Property.FromContentType("asset", "asset"));
            classDefinition.AddProperty(Property.FromContentType("modular_content", "modular_content"));
            classDefinition.AddProperty(Property.FromContentType("taxonomy", "taxonomy"));
            classDefinition.AddProperty(Property.FromContentType("url_slug", "url_slug"));

            classDefinition.AddSystemProperty();

            var classCodeGenerator = new ClassCodeGenerator(classDefinition);

            string compiledCode = classCodeGenerator.GenerateCode();

            string executingPath = AppContext.BaseDirectory;
            string expectedCode = File.ReadAllText(executingPath + "/Assets/CompleteContentType_CompiledCode.txt");

            Assert.AreEqual(expectedCode, compiledCode);
        }

        [TestCase]
        public void IntegrationTest_GeneratedCodeCompilesWithoutErrors()
        {
            var definition = new ClassDefinition("Complete content type");
            definition.AddProperty(Property.FromContentType("text", "text"));
            definition.AddProperty(Property.FromContentType("rich_text", "rich_text"));
            definition.AddProperty(Property.FromContentType("number", "number"));
            definition.AddProperty(Property.FromContentType("multiple_choice", "multiple_choice"));
            definition.AddProperty(Property.FromContentType("date_time", "date_time"));
            definition.AddProperty(Property.FromContentType("asset", "asset"));
            definition.AddProperty(Property.FromContentType("modular_content", "modular_content"));
            definition.AddProperty(Property.FromContentType("taxonomy", "taxonomy"));

            var classCodeGenerator = new ClassCodeGenerator(definition);
            string compiledCode = classCodeGenerator.GenerateCode();

            CSharpCompilation compilation = CSharpCompilation.Create(
                assemblyName: Path.GetRandomFileName(),
                syntaxTrees: new[] { CSharpSyntaxTree.ParseText(compiledCode) },
                references: new[] {
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

                Assert.IsTrue(result.Success, compilationErrors);
            }
        }
    }
}
