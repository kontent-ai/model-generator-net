﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Kentico.Kontent.ModelGenerator.Core.Common;
using Kentico.Kontent.ModelGenerator.Core.Generators.Class;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Xunit;

namespace Kentico.Kontent.ModelGenerator.Tests.Generators.Class
{
    public class DeliveryClassCodeGeneratorTests
    {
        [Fact]
        public void Constructor_CreatesInstance()
        {
            var classDefinition = new ClassDefinition("Complete content type");

            var classCodeGenerator = new DeliveryClassCodeGenerator(classDefinition, classDefinition.ClassName);

            Assert.NotNull(classCodeGenerator);
            Assert.True(classCodeGenerator.OverwriteExisting);
        }

        [Fact]
        public void Build_CreatesClassWithCompleteContentType()
        {
            var classDefinition = new ClassDefinition("Complete content type");
            classDefinition.AddProperty(Property.FromContentType("text", "text"));
            classDefinition.AddProperty(Property.FromContentType("rich_text", "rich_text"));
            classDefinition.AddProperty(Property.FromContentType("rich_text_structured", "rich_text(structured)"));
            classDefinition.AddProperty(Property.FromContentType("number", "number"));
            classDefinition.AddProperty(Property.FromContentType("multiple_choice", "multiple_choice"));
            classDefinition.AddProperty(Property.FromContentType("date_time", "date_time"));
            classDefinition.AddProperty(Property.FromContentType("asset", "asset"));
            classDefinition.AddProperty(Property.FromContentType("modular_content", "modular_content"));
            classDefinition.AddProperty(Property.FromContentType("taxonomy", "taxonomy"));
            classDefinition.AddProperty(Property.FromContentType("url_slug", "url_slug"));
            classDefinition.AddProperty(Property.FromContentType("custom", "custom"));

            classDefinition.AddSystemProperty();

            var classCodeGenerator = new DeliveryClassCodeGenerator(classDefinition, classDefinition.ClassName);

            string compiledCode = classCodeGenerator.GenerateCode();

            string executingPath = AppContext.BaseDirectory;
            string expectedCode = File.ReadAllText(executingPath + "/Assets/CompleteContentType_CompiledCode.txt");

            Assert.Equal(expectedCode, compiledCode, ignoreWhiteSpaceDifferences: true, ignoreLineEndingDifferences: true);
        }

        [Fact]
        public void IntegrationTest_GeneratedCodeCompilesWithoutErrors()
        {
            var definition = new ClassDefinition("Complete content type");
            definition.AddProperty(Property.FromContentType("text", "text"));
            definition.AddProperty(Property.FromContentType("rich_text", "rich_text"));
            definition.AddProperty(Property.FromContentType("rich_text_structured", "rich_text(structured)"));
            definition.AddProperty(Property.FromContentType("number", "number"));
            definition.AddProperty(Property.FromContentType("multiple_choice", "multiple_choice"));
            definition.AddProperty(Property.FromContentType("date_time", "date_time"));
            definition.AddProperty(Property.FromContentType("asset", "asset"));
            definition.AddProperty(Property.FromContentType("modular_content", "modular_content"));
            definition.AddProperty(Property.FromContentType("taxonomy", "taxonomy"));
            definition.AddProperty(Property.FromContentType("custom", "custom"));

            var classCodeGenerator = new DeliveryClassCodeGenerator(definition, definition.ClassName);
            string compiledCode = classCodeGenerator.GenerateCode();

            CSharpCompilation compilation = CSharpCompilation.Create(
                assemblyName: Path.GetRandomFileName(),
                syntaxTrees: new[] { CSharpSyntaxTree.ParseText(compiledCode) },
                references: new[] {
                    MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(Delivery.Abstractions.IApiResponse).GetTypeInfo().Assembly.Location)
                },
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            using var ms = new MemoryStream();
            EmitResult result = compilation.Emit(ms);
            string compilationErrors = "Compilation errors:\n";

            if (!result.Success)
            {
                IEnumerable<Diagnostic> failures = result.Diagnostics.Where(diagnostic =>
                    diagnostic.IsWarningAsError ||
                    diagnostic.Severity == DiagnosticSeverity.Error);

                foreach (Diagnostic diagnostic in failures)
                {
                    compilationErrors += $"{diagnostic.Id}: {diagnostic.GetMessage()}\n";
                }
            }

            Assert.True(result.Success, compilationErrors);
        }
    }
}
