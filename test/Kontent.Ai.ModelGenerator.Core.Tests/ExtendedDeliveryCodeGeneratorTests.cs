using Kontent.Ai.Delivery.Abstractions;
using Kontent.Ai.Management;
using Kontent.Ai.Management.Configuration;
using Kontent.Ai.Management.Models.Shared;
using Kontent.Ai.Management.Models.Types;
using Kontent.Ai.Management.Models.Types.Elements;
using Kontent.Ai.Management.Models.TypeSnippets;
using Kontent.Ai.ModelGenerator.Core.Common;
using Kontent.Ai.ModelGenerator.Core.Configuration;
using Kontent.Ai.ModelGenerator.Core.Contract;
using Kontent.Ai.ModelGenerator.Core.Generators.Class;
using Kontent.Ai.ModelGenerator.Core.Helpers;
using Kontent.Ai.ModelGenerator.Core.Services;
using Kontent.Ai.ModelGenerator.Core.Tests.TestHelpers;
using Microsoft.Extensions.Options;
using Moq;

namespace Kontent.Ai.ModelGenerator.Core.Tests;

public class ExtendedDeliveryCodeGeneratorTests : CodeGeneratorTestsBase
{
    /// <summary>
    /// represents count of elements in 'management_types.json'
    /// </summary>
    private const int NumberOfContentTypesWithDefaultContentItem = (14 * 2);

    private readonly IManagementClient _managementClient;
    private readonly IOutputProvider _outputProvider;
    private readonly Mock<IOutputProvider> _outputProviderMock;
    private readonly Mock<IDeliveryElementService> _deliveryElementService;
    protected override string TempDir => Path.Combine(Path.GetTempPath(), "ExtendedDeliveryCodeGeneratorIntegrationTests");

    public ExtendedDeliveryCodeGeneratorTests()
    {
        _managementClient = CreateManagementClient();
        _outputProvider = new Mock<IOutputProvider>().Object;
        _deliveryElementService = new Mock<IDeliveryElementService>();
        _outputProviderMock = new Mock<IOutputProvider>();
    }

