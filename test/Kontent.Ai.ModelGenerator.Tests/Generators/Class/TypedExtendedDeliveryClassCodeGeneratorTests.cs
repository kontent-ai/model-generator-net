﻿using Kontent.Ai.Management.Models.Types.Elements;
using Kontent.Ai.ModelGenerator.Core.Common;
using Kontent.Ai.ModelGenerator.Core.Generators.Class;
using System;
using System.IO;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using Kontent.Ai.Management.Models.Shared;
using System.Collections.Generic;
using Kontent.Ai.Management.Models.Types;
using Kontent.Ai.ModelGenerator.Core.Helpers;
using Kontent.Ai.ModelGenerator.Tests.TestHelpers;
using FluentAssertions;

namespace Kontent.Ai.ModelGenerator.Tests.Generators.Class;

public class TypedExtendedDeliveryClassCodeGeneratorTests : ClassCodeGeneratorTestsBase
{
    public TypedExtendedDeliveryClassCodeGeneratorTests()
    {
        var singleAllowedTypeMultiItemsTypeName = "Hero";
        var singleAllowedTypeExactlySingleItemTypeName = "Article";

        #region LinkedItems

        // Linked items elements are limited to a single type with at least 1 item.
        var singleAllowedTypeMultiItemsTypeCodename = "modular_content_heroes";
        var singleAllowedTypeMultiItems = (LinkedItemsElementMetadataModel)TestDataGenerator.
            GenerateElementMetadataBase(Guid.NewGuid(), singleAllowedTypeMultiItemsTypeCodename, ElementMetadataType.LinkedItems);
        singleAllowedTypeMultiItems.AllowedTypes = new List<Reference>(new List<Reference> { Reference.ByCodename(singleAllowedTypeMultiItemsTypeName) });
        singleAllowedTypeMultiItems.ItemCountLimit = new LimitModel { Condition = LimitType.AtLeast, Value = 1 };
        ClassDefinition.AddProperty(Property.FromContentTypeElement(
            singleAllowedTypeMultiItems,
            TextHelpers.GetEnumerableType(singleAllowedTypeMultiItemsTypeName),
            $"{singleAllowedTypeMultiItemsTypeCodename}_{singleAllowedTypeMultiItemsTypeName}"));

        // Linked items element limited to a single type with at most or exactly 1 item.
        var singleAllowedTypeExactlySingleItem = (LinkedItemsElementMetadataModel)TestDataGenerator.
            GenerateElementMetadataBase(Guid.NewGuid(), "modular_content_article", ElementMetadataType.LinkedItems);
        singleAllowedTypeExactlySingleItem.AllowedTypes = new List<Reference>(new List<Reference> { Reference.ByCodename(singleAllowedTypeExactlySingleItemTypeName) });
        singleAllowedTypeExactlySingleItem.ItemCountLimit = new LimitModel { Condition = LimitType.Exactly, Value = 1 };
        ClassDefinition.AddProperty(Property.FromContentTypeElement(singleAllowedTypeExactlySingleItem, singleAllowedTypeExactlySingleItemTypeName));

        #endregion

        #region Subpages

        // Subpages elements are limited to a single type with at least 1 item.
        var subpagesSingleAllowedTypeMultiItemsTypeCodename = "subpages_heroes";
        var subpagesSingleAllowedTypeMultiItems = (SubpagesElementMetadataModel)TestDataGenerator.
            GenerateElementMetadataBase(Guid.NewGuid(), subpagesSingleAllowedTypeMultiItemsTypeCodename, ElementMetadataType.Subpages);
        subpagesSingleAllowedTypeMultiItems.AllowedContentTypes = new List<Reference>(new List<Reference> { Reference.ByCodename(singleAllowedTypeMultiItemsTypeName) });
        subpagesSingleAllowedTypeMultiItems.ItemCountLimit = new LimitModel { Condition = LimitType.AtLeast, Value = 1 };
        ClassDefinition.AddProperty(Property.FromContentTypeElement(
            subpagesSingleAllowedTypeMultiItems,
            TextHelpers.GetEnumerableType(singleAllowedTypeMultiItemsTypeName),
            $"{subpagesSingleAllowedTypeMultiItemsTypeCodename}_{singleAllowedTypeMultiItemsTypeName}"));

        // Subpages element limited to a single type with at most or exactly 1 item.
        var subpagesSingleAllowedTypeExactlySingleItem = (SubpagesElementMetadataModel)TestDataGenerator.
            GenerateElementMetadataBase(Guid.NewGuid(), "subpages_article", ElementMetadataType.Subpages);
        subpagesSingleAllowedTypeExactlySingleItem.AllowedContentTypes = new List<Reference>(new List<Reference> { Reference.ByCodename(singleAllowedTypeExactlySingleItemTypeName) });
        subpagesSingleAllowedTypeExactlySingleItem.ItemCountLimit = new LimitModel { Condition = LimitType.Exactly, Value = 1 };
        ClassDefinition.AddProperty(Property.FromContentTypeElement(subpagesSingleAllowedTypeExactlySingleItem, singleAllowedTypeExactlySingleItemTypeName));

        #endregion
    }

    [Fact]
    public void Constructor_CreatesInstance()
    {
        var classCodeGenerator = new TypedExtendedDeliveryClassCodeGenerator(ClassDefinition, ClassDefinition.ClassName);

        classCodeGenerator.Should().NotBeNull();
        classCodeGenerator.OverwriteExisting.Should().BeTrue();
    }

    [Fact]
    public void Build_CreatesClassWithCompleteContentType()
    {
        var classCodeGenerator = new TypedExtendedDeliveryClassCodeGenerator(ClassDefinition, ClassDefinition.ClassName);

        var compiledCode = classCodeGenerator.GenerateCode();

        var executingPath = AppContext.BaseDirectory;
        var expectedCode = File.ReadAllText(executingPath + "/Assets/CompleteContentType_CompiledCode_TypedExtendedDeliveryModels.txt");

        compiledCode.Should().Be(expectedCode);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void IntegrationTest_GeneratedCodeCompilesWithoutErrors(bool generateStructuredModularContent)
    {
        var classCodeGenerator = new ExtendedDeliveryClassCodeGenerator(ClassDefinition, ClassDefinition.ClassName, generateStructuredModularContent);
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
