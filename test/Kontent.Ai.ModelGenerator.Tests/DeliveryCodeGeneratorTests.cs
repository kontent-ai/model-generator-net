using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
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

        var call = () => new DeliveryCodeGenerator(mockOptions.Object, outputProvider.Object, deliveryClient.Object);

        call.Should().ThrowExactly<InvalidOperationException>();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void GetClassCodeGenerator_Returns(bool structuredModel)
    {
        var mockOptions = new Mock<IOptions<CodeGeneratorOptions>>();
        mockOptions.SetupGet(option => option.Value).Returns(new CodeGeneratorOptions
        {
            ManagementApi = false,
            StructuredModel = structuredModel
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

        result.ClassFilename.Should().Be($"{contentTypeCodename}.Generated");
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
            StructuredModel = false
        });

        var deliveryClient = DeliveryClientBuilder.WithProjectId(ProjectId).WithDeliveryHttpClient(new DeliveryHttpClient(httpClient)).Build();

        var codeGenerator = new DeliveryCodeGenerator(mockOptions.Object, new FileSystemOutputProvider(mockOptions.Object), deliveryClient);

        await codeGenerator.RunAsync();

        Directory.GetFiles(Path.GetFullPath(TempDir)).Length.Should().Be(NumberOfContentTypes);

        Directory.EnumerateFiles(Path.GetFullPath(TempDir), "*.Generated.cs").Should().NotBeEmpty();
        Directory.EnumerateFiles(Path.GetFullPath(TempDir)).Where(p => !p.Contains("*.Generated.cs")).Should().NotBeEmpty();
        Directory.EnumerateFiles(Path.GetFullPath(TempDir), "*TypeProvider.cs").Should().BeEmpty();

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
            StructuredModel = false,
            WithTypeProvider = false,
            FileNameSuffix = transformFilename,
            ManagementApi = false
        });

        var deliveryClient = DeliveryClientBuilder.WithProjectId(ProjectId).WithDeliveryHttpClient(new DeliveryHttpClient(httpClient)).Build();

        var codeGenerator = new DeliveryCodeGenerator(mockOptions.Object, new FileSystemOutputProvider(mockOptions.Object), deliveryClient);

        await codeGenerator.RunAsync();

        Directory.GetFiles(Path.GetFullPath(TempDir)).Length.Should().Be(NumberOfContentTypes);

        foreach (var filepath in Directory.EnumerateFiles(Path.GetFullPath(TempDir)))
        {
            Path.GetFileName(filepath).Should().EndWith($".{transformFilename}.cs");
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
            StructuredModel = false,
            ManagementApi = false
        });

        var deliveryClient = DeliveryClientBuilder.WithProjectId(ProjectId)
            .WithDeliveryHttpClient(new DeliveryHttpClient(httpClient)).Build();

        var codeGenerator = new DeliveryCodeGenerator(mockOptions.Object, new FileSystemOutputProvider(mockOptions.Object), deliveryClient);

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
            StructuredModel = false
        });

        var deliveryClient = DeliveryClientBuilder.WithProjectId(ProjectId).WithDeliveryHttpClient(new DeliveryHttpClient(httpClient)).Build();

        var codeGenerator = new DeliveryCodeGenerator(mockOptions.Object, new FileSystemOutputProvider(mockOptions.Object), deliveryClient);

        await codeGenerator.RunAsync();

        Directory.GetFiles(Path.GetFullPath(TempDir)).Length.Should().Be(NumberOfContentTypes + 1);
        Directory.EnumerateFiles(Path.GetFullPath(TempDir), "*TypeProvider.cs").Should().NotBeEmpty();

        // Cleanup
        Directory.Delete(TempDir, true);
    }
}
