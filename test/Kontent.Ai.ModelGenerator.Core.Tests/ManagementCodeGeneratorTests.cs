using FluentAssertions;
using Kontent.Ai.Management;
using Kontent.Ai.Management.Configuration;
using Kontent.Ai.Management.Models.Types;
using Kontent.Ai.Management.Models.Types.Elements;
using Kontent.Ai.Management.Models.TypeSnippets;
using Kontent.Ai.ModelGenerator.Core.Configuration;
using Kontent.Ai.ModelGenerator.Core.Tests.TestHelpers;
using Microsoft.Extensions.Options;
using Moq;

namespace Kontent.Ai.ModelGenerator.Core.Tests;

public class ManagementCodeGeneratorTests : CodeGeneratorTestsBase
{
    /// <summary>
    /// represents count of elements in 'management_types.json'
    /// </summary>
    private const int NumberOfContentTypes = 14;
    private readonly IManagementClient _managementClient;
    protected override string TempDir => Path.Combine(Path.GetTempPath(), "ManagementCodeGeneratorIntegrationTests");

    public ManagementCodeGeneratorTests()
    {
        _managementClient = CreateManagementClient();
    }

    [Fact]
    public void Constructor_ManagementIsTrue_Throws()
    {
        var mockOptions = new Mock<IOptions<CodeGeneratorOptions>>();
        mockOptions.SetupGet(option => option.Value).Returns(new CodeGeneratorOptions
        {
            ManagementApi = false
        });

        var outputProvider = new Mock<IOutputProvider>();

        var call = () => new ManagementCodeGenerator(mockOptions.Object, outputProvider.Object, _managementClient);

        call.Should().ThrowExactly<InvalidOperationException>();
    }

    [Fact]
    public void GetClassCodeGenerator_Returns()
    {
        var mockOptions = new Mock<IOptions<CodeGeneratorOptions>>();
        mockOptions.SetupGet(option => option.Value).Returns(new CodeGeneratorOptions
        {
            ManagementApi = true
        });

        var outputProvider = new Mock<IOutputProvider>();
        var managementClient = new Mock<IManagementClient>();

        var contentTypeCodename = "Contenttype";
        var elementCodename = "element_codename";
        var contentType = new ContentTypeModel
        {
            Codename = contentTypeCodename,
            Elements = new List<ElementMetadataBase>
            {
                TestDataGenerator.GenerateElementMetadataBase(Guid.NewGuid(), elementCodename)
            }
        };

        var codeGenerator = new ManagementCodeGenerator(mockOptions.Object, outputProvider.Object, managementClient.Object);

        var result = codeGenerator.GetClassCodeGenerator(contentType, new List<ContentTypeSnippetModel>());

        result.ClassFilename.Should().Be($"{contentTypeCodename}.Generated");
    }

    [Fact]
    public async Task IntegrationTest_RunAsync_CorrectFiles()
    {
        var mockOptions = new Mock<IOptions<CodeGeneratorOptions>>();
        mockOptions.Setup(x => x.Value).Returns(new CodeGeneratorOptions
        {
            Namespace = "CustomNamespace",
            OutputDir = TempDir,
            ManagementApi = true,
            GeneratePartials = false,
            ManagementOptions = new ManagementOptions { ApiKey = "apiKey", ProjectId = ProjectId }
        });

        var codeGenerator = new ManagementCodeGenerator(mockOptions.Object, new FileSystemOutputProvider(mockOptions.Object), _managementClient);

        await codeGenerator.RunAsync();

        Directory.GetFiles(Path.GetFullPath(TempDir)).Length.Should().Be(NumberOfContentTypes);

        Directory.EnumerateFiles(Path.GetFullPath(TempDir), "*.Generated.cs").Should().NotBeEmpty();
        Directory.EnumerateFiles(Path.GetFullPath(TempDir)).Where(p => !p.Contains("*.Generated.cs")).Should().NotBeEmpty();

        // Cleanup
        Directory.Delete(TempDir, true);
    }

    [Fact]
    public async Task IntegrationTest_RunAsync_GeneratedSuffix_CorrectFiles()
    {
        const string transformFilename = "CustomSuffix";

        var mockOptions = new Mock<IOptions<CodeGeneratorOptions>>();
        mockOptions.Setup(x => x.Value).Returns(new CodeGeneratorOptions
        {
            ManagementOptions = new ManagementOptions { ApiKey = "apiKey", ProjectId = ProjectId },
            Namespace = "CustomNamespace",
            OutputDir = TempDir,
            GeneratePartials = false,
            FileNameSuffix = transformFilename,
            ManagementApi = true
        });

        var codeGenerator = new ManagementCodeGenerator(mockOptions.Object, new FileSystemOutputProvider(mockOptions.Object), _managementClient);

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
        const string transformFilename = "Generated";

        var mockOptions = new Mock<IOptions<CodeGeneratorOptions>>();
        mockOptions.Setup(x => x.Value).Returns(new CodeGeneratorOptions
        {
            Namespace = "CustomNamespace",
            OutputDir = TempDir,
            FileNameSuffix = transformFilename,
            GeneratePartials = true,
            ManagementApi = true,
            ManagementOptions = new ManagementOptions { ApiKey = "apiKey", ProjectId = ProjectId }
        });

        var codeGenerator = new ManagementCodeGenerator(mockOptions.Object, new FileSystemOutputProvider(mockOptions.Object), _managementClient);

        await codeGenerator.RunAsync();

        var allFilesCount = Directory.GetFiles(Path.GetFullPath(TempDir), "*.cs").Length;
        var generatedCount = Directory.GetFiles(Path.GetFullPath(TempDir), $"*.{transformFilename}.cs").Length;
        var resultFileCount = generatedCount * 2;

        resultFileCount.Should().Be(allFilesCount);

        foreach (var filepath in Directory.EnumerateFiles(Path.GetFullPath(TempDir), $"*.{transformFilename}.cs"))
        {
            var customFileExists = File.Exists(filepath.Replace($".{transformFilename}", ""));
            customFileExists.Should().BeTrue();
        }

        // Cleanup
        Directory.Delete(TempDir, true);
    }
}
