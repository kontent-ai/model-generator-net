using Kontent.Ai.Delivery;
using Kontent.Ai.Delivery.Abstractions;
using Kontent.Ai.Delivery.Builders.DeliveryClient;
using Kontent.Ai.ModelGenerator.Core.Common;
using Kontent.Ai.ModelGenerator.Core.Configuration;
using Kontent.Ai.ModelGenerator.Core.Contract;
using Kontent.Ai.ModelGenerator.Core.Generators.Class;
using Kontent.Ai.ModelGenerator.Core.Services;
using Microsoft.Extensions.Options;
using Moq;
using RichardSzalay.MockHttp;

namespace Kontent.Ai.ModelGenerator.Core.Tests;

public class DeliveryCodeGeneratorTests : CodeGeneratorTestsBase
{
    /// <summary>
    /// represents count of elements in 'delivery_types.json'
    /// </summary>
    private const int NumberOfContentTypes = 13;
    protected override string TempDir => Path.Combine(Path.GetTempPath(), "DeliveryCodeGeneratorIntegrationTests");

    private readonly Mock<IDeliveryElementService> _deliveryElementService;
    private readonly Mock<IDeliveryClient> _deliveryClientMock;
    private readonly Mock<IOutputProvider> _outputProviderMock;
    private readonly IDeliveryClient _deliveryClient;

