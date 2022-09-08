using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Kontent.Ai.Management.Models.Types.Elements;
using Kontent.Ai.ModelGenerator.Core.Common;
using Kontent.Ai.ModelGenerator.Core.Generators.Class;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace Kontent.Ai.ModelGenerator.Tests.Generators.Class;

public class ManagementClassCodeGeneratorTests
{
    [Fact]
    public void Constructor_CreatesInstance()
    {
        var classDefinition = new ClassDefinition("Complete content type");

        var classCodeGenerator = new ManagementClassCodeGenerator(classDefinition, classDefinition.ClassName);

        Assert.NotNull(classCodeGenerator);
        Assert.True(classCodeGenerator.OverwriteExisting);
    }

    [Fact]
    public void Build_CreatesClassWithCompleteContentType()
    {
        var classDefinition = new ClassDefinition("Complete content type");
        classDefinition.AddProperty(Property.FromContentTypeElement(
            TestHelper.GenerateElementMetadataBase(Guid.Parse("6712e528-8504-4a36-b716-a28327d6205f"), "text")));
        classDefinition.AddProperty(Property.FromContentTypeElement(
            TestHelper.GenerateElementMetadataBase(Guid.Parse("014d2125-923d-4428-93b4-ad1590274912"), "rich_text", ElementMetadataType.RichText)));
        classDefinition.AddProperty(Property.FromContentTypeElement(
            TestHelper.GenerateElementMetadataBase(Guid.Parse("9d23ff46-117c-432c-8fb2-3273acfbbbf5"), "number", ElementMetadataType.Number)));
        classDefinition.AddProperty(Property.FromContentTypeElement(
            TestHelper.GenerateElementMetadataBase(Guid.Parse("2115b9ad-5df5-45b8-aa0f-490b5119afa6"), "multiple_choice", ElementMetadataType.MultipleChoice)));
        classDefinition.AddProperty(Property.FromContentTypeElement(
            TestHelper.GenerateElementMetadataBase(Guid.Parse("66756a72-6af8-44a4-b58c-485425586a90"), "date_time", ElementMetadataType.DateTime)));
        classDefinition.AddProperty(Property.FromContentTypeElement(
            TestHelper.GenerateElementMetadataBase(Guid.Parse("af569649-ee18-4d6a-a095-ea6ffa012546"), "asset", ElementMetadataType.Asset)));
        classDefinition.AddProperty(Property.FromContentTypeElement(
            TestHelper.GenerateElementMetadataBase(Guid.Parse("4fa6bad6-d984-45e8-8ebb-f6be25626ee5"), "modular_content", ElementMetadataType.LinkedItems)));
        classDefinition.AddProperty(Property.FromContentTypeElement(
            TestHelper.GenerateElementMetadataBase(Guid.Parse("44924563-44d4-4272-a20f-b8745698b082"), "subpages", ElementMetadataType.Subpages)));
        classDefinition.AddProperty(Property.FromContentTypeElement(
            TestHelper.GenerateElementMetadataBase(Guid.Parse("83011da2-559d-458c-a4b5-c81a001f4139"), "taxonomy", ElementMetadataType.Taxonomy)));
        classDefinition.AddProperty(Property.FromContentTypeElement(
            TestHelper.GenerateElementMetadataBase(Guid.Parse("14390f27-213e-4f8d-9c31-cbf9a7c0a0d8"), "url_slug", ElementMetadataType.UrlSlug)));
        classDefinition.AddProperty(Property.FromContentTypeElement(
            TestHelper.GenerateElementMetadataBase(Guid.Parse("23154ba2-73fc-450c-99d4-c18ba45bb743"), "custom", ElementMetadataType.Custom)));

        var classCodeGenerator = new ManagementClassCodeGenerator(classDefinition, classDefinition.ClassName);

        var compiledCode = classCodeGenerator.GenerateCode();

        var executingPath = AppContext.BaseDirectory;
        var expectedCode = File.ReadAllText(executingPath + "/Assets/CompleteContentType_CompiledCode_ManagementApi.txt");

        Assert.Equal(expectedCode, compiledCode, ignoreWhiteSpaceDifferences: true, ignoreLineEndingDifferences: true);
    }

    [Fact]
    public void IntegrationTest_GeneratedCodeCompilesWithoutErrors()
    {
        var classDefinition = new ClassDefinition("Complete content type");
        classDefinition.AddProperty(Property.FromContentTypeElement(
            TestHelper.GenerateElementMetadataBase(Guid.Parse("6712e528-8504-4a36-b716-a28327d6205f"), "text")));
        classDefinition.AddProperty(Property.FromContentTypeElement(
            TestHelper.GenerateElementMetadataBase(Guid.Parse("014d2125-923d-4428-93b4-ad1590274912"), "rich_text", ElementMetadataType.RichText)));
        classDefinition.AddProperty(Property.FromContentTypeElement(
            TestHelper.GenerateElementMetadataBase(Guid.Parse("9d23ff46-117c-432c-8fb2-3273acfbbbf5"), "number", ElementMetadataType.Number)));
        classDefinition.AddProperty(Property.FromContentTypeElement(
            TestHelper.GenerateElementMetadataBase(Guid.Parse("2115b9ad-5df5-45b8-aa0f-490b5119afa6"), "multiple_choice", ElementMetadataType.MultipleChoice)));
        classDefinition.AddProperty(Property.FromContentTypeElement(
            TestHelper.GenerateElementMetadataBase(Guid.Parse("66756a72-6af8-44a4-b58c-485425586a90"), "date_time", ElementMetadataType.DateTime)));
        classDefinition.AddProperty(Property.FromContentTypeElement(
            TestHelper.GenerateElementMetadataBase(Guid.Parse("af569649-ee18-4d6a-a095-ea6ffa012546"), "asset", ElementMetadataType.Asset)));
        classDefinition.AddProperty(Property.FromContentTypeElement(
            TestHelper.GenerateElementMetadataBase(Guid.Parse("4fa6bad6-d984-45e8-8ebb-f6be25626ee5"), "modular_content", ElementMetadataType.LinkedItems)));
        classDefinition.AddProperty(Property.FromContentTypeElement(
            TestHelper.GenerateElementMetadataBase(Guid.Parse("44924563-44d4-4272-a20f-b8745698b082"), "subpages", ElementMetadataType.Subpages)));
        classDefinition.AddProperty(Property.FromContentTypeElement(
            TestHelper.GenerateElementMetadataBase(Guid.Parse("83011da2-559d-458c-a4b5-c81a001f4139"), "taxonomy", ElementMetadataType.Taxonomy)));
        classDefinition.AddProperty(Property.FromContentTypeElement(
            TestHelper.GenerateElementMetadataBase(Guid.Parse("14390f27-213e-4f8d-9c31-cbf9a7c0a0d8"), "url_slug", ElementMetadataType.UrlSlug)));
        classDefinition.AddProperty(Property.FromContentTypeElement(
            TestHelper.GenerateElementMetadataBase(Guid.Parse("23154ba2-73fc-450c-99d4-c18ba45bb743"), "custom", ElementMetadataType.Custom)));

        var classCodeGenerator = new ManagementClassCodeGenerator(classDefinition, classDefinition.ClassName);
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
