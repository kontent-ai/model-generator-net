using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Kontent.Ai.Delivery;
using Kontent.Ai.Delivery.Abstractions;
using Kontent.Ai.Delivery.Builders.DeliveryClient;
using Kontent.Ai.ModelGenerator.Core;
using Kontent.Ai.ModelGenerator.Core.Configuration;
using Microsoft.Extensions.Options;
using Moq;
using RichardSzalay.MockHttp;
using Xunit;

namespace Kontent.Ai.ModelGenerator.Tests;

public class DeliveryCodeGeneratorTests : CodeGeneratorTestsBase
{
    /// <summary>
    /// represents count of elements in 'delivery_types.json'
    /// </summary>
    private const int NumberOfContentTypes = 13;
    protected override string TempDir => Path.Combine(Path.GetTempPath(), "DeliveryCodeGeneratorIntegrationTests");

    [Fact]
    public void Constructor_ManagementIsTrue_Throws()
    {
        var mockOptions = new Mock<IOptions<CodeGeneratorOptions>>();
        mockOptions.SetupGet(option => option.Value).Returns(new CodeGeneratorOptions
        {
            ManagementApi = true
        });

        var deliveryClient = new Mock<IDeliveryClient>();
        var outputProvider = new Mock<IOutputProvider>();

        Assert.Throws<InvalidOperationException>(() => new DeliveryCodeGenerator(mockOptions.Object, outputProvider.Object, deliveryClient.Object));
    }

    [Fact]
    public void CreateCodeGeneratorOptions_NoOutputSetInJsonNorInParameters_OutputDirHasDefaultValue()
    {
        var mockOptions = new Mock<IOptions<CodeGeneratorOptions>>();
        var options = new CodeGeneratorOptions
        {
            OutputDir = ""
        };
        mockOptions.Setup(x => x.Value).Returns(options);

        var outputProvider = new FileSystemOutputProvider(mockOptions.Object);
        Assert.Empty(options.OutputDir);
        Assert.NotEmpty(outputProvider.OutputDir);
    }

    [Fact]
    public void CreateCodeGeneratorOptions_OutputSetInParameters_OutputDirHasCustomValue()
    {
        var expectedOutputDir = Environment.CurrentDirectory;
        var mockOptions = new Mock<IOptions<CodeGeneratorOptions>>();
        var options = new CodeGeneratorOptions
        {
            OutputDir = ""
        };
        mockOptions.Setup(x => x.Value).Returns(options);

        var outputProvider = new FileSystemOutputProvider(mockOptions.Object);
        Assert.Equal(expectedOutputDir.TrimEnd(Path.DirectorySeparatorChar), outputProvider.OutputDir.TrimEnd(Path.DirectorySeparatorChar));
    }

    [Theory]
    [InlineData(StructuredModelFlags.DateTime)]
    [InlineData(StructuredModelFlags.RichText)]
    [InlineData(StructuredModelFlags.RichText | StructuredModelFlags.DateTime)]
    [InlineData(StructuredModelFlags.NotSet)]
    public void GetClassCodeGenerator_Returns(StructuredModelFlags structuredModel)
    {
        var mockOptions = new Mock<IOptions<CodeGeneratorOptions>>();
        mockOptions.SetupGet(option => option.Value).Returns(new CodeGeneratorOptions
        {
            ManagementApi = false,
            StructuredModel = structuredModel.ToString()
        });

        var deliveryClient = new Mock<IDeliveryClient>();
        var outputProvider = new Mock<IOutputProvider>();

        var elementCodename = "element_codename";
        var contentElement = new Mock<IContentElement>();
        contentElement.SetupGet(element => element.Type).Returns("text");
        contentElement.SetupGet(element => element.Codename).Returns(elementCodename);

        var contentType = new Mock<IContentType>();
        var contentTypeCodename = "Contenttype";
        contentType.SetupGet(type => type.System.Codename).Returns(contentTypeCodename);
        contentType.SetupGet(type => type.Elements).Returns(new Dictionary<string, IContentElement> { { elementCodename, contentElement.Object } });

        var codeGenerator = new DeliveryCodeGenerator(mockOptions.Object, outputProvider.Object, deliveryClient.Object);

        var result = codeGenerator.GetClassCodeGenerator(contentType.Object);

        Assert.Equal($"{contentTypeCodename}.Generated", result.ClassFilename);
    }

