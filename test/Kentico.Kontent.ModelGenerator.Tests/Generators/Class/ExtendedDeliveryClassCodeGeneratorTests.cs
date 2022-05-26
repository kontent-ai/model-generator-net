using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Kentico.Kontent.Management.Models.Shared;
using Kentico.Kontent.Management.Models.Types.Elements;
using Kentico.Kontent.ModelGenerator.Core.Common;
using Kentico.Kontent.ModelGenerator.Core.Generators.Class;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace Kentico.Kontent.ModelGenerator.Tests.Generators.Class
{
    public class ExtendedDeliveryClassCodeGeneratorTests : TestBaseClassCodeGenerator
    {
        public ExtendedDeliveryClassCodeGeneratorTests()
        {
            ClassDefinition.AddProperty(Property.FromContentTypeElement(
                TestHelper.GenerateElementMetadataBase(Guid.Parse("6712e528-8504-4a36-b716-a28327d6205f"), "text"),
                ElementMetadataType.Text.ToString()));

            ClassDefinition.AddProperty(Property.FromContentTypeElement(
                TestHelper.GenerateElementMetadataBase(Guid.Parse("014d2125-923d-4428-93b4-ad1590274912"), "rich_text", ElementMetadataType.RichText),
                ElementMetadataType.RichText.ToString()));

            ClassDefinition.AddProperty(Property.FromContentTypeElement(
                TestHelper.GenerateElementMetadataBase(Guid.Parse("9d23ff46-117c-432c-8fb2-3273acfbbbf5"), "number", ElementMetadataType.Number),
                ElementMetadataType.Number.ToString()));

            ClassDefinition.AddProperty(Property.FromContentTypeElement(
                TestHelper.GenerateElementMetadataBase(Guid.Parse("2115b9ad-5df5-45b8-aa0f-490b5119afa6"), "multiple_choice", ElementMetadataType.MultipleChoice),
                ElementMetadataType.MultipleChoice.ToString()));

            ClassDefinition.AddProperty(Property.FromContentTypeElement(
                TestHelper.GenerateElementMetadataBase(Guid.Parse("66756a72-6af8-44a4-b58c-485425586a90"), "date_time", ElementMetadataType.DateTime),
                ElementMetadataType.DateTime.ToString()));

            ClassDefinition.AddProperty(Property.FromContentTypeElement(
                TestHelper.GenerateElementMetadataBase(Guid.Parse("af569649-ee18-4d6a-a095-ea6ffa012546"), "asset", ElementMetadataType.Asset),
                ElementMetadataType.Asset.ToString()));

            var heroTypeName = "Hero";
            var heroLinkedItem = (LinkedItemsElementMetadataModel)TestHelper.
                GenerateElementMetadataBase(Guid.Parse("4fa6bad6-d984-45e8-8ebb-f6be25626ee5"), "modular_content_hero", ElementMetadataType.LinkedItems);
            heroLinkedItem.AllowedTypes = new List<Reference>(new List<Reference> { Reference.ByCodename(heroTypeName) });
            ClassDefinition.AddProperty(Property.FromContentTypeElement(heroLinkedItem, $"IEnumerable<{heroTypeName}>"));

            var multiAllowedTypesLinkedItem = (LinkedItemsElementMetadataModel)TestHelper.
                GenerateElementMetadataBase(Guid.Parse("4fa6bad6-d984-45e8-8ebb-f6be25626ee5"), "modular_content_multi", ElementMetadataType.LinkedItems);
            multiAllowedTypesLinkedItem.AllowedTypes = new List<Reference>(new List<Reference>
            {
                Reference.ByCodename("Hero"),
                Reference.ByCodename("Article")
            });
            ClassDefinition.AddProperty(Property.FromContentTypeElement(multiAllowedTypesLinkedItem, $"IEnumerable<{ContentItemClassCodeGenerator.DefaultContentItemClassName}>"));

            ClassDefinition.AddProperty(Property.FromContentTypeElement(
                TestHelper.GenerateElementMetadataBase(Guid.Parse("114d2125-923d-4428-93b4-ad1590274912"), "rich_text_structured", ElementMetadataType.RichText),
                ElementMetadataType.RichText + Property.StructuredSuffix));

            ClassDefinition.AddProperty(Property.FromContentTypeElement(
                TestHelper.GenerateElementMetadataBase(Guid.Parse("83011da2-559d-458c-a4b5-c81a001f4139"), "taxonomy", ElementMetadataType.Taxonomy),
                ElementMetadataType.Taxonomy.ToString()));

            ClassDefinition.AddProperty(Property.FromContentTypeElement(
                TestHelper.GenerateElementMetadataBase(Guid.Parse("14390f27-213e-4f8d-9c31-cbf9a7c0a0d8"), "url_slug", ElementMetadataType.UrlSlug),
                ElementMetadataType.UrlSlug.ToString()));

            ClassDefinition.AddProperty(Property.FromContentTypeElement(
                TestHelper.GenerateElementMetadataBase(Guid.Parse("23154ba2-73fc-450c-99d4-c18ba45bb743"), "custom", ElementMetadataType.Custom),
                ElementMetadataType.Custom.ToString()));
        }

        [Fact]
        public void Constructor_CreatesInstance()
        {
            var classCodeGenerator = new ExtendedDeliveryClassCodeGenerator(ClassDefinition, ClassDefinition.ClassName);

            Assert.NotNull(classCodeGenerator);
            Assert.True(classCodeGenerator.OverwriteExisting);
        }

        [Fact]
        public void Build_CreatesClassWithCompleteContentType()
        {
            var classCodeGenerator = new ExtendedDeliveryClassCodeGenerator(ClassDefinition, ClassDefinition.ClassName);

            var compiledCode = classCodeGenerator.GenerateCode();

            var executingPath = AppContext.BaseDirectory;
            var expectedCode = File.ReadAllText(executingPath + "/Assets/CompleteContentType_CompiledCode_ExtendedDeliveryModels.txt");

            Assert.Equal(expectedCode, compiledCode, ignoreWhiteSpaceDifferences: true, ignoreLineEndingDifferences: true);
        }

        [Fact]
        public void IntegrationTest_GeneratedCodeCompilesWithoutErrors()
        {
            var classCodeGenerator = new ExtendedDeliveryClassCodeGenerator(ClassDefinition, ClassDefinition.ClassName);
            var compiledCode = classCodeGenerator.GenerateCode();

            var heroClassDefinition = new ClassDefinition("Hero");
            var heroClassCodeGenerator = new ExtendedDeliveryClassCodeGenerator(heroClassDefinition, heroClassDefinition.ClassName);
            var compiledHeroCode = heroClassCodeGenerator.GenerateCode();

            var contentItemCodeGenerator = new ContentItemClassCodeGenerator();
            var compiledContentItemCode = contentItemCodeGenerator.GenerateCode();

            var compilation = CSharpCompilation.Create(
                assemblyName: Path.GetRandomFileName(),
                syntaxTrees: new[]
                {
                    CSharpSyntaxTree.ParseText(compiledContentItemCode),
                    CSharpSyntaxTree.ParseText(compiledHeroCode),
                    CSharpSyntaxTree.ParseText(compiledCode),
                },
                references: new[] {
                    MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location),
                    MetadataReference.CreateFromFile(typeof(Delivery.Abstractions.IApiResponse).GetTypeInfo().Assembly.Location)
                },
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            AssertCompiledCode(compilation);
        }
    }
}
