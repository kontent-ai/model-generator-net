using System.Reflection;
using Kontent.Ai.ModelGenerator.Core.Common;
using Kontent.Ai.ModelGenerator.Core.Configuration;
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
    public void Build_EmitsNullableEnableDirective()
    {
        var classCodeGenerator = new DeliveryClassCodeGenerator(ClassDefinition, ClassDefinition.ClassName);

        var compiledCode = classCodeGenerator.GenerateCode();

        compiledCode.Should().Contain("#nullable enable");
    }

    [Fact]
    public void Build_SemanticNullability_CreatesClassWithCompleteContentType()
    {
        var classDefinition = new ClassDefinition("complete_content_type");
        foreach (var (codename, type) in new[]
        {
            ("text","text"), ("rich_text","rich_text"), ("number","number"),
            ("multiple_choice","multiple_choice"), ("date_time","date_time"),
            ("asset","asset"), ("modular_content","modular_content"),
            ("taxonomy","taxonomy"), ("url_slug","url_slug"), ("custom","custom")
        })
        {
            var property = Property.FromContentTypeElement(codename, type, NullabilityMode.Semantic);
            classDefinition.AddPropertyCodenameConstant(property.Codename);
            classDefinition.AddProperty(property);
        }

        var classCodeGenerator = new DeliveryClassCodeGenerator(classDefinition, classDefinition.ClassName);
        var compiledCode = classCodeGenerator.GenerateCode();

        var executingPath = AppContext.BaseDirectory;
        var expectedCode = File.ReadAllText(executingPath + "/Assets/CompleteContentType_CompiledCode_Semantic.txt");

        compiledCode.Trim().Should().Be(expectedCode.Trim());

        AssertCompiledCode(CreateCompilation(compiledCode));
    }

    [Fact]
    public void IntegrationTest_GeneratedCodeCompilesWithoutErrors()
    {
        var classCodeGenerator = new DeliveryClassCodeGenerator(ClassDefinition, ClassDefinition.ClassName);
        var compiledCode = classCodeGenerator.GenerateCode();

        AssertCompiledCode(CreateCompilation(compiledCode));
    }

    [Fact]
    public void Build_ContainsContentTypeCodenameConstant()
    {
        var classCodeGenerator = new DeliveryClassCodeGenerator(ClassDefinition, ClassDefinition.ClassName);
        var compiledCode = classCodeGenerator.GenerateCode();

        compiledCode.Should().Contain("public const string ContentTypeCodename = \"complete_content_type\";");
    }

    [Fact]
    public void Build_ElementPropertyCollision_RenamesElementProperty()
    {
        var classDefinition = new ClassDefinition("test_type");
        var property = Property.FromContentTypeElement("content_type_codename", "text");
        classDefinition.AddPropertyCodenameConstant(property.Codename);
        classDefinition.AddProperty(property);

        var classCodeGenerator = new DeliveryClassCodeGenerator(classDefinition, classDefinition.ClassName);
        var compiledCode = classCodeGenerator.GenerateCode();

        compiledCode.Should().Contain("public const string ContentTypeCodename = \"test_type\";");
        compiledCode.Should().Contain("public string? _ContentTypeCodename { get; init; }");

        AssertCompiledCode(CreateCompilation(compiledCode));
    }

    [Fact]
    public void Build_CodenameConstantCollision_RenamesConstant()
    {
        var classDefinition = new ClassDefinition("test_type");
        var property = Property.FromContentTypeElement("content_type", "text");
        classDefinition.AddPropertyCodenameConstant(property.Codename);
        classDefinition.AddProperty(property);

        var classCodeGenerator = new DeliveryClassCodeGenerator(classDefinition, classDefinition.ClassName);
        var compiledCode = classCodeGenerator.GenerateCode();

        compiledCode.Should().Contain("public const string ContentTypeCodename = \"test_type\";");
        compiledCode.Should().Contain("public const string _ContentTypeCodename = \"content_type\";");
        compiledCode.Should().Contain("public string? ContentType { get; init; }");

        AssertCompiledCode(CreateCompilation(compiledCode));
    }

    private static CSharpCompilation CreateCompilation(string code) =>
        CSharpCompilation.Create(
            assemblyName: Path.GetRandomFileName(),
            syntaxTrees:
            [
                CSharpSyntaxTree.ParseText(code)
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
}