    public DeliveryCodeGeneratorTests()
    {
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When("https://deliver.kontent.ai/*")
            .Respond("application/json", File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures/delivery_types.json")));
        var httpClient = mockHttp.ToHttpClient();
        _deliveryClient = DeliveryClientBuilder.WithEnvironmentId(EnvironmentId).WithDeliveryHttpClient(new DeliveryHttpClient(httpClient)).Build();

        _deliveryClientMock = new Mock<IDeliveryClient>();
        _outputProviderMock = new Mock<IOutputProvider>();
        _deliveryElementService = new Mock<IDeliveryElementService>();
    }

    [Fact]
    public void Constructor_ManagementIsTrue_Throws()
    {
        var mockOptions = new Mock<IOptions<CodeGeneratorOptions>>();
        mockOptions.SetupGet(option => option.Value).Returns(new CodeGeneratorOptions
        {
            ManagementApi = true
        });

        var call = () => new DeliveryCodeGenerator(
            mockOptions.Object,
            _outputProviderMock.Object,
            _deliveryClientMock.Object,
            ClassCodeGeneratorFactory,
            ClassDefinitionFactory,
            _deliveryElementService.Object,
            Logger.Object);

        Logger.VerifyNoOtherCalls();
        call.Should().ThrowExactly<InvalidOperationException>();
    }

    [Theory]
    [InlineData(StructuredModelFlags.DateTime)]
    [InlineData(StructuredModelFlags.RichText)]
    [InlineData(StructuredModelFlags.True)]
    [InlineData(StructuredModelFlags.ModularContent)]
    [InlineData(StructuredModelFlags.RichText | StructuredModelFlags.DateTime | StructuredModelFlags.True | StructuredModelFlags.ModularContent)]
    [InlineData(StructuredModelFlags.NotSet)]
    public void GetClassCodeGenerator_Returns(StructuredModelFlags structuredModel)
    {
        var mockOptions = new Mock<IOptions<CodeGeneratorOptions>>();
        mockOptions.SetupGet(option => option.Value).Returns(new CodeGeneratorOptions
        {
            ManagementApi = false,
            StructuredModel = structuredModel.ToString()
        });

        var elementCodename = "element_codename";
        var elementType = "text";
        var contentElement = new Mock<IContentElement>();
        contentElement.SetupGet(element => element.Type).Returns(elementType);
        contentElement.SetupGet(element => element.Codename).Returns(elementCodename);

        var contentType = new Mock<IContentType>();
        var contentTypeCodename = "Contenttype";
        contentType.SetupGet(type => type.System.Codename).Returns(contentTypeCodename);
        contentType.SetupGet(type => type.Elements).Returns(new Dictionary<string, IContentElement> { { elementCodename, contentElement.Object } });

        _deliveryElementService.Setup(x => x.GetElementType(elementType)).Returns(elementType);

        var codeGenerator = new DeliveryCodeGenerator(
            mockOptions.Object,
            _outputProviderMock.Object,
            _deliveryClientMock.Object,
            ClassCodeGeneratorFactory,
            ClassDefinitionFactory,
            _deliveryElementService.Object,
            Logger.Object);

        var result = codeGenerator.GetClassCodeGenerator(contentType.Object);

        Logger.VerifyNoOtherCalls();
        result.ClassFilename.Should().Be($"{contentTypeCodename}.Generated");
    }

    [Theory]
    [InlineData("CustomNamespace", "CustomNamespace")]
    [InlineData(null, ClassCodeGenerator.DefaultNamespace)]
    public void GetClassCodeGenerator_CustomNamespace_Returns(string customNamespace, string expectedNamespace)
    {
        var mockOptions = new Mock<IOptions<CodeGeneratorOptions>>();
        mockOptions.SetupGet(option => option.Value).Returns(new CodeGeneratorOptions
        {
            ManagementApi = false,
            Namespace = customNamespace
        });

        var elementCodename = "element_codename";
        var elementType = "text";
        var contentElement = new Mock<IContentElement>();
        contentElement.SetupGet(element => element.Type).Returns(elementType);
        contentElement.SetupGet(element => element.Codename).Returns(elementCodename);

        var contentType = new Mock<IContentType>();
        var contentTypeCodename = "Contenttype";
        contentType.SetupGet(type => type.System.Codename).Returns(contentTypeCodename);
        contentType.SetupGet(type => type.Elements).Returns(new Dictionary<string, IContentElement> { { elementCodename, contentElement.Object } });

        _deliveryElementService.Setup(x => x.GetElementType(elementType)).Returns(elementType);

        var codeGenerator = new DeliveryCodeGenerator(
            mockOptions.Object,
            _outputProviderMock.Object,
            _deliveryClientMock.Object,
            ClassCodeGeneratorFactory,
            ClassDefinitionFactory,
            _deliveryElementService.Object,
            Logger.Object);

        var result = codeGenerator.GetClassCodeGenerator(contentType.Object);

        Logger.VerifyNoOtherCalls();
        result.ClassFilename.Should().Be($"{contentTypeCodename}.Generated");
        result.Namespace.Should().Be(expectedNamespace);
    }

    [Fact]
    public void GetClassCodeGenerator_DuplicateSystemProperty_MessageIsLogged()
    {
        var mockOptions = new Mock<IOptions<CodeGeneratorOptions>>();
        mockOptions.SetupGet(option => option.Value).Returns(new CodeGeneratorOptions
        {
            ManagementApi = false
        });

        var contentTypeCodename = "Contenttype";
        var classDefinition = new ClassDefinition(contentTypeCodename);
        classDefinition.AddSystemProperty();

        var contentType = new Mock<IContentType>();
        contentType.SetupGet(type => type.System.Codename).Returns(contentTypeCodename);
        contentType.SetupGet(type => type.Elements).Returns(new Dictionary<string, IContentElement>());

        var classDefinitionFactoryMock = new Mock<IClassDefinitionFactory>();
        classDefinitionFactoryMock
            .Setup(x => x.CreateClassDefinition(It.IsAny<string>()))
            .Returns(classDefinition);

        var codeGenerator = new DeliveryCodeGenerator(
            mockOptions.Object,
            _outputProviderMock.Object,
            _deliveryClientMock.Object,
            ClassCodeGeneratorFactory,
            classDefinitionFactoryMock.Object,
            _deliveryElementService.Object,
            Logger.Object);

        var result = codeGenerator.GetClassCodeGenerator(contentType.Object);

        Logger.Verify(n => n.LogWarning(It.Is<string>(m => m == $"Can't add 'System' property. It's in collision with existing element in Content Type '{contentTypeCodename}'.")),
            Times.Once());
        result.ClassFilename.Should().Be($"{contentTypeCodename}.Generated");
    }

    [Theory]
    [InlineData(true, "BaseClass")]
    [InlineData(false, "BaseClass")]
    [InlineData(true, null)]
    [InlineData(false, null)]
    public async Task RunAsync_NoContentTypes_MessageIsLogged(bool withTypeProvider, string baseClass)
    {
        var environmentId = Guid.NewGuid().ToString();
        var mockOptions = new Mock<IOptions<CodeGeneratorOptions>>();
        mockOptions.SetupGet(option => option.Value).Returns(new CodeGeneratorOptions
        {
            ManagementApi = false,
            WithTypeProvider = withTypeProvider,
            BaseClass = baseClass,
            DeliveryOptions = new DeliveryOptions
            {
                EnvironmentId = environmentId
            }
        });

        var responseModelMock = new Mock<IDeliveryTypeListingResponse>();
        responseModelMock
            .Setup(x => x.Types)
            .Returns([]);


        _deliveryClientMock
            .Setup(x => x.GetTypesAsync(It.IsAny<IEnumerable<IQueryParameter>>()))
            .Returns(Task.FromResult(responseModelMock.Object));

        var codeGenerator = new DeliveryCodeGenerator(
            mockOptions.Object,
            _outputProviderMock.Object,
            _deliveryClientMock.Object,
            ClassCodeGeneratorFactory,
            ClassDefinitionFactory,
            _deliveryElementService.Object,
            Logger.Object);

        var result = await codeGenerator.RunAsync();

        Logger.Verify(
            n => n.LogInfo(It.Is<string>(m => m == $"No content type available for the environment ({environmentId}). Check the environment and project settings at https://app.kontent.ai/.")),
            Times.Once());
        result.Should().Be(0);
    }

    [Fact]
    public async Task RunAsync_TypesIsNull_MessageIsLogged()
    {
        var environmentId = Guid.NewGuid().ToString();
        var mockOptions = new Mock<IOptions<CodeGeneratorOptions>>();
        mockOptions.SetupGet(option => option.Value).Returns(new CodeGeneratorOptions
        {
            ManagementApi = false,
            DeliveryOptions = new DeliveryOptions
            {
                EnvironmentId = environmentId
            }
        });

        var responseModelMock = new Mock<IDeliveryTypeListingResponse>();
        responseModelMock
            .Setup(x => x.Types)
            .Returns((IList<IContentType>)null);

        _deliveryClientMock
            .Setup(x => x.GetTypesAsync(It.IsAny<IEnumerable<IQueryParameter>>()))
            .Returns(Task.FromResult(responseModelMock.Object));

        var codeGenerator = new DeliveryCodeGenerator(
            mockOptions.Object,
            _outputProviderMock.Object,
            _deliveryClientMock.Object,
            ClassCodeGeneratorFactory,
            ClassDefinitionFactory,
            _deliveryElementService.Object,
            Logger.Object);

        var result = await codeGenerator.RunAsync();

        Logger.Verify(n => n.LogInfo(It.Is<string>(m => m == $"No content type available for the environment ({environmentId}). Check the environment and project settings at https://app.kontent.ai/.")),
            Times.Once());
        result.Should().Be(0);
    }

    [Fact]
    public async Task RunAsync_DeliveryElementServiceThrowsException_MessageIsLogged()
    {
        var environmentId = Guid.NewGuid().ToString();
        var mockOptions = new Mock<IOptions<CodeGeneratorOptions>>();
        mockOptions.SetupGet(option => option.Value).Returns(new CodeGeneratorOptions
        {
            ManagementApi = false,
            DeliveryOptions = new DeliveryOptions
            {
                EnvironmentId = environmentId
            }
        });

        var responseModelMock = new Mock<IDeliveryTypeListingResponse>();
        responseModelMock
            .Setup(x => x.Types)
            .Returns((IList<IContentType>)null);

        _deliveryElementService.Setup(x => x.GetElementType(It.IsAny<string>())).Throws<ArgumentNullException>();

        var codeGenerator = new DeliveryCodeGenerator(
            mockOptions.Object,
            _outputProviderMock.Object,
            _deliveryClient,
            ClassCodeGeneratorFactory,
            ClassDefinitionFactory,
            _deliveryElementService.Object,
            Logger.Object);

        Logger.Setup(n => n.LogWarning(It.Is<string>(m => m.Contains("Skipping unknown Content Element type "))));
        Logger.Setup(n => n.LogInfo(It.Is<string>(m => m == "26 content type models were successfully created.")));

        var result = await codeGenerator.RunAsync();

        Logger.VerifyAll();
        result.Should().Be(0);
    }

    [Fact]
    public async Task RunAsync_ContentTypeHasInvalidIdentifier_MessageIsLogged()
    {
        var environmentId = Guid.NewGuid().ToString();
        var mockOptions = new Mock<IOptions<CodeGeneratorOptions>>();
        mockOptions.SetupGet(option => option.Value).Returns(new CodeGeneratorOptions
        {
            ManagementApi = false,
            WithTypeProvider = false,
            DeliveryOptions = new DeliveryOptions
            {
                EnvironmentId = environmentId
            }
        });

        var contentTypeCodename = "";
        var classDefinition = new ClassDefinition(contentTypeCodename);

        var contentType = new Mock<IContentType>();
        contentType.SetupGet(type => type.System.Codename).Returns(contentTypeCodename);
        contentType.SetupGet(type => type.Elements).Returns(new Dictionary<string, IContentElement>());

        var responseModelMock = new Mock<IDeliveryTypeListingResponse>();
        responseModelMock
            .Setup(x => x.Types)
            .Returns([contentType.Object]);

        _deliveryClientMock
            .Setup(x => x.GetTypesAsync(It.IsAny<IEnumerable<IQueryParameter>>()))
            .Returns(Task.FromResult(responseModelMock.Object));

        var classDefinitionFactory = new Mock<IClassDefinitionFactory>();
        classDefinitionFactory
            .Setup(x => x.CreateClassDefinition(It.IsAny<string>()))
            .Returns(classDefinition);

        var codeGenerator = new DeliveryCodeGenerator(
            mockOptions.Object,
            _outputProviderMock.Object,
            _deliveryClientMock.Object,
            ClassCodeGeneratorFactory,
            classDefinitionFactory.Object,
            _deliveryElementService.Object,
            Logger.Object);

        var result = await codeGenerator.RunAsync();

        Logger.Verify(n => n.LogWarning(It.Is<string>(m => m.Contains($"Skipping Content Type '{contentTypeCodename}'. Can't create valid C# identifier from its name."))),
            Times.Once());
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
            DeliveryOptions = new DeliveryOptions { EnvironmentId = EnvironmentId },
            Namespace = "CustomNamespace",
            OutputDir = TempDir,
            ManagementApi = false,
            GeneratePartials = false,
            WithTypeProvider = false,
            StructuredModel = structuredModel.ToString()
        });

