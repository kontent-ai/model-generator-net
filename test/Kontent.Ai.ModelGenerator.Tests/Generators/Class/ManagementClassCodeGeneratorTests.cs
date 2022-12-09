using System;
using System.IO;
using System.Reflection;
using Kontent.Ai.Management.Models.Types.Elements;
using Kontent.Ai.ModelGenerator.Core.Common;
using Kontent.Ai.ModelGenerator.Core.Configuration;
using Kontent.Ai.ModelGenerator.Core.Generators.Class;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace Kontent.Ai.ModelGenerator.Tests.Generators.Class;

public class ManagementClassCodeGeneratorTests : ClassCodeGeneratorTestsBase
{
    public ManagementClassCodeGeneratorTests()
    {
        ClassDefinition.AddProperty(Property.FromContentTypeElement(
            TestHelper.GenerateElementMetadataBase(
                Guid.Parse("6712e528-8504-4a36-b716-a28327d6205f"),
                "text",
                "text_external_id")));
        ClassDefinition.AddProperty(Property.FromContentTypeElement(
            TestHelper.GenerateElementMetadataBase(
                Guid.Parse("014d2125-923d-4428-93b4-ad1590274912"),
                "rich_text",
                "rich_text_external_id",
                ElementMetadataType.RichText)));
        ClassDefinition.AddProperty(Property.FromContentTypeElement(
            TestHelper.GenerateElementMetadataBase(
                Guid.Parse("9d23ff46-117c-432c-8fb2-3273acfbbbf5"),
                "number",
                "number_external_id",
                ElementMetadataType.Number)));
        ClassDefinition.AddProperty(Property.FromContentTypeElement(
            TestHelper.GenerateElementMetadataBase(
                Guid.Parse("2115b9ad-5df5-45b8-aa0f-490b5119afa6"),
                "multiple_choice",
                "multiple_choice_external_id",
                ElementMetadataType.MultipleChoice)));
        ClassDefinition.AddProperty(Property.FromContentTypeElement(
            TestHelper.GenerateElementMetadataBase(
                Guid.Parse("66756a72-6af8-44a4-b58c-485425586a90"),
                "date_time",
                "date_time_external_id",
                ElementMetadataType.DateTime)));
        ClassDefinition.AddProperty(Property.FromContentTypeElement(
            TestHelper.GenerateElementMetadataBase(
                Guid.Parse("af569649-ee18-4d6a-a095-ea6ffa012546"),
                "asset",
                "asset_external_id",
                ElementMetadataType.Asset)));
        ClassDefinition.AddProperty(Property.FromContentTypeElement(
            TestHelper.GenerateElementMetadataBase(
                Guid.Parse("4fa6bad6-d984-45e8-8ebb-f6be25626ee5"),
                "modular_content",
                "modular_content_external_id",
                ElementMetadataType.LinkedItems)));
        ClassDefinition.AddProperty(Property.FromContentTypeElement(
            TestHelper.GenerateElementMetadataBase(
                Guid.Parse("44924563-44d4-4272-a20f-b8745698b082"),
                "subpages",
                "subpages_external_id",
                ElementMetadataType.Subpages)));
        ClassDefinition.AddProperty(Property.FromContentTypeElement(
            TestHelper.GenerateElementMetadataBase(
                Guid.Parse("83011da2-559d-458c-a4b5-c81a001f4139"),
                "taxonomy",
                "taxonomy_external_id",
                ElementMetadataType.Taxonomy)));
        ClassDefinition.AddProperty(Property.FromContentTypeElement(
            TestHelper.GenerateElementMetadataBase(
                Guid.Parse("14390f27-213e-4f8d-9c31-cbf9a7c0a0d8"),
                "url_slug",
                "url_slug_external_id",
                ElementMetadataType.UrlSlug)));
        ClassDefinition.AddProperty(Property.FromContentTypeElement(
            TestHelper.GenerateElementMetadataBase(
                Guid.Parse("23154ba2-73fc-450c-99d4-c18ba45bb743"),
                "custom",
                "custom_external_id",
                ElementMetadataType.Custom)));
    }

    [Fact]
    public void Constructor_CreatesInstance()
    {
        var classCodeGenerator = new ManagementClassCodeGenerator(
            ClassDefinition,
            ClassDefinition.ClassName,
            ElementReferenceType.Codename | ElementReferenceType.ExternalId | ElementReferenceType.Id);

        Assert.NotNull(classCodeGenerator);
        Assert.True(classCodeGenerator.OverwriteExisting);
    }

