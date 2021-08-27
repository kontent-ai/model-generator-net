using Kentico.Kontent.Delivery;
using Kentico.Kontent.Delivery.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using RichardSzalay.MockHttp;
using System;
using System.IO;
using System.Threading.Tasks;
using Kentico.Kontent.Delivery.Builders.DeliveryClient;
using Kentico.Kontent.ModelGenerator.Core;
using Kentico.Kontent.ModelGenerator.Core.Configuration;
using Xunit;
using System.Linq;

namespace Kentico.Kontent.ModelGenerator.Tests
{
    public class CodeGeneratorTests
    {
        private readonly string TempDir = Path.Combine(Path.GetTempPath(), "CodeGeneratorTests");
        private const string ProjectId = "975bf280-fd91-488c-994c-2f04416e5ee3";

        public CodeGeneratorTests()
        {
            // Cleanup
            if (Directory.Exists(TempDir))
            {
                Directory.Delete(TempDir, true);
            }
            Directory.CreateDirectory(TempDir);
        }

        [Fact]
        public void CreateCodeGeneratorOptions_NoOutputSetInJsonNorInParameters_OuputDirHasDefaultValue()
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
        public void CreateCodeGeneratorOptions_OutputSetInParameters_OuputDirHasCustomValue()
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
        [InlineData(false)]
        [InlineData(true)]
        public async Task IntegrationTest(bool cmApi)
        {
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When("https://deliver.kontent.ai/*")
                    .Respond("application/json", await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "Fixtures/types.json")));
            var httpClient = mockHttp.ToHttpClient();

            var mockOptions = new Mock<IOptions<CodeGeneratorOptions>>();
            mockOptions.Setup(x => x.Value).Returns(new CodeGeneratorOptions
            {
                Namespace = "CustomNamespace",
                OutputDir = TempDir,
                ContentManagementApi = cmApi
            });

            var client = DeliveryClientBuilder.WithProjectId(ProjectId).WithDeliveryHttpClient(new DeliveryHttpClient(httpClient)).Build();

            var codeGenerator = new CodeGenerator(mockOptions.Object, client, new FileSystemOutputProvider(mockOptions.Object));

            await codeGenerator.GenerateContentTypeModels();
            await codeGenerator.GenerateTypeProvider();

            Assert.True(Directory.GetFiles(Path.GetFullPath(TempDir)).Length > 10);

            Assert.NotEmpty(Directory.EnumerateFiles(Path.GetFullPath(TempDir), "*.Generated.cs"));
            Assert.NotEmpty(Directory.EnumerateFiles(Path.GetFullPath(TempDir)).Where(p => !p.Contains("*.Generated.cs")));
            Assert.NotEmpty(Directory.EnumerateFiles(Path.GetFullPath(TempDir), "*TypeProvider.cs"));

            // Cleanup
            Directory.Delete(TempDir, true);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task IntegrationTestWithGeneratedSuffix(bool cmApi)
        {
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When("https://deliver.kontent.ai/*")
                    .Respond("application/json", await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "Fixtures/types.json")));
            var httpClient = mockHttp.ToHttpClient();

            const string transformFilename = "CustomSuffix";

            var mockOptions = new Mock<IOptions<CodeGeneratorOptions>>();
            mockOptions.Setup(x => x.Value).Returns(new CodeGeneratorOptions
            {
                DeliveryOptions = new DeliveryOptions { ProjectId = ProjectId },
                Namespace = "CustomNamespace",
                OutputDir = TempDir,
                GeneratePartials = false,
                FileNameSuffix = transformFilename,
                ContentManagementApi = cmApi
            });

            var client = DeliveryClientBuilder.WithProjectId(ProjectId).WithDeliveryHttpClient(new DeliveryHttpClient(httpClient)).Build();

            var codeGenerator = new CodeGenerator(mockOptions.Object, client, new FileSystemOutputProvider(mockOptions.Object));

            await codeGenerator.GenerateContentTypeModels();

            Assert.True(Directory.GetFiles(Path.GetFullPath(TempDir)).Length > 10);

            foreach (var filepath in Directory.EnumerateFiles(Path.GetFullPath(TempDir)))
            {
                Assert.EndsWith($".{transformFilename}.cs", Path.GetFileName(filepath));
            }

            // Cleanup
            Directory.Delete(TempDir, true);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task IntegrationTestWithGeneratePartials(bool cmApi)
        {
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When("https://deliver.kontent.ai/*")
                    .Respond("application/json", await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "Fixtures/types.json")));
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
                ContentManagementApi = cmApi
            });

            var client = DeliveryClientBuilder.WithProjectId(ProjectId).WithDeliveryHttpClient(new DeliveryHttpClient(httpClient)).Build();

            var codeGenerator = new CodeGenerator(mockOptions.Object, client, new FileSystemOutputProvider(mockOptions.Object));

            await codeGenerator.GenerateContentTypeModels();

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
    }
}
