﻿using System;
using System.IO;
using System.Reflection;
using Kentico.Kontent.ModelGenerator.Core.Common;
using Kentico.Kontent.ModelGenerator.Core.Generators.Class;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace Kentico.Kontent.ModelGenerator.Tests.Generators.Class
{
    public class DeliveryClassCodeGeneratorTests : TestBaseClassCodeGenerator
    {
        public DeliveryClassCodeGeneratorTests()
        {
            ClassDefinition.AddProperty(Property.FromContentTypeElement("text", "text"));
            ClassDefinition.AddProperty(Property.FromContentTypeElement("rich_text", "rich_text"));
            ClassDefinition.AddProperty(Property.FromContentTypeElement("rich_text_structured", "rich_text(structured)"));
            ClassDefinition.AddProperty(Property.FromContentTypeElement("number", "number"));
            ClassDefinition.AddProperty(Property.FromContentTypeElement("multiple_choice", "multiple_choice"));
            ClassDefinition.AddProperty(Property.FromContentTypeElement("date_time", "date_time"));
            ClassDefinition.AddProperty(Property.FromContentTypeElement("asset", "asset"));
            ClassDefinition.AddProperty(Property.FromContentTypeElement("modular_content", "modular_content"));
            ClassDefinition.AddProperty(Property.FromContentTypeElement("taxonomy", "taxonomy"));
            ClassDefinition.AddProperty(Property.FromContentTypeElement("url_slug", "url_slug"));
            ClassDefinition.AddProperty(Property.FromContentTypeElement("custom", "custom"));

            ClassDefinition.TryAddSystemProperty();
        }

        [Fact]
        public void Constructor_CreatesInstance()
        {
            var classCodeGenerator = new DeliveryClassCodeGenerator(ClassDefinition, ClassDefinition.ClassName);

            Assert.NotNull(classCodeGenerator);
            Assert.True(classCodeGenerator.OverwriteExisting);
        }

        [Fact]
        public void Build_CreatesClassWithCompleteContentType()
        {
            var classCodeGenerator = new DeliveryClassCodeGenerator(ClassDefinition, ClassDefinition.ClassName);

            var compiledCode = classCodeGenerator.GenerateCode();

            var executingPath = AppContext.BaseDirectory;
            var expectedCode = File.ReadAllText(executingPath + "/Assets/CompleteContentType_CompiledCode.txt");

            Assert.Equal(expectedCode, compiledCode, ignoreWhiteSpaceDifferences: true, ignoreLineEndingDifferences: true);
        }

        [Fact]
        public void IntegrationTest_GeneratedCodeCompilesWithoutErrors()
        {
            var classCodeGenerator = new DeliveryClassCodeGenerator(ClassDefinition, ClassDefinition.ClassName);
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
