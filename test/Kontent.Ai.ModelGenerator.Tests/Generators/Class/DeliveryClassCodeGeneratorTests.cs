using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Kontent.Ai.ModelGenerator.Core.Common;
using Kontent.Ai.ModelGenerator.Core.Generators.Class;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace Kontent.Ai.ModelGenerator.Tests.Generators.Class;

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
        classDefinition.AddProperty(Property.FromContentTypeElement("text", "text"));
        classDefinition.AddProperty(Property.FromContentTypeElement("rich_text", "rich_text"));
        classDefinition.AddProperty(Property.FromContentTypeElement("rich_text_structured", "rich_text(structured)"));
        classDefinition.AddProperty(Property.FromContentTypeElement("number", "number"));
        classDefinition.AddProperty(Property.FromContentTypeElement("multiple_choice", "multiple_choice"));
        classDefinition.AddProperty(Property.FromContentTypeElement("date_time", "date_time"));
        classDefinition.AddProperty(Property.FromContentTypeElement("asset", "asset"));
        classDefinition.AddProperty(Property.FromContentTypeElement("modular_content", "modular_content"));
        classDefinition.AddProperty(Property.FromContentTypeElement("taxonomy", "taxonomy"));
        classDefinition.AddProperty(Property.FromContentTypeElement("url_slug", "url_slug"));
        classDefinition.AddProperty(Property.FromContentTypeElement("custom", "custom"));

        classDefinition.AddSystemProperty();

        var classCodeGenerator = new DeliveryClassCodeGenerator(classDefinition, classDefinition.ClassName);

        var compiledCode = classCodeGenerator.GenerateCode();

        var executingPath = AppContext.BaseDirectory;
        var expectedCode = File.ReadAllText(executingPath + "/Assets/CompleteContentType_CompiledCode.txt");

        Assert.Equal(expectedCode, compiledCode, ignoreWhiteSpaceDifferences: true, ignoreLineEndingDifferences: true);
    }

    [Fact]
    public void IntegrationTest_GeneratedCodeCompilesWithoutErrors()
    {
        var definition = new ClassDefinition("Complete content type");
        definition.AddProperty(Property.FromContentTypeElement("text", "text"));
        definition.AddProperty(Property.FromContentTypeElement("rich_text", "rich_text"));
        definition.AddProperty(Property.FromContentTypeElement("rich_text_structured", "rich_text(structured)"));
        definition.AddProperty(Property.FromContentTypeElement("number", "number"));
        definition.AddProperty(Property.FromContentTypeElement("multiple_choice", "multiple_choice"));
        definition.AddProperty(Property.FromContentTypeElement("date_time", "date_time"));
        definition.AddProperty(Property.FromContentTypeElement("asset", "asset"));
        definition.AddProperty(Property.FromContentTypeElement("modular_content", "modular_content"));
        definition.AddProperty(Property.FromContentTypeElement("taxonomy", "taxonomy"));
        definition.AddProperty(Property.FromContentTypeElement("custom", "custom"));

        var classCodeGenerator = new DeliveryClassCodeGenerator(definition, definition.ClassName);
        var compiledCode = classCodeGenerator.GenerateCode();

        var compilation = CSharpCompilation.Create(
            assemblyName: Path.GetRandomFileName(),
            syntaxTrees: new[] { CSharpSyntaxTree.ParseText(compiledCode) },
            references: new[] {
                MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Delivery.Abstractions.IApiResponse).GetTypeInfo().Assembly.Location)
            },
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        using var ms = new MemoryStream();
        var result = compilation.Emit(ms);
        var compilationErrors = "Compilation errors:\n";

        if (!result.Success)
        {
            var failures = result.Diagnostics.Where(diagnostic =>
                diagnostic.IsWarningAsError ||
                diagnostic.Severity == DiagnosticSeverity.Error);

            foreach (var diagnostic in failures)
            {
                compilationErrors += $"{diagnostic.Id}: {diagnostic.GetMessage()}\n";
            }
        }

        Assert.True(result.Success, compilationErrors);
    }
}