    [Fact]
    public void Constructor_ManagementIsTrue_Throws()
    {
        var mockOptions = new Mock<IOptions<CodeGeneratorOptions>>();
        mockOptions.SetupGet(option => option.Value).Returns(new CodeGeneratorOptions
        {
            ManagementApi = true,
            ExtendedDeliveryModels = true
        });

        Logger.VerifyNoOtherCalls();
        Creator(mockOptions.Object).Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Constructor_ExtendedDeliveryModelsIsFalse_Throws()
    {
        var mockOptions = new Mock<IOptions<CodeGeneratorOptions>>();
        mockOptions.SetupGet(option => option.Value).Returns(new CodeGeneratorOptions
        {
            ManagementApi = false,
            ExtendedDeliveryModels = false
        });

        Logger.VerifyNoOtherCalls();
        Creator(mockOptions.Object).Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Constructor_CreatesInstance()
    {
        var mockOptions = new Mock<IOptions<CodeGeneratorOptions>>();
        mockOptions.SetupGet(option => option.Value).Returns(new CodeGeneratorOptions
        {
            ManagementApi = false,
            ExtendedDeliveryModels = true
        });

        var extendedDeliveryCodeGenerator = new ExtendedDeliveryCodeGenerator(
            mockOptions.Object,
            _outputProvider,
            _managementClient,
            ClassCodeGeneratorFactory,
            ClassDefinitionFactory,
            _deliveryElementService.Object,
            Logger.Object);

        Logger.VerifyNoOtherCalls();
        extendedDeliveryCodeGenerator.Should().NotBeNull();
    }

    [Theory]
    [InlineData(StructuredModelFlags.ModularContent, true)]
    [InlineData(StructuredModelFlags.NotSet, false)]
    public void GetClassCodeGenerators_ExtendedDeliverPreviewModelsIsFalse_Returns(StructuredModelFlags structuredModel, bool generateStructuredModularContent)
    {
        var mockOptions = new Mock<IOptions<CodeGeneratorOptions>>();
        mockOptions.SetupGet(option => option.Value).Returns(new CodeGeneratorOptions
        {
            ManagementApi = false,
            ExtendedDeliveryModels = true,
            StructuredModel = structuredModel.ToString()
        });

        var contentType = new ContentTypeModel
        {
            Codename = "content_type",
            Elements = new List<ElementMetadataBase>
            {
                LinkedItemsContentTypeData.SingleAllowedTypeMultiItems,
                LinkedItemsContentTypeData.SingleAllowedTypeExactlySingleItem,
                LinkedItemsContentTypeData.SingleAllowedTypeAtMostSingleItem,
                LinkedItemsContentTypeData.MultiAllowedTypesExactlySingleItem,
                LinkedItemsContentTypeData.MultiAllowedTypesAtMostSingleItem,
                LinkedItemsContentTypeData.MultiAllowedTypesMultiItems,
                SubpagesContentTypeData.SingleAllowedTypeMultiItems,
                SubpagesContentTypeData.SingleAllowedTypeExactlySingleItem,
                SubpagesContentTypeData.SingleAllowedTypeAtMostSingleItem,
                SubpagesContentTypeData.MultiAllowedTypesExactlySingleItem,
                SubpagesContentTypeData.MultiAllowedTypesAtMostSingleItem,
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

        foreach (var elementMetadataBase in contentType.Elements)
        {
            _deliveryElementService.Setup(x => x.GetElementType(elementMetadataBase.Type.ToString())).Returns(elementMetadataBase.Type.ToString());
        }

        var codeGenerator = new ExtendedDeliveryCodeGenerator(
            mockOptions.Object,
            _outputProvider,
            _managementClient,
            ClassCodeGeneratorFactory,
            ClassDefinitionFactory,
            _deliveryElementService.Object,
            Logger.Object);

        var result = codeGenerator.GetClassCodeGenerators(contentType, new List<ContentTypeSnippetModel>(), contentTypes).ToList();

        var expectedTypedExtendedDeliveryClassDefinition = new ClassDefinition(contentType.Codename);
        expectedTypedExtendedDeliveryClassDefinition.Properties.AddRange(new List<Property>
        {
            Property.FromContentTypeElement(
                LinkedItemsContentTypeData.SingleAllowedTypeMultiItems,
                $"IEnumerable<{LinkedItemsContentTypeData.HeroContentType.Name}>",
                "Modular_Content_Heroes_Hero"),
            Property.FromContentTypeElement(LinkedItemsContentTypeData.SingleAllowedTypeExactlySingleItem, LinkedItemsContentTypeData.ArticleContentType.Name),
            Property.FromContentTypeElement(LinkedItemsContentTypeData.SingleAllowedTypeAtMostSingleItem, LinkedItemsContentTypeData.HeroContentType.Name),
            Property.FromContentTypeElement(
                SubpagesContentTypeData.SingleAllowedTypeMultiItems,
                $"IEnumerable<{SubpagesContentTypeData.HeroContentType.Name}>",
                "Subpages_Heroes_Hero"),
            Property.FromContentTypeElement(SubpagesContentTypeData.SingleAllowedTypeExactlySingleItem, SubpagesContentTypeData.ArticleContentType.Name),
            Property.FromContentTypeElement(SubpagesContentTypeData.SingleAllowedTypeAtMostSingleItem, SubpagesContentTypeData.HeroContentType.Name)
        });
        expectedTypedExtendedDeliveryClassDefinition.PropertyCodenameConstants.AddRange(new List<string>
        {
            "Modular_Content_Heroes_Hero",
            "modular_content_article",
            "modular_content_hero",
            "Subpages_Heroes_Hero",
            "subpages_article",
            "subpages_hero"
        });

        var expectedExtendedDeliveryClassDefinition = new ClassDefinition(contentType.Codename);
        expectedExtendedDeliveryClassDefinition.Properties.AddRange(new List<Property>
        {
            Property.FromContentTypeElement(LinkedItemsContentTypeData.SingleAllowedTypeMultiItems, DefaultLinkedItemsType(structuredModel)),
            Property.FromContentTypeElement(LinkedItemsContentTypeData.SingleAllowedTypeExactlySingleItem, DefaultLinkedItemsType(structuredModel)),
            Property.FromContentTypeElement(LinkedItemsContentTypeData.SingleAllowedTypeAtMostSingleItem, DefaultLinkedItemsType(structuredModel)),
            Property.FromContentTypeElement(LinkedItemsContentTypeData.MultiAllowedTypesExactlySingleItem, DefaultLinkedItemsType(structuredModel)),
            Property.FromContentTypeElement(LinkedItemsContentTypeData.MultiAllowedTypesAtMostSingleItem, DefaultLinkedItemsType(structuredModel)),
            Property.FromContentTypeElement(LinkedItemsContentTypeData.MultiAllowedTypesMultiItems, DefaultLinkedItemsType(structuredModel)),
            Property.FromContentTypeElement(SubpagesContentTypeData.SingleAllowedTypeMultiItems, DefaultLinkedItemsType(structuredModel)),
            Property.FromContentTypeElement(SubpagesContentTypeData.SingleAllowedTypeExactlySingleItem, DefaultLinkedItemsType(structuredModel)),
            Property.FromContentTypeElement(SubpagesContentTypeData.SingleAllowedTypeAtMostSingleItem, DefaultLinkedItemsType(structuredModel)),
            Property.FromContentTypeElement(SubpagesContentTypeData.MultiAllowedTypesExactlySingleItem, DefaultLinkedItemsType(structuredModel)),
            Property.FromContentTypeElement(SubpagesContentTypeData.MultiAllowedTypesAtMostSingleItem, DefaultLinkedItemsType(structuredModel)),
            Property.FromContentTypeElement(SubpagesContentTypeData.MultiAllowedTypesMultiItems, DefaultLinkedItemsType(structuredModel))
        });
        expectedExtendedDeliveryClassDefinition.PropertyCodenameConstants.AddRange(new List<string>
        {
            "modular_content_heroes",
            "modular_content_article",
            "modular_content_hero",
            "modular_content_blog",
            "modular_content_coffee",
            "modular_content_coffees",
            "subpages_heroes",
            "subpages_article",
            "subpages_hero",
            "subpages_blog",
            "subpages_coffee",
            "subpages_coffees"
        });

        var expected = new List<ClassCodeGenerator>
        {
            new TypedExtendedDeliveryClassCodeGenerator(expectedTypedExtendedDeliveryClassDefinition, "ContentType.Typed.Generated"),
            new ExtendedDeliveryClassCodeGenerator(expectedExtendedDeliveryClassDefinition, "ContentType.Generated", generateStructuredModularContent)
        };

        Logger.VerifyNoOtherCalls();
        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void GetClassCodeGenerators_InvalidIdentifier_MessageIsLogged()
    {
        var mockOptions = new Mock<IOptions<CodeGeneratorOptions>>();
        mockOptions.SetupGet(option => option.Value).Returns(new CodeGeneratorOptions
        {
            ManagementApi = false,
            ExtendedDeliveryModels = true,
        });

        var elementCodename = "";
        var contentType = new ContentTypeModel
        {
            Codename = "content_type",
            Elements = new List<ElementMetadataBase>
            {
                new LinkedItemsElementMetadataModel
                {
                    Name = "",
                    Codename = elementCodename,
                    AllowedTypes = new List<Reference>
                    {
                        Reference.ById(LinkedItemsContentTypeData.ArticleContentType.Id)
                    }
                }
            }
        };

        var contentTypes = new List<ContentTypeModel>
        {
            contentType,
            LinkedItemsContentTypeData.ArticleContentType
        };

        foreach (var elementMetadataBase in contentType.Elements)
        {
            _deliveryElementService.Setup(x => x.GetElementType(elementMetadataBase.Type.ToString())).Returns(elementMetadataBase.Type.ToString());
        }

        var codeGenerator = new ExtendedDeliveryCodeGenerator(
            mockOptions.Object,
            _outputProvider,
            _managementClient,
            ClassCodeGeneratorFactory,
            ClassDefinitionFactory,
            _deliveryElementService.Object,
            Logger.Object);

        var expected = new List<ClassCodeGenerator>
        {
            new TypedExtendedDeliveryClassCodeGenerator(new ClassDefinition(contentType.Codename), "ContentType.Typed.Generated"),
            new ExtendedDeliveryClassCodeGenerator(new ClassDefinition(contentType.Codename), "ContentType.Generated", false)
        };

        var result = codeGenerator.GetClassCodeGenerators(contentType, new List<ContentTypeSnippetModel>(), contentTypes).ToList();

        Logger.Verify(n => n.Log(It.Is<string>(m => m == $"Warning: Can't create valid C# Identifier from '{elementCodename}'. Skipping element.")),
            Times.Exactly(1));
        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void GetClassCodeGenerators_DuplicateProperty_MessageIsLogged()
    {
        var mockOptions = new Mock<IOptions<CodeGeneratorOptions>>();
        mockOptions.SetupGet(option => option.Value).Returns(new CodeGeneratorOptions
        {
            ManagementApi = false,
            ExtendedDeliveryModels = true,
        });

        var elementCodename = "duplicate_codename";
        var contentType = new ContentTypeModel
        {
            Name = "ContentType",
            Codename = "content_type",
            Elements = new List<ElementMetadataBase>
            {
                TestDataGenerator.GenerateElementMetadataBase(Guid.NewGuid(), elementCodename),
                TestDataGenerator.GenerateElementMetadataBase(Guid.NewGuid(), elementCodename)
            }
        };

        var contentTypes = new List<ContentTypeModel>
        {
            contentType
        };

        foreach (var elementMetadataBase in contentType.Elements)
        {
            _deliveryElementService.Setup(x => x.GetElementType(elementMetadataBase.Type.ToString())).Returns(elementMetadataBase.Type.ToString());
        }

        var codeGenerator = new ExtendedDeliveryCodeGenerator(
            mockOptions.Object,
            _outputProvider,
            _managementClient,
            ClassCodeGeneratorFactory,
            ClassDefinitionFactory,
            _deliveryElementService.Object,
            Logger.Object);

        var expectedClassDefinition = new ClassDefinition(contentType.Codename);
        expectedClassDefinition.AddProperty(new Property(elementCodename, "string"));
        expectedClassDefinition.AddPropertyCodenameConstant(elementCodename);
        var expected = new List<ClassCodeGenerator>
        {
            new ExtendedDeliveryClassCodeGenerator(expectedClassDefinition, "ContentType.Generated", false),
            new TypedExtendedDeliveryClassCodeGenerator( new ClassDefinition(contentType.Codename), "ContentType.Typed.Generated")
        };

        var result = codeGenerator.GetClassCodeGenerators(contentType, new List<ContentTypeSnippetModel>(), contentTypes).ToList();

        Logger.Verify(n => n.Log(It.Is<string>(m => m == $"Warning: Element '{elementCodename}' is already present in Content Type '{contentType.Name}'.")),
            Times.Exactly(1));
        result.Should().BeEquivalentTo(expected);
    }

    [Theory]
    [InlineData(true, "BaseClass")]
    [InlineData(true, null)]
    [InlineData(false, "BaseClass")]
    [InlineData(false, null)]
    public async Task RunAsync_NoContentTypes_MessageIsLogged(bool withTypeProvider, string baseClass)
    {
        var projectId = Guid.NewGuid().ToString();
        var mockOptions = new Mock<IOptions<CodeGeneratorOptions>>();
        mockOptions.SetupGet(option => option.Value).Returns(new CodeGeneratorOptions
        {
            ManagementApi = false,
            ExtendedDeliveryModels = true,
            WithTypeProvider = withTypeProvider,
            BaseClass = baseClass,
            ManagementOptions = new ManagementOptions
            {
                ProjectId = projectId,
                ApiKey = "api_key"
            }
        });

        var contentTypeListingResponseModel = new Mock<IListingResponseModel<ContentTypeModel>>();
        contentTypeListingResponseModel.As<IEnumerable<ContentTypeModel>>()
        .Setup(c => c.GetEnumerator())
            .Returns(new List<ContentTypeModel>().GetEnumerator);

        var contentTypeSnippetListingResponseModel = new Mock<IListingResponseModel<ContentTypeSnippetModel>>();
        contentTypeSnippetListingResponseModel.As<IEnumerable<ContentTypeSnippetModel>>()
        .Setup(c => c.GetEnumerator())
            .Returns(new List<ContentTypeSnippetModel>().GetEnumerator);

        var managementClient = new Mock<IManagementClient>();
        managementClient
            .Setup(x => x.ListContentTypesAsync())
            .Returns(Task.FromResult(contentTypeListingResponseModel.Object));
        managementClient
            .Setup(x => x.ListContentTypeSnippetsAsync())
            .Returns(Task.FromResult(contentTypeSnippetListingResponseModel.Object));

        var codeGenerator = new ExtendedDeliveryCodeGenerator(
            mockOptions.Object,
            _outputProviderMock.Object,
            managementClient.Object,
            ClassCodeGeneratorFactory,
            ClassDefinitionFactory,
            _deliveryElementService.Object,
            Logger.Object);

        var result = await codeGenerator.RunAsync();

        Logger.Verify(n => n.Log(It.Is<string>(m => m == $"No content type available for the project ({projectId}). Please make sure you have the Delivery API enabled at https://app.kontent.ai/.")),
            Times.Exactly(1));
        result.Should().Be(0);
    }

    [Fact]
    public async Task RunAsync_ContentTypeHasInvalidIdentifier_MessageIsLogged()
    {
        var mockOptions = new Mock<IOptions<CodeGeneratorOptions>>();
        mockOptions.Setup(x => x.Value).Returns(new CodeGeneratorOptions
        {
            ExtendedDeliveryModels = true,
            Namespace = "CustomNamespace",
            OutputDir = TempDir,
            ManagementApi = false,
            GeneratePartials = false,
            WithTypeProvider = false,
            ManagementOptions = new ManagementOptions { ApiKey = "apiKey", ProjectId = ProjectId }
        });

        var contentType = new ContentTypeModel
        {
            Codename = "",
            Elements = new List<ElementMetadataBase>()
        };
        var contentTypes = new List<ContentTypeModel>
        {
            contentType
        };
        var contentTypeListingResponseModel = new Mock<IListingResponseModel<ContentTypeModel>>();
        contentTypeListingResponseModel.As<IEnumerable<ContentTypeModel>>()
            .Setup(c => c.GetEnumerator())
            .Returns(contentTypes.GetEnumerator);

        var contentTypeSnippetListingResponseModel = new Mock<IListingResponseModel<ContentTypeSnippetModel>>();
        contentTypeSnippetListingResponseModel.As<IEnumerable<ContentTypeSnippetModel>>()
            .Setup(c => c.GetEnumerator())
            .Returns(new List<ContentTypeSnippetModel>().GetEnumerator);

        var managementClient = new Mock<IManagementClient>();
        managementClient
            .Setup(x => x.ListContentTypesAsync())
            .Returns(Task.FromResult(contentTypeListingResponseModel.Object));
        managementClient
            .Setup(x => x.ListContentTypeSnippetsAsync())
            .Returns(Task.FromResult(contentTypeSnippetListingResponseModel.Object));

        var classDefinitionFactory = new Mock<IClassDefinitionFactory>();
        classDefinitionFactory
            .Setup(x => x.CreateClassDefinition(It.IsAny<string>()))
            .Returns(new ClassDefinition(contentType.Codename));

        var codeGenerator = new ExtendedDeliveryCodeGenerator(
            mockOptions.Object,
            _outputProviderMock.Object,
            managementClient.Object,
            ClassCodeGeneratorFactory,
            classDefinitionFactory.Object,
            _deliveryElementService.Object,
            Logger.Object);

        var result = await codeGenerator.RunAsync();

        Logger.Verify(n => n.Log(It.Is<string>(m => m.Contains($"Warning: Skipping Content Type '{contentType.Codename}'. Can't create valid C# identifier from its name."))),
            Times.Exactly(1));
        result.Should().Be(0);
    }

    [Fact]
    public async Task RunAsync_DeliveryElementServiceThrowsException_MessageIsLogged()
    {
        var mockOptions = new Mock<IOptions<CodeGeneratorOptions>>();
        mockOptions.Setup(x => x.Value).Returns(new CodeGeneratorOptions
        {
            ExtendedDeliveryModels = true,
            Namespace = "CustomNamespace",
            OutputDir = TempDir,
            ManagementApi = false,
            GeneratePartials = false,
            WithTypeProvider = false,
            ManagementOptions = new ManagementOptions { ApiKey = "apiKey", ProjectId = ProjectId }
        });

        _deliveryElementService.Setup(x => x.GetElementType(It.IsAny<string>())).Throws<ArgumentNullException>();

        var codeGenerator = new ExtendedDeliveryCodeGenerator(
            mockOptions.Object,
            _outputProviderMock.Object,
            _managementClient,
            ClassCodeGeneratorFactory,
            ClassDefinitionFactory,
            _deliveryElementService.Object,
            Logger.Object);

        Logger.Setup(n => n.Log(It.Is<string>(m => m.Contains("Warning: Skipping unknown Content Element type "))));

        var result = await codeGenerator.RunAsync();

        Logger.VerifyAll();
        result.Should().Be(0);
    }

    [Theory]
    [InlineData(StructuredModelFlags.ModularContent)]
    [InlineData(StructuredModelFlags.NotSet)]
    public async Task IntegrationTest_RunAsync_CorrectFiles(StructuredModelFlags structuredModel)
    {
        var mockOptions = new Mock<IOptions<CodeGeneratorOptions>>();
        mockOptions.Setup(x => x.Value).Returns(new CodeGeneratorOptions
        {
            ExtendedDeliveryModels = true,
            Namespace = "CustomNamespace",
            OutputDir = TempDir,
            ManagementApi = false,
            GeneratePartials = false,
            WithTypeProvider = false,
            StructuredModel = structuredModel.ToString(),
            ManagementOptions = new ManagementOptions { ApiKey = "apiKey", ProjectId = ProjectId }
        });

        var codeGenerator = new ExtendedDeliveryCodeGenerator(
            mockOptions.Object,
            new FileSystemOutputProvider(mockOptions.Object),
            _managementClient,
            ClassCodeGeneratorFactory,
            ClassDefinitionFactory,
            new DeliveryElementService(mockOptions.Object),
            Logger.Object);

        await codeGenerator.RunAsync();

        Directory.GetFiles(Path.GetFullPath(TempDir)).Length.Should().Be(NumberOfContentTypesWithDefaultContentItem);

        Directory.EnumerateFiles(Path.GetFullPath(TempDir), "*.Generated.cs").Should().NotBeEmpty();
        Directory.EnumerateFiles(Path.GetFullPath(TempDir)).Where(p => !p.Contains("*.Generated.cs")).Should().NotBeEmpty();

        Logger.Verify(a =>
            a.Log(It.Is<string>(m => m == $"{NumberOfContentTypesWithDefaultContentItem} content type models were successfully created.")),
            Times.Exactly(1));

        // Cleanup
        Directory.Delete(TempDir, true);
    }

    [Theory]
    [InlineData(StructuredModelFlags.ModularContent)]
    [InlineData(StructuredModelFlags.NotSet)]
    public async Task IntegrationTest_RunAsync_GeneratedSuffix_CorrectFiles(StructuredModelFlags structuredModel)
    {
        const string transformFilename = "CustomSuffix";
        var mockOptions = new Mock<IOptions<CodeGeneratorOptions>>();
        mockOptions.Setup(x => x.Value).Returns(new CodeGeneratorOptions
        {
            ExtendedDeliveryModels = true,
            Namespace = "CustomNamespace",
            OutputDir = TempDir,
            ManagementApi = false,
            GeneratePartials = false,
            WithTypeProvider = false,
            StructuredModel = structuredModel.ToString(),
            ManagementOptions = new ManagementOptions { ApiKey = "apiKey", ProjectId = ProjectId },
            FileNameSuffix = transformFilename
        });

        var codeGenerator = new ExtendedDeliveryCodeGenerator(
            mockOptions.Object,
            new FileSystemOutputProvider(mockOptions.Object),
            _managementClient,
            ClassCodeGeneratorFactory,
            ClassDefinitionFactory,
            new DeliveryElementService(mockOptions.Object),
            Logger.Object);

        await codeGenerator.RunAsync();

        Directory.GetFiles(Path.GetFullPath(TempDir)).Length.Should().Be(NumberOfContentTypesWithDefaultContentItem);

        foreach (var filepath in Directory.EnumerateFiles(Path.GetFullPath(TempDir)))
        {
            Path.GetFileName(filepath).Should().EndWith($".{transformFilename}.cs");
        }

        Logger.Verify(a =>
            a.Log(It.Is<string>(m => m == $"{NumberOfContentTypesWithDefaultContentItem} content type models were successfully created.")),
            Times.Exactly(1));

        // Cleanup
        Directory.Delete(TempDir, true);
    }

    [Theory]
    [InlineData(StructuredModelFlags.ModularContent)]
    [InlineData(StructuredModelFlags.NotSet)]
    public async Task IntegrationTest_RunAsync_GeneratePartials_CorrectFiles(StructuredModelFlags structuredModel)
    {
        const string transformFilename = "Generated";
        var mockOptions = new Mock<IOptions<CodeGeneratorOptions>>();
        mockOptions.Setup(x => x.Value).Returns(new CodeGeneratorOptions
        {
            ExtendedDeliveryModels = true,
            Namespace = "CustomNamespace",
            OutputDir = TempDir,
            ManagementApi = false,
            GeneratePartials = true,
            WithTypeProvider = false,
            StructuredModel = structuredModel.ToString(),
            ManagementOptions = new ManagementOptions { ApiKey = "apiKey", ProjectId = ProjectId },
            FileNameSuffix = transformFilename
        });

        var codeGenerator = new ExtendedDeliveryCodeGenerator(
            mockOptions.Object,
            new FileSystemOutputProvider(mockOptions.Object),
            _managementClient,
            ClassCodeGeneratorFactory,
            ClassDefinitionFactory,
            new DeliveryElementService(mockOptions.Object),
            Logger.Object);

        await codeGenerator.RunAsync();

        var allFilesCount = Directory.GetFiles(Path.GetFullPath(TempDir), "*.cs").Length;
        var generatedCount = Directory.GetFiles(Path.GetFullPath(TempDir), $"*.{transformFilename}.cs").Length;
        var result = generatedCount + (generatedCount / 2);

        result.Should().Be(allFilesCount);

        foreach (var filepath in Directory.EnumerateFiles(Path.GetFullPath(TempDir))
            .Where(f =>
                !f.Contains($".{transformFilename}.cs") &&
                !f.Contains($"{ExtendedDeliveryCodeGenerator.TypedSuffixFileName}.{transformFilename}.cs")))
        {
            var generatedFileExists = File.Exists(filepath.Replace(".cs", $".{transformFilename}.cs"));
            var typedGeneratedFileExists = File.Exists(filepath.Replace(".cs", $"{ExtendedDeliveryCodeGenerator.TypedSuffixFileName}.{transformFilename}.cs"));

            generatedFileExists.Should().BeTrue();
            typedGeneratedFileExists.Should().BeTrue();
        }

        Logger.Verify(a =>
            a.Log(It.Is<string>(m => m == $"{allFilesCount} content type models were successfully created.")),
            Times.Exactly(1));

        // Cleanup
        Directory.Delete(TempDir, true);
    }

    [Theory]
    [InlineData(StructuredModelFlags.ModularContent)]
    [InlineData(StructuredModelFlags.NotSet)]
    public async Task IntegrationTest_RunAsync_TypeProvider_CorrectFiles(StructuredModelFlags structuredModel)
    {
        var mockOptions = new Mock<IOptions<CodeGeneratorOptions>>();
        mockOptions.Setup(x => x.Value).Returns(new CodeGeneratorOptions
        {
            ExtendedDeliveryModels = true,
            Namespace = "CustomNamespace",
            OutputDir = TempDir,
            ManagementApi = false,
            GeneratePartials = false,
            WithTypeProvider = true,
            StructuredModel = structuredModel.ToString(),
            ManagementOptions = new ManagementOptions { ApiKey = "apiKey", ProjectId = ProjectId },
        });

        var codeGenerator = new ExtendedDeliveryCodeGenerator(
            mockOptions.Object,
            new FileSystemOutputProvider(mockOptions.Object),
            _managementClient,
            ClassCodeGeneratorFactory,
            ClassDefinitionFactory,
            new DeliveryElementService(mockOptions.Object),
            Logger.Object);

        await codeGenerator.RunAsync();

        var generatedFilesWithoutTypeProvider = Directory.GetFiles(Path.GetFullPath(TempDir)).Where(f => !f.EndsWith("TypeProvider.cs")).ToList();

        generatedFilesWithoutTypeProvider.Count.Should().Be(NumberOfContentTypesWithDefaultContentItem);

        Directory.EnumerateFiles(Path.GetFullPath(TempDir), "*TypeProvider.cs").Should().NotBeEmpty();

        Logger.Verify(a =>
            a.Log(It.Is<string>(m => m == $"{NumberOfContentTypesWithDefaultContentItem} content type models were successfully created.")),
            Times.Exactly(1));

        // Cleanup
        Directory.Delete(TempDir, true);
    }

    private Func<ExtendedDeliveryCodeGenerator> Creator(IOptions<CodeGeneratorOptions> options) =>
        () => new ExtendedDeliveryCodeGenerator(options, _outputProvider, _managementClient, ClassCodeGeneratorFactory, ClassDefinitionFactory, _deliveryElementService.Object, Logger.Object);

    private string DefaultLinkedItemsType(StructuredModelFlags structuredModel) =>
        TextHelpers.GetEnumerableType(
            structuredModel.HasFlag(StructuredModelFlags.ModularContent)
                ? nameof(IContentItem)
                : Property.ObjectType
        );
}