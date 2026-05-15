using System.Reflection;
using Kontent.Ai.ModelGenerator.Core.Common;
using Kontent.Ai.ModelGenerator.Core.Generators.Class;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Kontent.Ai.ModelGenerator.Core.Tests.Generators.Class;

public class ManagementClassCodeGeneratorTests
{
    [Fact]
    public void Constructor_NullClassDefinition_Throws()
    {
        var call = () => new ManagementClassCodeGenerator(classDefinition: null, classFilename: "x");

        call.Should().Throw<ArgumentNullException>().WithParameterName("classDefinition");
    }

    [Fact]
    public void Build_EmptyType_EmitsSealedPartialRecordImplementingIContentItem()
    {
        var classDefinition = new ClassDefinition("article");
        var sut = new ManagementClassCodeGenerator(classDefinition, classDefinition.ClassName);

        var code = sut.GenerateCode();

        code.Should().Contain("[KontentContentType(Codename = \"article\")]");
        code.Should().MatchRegex(@"public\s+sealed\s+partial\s+record\s+Article\s*:\s*IContentItem");
    }

    [Fact]
    public void Build_EmitsExpectedUsings()
    {
        var classDefinition = new ClassDefinition("article");
        var sut = new ManagementClassCodeGenerator(classDefinition, classDefinition.ClassName);

        var code = sut.GenerateCode();

        code.Should().Contain("using System;");
        code.Should().Contain("using System.ComponentModel.DataAnnotations;");
        code.Should().Contain("using Kontent.Ai.Management.Annotations;");
        code.Should().Contain("using Kontent.Ai.Management.Models;");
    }

    [Fact]
    public void Build_TextPropertyWithStringLength_EmitsExpectedAttributesAndType()
    {
        var classDefinition = new ClassDefinition("article");
        var property = new ManagementProperty(
            codename: "title",
            typeName: "string?",
            id: "11111111-2222-3333-4444-555555555555",
            attributes:
            [
                new AttributeSpec("KontentElement",
                [
                    AttributeArg.Named("Codename", "title"),
                    AttributeArg.Named("Id", "11111111-2222-3333-4444-555555555555"),
                ]),
                new AttributeSpec("StringLength", [AttributeArg.Positional(100)]),
            ]);
        classDefinition.AddProperty(property);

        var code = new ManagementClassCodeGenerator(classDefinition, classDefinition.ClassName).GenerateCode();

        code.Should().Contain(
            "[KontentElement(Codename = \"title\", Id = \"11111111-2222-3333-4444-555555555555\")]");
        code.Should().Contain("[StringLength(100)]");
        code.Should().Contain("public string? Title { get; init; }");
    }

    [Fact]
    public void Build_MultipleProperties_EmittedInIdentifierOrder()
    {
        var classDefinition = new ClassDefinition("article");
        classDefinition.AddProperty(new ManagementProperty("zeta", "string?", "id-z",
            [new AttributeSpec("KontentElement", [AttributeArg.Named("Codename", "zeta"), AttributeArg.Named("Id", "id-z")])]));
        classDefinition.AddProperty(new ManagementProperty("alpha", "string?", "id-a",
            [new AttributeSpec("KontentElement", [AttributeArg.Named("Codename", "alpha"), AttributeArg.Named("Id", "id-a")])]));

        var code = new ManagementClassCodeGenerator(classDefinition, classDefinition.ClassName).GenerateCode();

        var alphaIndex = code.IndexOf("public string? Alpha");
        var zetaIndex = code.IndexOf("public string? Zeta");
        alphaIndex.Should().BeGreaterThan(0);
        zetaIndex.Should().BeGreaterThan(alphaIndex);
    }

