using Kontent.Ai.Management.Models.Types.Elements;
using Kontent.Ai.ModelGenerator.Core.Common;
using Kontent.Ai.ModelGenerator.Core.Generators.Class;
using System;
using System.Collections;
using System.IO;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Kontent.Ai.Management.Models.Shared;
using System.Collections.Generic;
using Kontent.Ai.Management.Models.Types;
using Kontent.Ai.ModelGenerator.Tests.TestHelpers;

namespace Kontent.Ai.ModelGenerator.Tests.Generators.Class;

public class TypedExtendedDeliveryClassCodeGeneratorTests : ClassCodeGeneratorTestsBase
{
    public TypedExtendedDeliveryClassCodeGeneratorTests()
    {
        // Linked items elements are limited to a single type with at least 1 item.
        var singleAllowedTypeMultiItemsTypeName = "Hero";
        var singleAllowedTypeMultiItemsTypeCodename = "modular_content_heroes";
        var singleAllowedTypeMultiItems = (LinkedItemsElementMetadataModel)TestDataGenerator.
            GenerateElementMetadataBase(Guid.NewGuid(), singleAllowedTypeMultiItemsTypeCodename, ElementMetadataType.LinkedItems);
        singleAllowedTypeMultiItems.AllowedTypes = new List<Reference>(new List<Reference> { Reference.ByCodename(singleAllowedTypeMultiItemsTypeName) });
        singleAllowedTypeMultiItems.ItemCountLimit = new LimitModel { Condition = LimitType.AtLeast, Value = 1 };
        ClassDefinition.AddProperty(Property.FromContentTypeElement(
            singleAllowedTypeMultiItems,
            $"{nameof(IEnumerable)}<{singleAllowedTypeMultiItemsTypeName}>",
            $"{singleAllowedTypeMultiItemsTypeCodename}_{singleAllowedTypeMultiItemsTypeName}"));

        // Linked items element limited to a single type with at most or exactly 1 item.
        var singleAllowedTypeExactlySingleItemTypeName = "Article";
        var singleAllowedTypeExactlySingleItem = (LinkedItemsElementMetadataModel)TestDataGenerator.
                GenerateElementMetadataBase(Guid.NewGuid(), "modular_content_article", ElementMetadataType.LinkedItems);
        singleAllowedTypeExactlySingleItem.AllowedTypes = new List<Reference>(new List<Reference> { Reference.ByCodename(singleAllowedTypeExactlySingleItemTypeName) });
        singleAllowedTypeExactlySingleItem.ItemCountLimit = new LimitModel { Condition = LimitType.Exactly, Value = 1 };
        ClassDefinition.AddProperty(Property.FromContentTypeElement(singleAllowedTypeExactlySingleItem, singleAllowedTypeExactlySingleItemTypeName));
    }

    [Fact]
    public void Constructor_CreatesInstance()
    {
        var classCodeGenerator = new TypedExtendedDeliveryClassCodeGenerator(ClassDefinition, ClassDefinition.ClassName);

        Assert.NotNull(classCodeGenerator);
        Assert.True(classCodeGenerator.OverwriteExisting);
    }

    [Fact]
    public void Build_CreatesClassWithCompleteContentType()
    {
        var classCodeGenerator = new TypedExtendedDeliveryClassCodeGenerator(ClassDefinition, ClassDefinition.ClassName);

        var compiledCode = classCodeGenerator.GenerateCode();

        var executingPath = AppContext.BaseDirectory;
        var expectedCode = File.ReadAllText(executingPath + "/Assets/CompleteContentType_CompiledCode_TypedExtendedDeliveryModels.txt");

        Assert.Equal(expectedCode, compiledCode, ignoreWhiteSpaceDifferences: true, ignoreLineEndingDifferences: true);
    }

    [Fact]
    public void IntegrationTest_GeneratedCodeCompilesWithoutErrors()
    {
        var classCodeGenerator = new ExtendedDeliveryClassCodeGenerator(ClassDefinition, ClassDefinition.ClassName);
        var compiledCode = classCodeGenerator.GenerateCode();

        var heroClassDefinition = new ClassDefinition("Hero");
        var heroClassCodeGenerator = new TypedExtendedDeliveryClassCodeGenerator(heroClassDefinition, heroClassDefinition.ClassName);
        var compiledHeroCode = heroClassCodeGenerator.GenerateCode();

        var articleClassDefinition = new ClassDefinition("Article");
        var articleClassCodeGenerator = new TypedExtendedDeliveryClassCodeGenerator(articleClassDefinition, articleClassDefinition.ClassName);
        var compiledArticleCode = articleClassCodeGenerator.GenerateCode();

        var contentItemCodeGenerator = new ContentItemClassCodeGenerator();
        var compiledContentItemCode = contentItemCodeGenerator.GenerateCode();

        var compilation = CSharpCompilation.Create(
            assemblyName: Path.GetRandomFileName(),
            syntaxTrees: new[]
            {
                CSharpSyntaxTree.ParseText(compiledContentItemCode),
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
}
