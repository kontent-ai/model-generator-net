using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Kontent.Ai.Management;
using Kontent.Ai.Management.Configuration;
using Kontent.Ai.Management.Models.Types;
using Kontent.Ai.Management.Models.Types.Elements;
using Kontent.Ai.Management.Models.TypeSnippets;
using Kontent.Ai.ModelGenerator.Core;
using Kontent.Ai.ModelGenerator.Core.Common;
using Kontent.Ai.ModelGenerator.Core.Configuration;
using Kontent.Ai.ModelGenerator.Core.Generators.Class;
using Kontent.Ai.ModelGenerator.Core.Helpers;
using Kontent.Ai.ModelGenerator.Tests.TestHelpers;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Kontent.Ai.ModelGenerator.Tests;

public class ExtendedDeliveryCodeGeneratorTests : CodeGeneratorTestsBase
{
    /// <summary>
    /// represents count of elements in 'management_types.json'
    /// </summary>
    private const int NumberOfContentTypesWithDefaultContentItem = (14 * 2) + 1;

    private readonly string DefaultLinkedItemsType = TextHelpers.GetEnumerableType(ContentItemClassCodeGenerator.DefaultContentItemClassName);
    private readonly IManagementClient _managementClient;
    private readonly IOutputProvider _outputProvider;
    protected override string TempDir => Path.Combine(Path.GetTempPath(), "ExtendedDeliveryCodeGeneratorIntegrationTests");

    public ExtendedDeliveryCodeGeneratorTests()
    {
        _managementClient = CreateManagementClient();
        _outputProvider = new Mock<IOutputProvider>().Object;
    }