    [Fact]
    public void Build_NumberAndDateTimeProperties_EmitsExpectedTypes()
    {
        var classDefinition = new ClassDefinition("article");
        classDefinition.AddProperty(new ManagementProperty("priority", "decimal?", "id-p",
            [new AttributeSpec("KontentElement", [AttributeArg.Named("Codename", "priority"), AttributeArg.Named("Id", "id-p")])]));
        classDefinition.AddProperty(new ManagementProperty("published_at", "DateTimeOffset?", "id-d",
            [new AttributeSpec("KontentElement", [AttributeArg.Named("Codename", "published_at"), AttributeArg.Named("Id", "id-d")])]));

        var code = new ManagementClassCodeGenerator(classDefinition, classDefinition.ClassName).GenerateCode();

        code.Should().Contain("public decimal? Priority { get; init; }");
        code.Should().Contain("public DateTimeOffset? PublishedAt { get; init; }");
    }

    [Fact]
    public void Build_PlainPropertyWithoutAttributes_NoAttributesEmitted()
    {
        // A plain Property (not a ManagementProperty) should produce no attribute lists,
        // verifying the BuildPropertyAttributes guard rejects non-management properties.
        var classDefinition = new ClassDefinition("article");
        classDefinition.AddProperty(new Property("title", "string?"));

        var code = new ManagementClassCodeGenerator(classDefinition, classDefinition.ClassName).GenerateCode();

        code.Should().Contain("public string? Title");
        code.Should().NotContain("[KontentElement");
        code.Should().NotContain("[JsonPropertyName");
    }

    [Fact]
    public void Build_WithEnum_EmitsEnumAsSiblingType()
    {
        var classDefinition = new ClassDefinition("article");
        classDefinition.AddProperty(new ManagementProperty("category", "IReadOnlyList<ArticleCategory>?", "mc-id",
        [
            new AttributeSpec("KontentElement",
            [
                AttributeArg.Named("Codename", "category"),
                AttributeArg.Named("Id", "mc-id"),
            ]),
            new AttributeSpec("MaxElements", [AttributeArg.Positional(1)]),
        ]));
        classDefinition.AddEnum(new EnumDefinition("ArticleCategory",
        [
            new EnumMember("News",
            [
                new AttributeSpec("KontentEnumValue",
                [
                    AttributeArg.Named("Codename", "news"),
                    AttributeArg.Named("Id", "opt-1"),
                ]),
            ]),
            new EnumMember("Release",
            [
                new AttributeSpec("KontentEnumValue",
                [
                    AttributeArg.Named("Codename", "release"),
                    AttributeArg.Named("Id", "opt-2"),
                ]),
            ]),
        ]));

        var code = new ManagementClassCodeGenerator(classDefinition, classDefinition.ClassName).GenerateCode();

        code.Should().Contain("public IReadOnlyList<ArticleCategory>? Category { get; init; }");
        code.Should().Contain("[MaxElements(1)]");
        code.Should().Contain("public enum ArticleCategory");
        code.Should().Contain("[KontentEnumValue(Codename = \"news\", Id = \"opt-1\")]");
        code.Should().Contain("News");
        code.Should().Contain("[KontentEnumValue(Codename = \"release\", Id = \"opt-2\")]");
        code.Should().Contain("Release");
        // Enum should be a sibling, not nested — appears outside the record's braces
        var recordEndIndex = code.IndexOf("public sealed partial record Article");
        var enumStartIndex = code.IndexOf("public enum ArticleCategory");
        enumStartIndex.Should().BeGreaterThan(recordEndIndex);
    }

    [Fact]
    public void Build_NoEnums_NoEnumDeclarations()
    {
        var classDefinition = new ClassDefinition("article");
        classDefinition.AddProperty(new ManagementProperty("title", "string?", "t-id",
        [
            new AttributeSpec("KontentElement",
            [
                AttributeArg.Named("Codename", "title"),
                AttributeArg.Named("Id", "t-id"),
            ]),
        ]));

        var code = new ManagementClassCodeGenerator(classDefinition, classDefinition.ClassName).GenerateCode();

        code.Should().NotContain("public enum");
    }

