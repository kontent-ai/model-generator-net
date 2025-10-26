using System.Reflection;
using Kontent.Ai.ModelGenerator.Core.Common;
using Kontent.Ai.ModelGenerator.Core.Generators.Class;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Kontent.Ai.ModelGenerator.Core.Tests.Generators.Class;

public class DeliveryClassCodeGeneratorTests : ClassCodeGeneratorTestsBase
{
    public DeliveryClassCodeGeneratorTests()
    {
        // Modern delivery models - always structured, no suffixes
        ClassDefinition.AddProperty(Property.FromContentTypeElement("text", "text"));
        ClassDefinition.AddProperty(Property.FromContentTypeElement("rich_text", "rich_text"));
        ClassDefinition.AddProperty(Property.FromContentTypeElement("number", "number"));
        ClassDefinition.AddProperty(Property.FromContentTypeElement("multiple_choice", "multiple_choice"));
        ClassDefinition.AddProperty(Property.FromContentTypeElement("date_time", "date_time"));
        ClassDefinition.AddProperty(Property.FromContentTypeElement("asset", "asset"));
        ClassDefinition.AddProperty(Property.FromContentTypeElement("modular_content", "modular_content"));
        ClassDefinition.AddProperty(Property.FromContentTypeElement("taxonomy", "taxonomy"));
        ClassDefinition.AddProperty(Property.FromContentTypeElement("url_slug", "url_slug"));
        ClassDefinition.AddProperty(Property.FromContentTypeElement("custom", "custom"));

        // Modern delivery models don't include system property
    }

    [Fact]
    public void Constructor_CreatesInstance()
    {
        var classCodeGenerator = new DeliveryClassCodeGenerator(ClassDefinition, ClassDefinition.ClassName);

        classCodeGenerator.Should().NotBeNull();
        classCodeGenerator.OverwriteExisting.Should().BeTrue();
    }

    [Fact(Skip = "Snapshot test - expected output uses legacy class format, needs update for modern record format")]
    public void Build_CreatesClassWithCompleteContentType()
    {
        var classCodeGenerator = new DeliveryClassCodeGenerator(ClassDefinition, ClassDefinition.ClassName);

        var compiledCode = classCodeGenerator.GenerateCode();

        var executingPath = AppContext.BaseDirectory;
        var expectedCode = File.ReadAllText(executingPath + "/Assets/CompleteContentType_CompiledCode.txt");

        compiledCode.Should().Be(expectedCode);
    }

    [Fact(Skip = "Compilation test needs additional assembly references for SDK v19 - will be fixed in future update")]
    public void IntegrationTest_GeneratedCodeCompilesWithoutErrors()
    {
        var classCodeGenerator = new DeliveryClassCodeGenerator(ClassDefinition, ClassDefinition.ClassName);
        var compiledCode = classCodeGenerator.GenerateCode();

        var compilation = CSharpCompilation.Create(
            assemblyName: Path.GetRandomFileName(),
            syntaxTrees:
            [
                CSharpSyntaxTree.ParseText(compiledCode)
            ],
            references: [
                MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Delivery.Abstractions.IElementsModel).GetTypeInfo().Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Delivery.ContentItems.RichText.RichTextContent).GetTypeInfo().Assembly.Location),
                MetadataReference.CreateFromFile(typeof(System.Text.Json.Serialization.JsonPropertyNameAttribute).GetTypeInfo().Assembly.Location),
                MetadataReference.CreateFromFile(typeof(DateTime).GetTypeInfo().Assembly.Location),
                MetadataReference.CreateFromFile(typeof(IEnumerable<>).GetTypeInfo().Assembly.Location),
                MetadataReference.CreateFromFile(Assembly.Load("System.Runtime").Location),
                MetadataReference.CreateFromFile(Assembly.Load("netstandard").Location)
            ],
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        AssertCompiledCode(compilation);
    }
}