    [Theory]
    [InlineData(ElementReferenceType.NotSet)]
    [InlineData(ElementReferenceType.ValidationIssue)]
    [InlineData(ElementReferenceType.ValidationIssue | ElementReferenceType.Id)]
    [InlineData(ElementReferenceType.Id | ElementReferenceType.NotSet)]
    public void Constructor_ElementReferenceSetToErrorFlags_ThrowsException(ElementReferenceType elementReference)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new ManagementClassCodeGenerator(
            ClassDefinition,
            ClassDefinition.ClassName,
            elementReference));
    }

    [Fact]
    public void Constructor_ExternalIdOfElementIsNull_ElementReferenceIsExternalId_ThrowsException()
    {
        ClassDefinition.AddProperty(Property.FromContentTypeElement(
            TestHelper.GenerateElementMetadataBase(
                Guid.Parse("9712e528-8504-4a36-b716-a28327d6205f"),
                "text_no_external_id",
                null)));

        Assert.Throws<InvalidExternalIdentifierException>(() =>
        {
            new ManagementClassCodeGenerator(ClassDefinition, ClassDefinition.ClassName, ElementReferenceType.ExternalId);
        });
    }

    [Fact]
    public void Constructor_ExternalIdOfElementIsNull_ElementReferenceIsExternalIdAndOtherType_CreatesInstance()
    {
        ClassDefinition.AddProperty(Property.FromContentTypeElement(
            TestHelper.GenerateElementMetadataBase(
                Guid.Parse("9712e528-8504-4a36-b716-a28327d6205f"),
                "text_no_external_id",
                null)));

        var classCodeGenerator = new ManagementClassCodeGenerator(
            ClassDefinition,
            ClassDefinition.ClassName,
            ElementReferenceType.ExternalId | ElementReferenceType.Codename | ElementReferenceType.Id);

        Assert.NotNull(classCodeGenerator);
        Assert.True(classCodeGenerator.OverwriteExisting);
    }

    [Theory]
    //[InlineData(
    //    ElementReferenceType.Codename | ElementReferenceType.Id | ElementReferenceType.ExternalId,
    //    "CompleteContentType_CompiledCode_AllElementReferences.txt")]
    [InlineData(
        ElementReferenceType.Codename | ElementReferenceType.Id,
        "CompleteContentType_CompiledCode_CodenameAndIdElementReferences.txt")]
    [InlineData(
        ElementReferenceType.Codename,
        "CompleteContentType_CompiledCode_CodenameElementReference.txt")]
    //[InlineData(
    //    ElementReferenceType.ExternalId,
    //    "CompleteContentType_CompiledCode_ExternalIdElementReference.txt")]
    [InlineData(
        ElementReferenceType.Id,
        "CompleteContentType_CompiledCode_IdElementReference.txt")]
    public void Build_CreatesClassWithCompleteContentType(ElementReferenceType elementReference, string fileName)
    {
        var classCodeGenerator = new ManagementClassCodeGenerator(ClassDefinition, ClassDefinition.ClassName, elementReference);

        var compiledCode = classCodeGenerator.GenerateCode();

        var executingPath = AppContext.BaseDirectory;
        var expectedCode = File.ReadAllText($"{executingPath}/Assets/ManagementApi/{fileName}");

        Assert.Equal(expectedCode, compiledCode, ignoreWhiteSpaceDifferences: true, ignoreLineEndingDifferences: true);
    }

    [Fact]
    public void Build_ExternalIdNull_ElementReferenceHasExternalId_CreatesClassWithoutExternalIdElementAttributes()
    {
        var classDefinition = new ClassDefinition("External Id Is Null");
        classDefinition.AddProperty(Property.FromContentTypeElement(
            TestHelper.GenerateElementMetadataBase(
                Guid.Parse("6712e528-8504-4a36-b716-a28327d6205f"),
                "text",
                null)));
        classDefinition.AddProperty(Property.FromContentTypeElement(
            TestHelper.GenerateElementMetadataBase(
                Guid.Parse("014d2125-923d-4428-93b4-ad1590274912"),
                "rich_text",
                null,
                ElementMetadataType.RichText)));

        var classCodeGenerator = new ManagementClassCodeGenerator(classDefinition, classDefinition.ClassName, ElementReferenceType.Codename | ElementReferenceType.ExternalId);

        var compiledCode = classCodeGenerator.GenerateCode();

        var executingPath = AppContext.BaseDirectory;
        var expectedCode = File.ReadAllText($"{executingPath}/Assets/ManagementApi/CompleteContentType_CompiledCode_ExternalIdAndCodenameElementReferences_ExternalIdsAreNull.txt");

        Assert.Equal(expectedCode, compiledCode, ignoreWhiteSpaceDifferences: true, ignoreLineEndingDifferences: true);
    }

    [Theory]
    //[InlineData(ElementReferenceType.Codename | ElementReferenceType.Id | ElementReferenceType.ExternalId)]
    [InlineData(ElementReferenceType.Codename | ElementReferenceType.Id)]
    [InlineData(ElementReferenceType.Codename)]
    //[InlineData(ElementReferenceType.ExternalId)]
    [InlineData(ElementReferenceType.Id)]
    public void IntegrationTest_GeneratedCodeCompilesWithoutErrors(ElementReferenceType elementReference)
    {
        var classCodeGenerator = new ManagementClassCodeGenerator(ClassDefinition, ClassDefinition.ClassName, elementReference);
        var compiledCode = classCodeGenerator.GenerateCode();

        var compilation = CSharpCompilation.Create(
            assemblyName: Path.GetRandomFileName(),
            syntaxTrees: new[] { CSharpSyntaxTree.ParseText(compiledCode) },
            references: new[] {
                MetadataReference.CreateFromFile(Assembly.Load(new AssemblyName("netstandard")).Location),
                MetadataReference.CreateFromFile(Assembly.Load(new AssemblyName("mscorlib")).Location),
                MetadataReference.CreateFromFile(Assembly.Load(new AssemblyName("System.Runtime")).Location),
                MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Management.Modules.ModelBuilders.IModelProvider).GetTypeInfo().Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Management.Models.LanguageVariants.Elements.BaseElement).GetTypeInfo().Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Newtonsoft.Json.IJsonLineInfo).GetTypeInfo().Assembly.Location)
            },
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        AssertCompiledCode(compilation);
    }
}