    [Fact]
    public async Task IntegrationTest_RunAsync_CorrectFiles()
    {
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When("https://deliver.kontent.ai/*")
            .Respond("application/json", await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "Fixtures/delivery_types.json")));
        var httpClient = mockHttp.ToHttpClient();

        var mockOptions = new Mock<IOptions<CodeGeneratorOptions>>();
        mockOptions.Setup(x => x.Value).Returns(new CodeGeneratorOptions
        {
            DeliveryOptions = new DeliveryOptions { ProjectId = ProjectId },
            Namespace = "CustomNamespace",
            OutputDir = TempDir,
            ManagementApi = false,
            GeneratePartials = false,
            WithTypeProvider = false,
            StructuredModel = null
        });

        var deliveryClient = DeliveryClientBuilder.WithProjectId(ProjectId).WithDeliveryHttpClient(new DeliveryHttpClient(httpClient)).Build();

        var codeGenerator = new DeliveryCodeGenerator(mockOptions.Object, new FileSystemOutputProvider(mockOptions.Object), deliveryClient);

        await codeGenerator.RunAsync();

        Assert.Equal(NumberOfContentTypes, Directory.GetFiles(Path.GetFullPath(TempDir)).Length);

        Assert.NotEmpty(Directory.EnumerateFiles(Path.GetFullPath(TempDir), "*.Generated.cs"));
        Assert.NotEmpty(Directory.EnumerateFiles(Path.GetFullPath(TempDir)).Where(p => !p.Contains("*.Generated.cs")));
        Assert.Empty(Directory.EnumerateFiles(Path.GetFullPath(TempDir), "*TypeProvider.cs"));

        // Cleanup
        Directory.Delete(TempDir, true);
    }

    [Fact]
    public async Task IntegrationTest_RunAsync_GeneratedSuffix_CorrectFiles()
    {
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When("https://deliver.kontent.ai/*")
            .Respond("application/json", await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "Fixtures/delivery_types.json")));
        var httpClient = mockHttp.ToHttpClient();

        const string transformFilename = "CustomSuffix";

        var mockOptions = new Mock<IOptions<CodeGeneratorOptions>>();
        mockOptions.Setup(x => x.Value).Returns(new CodeGeneratorOptions
        {
            DeliveryOptions = new DeliveryOptions { ProjectId = ProjectId },
            Namespace = "CustomNamespace",
            OutputDir = TempDir,
            GeneratePartials = false,
            StructuredModel = null,
            WithTypeProvider = false,
            FileNameSuffix = transformFilename,
            ManagementApi = false
        });

        var deliveryClient = DeliveryClientBuilder.WithProjectId(ProjectId).WithDeliveryHttpClient(new DeliveryHttpClient(httpClient)).Build();

        var codeGenerator = new DeliveryCodeGenerator(mockOptions.Object, new FileSystemOutputProvider(mockOptions.Object), deliveryClient);

        await codeGenerator.RunAsync();

        Assert.Equal(NumberOfContentTypes, Directory.GetFiles(Path.GetFullPath(TempDir)).Length);

        foreach (var filepath in Directory.EnumerateFiles(Path.GetFullPath(TempDir)))
        {
            Assert.EndsWith($".{transformFilename}.cs", Path.GetFileName(filepath));
        }

        // Cleanup
        Directory.Delete(TempDir, true);
    }

    [Fact]
    public async Task IntegrationTest_RunAsync_GeneratePartials_CorrectFiles()
    {
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When("https://deliver.kontent.ai/*")
            .Respond("application/json", await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "Fixtures/delivery_types.json")));
        var httpClient = mockHttp.ToHttpClient();

        const string transformFilename = "Generated";

        var mockOptions = new Mock<IOptions<CodeGeneratorOptions>>();
        mockOptions.Setup(x => x.Value).Returns(new CodeGeneratorOptions
        {
            DeliveryOptions = new DeliveryOptions { ProjectId = ProjectId },
            Namespace = "CustomNamespace",
            OutputDir = TempDir,
            FileNameSuffix = transformFilename,
            GeneratePartials = true,
            WithTypeProvider = false,
            StructuredModel = null,
            ManagementApi = false
        });

        var deliveryClient = DeliveryClientBuilder.WithProjectId(ProjectId)
            .WithDeliveryHttpClient(new DeliveryHttpClient(httpClient)).Build();

        var codeGenerator = new DeliveryCodeGenerator(mockOptions.Object, new FileSystemOutputProvider(mockOptions.Object), deliveryClient);

        await codeGenerator.RunAsync();

        var allFilesCount = Directory.GetFiles(Path.GetFullPath(TempDir), "*.cs").Length;
        var generatedCount = Directory.GetFiles(Path.GetFullPath(TempDir), $"*.{transformFilename}.cs").Length;
        Assert.Equal(allFilesCount, generatedCount * 2);

        foreach (var filepath in Directory.EnumerateFiles(Path.GetFullPath(TempDir), $"*.{transformFilename}.cs"))
        {
            var customFileExists = File.Exists(filepath.Replace($".{transformFilename}", ""));
            Assert.True(customFileExists);
        }

        // Cleanup
        Directory.Delete(TempDir, true);
    }

    [Fact]
    public async Task IntegrationTest_RunAsync_TypeProvider_CorrectFiles()
    {
        var mockHttp = new MockHttpMessageHandler();
        mockHttp.When("https://deliver.kontent.ai/*")
            .Respond("application/json", await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "Fixtures/delivery_types.json")));
        var httpClient = mockHttp.ToHttpClient();

        var mockOptions = new Mock<IOptions<CodeGeneratorOptions>>();
        mockOptions.Setup(x => x.Value).Returns(new CodeGeneratorOptions
        {
            DeliveryOptions = new DeliveryOptions { ProjectId = ProjectId },
            Namespace = "CustomNamespace",
            OutputDir = TempDir,
            ManagementApi = false,
            GeneratePartials = false,
            WithTypeProvider = true,
            StructuredModel = null
        });

        var deliveryClient = DeliveryClientBuilder.WithProjectId(ProjectId).WithDeliveryHttpClient(new DeliveryHttpClient(httpClient)).Build();

        var codeGenerator = new DeliveryCodeGenerator(mockOptions.Object, new FileSystemOutputProvider(mockOptions.Object), deliveryClient);

        await codeGenerator.RunAsync();

        Assert.Equal(NumberOfContentTypes + 1, Directory.GetFiles(Path.GetFullPath(TempDir)).Length);

        Assert.NotEmpty(Directory.EnumerateFiles(Path.GetFullPath(TempDir), "*TypeProvider.cs"));

        // Cleanup
        Directory.Delete(TempDir, true);
    }
}