    [Fact]
    public void IntegrationTest_EmittedCodeCompilesAgainstSdkStubs()
    {
        var classDefinition = new ClassDefinition("article");

        classDefinition.AddProperty(new ManagementProperty("title", "string?", "11111111-1111-1111-1111-111111111111",
        [
            new AttributeSpec("KontentElement",
            [
                AttributeArg.Named("Codename", "title"),
                AttributeArg.Named("Id", "11111111-1111-1111-1111-111111111111"),
            ]),
            new AttributeSpec("StringLength", [AttributeArg.Positional(100)]),
        ]));
        classDefinition.AddProperty(new ManagementProperty("priority", "decimal?", "22222222-2222-2222-2222-222222222222",
        [
            new AttributeSpec("KontentElement",
            [
                AttributeArg.Named("Codename", "priority"),
                AttributeArg.Named("Id", "22222222-2222-2222-2222-222222222222"),
            ]),
        ]));
        classDefinition.AddProperty(new ManagementProperty("published_at", "DateTimeOffset?", "33333333-3333-3333-3333-333333333333",
        [
            new AttributeSpec("KontentElement",
            [
                AttributeArg.Named("Codename", "published_at"),
                AttributeArg.Named("Id", "33333333-3333-3333-3333-333333333333"),
            ]),
        ]));
        classDefinition.AddProperty(new ManagementProperty("category", "IReadOnlyList<ArticleCategory>?", "44444444-4444-4444-4444-444444444444",
        [
            new AttributeSpec("KontentElement",
            [
                AttributeArg.Named("Codename", "category"),
                AttributeArg.Named("Id", "44444444-4444-4444-4444-444444444444"),
            ]),
            new AttributeSpec("MaxElements", [AttributeArg.Positional(1)]),
        ]));
        classDefinition.AddEnum(new EnumDefinition("ArticleCategory",
        [
            new EnumMember("News",
            [
                new AttributeSpec("KontentEnumValue",
                [
                    AttributeArg.Named("Codename", "news"),
                    AttributeArg.Named("Id", "opt-1"),
                ]),
            ]),
            new EnumMember("Release",
            [
                new AttributeSpec("KontentEnumValue",
                [
                    AttributeArg.Named("Codename", "release"),
                    AttributeArg.Named("Id", "opt-2"),
                ]),
            ]),
        ]));

        var code = new ManagementClassCodeGenerator(classDefinition, classDefinition.ClassName).GenerateCode();

        var compilation = CSharpCompilation.Create(
            assemblyName: Path.GetRandomFileName(),
            syntaxTrees:
            [
                CSharpSyntaxTree.ParseText(SdkStubsSource),
                CSharpSyntaxTree.ParseText(code),
            ],
            references:
            [
                MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location),
                MetadataReference.CreateFromFile(typeof(System.ComponentModel.DataAnnotations.StringLengthAttribute).Assembly.Location),
                MetadataReference.CreateFromFile(Assembly.Load("System.Runtime").Location),
                MetadataReference.CreateFromFile(Assembly.Load("netstandard").Location),
            ],
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        using var stream = new MemoryStream();
        var result = compilation.Emit(stream);

        var errors = string.Join(
            Environment.NewLine,
            result.Diagnostics
                .Where(d => d.Severity == DiagnosticSeverity.Error)
                .Select(d => d.ToString()));

        result.Success.Should().BeTrue(errors);
    }

    // Minimal stubs for the SDK types the emitted code references. The generator pulls in
    // [KontentContentType] / [KontentElement] / [KontentEnumValue] / [MaxElements] /
    // IContentItem from the SDK side; constraint attributes ([StringLength],
    // [RegularExpression]) come from BCL.
    private const string SdkStubsSource = @"
namespace Kontent.Ai.Management.Models
{
    public interface IContentItem { }
}

namespace Kontent.Ai.Management.Annotations
{
    using System;

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public sealed class KontentContentTypeAttribute : Attribute
    {
        public string Codename { get; init; }
        public string Id { get; init; }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public sealed class KontentElementAttribute : Attribute
    {
        public string Codename { get; init; }
        public string Id { get; init; }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public sealed class KontentEnumValueAttribute : Attribute
    {
        public string Codename { get; init; }
        public string Id { get; init; }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public sealed class MaxElementsAttribute : Attribute
    {
        public MaxElementsAttribute(int n) { N = n; }
        public int N { get; }
    }
}
";
}
