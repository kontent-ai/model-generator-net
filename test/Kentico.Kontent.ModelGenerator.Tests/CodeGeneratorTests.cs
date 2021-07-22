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
        private readonly string TEMP_DIR = Path.Combine(Path.GetTempPath(), "CodeGeneratorTests");
        private readonly string PROJECT_ID = "975bf280-fd91-488c-994c-2f04416e5ee3";

        public CodeGeneratorTests()
        {
            // Cleanup
            if (Directory.Exists(TEMP_DIR))
            {
                Directory.Delete(TEMP_DIR, true);
            }
            Directory.CreateDirectory(TEMP_DIR);
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
                OutputDir = TEMP_DIR,
                ContentManagementApi = cmApi
            });

            var deliveryClient = DeliveryClientBuilder.WithProjectId(PROJECT_ID).WithDeliveryHttpClient(new DeliveryHttpClient(httpClient)).Build();
            var managementClient = new Mock<IManagementClient>();

            var codeGenerator = new CodeGenerator(mockOptions.Object, deliveryClient, new FileSystemOutputProvider(mockOptions.Object), managementClient.Object);

            await codeGenerator.GenerateContentTypeModels();
            await codeGenerator.GenerateTypeProvider();

            Assert.True(Directory.GetFiles(Path.GetFullPath(TEMP_DIR)).Length > 10);

            Assert.NotEmpty(Directory.EnumerateFiles(Path.GetFullPath(TEMP_DIR), "*.Generated.cs"));
            Assert.NotEmpty(Directory.EnumerateFiles(Path.GetFullPath(TEMP_DIR)).Where(p => !p.Contains("*.Generated.cs")));
            Assert.NotEmpty(Directory.EnumerateFiles(Path.GetFullPath(TEMP_DIR), "*TypeProvider.cs"));

            // Cleanup
            Directory.Delete(TEMP_DIR, true);
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
                DeliveryOptions = new DeliveryOptions { ProjectId = PROJECT_ID },
                Namespace = "CustomNamespace",
                OutputDir = TEMP_DIR,
                GeneratePartials = false,
                FileNameSuffix = transformFilename,
                ContentManagementApi = cmApi
            });

            var deliveryClient = DeliveryClientBuilder.WithProjectId(PROJECT_ID).WithDeliveryHttpClient(new DeliveryHttpClient(httpClient)).Build();
            var managementClient = new Mock<IManagementClient>();

            var codeGenerator = new CodeGenerator(mockOptions.Object, deliveryClient, new FileSystemOutputProvider(mockOptions.Object), managementClient.Object);

            await codeGenerator.GenerateContentTypeModels();

            Assert.True(Directory.GetFiles(Path.GetFullPath(TEMP_DIR)).Length > 10);

            foreach (var filepath in Directory.EnumerateFiles(Path.GetFullPath(TEMP_DIR)))
            {
                Assert.EndsWith($".{transformFilename}.cs", Path.GetFileName(filepath));
            }

            // Cleanup
            Directory.Delete(TEMP_DIR, true);
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
                DeliveryOptions = new DeliveryOptions { ProjectId = PROJECT_ID },
                Namespace = "CustomNamespace",
                OutputDir = TEMP_DIR,
                FileNameSuffix = transformFilename,
                GeneratePartials = true,
                ContentManagementApi = cmApi
            });

            var deliveryClient = DeliveryClientBuilder.WithProjectId(PROJECT_ID).WithDeliveryHttpClient(new DeliveryHttpClient(httpClient)).Build();
            var managementClient = new Mock<IManagementClient>();

            var codeGenerator = new CodeGenerator(mockOptions.Object, deliveryClient, new FileSystemOutputProvider(mockOptions.Object), managementClient.Object);

            await codeGenerator.GenerateContentTypeModels();

            var allFilesCount = Directory.GetFiles(Path.GetFullPath(TEMP_DIR), "*.cs").Length;
            var generatedCount = Directory.GetFiles(Path.GetFullPath(TEMP_DIR), $"*.{transformFilename}.cs").Length;
            Assert.Equal(allFilesCount, generatedCount * 2);

            foreach (var filepath in Directory.EnumerateFiles(Path.GetFullPath(TEMP_DIR), $"*.{transformFilename}.cs"))
            {
                var customFileExists = File.Exists(filepath.Replace($".{transformFilename}", ""));
                Assert.True(customFileExists);
            }

            // Cleanup
            Directory.Delete(TEMP_DIR, true);
        }
    }
}