        var codeGenerator = new DeliveryCodeGenerator(
            mockOptions.Object,
            new FileSystemOutputProvider(mockOptions.Object),
            _deliveryClient,
            ClassCodeGeneratorFactory,
            ClassDefinitionFactory,
            new DeliveryElementService(mockOptions.Object),
            Logger.Object);

        await codeGenerator.RunAsync();

        Directory.GetFiles(Path.GetFullPath(TempDir)).Length.Should().Be(NumberOfContentTypes);

        Directory.EnumerateFiles(Path.GetFullPath(TempDir), "*.Generated.cs").Should().NotBeEmpty();
        Directory.EnumerateFiles(Path.GetFullPath(TempDir)).Where(p => !p.Contains("*.Generated.cs")).Should().NotBeEmpty();
        Directory.EnumerateFiles(Path.GetFullPath(TempDir), "*TypeProvider.cs").Should().BeEmpty();

        Logger.Verify(a => a.LogInfo(It.Is<string>(m => m == $"{NumberOfContentTypes} content type models were successfully created.")),
            Times.Once());

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
            DeliveryOptions = new DeliveryOptions { EnvironmentId = EnvironmentId },
            Namespace = "CustomNamespace",
            OutputDir = TempDir,
            GeneratePartials = false,
            StructuredModel = structuredModel.ToString(),
            WithTypeProvider = false,
            FileNameSuffix = transformFilename,
            ManagementApi = false
        });

        var codeGenerator = new DeliveryCodeGenerator(
            mockOptions.Object,
            new FileSystemOutputProvider(mockOptions.Object),
            _deliveryClient,
            ClassCodeGeneratorFactory,
            ClassDefinitionFactory,
            new DeliveryElementService(mockOptions.Object),
            Logger.Object);

        await codeGenerator.RunAsync();

        Directory.GetFiles(Path.GetFullPath(TempDir)).Length.Should().Be(NumberOfContentTypes);

        foreach (var filepath in Directory.EnumerateFiles(Path.GetFullPath(TempDir)))
        {
            Path.GetFileName(filepath).Should().EndWith($".{transformFilename}.cs");
        }

        Logger.Verify(a => a.LogInfo(It.Is<string>(m => m == $"{NumberOfContentTypes} content type models were successfully created.")),
            Times.Once());

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
            DeliveryOptions = new DeliveryOptions { EnvironmentId = EnvironmentId },
            Namespace = "CustomNamespace",
            OutputDir = TempDir,
            FileNameSuffix = transformFilename,
            GeneratePartials = true,
            WithTypeProvider = false,
            StructuredModel = structuredModel.ToString(),
            ManagementApi = false
        });

        var codeGenerator = new DeliveryCodeGenerator(
            mockOptions.Object,
            new FileSystemOutputProvider(mockOptions.Object),
            _deliveryClient,
            ClassCodeGeneratorFactory,
            ClassDefinitionFactory,
            new DeliveryElementService(mockOptions.Object),
            Logger.Object);

        await codeGenerator.RunAsync();

        var allFilesCount = Directory.GetFiles(Path.GetFullPath(TempDir), "*.cs").Length;
        var generatedCount = Directory.GetFiles(Path.GetFullPath(TempDir), $"*.{transformFilename}.cs").Length;

        var resultGeneratedFilesCount = generatedCount * 2;
        resultGeneratedFilesCount.Should().Be(allFilesCount);

        foreach (var filepath in Directory.EnumerateFiles(Path.GetFullPath(TempDir), $"*.{transformFilename}.cs"))
        {
            var customFileExists = File.Exists(filepath.Replace($".{transformFilename}", ""));
            customFileExists.Should().BeTrue();
        }

        Logger.Verify(a => a.LogInfo(It.Is<string>(m => m == $"{allFilesCount} content type models were successfully created.")),
            Times.Once());

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
            DeliveryOptions = new DeliveryOptions { EnvironmentId = EnvironmentId },
            Namespace = "CustomNamespace",
            OutputDir = TempDir,
            ManagementApi = false,
            GeneratePartials = false,
            WithTypeProvider = true,
            StructuredModel = structuredModel.ToString()
        });

        var codeGenerator = new DeliveryCodeGenerator(
            mockOptions.Object,
            new FileSystemOutputProvider(mockOptions.Object),
            _deliveryClient,
            ClassCodeGeneratorFactory,
            ClassDefinitionFactory,
            new DeliveryElementService(mockOptions.Object),
            Logger.Object);

        Logger.Setup(f => f.LogInfo($"{NumberOfContentTypes} content type models were successfully created."));
        Logger.Setup(f => f.LogInfo("CustomTypeProvider class was successfully created."));

        await codeGenerator.RunAsync();

        Directory.GetFiles(Path.GetFullPath(TempDir)).Length.Should().Be(NumberOfContentTypes + 1);
        Directory.EnumerateFiles(Path.GetFullPath(TempDir), "*TypeProvider.cs").Should().NotBeEmpty();

        Logger.VerifyAll();

        // Cleanup
        Directory.Delete(TempDir, true);
    }

    [Theory]
    [InlineData(StructuredModelFlags.ModularContent)]
    [InlineData(StructuredModelFlags.NotSet)]
    public async Task IntegrationTest_RunAsync_BaseClass_CorrectFiles(StructuredModelFlags structuredModel)
    {
        var baseClassName = "CustomBaseClass";
        var mockOptions = new Mock<IOptions<CodeGeneratorOptions>>();
        mockOptions.Setup(x => x.Value).Returns(new CodeGeneratorOptions
        {
            DeliveryOptions = new DeliveryOptions { EnvironmentId = EnvironmentId },
            Namespace = "CustomNamespace",
            OutputDir = TempDir,
            ManagementApi = false,
            GeneratePartials = false,
            WithTypeProvider = false,
            StructuredModel = structuredModel.ToString(),
            BaseClass = baseClassName
        });

        var codeGenerator = new DeliveryCodeGenerator(
            mockOptions.Object,
            new FileSystemOutputProvider(mockOptions.Object),
            _deliveryClient,
            ClassCodeGeneratorFactory,
            ClassDefinitionFactory,
            new DeliveryElementService(mockOptions.Object),
            Logger.Object);

        Logger.Setup(f => f.LogInfo($"{NumberOfContentTypes} content type models were successfully created."));
        Logger.Setup(f => f.LogInfo($"{baseClassName} class was successfully created."));
        Logger.Setup(f => f.LogInfo($"{baseClassName}Extender class was successfully created."));

        await codeGenerator.RunAsync();

        var x = Directory.GetFiles(Path.GetFullPath(TempDir));

        Directory.GetFiles(Path.GetFullPath(TempDir)).Length.Should().Be(NumberOfContentTypes + 2);
        Directory.EnumerateFiles(Path.GetFullPath(TempDir), $"*{baseClassName}*").Count().Should().Be(2);

        Logger.VerifyAll();

        // Cleanup
        Directory.Delete(TempDir, true);
    }
}