    [Fact]
    public void Constructor_ManagementIsTrue_Throws()
    {
        var mockOptions = new Mock<IOptions<CodeGeneratorOptions>>();
        mockOptions.SetupGet(option => option.Value).Returns(new CodeGeneratorOptions
        {
            ManagementApi = true,
            ExtendedDeliverModels = true
        });

        Creator(mockOptions.Object).Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Constructor_ExtendedDeliverModelsIsFalse_Throws()
    {
        var mockOptions = new Mock<IOptions<CodeGeneratorOptions>>();
        mockOptions.SetupGet(option => option.Value).Returns(new CodeGeneratorOptions
        {
            ManagementApi = false,
            ExtendedDeliverModels = false,
            ExtendedDeliverPreviewModels = false
        });

        Creator(mockOptions.Object).Should().Throw<InvalidOperationException>();
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void Constructor_CreatesInstance(bool extendedDeliverModels, bool extendedDeliverPreviewModels)
    {
        var mockOptions = new Mock<IOptions<CodeGeneratorOptions>>();
        mockOptions.SetupGet(option => option.Value).Returns(new CodeGeneratorOptions
        {
            ManagementApi = false,
            ExtendedDeliverModels = extendedDeliverModels,
            ExtendedDeliverPreviewModels = extendedDeliverPreviewModels
        });

        var extendedDeliveryCodeGenerator = new ExtendedDeliveryCodeGenerator(mockOptions.Object, _outputProvider, _managementClient);

        extendedDeliveryCodeGenerator.Should().NotBeNull();
    }

    [Fact]
    public void GetClassCodeGenerators_ExtendedDeliverPreviewModelsIsFalse_Returns()
    {
        var mockOptions = new Mock<IOptions<CodeGeneratorOptions>>();
        mockOptions.SetupGet(option => option.Value).Returns(new CodeGeneratorOptions
        {
            ManagementApi = false,
            ExtendedDeliverModels = true,
            ExtendedDeliverPreviewModels = false
        });

        var contentType = new ContentTypeModel
        {
            Codename = "content_type",
            Elements = new List<ElementMetadataBase>
            {
                LinkedItemsContentTypeData.SingleAllowedTypeMultiItems,
                LinkedItemsContentTypeData.SingleAllowedTypeExactlySingleItem,
                LinkedItemsContentTypeData.MultiAllowedTypesSingleItem,
                LinkedItemsContentTypeData.MultiAllowedTypesMultiItems,
                SubpagesContentTypeData.SingleAllowedTypeMultiItems,
                SubpagesContentTypeData.SingleAllowedTypeExactlySingleItem,
                SubpagesContentTypeData.MultiAllowedTypesSingleItem,
                SubpagesContentTypeData.MultiAllowedTypesMultiItems,
            }
        };

        var contentTypes = new List<ContentTypeModel>
        {
            contentType,
            LinkedItemsContentTypeData.ArticleContentType,
            LinkedItemsContentTypeData.HeroContentType,
            SubpagesContentTypeData.HeroContentType,
            SubpagesContentTypeData.ArticleContentType
        };

        var codeGenerator = new ExtendedDeliveryCodeGenerator(mockOptions.Object, _outputProvider, _managementClient);

        var result = codeGenerator.GetClassCodeGenerators(contentType, new List<ContentTypeSnippetModel>(), contentTypes).ToList();

        var expectedTypedExtendedDeliveryClassDefinition = new ClassDefinition(contentType.Codename);
        expectedTypedExtendedDeliveryClassDefinition.Properties.AddRange(new List<Property>
        {
            Property.FromContentTypeElement(
                LinkedItemsContentTypeData.SingleAllowedTypeMultiItems,
                $"IEnumerable<{LinkedItemsContentTypeData.HeroContentType.Name}>",
                "ModularContentHeroes_Hero"),
            Property.FromContentTypeElement(LinkedItemsContentTypeData.SingleAllowedTypeExactlySingleItem, LinkedItemsContentTypeData.ArticleContentType.Name),
            Property.FromContentTypeElement(
                SubpagesContentTypeData.SingleAllowedTypeMultiItems,
                $"IEnumerable<{SubpagesContentTypeData.HeroContentType.Name}>",
                "SubpagesHeroes_Hero"),
            Property.FromContentTypeElement(SubpagesContentTypeData.SingleAllowedTypeExactlySingleItem, SubpagesContentTypeData.ArticleContentType.Name)
        });
        expectedTypedExtendedDeliveryClassDefinition.PropertyCodenameConstants.AddRange(new List<string>
        {
            "ModularContentHeroes_Hero",
            "modular_content_article",
            "SubpagesHeroes_Hero",
            "subpages_article"
        });

        var expectedExtendedDeliveryClassDefinition = new ClassDefinition(contentType.Codename);
        expectedExtendedDeliveryClassDefinition.Properties.AddRange(new List<Property>
        {
            Property.FromContentTypeElement(LinkedItemsContentTypeData.SingleAllowedTypeMultiItems, DefaultLinkedItemsType),
            Property.FromContentTypeElement(LinkedItemsContentTypeData.SingleAllowedTypeExactlySingleItem, DefaultLinkedItemsType),
            Property.FromContentTypeElement(LinkedItemsContentTypeData.MultiAllowedTypesSingleItem, DefaultLinkedItemsType),
            Property.FromContentTypeElement(LinkedItemsContentTypeData.MultiAllowedTypesMultiItems, DefaultLinkedItemsType),
            Property.FromContentTypeElement(SubpagesContentTypeData.SingleAllowedTypeMultiItems, DefaultLinkedItemsType),
            Property.FromContentTypeElement(SubpagesContentTypeData.SingleAllowedTypeExactlySingleItem, DefaultLinkedItemsType),
            Property.FromContentTypeElement(SubpagesContentTypeData.MultiAllowedTypesSingleItem, DefaultLinkedItemsType),
            Property.FromContentTypeElement(SubpagesContentTypeData.MultiAllowedTypesMultiItems, DefaultLinkedItemsType)
        });
        expectedExtendedDeliveryClassDefinition.PropertyCodenameConstants.AddRange(new List<string>
        {
            "modular_content_heroes",
            "modular_content_article",
            "modular_content_blog",
            "modular_content_coffees",
            "subpages_heroes",
            "subpages_article",
            "subpages_blog",
            "subpages_coffees"
        });

        var expected = new List<ClassCodeGenerator>
        {
            new TypedExtendedDeliveryClassCodeGenerator(expectedTypedExtendedDeliveryClassDefinition, "ContentType.Typed.Generated"),
            new ExtendedDeliveryClassCodeGenerator(expectedExtendedDeliveryClassDefinition, "ContentType.Generated")
        };

        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void GetClassCodeGenerators_ExtendedDeliverPreviewModelsIsTrue_Returns()
    {
        var mockOptions = new Mock<IOptions<CodeGeneratorOptions>>();
        mockOptions.SetupGet(option => option.Value).Returns(new CodeGeneratorOptions
        {
            ManagementApi = false,
            ExtendedDeliverModels = false,
            ExtendedDeliverPreviewModels = true
        });

        var contentType = new ContentTypeModel
        {
            Codename = "content_type",
            Elements = new List<ElementMetadataBase>
            {
                LinkedItemsContentTypeData.SingleAllowedTypeMultiItems,
                LinkedItemsContentTypeData.SingleAllowedTypeExactlySingleItem,
                LinkedItemsContentTypeData.MultiAllowedTypesSingleItem,
                LinkedItemsContentTypeData.MultiAllowedTypesMultiItems
            }
        };

        var contentTypes = new List<ContentTypeModel> { contentType, LinkedItemsContentTypeData.ArticleContentType, LinkedItemsContentTypeData.HeroContentType };

        var codeGenerator = new ExtendedDeliveryCodeGenerator(mockOptions.Object, _outputProvider, _managementClient);

        var result = codeGenerator.GetClassCodeGenerators(contentType, new List<ContentTypeSnippetModel>(), contentTypes).ToList();

        var expectedTypedExtendedDeliveryClassDefinition = new ClassDefinition(contentType.Codename);
        expectedTypedExtendedDeliveryClassDefinition.Properties.AddRange(new List<Property>
        {
            Property.FromContentTypeElement(
                LinkedItemsContentTypeData.SingleAllowedTypeMultiItems,
                $"IEnumerable<{ContentItemClassCodeGenerator.DefaultContentItemClassName}>",
                "ModularContentHeroes_Hero"),
            Property.FromContentTypeElement(
                LinkedItemsContentTypeData.SingleAllowedTypeExactlySingleItem,
                $"IEnumerable<{ContentItemClassCodeGenerator.DefaultContentItemClassName}>",
                "modular_content_article")
        });
        expectedTypedExtendedDeliveryClassDefinition.PropertyCodenameConstants.AddRange(new List<string>
        {
            "ModularContentHeroes_Hero",
            "modular_content_article"
        });

        var expectedExtendedDeliveryClassDefinition = new ClassDefinition(contentType.Codename);
        expectedExtendedDeliveryClassDefinition.Properties.AddRange(new List<Property>
        {
            Property.FromContentTypeElement(LinkedItemsContentTypeData.SingleAllowedTypeMultiItems, DefaultLinkedItemsType),
            Property.FromContentTypeElement(LinkedItemsContentTypeData.SingleAllowedTypeExactlySingleItem, DefaultLinkedItemsType),
            Property.FromContentTypeElement(LinkedItemsContentTypeData.MultiAllowedTypesSingleItem, DefaultLinkedItemsType),
            Property.FromContentTypeElement(LinkedItemsContentTypeData.MultiAllowedTypesMultiItems, DefaultLinkedItemsType)
        });
        expectedExtendedDeliveryClassDefinition.PropertyCodenameConstants.AddRange(new List<string>
        {
            "modular_content_heroes",
            "modular_content_article",
            "modular_content_blog",
            "modular_content_coffees"
        });

        var expected = new List<ClassCodeGenerator>
        {
            new TypedExtendedDeliveryClassCodeGenerator(expectedTypedExtendedDeliveryClassDefinition, "ContentType.Typed.Generated"),
            new ExtendedDeliveryClassCodeGenerator(expectedExtendedDeliveryClassDefinition, "ContentType.Generated")
        };

        result.Should().BeEquivalentTo(expected);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task IntegrationTest_RunAsync_CorrectFiles(bool extendedDeliverPreviewModels)
    {
        var mockOptions = new Mock<IOptions<CodeGeneratorOptions>>();
        mockOptions.Setup(x => x.Value).Returns(new CodeGeneratorOptions
        {
            ExtendedDeliverModels = true,
            Namespace = "CustomNamespace",
            OutputDir = TempDir,
            ManagementApi = false,
            GeneratePartials = false,
            WithTypeProvider = false,
            StructuredModel = false,
            ManagementOptions = new ManagementOptions { ApiKey = "apiKey", ProjectId = ProjectId },
            ExtendedDeliverPreviewModels = extendedDeliverPreviewModels
        });

        var codeGenerator = new ExtendedDeliveryCodeGenerator(mockOptions.Object, new FileSystemOutputProvider(mockOptions.Object), _managementClient);

        await codeGenerator.RunAsync();

        Directory.GetFiles(Path.GetFullPath(TempDir)).Length.Should().Be(NumberOfContentTypesWithDefaultContentItem);

        Directory.EnumerateFiles(Path.GetFullPath(TempDir), "*.Generated.cs").Should().NotBeEmpty();
        Directory.EnumerateFiles(Path.GetFullPath(TempDir)).Where(p => !p.Contains("*.Generated.cs")).Should().NotBeEmpty();

        // Cleanup
        Directory.Delete(TempDir, true);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task IntegrationTest_RunAsync_GeneratedSuffix_CorrectFiles(bool extendedDeliverPreviewModels)
    {
        const string transformFilename = "CustomSuffix";
        var mockOptions = new Mock<IOptions<CodeGeneratorOptions>>();
        mockOptions.Setup(x => x.Value).Returns(new CodeGeneratorOptions
        {
            ExtendedDeliverModels = true,
            Namespace = "CustomNamespace",
            OutputDir = TempDir,
            ManagementApi = false,
            GeneratePartials = false,
            WithTypeProvider = false,
            StructuredModel = false,
            ManagementOptions = new ManagementOptions { ApiKey = "apiKey", ProjectId = ProjectId },
            FileNameSuffix = transformFilename,
            ExtendedDeliverPreviewModels = extendedDeliverPreviewModels
        });

        var codeGenerator = new ExtendedDeliveryCodeGenerator(mockOptions.Object, new FileSystemOutputProvider(mockOptions.Object), _managementClient);

        await codeGenerator.RunAsync();

        Directory.GetFiles(Path.GetFullPath(TempDir)).Length.Should().Be(NumberOfContentTypesWithDefaultContentItem);

        foreach (var filepath in Directory.EnumerateFiles(Path.GetFullPath(TempDir)).Where(f => !f.Contains($"{ContentItemClassCodeGenerator.DefaultContentItemClassName}.cs")))
        {
            Path.GetFileName(filepath).Should().EndWith($".{transformFilename}.cs");
        }

        // Cleanup
        Directory.Delete(TempDir, true);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task IntegrationTest_RunAsync_GeneratePartials_CorrectFiles(bool extendedDeliverPreviewModels)
    {
        const string transformFilename = "Generated";
        var mockOptions = new Mock<IOptions<CodeGeneratorOptions>>();
        mockOptions.Setup(x => x.Value).Returns(new CodeGeneratorOptions
        {
            ExtendedDeliverModels = true,
            Namespace = "CustomNamespace",
            OutputDir = TempDir,
            ManagementApi = false,
            GeneratePartials = true,
            WithTypeProvider = false,
            StructuredModel = false,
            ManagementOptions = new ManagementOptions { ApiKey = "apiKey", ProjectId = ProjectId },
            FileNameSuffix = transformFilename,
            ExtendedDeliverPreviewModels = extendedDeliverPreviewModels
        });

        var codeGenerator = new ExtendedDeliveryCodeGenerator(mockOptions.Object, new FileSystemOutputProvider(mockOptions.Object), _managementClient);

        await codeGenerator.RunAsync();

        var allFilesCount = Directory.GetFiles(Path.GetFullPath(TempDir), "*.cs").Length;
        var generatedCount = Directory.GetFiles(Path.GetFullPath(TempDir), $"*.{transformFilename}.cs").Length;
        var result = generatedCount + (generatedCount / 2) + 1;

        result.Should().Be(allFilesCount);

        var defaultContentItemClassCodeGeneratorExists = File.Exists(Path.GetFullPath($"{TempDir}//{ContentItemClassCodeGenerator.DefaultContentItemClassName}.cs"));
        defaultContentItemClassCodeGeneratorExists.Should().BeTrue();


        foreach (var filepath in Directory.EnumerateFiles(Path.GetFullPath(TempDir))
            .Where(f =>
                !f.Contains($"{ContentItemClassCodeGenerator.DefaultContentItemClassName}.cs") &&
                !f.Contains($".{transformFilename}.cs") &&
                !f.Contains($"{ExtendedDeliveryCodeGenerator.TypedSuffixFileName}.{transformFilename}.cs")))
        {
            var generatedFileExists = File.Exists(filepath.Replace(".cs", $".{transformFilename}.cs"));
            var typedGeneratedFileExists = File.Exists(filepath.Replace(".cs", $"{ExtendedDeliveryCodeGenerator.TypedSuffixFileName}.{transformFilename}.cs"));

            generatedFileExists.Should().BeTrue();
            typedGeneratedFileExists.Should().BeTrue();
        }

        // Cleanup
        Directory.Delete(TempDir, true);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task IntegrationTest_RunAsync_TypeProvider_CorrectFiles(bool extendedDeliverPreviewModels)
    {
        var mockOptions = new Mock<IOptions<CodeGeneratorOptions>>();
        mockOptions.Setup(x => x.Value).Returns(new CodeGeneratorOptions
        {
            ExtendedDeliverModels = true,
            Namespace = "CustomNamespace",
            OutputDir = TempDir,
            ManagementApi = false,
            GeneratePartials = false,
            WithTypeProvider = true,
            StructuredModel = false,
            ManagementOptions = new ManagementOptions { ApiKey = "apiKey", ProjectId = ProjectId },
            ExtendedDeliverPreviewModels = extendedDeliverPreviewModels
        });

        var codeGenerator = new ExtendedDeliveryCodeGenerator(mockOptions.Object, new FileSystemOutputProvider(mockOptions.Object), _managementClient);

        await codeGenerator.RunAsync();

        Directory.GetFiles(Path.GetFullPath(TempDir)).Length.Should().Be(NumberOfContentTypesWithDefaultContentItem + 1);

        Directory.EnumerateFiles(Path.GetFullPath(TempDir), "*TypeProvider.cs").Should().NotBeEmpty();

        // Cleanup
        Directory.Delete(TempDir, true);
    }

    private Func<ExtendedDeliveryCodeGenerator> Creator(IOptions<CodeGeneratorOptions> options) =>
        () => new ExtendedDeliveryCodeGenerator(options, _outputProvider, _managementClient);
}