using System.Reflection;
using FluentAssertions;
using Kontent.Ai.Delivery.Abstractions;
using Kontent.Ai.Management.Models.Shared;
using Kontent.Ai.Management.Models.Types;
using Kontent.Ai.Management.Models.Types.Elements;
using Kontent.Ai.ModelGenerator.Core.Common;
using Kontent.Ai.ModelGenerator.Core.Generators.Class;
using Kontent.Ai.ModelGenerator.Core.Tests.TestHelpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Kontent.Ai.ModelGenerator.Core.Tests.Generators.Class;

public class ExtendedDeliveryClassCodeGeneratorTests : ClassCodeGeneratorTestsBase
{
    public ExtendedDeliveryClassCodeGeneratorTests()
    {
        ClassDefinition.AddProperty(Property.FromContentTypeElement(
            TestDataGenerator.GenerateElementMetadataBase(Guid.Parse("6712e528-8504-4a36-b716-a28327d6205f"), "text"),
            ElementMetadataType.Text.ToString()));

        ClassDefinition.AddProperty(Property.FromContentTypeElement(
            TestDataGenerator.GenerateElementMetadataBase(Guid.Parse("014d2125-923d-4428-93b4-ad1590274912"), "rich_text", ElementMetadataType.RichText),
            ElementMetadataType.RichText.ToString()));

        ClassDefinition.AddProperty(Property.FromContentTypeElement(
            TestDataGenerator.GenerateElementMetadataBase(Guid.Parse("9d23ff46-117c-432c-8fb2-3273acfbbbf5"), "number", ElementMetadataType.Number),
            ElementMetadataType.Number.ToString()));

        ClassDefinition.AddProperty(Property.FromContentTypeElement(
            TestDataGenerator.GenerateElementMetadataBase(Guid.Parse("2115b9ad-5df5-45b8-aa0f-490b5119afa6"), "multiple_choice", ElementMetadataType.MultipleChoice),
            ElementMetadataType.MultipleChoice.ToString()));

        ClassDefinition.AddProperty(Property.FromContentTypeElement(
            TestDataGenerator.GenerateElementMetadataBase(Guid.Parse("66756a72-6af8-44a4-b58c-485425586a90"), "date_time", ElementMetadataType.DateTime),
            ElementMetadataType.DateTime.ToString()));

        ClassDefinition.AddProperty(Property.FromContentTypeElement(
            TestDataGenerator.GenerateElementMetadataBase(Guid.Parse("af569649-ee18-4d6a-a095-ea6ffa012546"), "asset", ElementMetadataType.Asset),
            ElementMetadataType.Asset.ToString()));

        ClassDefinition.AddProperty(Property.FromContentTypeElement(
            TestDataGenerator.GenerateElementMetadataBase(Guid.Parse("114d2125-923d-4428-93b4-ad1590274912"), "rich_text_structured", ElementMetadataType.RichText),
            ElementMetadataType.RichText + Property.StructuredSuffix));

        ClassDefinition.AddProperty(Property.FromContentTypeElement(
            TestDataGenerator.GenerateElementMetadataBase(Guid.Parse("66756a72-6af8-44a4-b58c-485425586a91"), "date_time_structured", ElementMetadataType.DateTime),
            ElementMetadataType.DateTime + Property.StructuredSuffix));

        ClassDefinition.AddProperty(Property.FromContentTypeElement(
            TestDataGenerator.GenerateElementMetadataBase(Guid.Parse("83011da2-559d-458c-a4b5-c81a001f4139"), "taxonomy", ElementMetadataType.Taxonomy),
            ElementMetadataType.Taxonomy.ToString()));

        ClassDefinition.AddProperty(Property.FromContentTypeElement(
            TestDataGenerator.GenerateElementMetadataBase(Guid.Parse("14390f27-213e-4f8d-9c31-cbf9a7c0a0d8"), "url_slug", ElementMetadataType.UrlSlug),
            ElementMetadataType.UrlSlug.ToString()));

        ClassDefinition.AddProperty(Property.FromContentTypeElement(
            TestDataGenerator.GenerateElementMetadataBase(Guid.Parse("23154ba2-73fc-450c-99d4-c18ba45bb743"), "custom", ElementMetadataType.Custom),
            ElementMetadataType.Custom.ToString()));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Constructor_CreatesInstance(bool generateStructuredModularContent)
    {
        AddModularContent(generateStructuredModularContent);

        var classCodeGenerator = new ExtendedDeliveryClassCodeGenerator(ClassDefinition, ClassDefinition.ClassName, generateStructuredModularContent);

        classCodeGenerator.Should().NotBeNull();
        classCodeGenerator.OverwriteExisting.Should().BeTrue();
    }

    [Theory]
    [InlineData("CompleteContentType_CompiledCode_StructuredModularContent_ExtendedDeliveryModels", true)]
    [InlineData("CompleteContentType_CompiledCode_ExtendedDeliveryModels", false)]
    public void Build_CreatesClassWithCompleteContentType(string fileName, bool generateStructuredModularContent)
    {
        AddModularContent(generateStructuredModularContent);

        var classCodeGenerator = new ExtendedDeliveryClassCodeGenerator(ClassDefinition, ClassDefinition.ClassName, generateStructuredModularContent);

        var compiledCode = classCodeGenerator.GenerateCode();

        var executingPath = AppContext.BaseDirectory;
        var expectedCode = File.ReadAllText($"{executingPath}/Assets/{fileName}.txt");

        compiledCode.Should().Be(expectedCode);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void IntegrationTest_GeneratedCodeCompilesWithoutErrors(bool generateStructuredModularContent)
    {
        AddModularContent(generateStructuredModularContent);

        var classCodeGenerator = new ExtendedDeliveryClassCodeGenerator(ClassDefinition, ClassDefinition.ClassName, generateStructuredModularContent);
        var compiledCode = classCodeGenerator.GenerateCode();

        var heroClassDefinition = new ClassDefinition("Hero");
        var heroClassCodeGenerator = new ExtendedDeliveryClassCodeGenerator(heroClassDefinition, heroClassDefinition.ClassName, generateStructuredModularContent);
        var compiledHeroCode = heroClassCodeGenerator.GenerateCode();

        var articleClassDefinition = new ClassDefinition("Article");
        var articleClassCodeGenerator = new TypedExtendedDeliveryClassCodeGenerator(articleClassDefinition, articleClassDefinition.ClassName);
        var compiledArticleCode = articleClassCodeGenerator.GenerateCode();

        var compilation = CSharpCompilation.Create(
            assemblyName: Path.GetRandomFileName(),
            syntaxTrees: new[]
            {
                    CSharpSyntaxTree.ParseText(compiledHeroCode),
                    CSharpSyntaxTree.ParseText(compiledArticleCode),
                    CSharpSyntaxTree.ParseText(compiledCode),
            },
            references: new[] {
                    MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location),
                    MetadataReference.CreateFromFile(Assembly.Load(new AssemblyName("System.Linq")).Location),
                    MetadataReference.CreateFromFile(typeof(Delivery.Abstractions.IApiResponse).GetTypeInfo().Assembly.Location)
            },
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        AssertCompiledCode(compilation);
    }

    private void AddModularContent(bool generateStructuredModularContent)
    {
        var singleAllowedTypeMultiItemsTypeName = "Hero";
        var singleAllowedTypeExactlySingleItemTypeName = "Article";
        var modularContentType = generateStructuredModularContent
            ? nameof(IContentItem)
            : Property.ObjectType;

        #region LinkedItems

        // Linked items elements are limited to a single type with at least 1 item.
        var singleAllowedTypeMultiItems = (LinkedItemsElementMetadataModel)TestDataGenerator.
            GenerateElementMetadataBase(Guid.Parse("4fa6bad6-d984-45e8-8ebb-f6be25626ee5"), "modular_content_heroes", ElementMetadataType.LinkedItems);
        singleAllowedTypeMultiItems.AllowedTypes = new List<Reference>(new List<Reference> { Reference.ByCodename(singleAllowedTypeMultiItemsTypeName) });
        singleAllowedTypeMultiItems.ItemCountLimit = new LimitModel { Condition = LimitType.AtLeast, Value = 1 };
        ClassDefinition.AddProperty(Property.FromContentTypeElement(singleAllowedTypeMultiItems, $"IEnumerable<{modularContentType}>"));

        // Linked items element limited to a single type with exactly 1 item.
        var singleAllowedTypeExactlySingleItem = (LinkedItemsElementMetadataModel)TestDataGenerator.
            GenerateElementMetadataBase(Guid.Parse("4fa6bad6-d984-45e8-8ebb-f6be25626ee8"), "modular_content_article", ElementMetadataType.LinkedItems);
        singleAllowedTypeExactlySingleItem.AllowedTypes = new List<Reference>(new List<Reference> { Reference.ByCodename(singleAllowedTypeExactlySingleItemTypeName) });
        singleAllowedTypeExactlySingleItem.ItemCountLimit = new LimitModel { Condition = LimitType.Exactly, Value = 1 };
        ClassDefinition.AddProperty(Property.FromContentTypeElement(singleAllowedTypeExactlySingleItem, $"IEnumerable<{modularContentType}>"));

        // Linked items element limited to a single type with at most 1 item.
        var singleAllowedTypeAtMostSingleItem = (LinkedItemsElementMetadataModel)TestDataGenerator.
            GenerateElementMetadataBase(Guid.Parse("4fa6bad6-d984-45e8-8ebb-f6be25626ee8"), "modular_content_hero", ElementMetadataType.LinkedItems);
        singleAllowedTypeAtMostSingleItem.AllowedTypes = new List<Reference>(new List<Reference> { Reference.ByCodename(singleAllowedTypeExactlySingleItemTypeName) });
        singleAllowedTypeAtMostSingleItem.ItemCountLimit = new LimitModel { Condition = LimitType.Exactly, Value = 1 };
        ClassDefinition.AddProperty(Property.FromContentTypeElement(singleAllowedTypeAtMostSingleItem, $"IEnumerable<{modularContentType}>"));

        // Linked items element limited to multiple types with exactly 1 item.
        var multiAllowedTypesExactlySingleItem = (LinkedItemsElementMetadataModel)TestDataGenerator.
            GenerateElementMetadataBase(Guid.Parse("4fa6bad6-d984-45e8-8ebb-f6be25626ee9"), "modular_content_blog", ElementMetadataType.LinkedItems);
        multiAllowedTypesExactlySingleItem.AllowedTypes = new List<Reference>(new List<Reference>
        {
            Reference.ByCodename("Hero"),
            Reference.ByCodename("Article")
        });
        multiAllowedTypesExactlySingleItem.ItemCountLimit = new LimitModel { Condition = LimitType.Exactly, Value = 1 };
        ClassDefinition.AddProperty(Property.FromContentTypeElement(multiAllowedTypesExactlySingleItem, $"IEnumerable<{modularContentType}>"));

        // Linked items element limited to multiple types with at most 1 item.
        var multiAllowedTypesAtMostSingleItem = (LinkedItemsElementMetadataModel)TestDataGenerator.
            GenerateElementMetadataBase(Guid.Parse("4fa6bad6-d984-45e8-8ebb-f6be25626ccc"), "modular_content_coffee", ElementMetadataType.LinkedItems);
        multiAllowedTypesAtMostSingleItem.AllowedTypes = new List<Reference>(new List<Reference>
        {
            Reference.ByCodename("Hero"),
            Reference.ByCodename("Article")
        });
        multiAllowedTypesAtMostSingleItem.ItemCountLimit = new LimitModel { Condition = LimitType.AtMost, Value = 1 };
        ClassDefinition.AddProperty(Property.FromContentTypeElement(multiAllowedTypesAtMostSingleItem, $"IEnumerable<{modularContentType}>"));

        // Linked items element limited to multiple types with at least 1 item.
        var multiAllowedTypesMultiItems = (LinkedItemsElementMetadataModel)TestDataGenerator.
            GenerateElementMetadataBase(Guid.Parse("9fa6bad6-d984-45e8-8ebb-f6be25626ee5"), "modular_content_coffees", ElementMetadataType.LinkedItems);
        multiAllowedTypesMultiItems.AllowedTypes = new List<Reference>(new List<Reference>
        {
            Reference.ByCodename("Hero"),
            Reference.ByCodename("Article")
        });
        multiAllowedTypesMultiItems.ItemCountLimit = new LimitModel { Condition = LimitType.AtLeast, Value = 1 };
        ClassDefinition.AddProperty(Property.FromContentTypeElement(multiAllowedTypesMultiItems, $"IEnumerable<{modularContentType}>"));

        #endregion

        #region Subpages

        // Linked items elements are limited to a single type with at least 1 item.
        var subpagesSingleAllowedTypeMultiItems = (SubpagesElementMetadataModel)TestDataGenerator.
            GenerateElementMetadataBase(Guid.Parse("8fa6bad6-d984-45e8-8ebb-f6be25626ee5"), "subpages_heroes", ElementMetadataType.Subpages);
        subpagesSingleAllowedTypeMultiItems.AllowedContentTypes = new List<Reference>(new List<Reference> { Reference.ByCodename(singleAllowedTypeMultiItemsTypeName) });
        subpagesSingleAllowedTypeMultiItems.ItemCountLimit = new LimitModel { Condition = LimitType.AtLeast, Value = 1 };
        ClassDefinition.AddProperty(Property.FromContentTypeElement(subpagesSingleAllowedTypeMultiItems, $"IEnumerable<{modularContentType}>"));

        // Linked items element limited to a single type with exactly 1 item.
        var subpagesSingleAllowedTypeExactlySingleItem = (SubpagesElementMetadataModel)TestDataGenerator.
            GenerateElementMetadataBase(Guid.Parse("8fa6bad6-d984-45e8-8ebb-f6be25626ee8"), "subpages_article", ElementMetadataType.Subpages);
        subpagesSingleAllowedTypeExactlySingleItem.AllowedContentTypes = new List<Reference>(new List<Reference> { Reference.ByCodename(singleAllowedTypeExactlySingleItemTypeName) });
        subpagesSingleAllowedTypeExactlySingleItem.ItemCountLimit = new LimitModel { Condition = LimitType.Exactly, Value = 1 };
        ClassDefinition.AddProperty(Property.FromContentTypeElement(subpagesSingleAllowedTypeExactlySingleItem, $"IEnumerable<{modularContentType}>"));

        // Linked items element limited to a single type with at most 1 item.
        var subpagesSingleAllowedTypeAtMostSingleItem = (SubpagesElementMetadataModel)TestDataGenerator.
            GenerateElementMetadataBase(Guid.Parse("8fa6bad6-d984-45e8-8ebb-f6be25626ee8"), "subpages_hero", ElementMetadataType.Subpages);
        subpagesSingleAllowedTypeAtMostSingleItem.AllowedContentTypes = new List<Reference>(new List<Reference> { Reference.ByCodename(singleAllowedTypeExactlySingleItemTypeName) });
        subpagesSingleAllowedTypeAtMostSingleItem.ItemCountLimit = new LimitModel { Condition = LimitType.AtMost, Value = 1 };
        ClassDefinition.AddProperty(Property.FromContentTypeElement(subpagesSingleAllowedTypeAtMostSingleItem, $"IEnumerable<{modularContentType}>"));

        // Linked items element limited to multiple types with exactly 1 item.
        var subpagesMultiAllowedTypesExactlySingleItem = (SubpagesElementMetadataModel)TestDataGenerator.
            GenerateElementMetadataBase(Guid.Parse("8fa6bad6-d984-45e8-8ebb-f6be25626ee9"), "subpages_blog", ElementMetadataType.Subpages);
        subpagesMultiAllowedTypesExactlySingleItem.AllowedContentTypes = new List<Reference>(new List<Reference>
        {
            Reference.ByCodename("Hero"),
            Reference.ByCodename("Article")
        });
        subpagesMultiAllowedTypesExactlySingleItem.ItemCountLimit = new LimitModel { Condition = LimitType.Exactly, Value = 1 };
        ClassDefinition.AddProperty(Property.FromContentTypeElement(subpagesMultiAllowedTypesExactlySingleItem, $"IEnumerable<{modularContentType}>"));

        // Linked items element limited to multiple types with at least 1 at most or exactly 1 item.
        var subpagesMultiAllowedTypesAtMostSingleItem = (SubpagesElementMetadataModel)TestDataGenerator.
            GenerateElementMetadataBase(Guid.Parse("8fa6bad6-d984-45e8-8ebb-f6be25626ee9"), "subpages_coffee", ElementMetadataType.Subpages);
        subpagesMultiAllowedTypesAtMostSingleItem.AllowedContentTypes = new List<Reference>(new List<Reference>
        {
            Reference.ByCodename("Hero"),
            Reference.ByCodename("Article")
        });
        subpagesMultiAllowedTypesAtMostSingleItem.ItemCountLimit = new LimitModel { Condition = LimitType.AtMost, Value = 1 };
        ClassDefinition.AddProperty(Property.FromContentTypeElement(subpagesMultiAllowedTypesAtMostSingleItem, $"IEnumerable<{modularContentType}>"));

        // Linked items element limited to multiple types with at least 1 item.
        var subpagesMultiAllowedTypesMultiItems = (SubpagesElementMetadataModel)TestDataGenerator.
            GenerateElementMetadataBase(Guid.Parse("8fa6bad6-d984-45e8-8ebb-f6be25626ee6"), "subpages_coffees", ElementMetadataType.Subpages);
        subpagesMultiAllowedTypesMultiItems.AllowedContentTypes = new List<Reference>(new List<Reference>
        {
            Reference.ByCodename("Hero"),
            Reference.ByCodename("Article")
        });
        subpagesMultiAllowedTypesMultiItems.ItemCountLimit = new LimitModel { Condition = LimitType.AtLeast, Value = 1 };
        ClassDefinition.AddProperty(Property.FromContentTypeElement(subpagesMultiAllowedTypesMultiItems, $"IEnumerable<{modularContentType}>"));

        #endregion
    }
}
