using KenticoCloud.Delivery;
using Microsoft.Extensions.Options;
using Moq;
using RichardSzalay.MockHttp;
using System;
using System.IO;
using Xunit;

namespace CloudModelGenerator.Tests
{
    public class CodeGeneratorTests
    {
        private readonly string TEMP_DIR = Path.Combine(Path.GetTempPath(), "CodeGeneratorTests");

        public CodeGeneratorTests()
        {
            // Cleanup
            if (Directory.Exists(TEMP_DIR))
            {
                Directory.Delete(TEMP_DIR, true);
            }
            Directory.CreateDirectory(TEMP_DIR);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void IntegrationTest(bool cmApi)
        {
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When("https://deliver.kenticocloud.com/*")
                    .Respond("application/json", File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures/types.json")));
            var httpClient = mockHttp.ToHttpClient();

            var mockOptions = new Mock<IOptions<CodeGeneratorOptions>>();
            mockOptions.Setup(x => x.Value).Returns(new CodeGeneratorOptions
            {
                Namespace = "CustomNamespace",
                OutputDir = TEMP_DIR,
                ContentManagementApi = cmApi
            });

            var client = DeliveryClientBuilder.WithProjectId("975bf280-fd91-488c-994c-2f04416e5ee3").WithHttpClient(httpClient).Build();

            var codeGenerator = new CodeGenerator(mockOptions.Object, client);

            codeGenerator.GenerateContentTypeModels();
            codeGenerator.GenerateTypeProvider();

            Assert.True(Directory.GetFiles(Path.GetFullPath(TEMP_DIR)).Length > 10);

            foreach (var filepath in Directory.EnumerateFiles(Path.GetFullPath(TEMP_DIR)))
            {
                Assert.DoesNotContain(".Generated.cs", Path.GetFileName(filepath));
            }

            // Cleanup
            Directory.Delete(TEMP_DIR, true);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void IntegrationTestWithGeneratedSuffix(bool cmApi)
        {
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When("https://deliver.kenticocloud.com/*")
                    .Respond("application/json", File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures/types.json")));
            var httpClient = mockHttp.ToHttpClient();

            const string transformFilename = "Generated";

            var mockOptions = new Mock<IOptions<CodeGeneratorOptions>>();
            mockOptions.Setup(x => x.Value).Returns(new CodeGeneratorOptions
            {
                DeliveryOptions = { ProjectId = "975bf280-fd91-488c-994c-2f04416e5ee3" },
                Namespace = "CustomNamespace",
                OutputDir = TEMP_DIR,
                FileNameSuffix = transformFilename,
                ContentManagementApi = cmApi
            });

            var client = DeliveryClientBuilder.WithProjectId("975bf280-fd91-488c-994c-2f04416e5ee3").WithHttpClient(httpClient).Build();

            var codeGenerator = new CodeGenerator(mockOptions.Object, client);

            codeGenerator.GenerateContentTypeModels();

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
        public void IntegrationTestWithGeneratePartials(bool cmApi)
        {
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When("https://deliver.kenticocloud.com/*")
                    .Respond("application/json", File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures/types.json")));
            var httpClient = mockHttp.ToHttpClient();

            const string transformFilename = "Generated";

            var mockOptions = new Mock<IOptions<CodeGeneratorOptions>>();
            mockOptions.Setup(x => x.Value).Returns(new CodeGeneratorOptions
            {
                DeliveryOptions = { ProjectId = "975bf280-fd91-488c-994c-2f04416e5ee3" },
                Namespace = "CustomNamespace",
                OutputDir = TEMP_DIR,
                FileNameSuffix = transformFilename,
                GeneratePartials = true,
                ContentManagementApi = cmApi
            });

            var client = DeliveryClientBuilder.WithProjectId("975bf280-fd91-488c-994c-2f04416e5ee3").WithHttpClient(httpClient).Build();

            var codeGenerator = new CodeGenerator(mockOptions.Object, client);

            codeGenerator.GenerateContentTypeModels();

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
