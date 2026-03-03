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
        AddElement("text", "text");
        AddElement("rich_text", "rich_text");
        AddElement("number", "number");
        AddElement("multiple_choice", "multiple_choice");
        AddElement("date_time", "date_time");
        AddElement("asset", "asset");
        AddElement("modular_content", "modular_content");
        AddElement("taxonomy", "taxonomy");
        AddElement("url_slug", "url_slug");
        AddElement("custom", "custom");
    }

    private void AddElement(string codename, string elementType)
    {
        var property = Property.FromContentTypeElement(codename, elementType);
        ClassDefinition.AddPropertyCodenameConstant(property.Codename);
        ClassDefinition.AddProperty(property);
    }

    [Fact]
    public void Constructor_CreatesInstance()
    {
        var classCodeGenerator = new DeliveryClassCodeGenerator(ClassDefinition, ClassDefinition.ClassName);

        classCodeGenerator.Should().NotBeNull();
        classCodeGenerator.OverwriteExisting.Should().BeTrue();
    }

    [Fact]
    public void Build_CreatesClassWithCompleteContentType()
    {
        var classCodeGenerator = new DeliveryClassCodeGenerator(ClassDefinition, ClassDefinition.ClassName);

        var compiledCode = classCodeGenerator.GenerateCode();

        var executingPath = AppContext.BaseDirectory;
        var expectedCode = File.ReadAllText(executingPath + "/Assets/CompleteContentType_CompiledCode.txt");

        compiledCode.Trim().Should().Be(expectedCode.Trim());
    }

    [Fact]
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
                MetadataReference.CreateFromFile(typeof(Delivery.Abstractions.IEmbeddedContent).GetTypeInfo().Assembly.Location),
                MetadataReference.CreateFromFile(Assembly.Load("Kontent.Ai.Delivery").Location),
                MetadataReference.CreateFromFile(typeof(Delivery.Attributes.ContentTypeCodenameAttribute).GetTypeInfo().Assembly.Location),
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
