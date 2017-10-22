using System;
using System.IO;
using RichardSzalay.MockHttp;
using Xunit;

namespace CloudModelGenerator.Tests
{
    public class CodeGeneratorTests
    {
        private readonly string TEMP_DIR = Path.GetTempPath() + "/CodeGeneratorTests/";

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
        public void IntegrationTest()
        {
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When("https://deliver.kenticocloud.com/*")
                    .Respond("application/json", File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures\\types.json")));
            var httpClient = mockHttp.ToHttpClient();

            const string PROJECT_ID = "975bf280-fd91-488c-994c-2f04416e5ee3";
            const string @namespace = "CustomNamespace";

            var codeGenerator = new CodeGenerator(PROJECT_ID, TEMP_DIR, @namespace);
            codeGenerator.Client.HttpClient = httpClient;

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

        [Fact]
        public void IntegrationTestWithGeneratedSuffix()
        {
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When("https://deliver.kenticocloud.com/*")
                    .Respond("application/json", File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures\\types.json")));
            var httpClient = mockHttp.ToHttpClient();

            const string PROJECT_ID = "975bf280-fd91-488c-994c-2f04416e5ee3";
            const string @namespace = "CustomNamespace";
            const string transformFilename = "Generated";

            var codeGenerator = new CodeGenerator(PROJECT_ID, TEMP_DIR, @namespace, transformFilename);
            codeGenerator.Client.HttpClient = httpClient;

            codeGenerator.GenerateContentTypeModels();
            codeGenerator.GenerateTypeProvider();

            Assert.True(Directory.GetFiles(Path.GetFullPath(TEMP_DIR)).Length > 10);

            foreach (var filepath in Directory.EnumerateFiles(Path.GetFullPath(TEMP_DIR)))
            {
                Assert.EndsWith($".{transformFilename}.cs", Path.GetFileName(filepath));
            }

            // Cleanup
            Directory.Delete(TEMP_DIR, true);
        }

        [Fact]
        public void IntegrationTestWithGeneratePartials()
        {
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When("https://deliver.kenticocloud.com/*")
                    .Respond("application/json", File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures\\types.json")));
            var httpClient = mockHttp.ToHttpClient();

            const string PROJECT_ID = "975bf280-fd91-488c-994c-2f04416e5ee3";
            const string @namespace = "CustomNamespace";
            const string transformFilename = "Generated";

            var codeGenerator = new CodeGenerator(PROJECT_ID, TEMP_DIR, @namespace, transformFilename, true);
            codeGenerator.Client.HttpClient = httpClient;

            codeGenerator.GenerateContentTypeModels();

            var allFilesCount = Directory.GetFiles(Path.GetFullPath(TEMP_DIR), ".cs").Length;
            var generatedCount = Directory.GetFiles(Path.GetFullPath(TEMP_DIR), $".{transformFilename}.cs").Length;
            Assert.Equal(allFilesCount, generatedCount * 2);

            foreach (var filepath in Directory.EnumerateFiles(Path.GetFullPath(TEMP_DIR), $".{transformFilename}.cs"))
            {
                var customFileExists = File.Exists(filepath.Replace($".{transformFilename}", ""));
                Assert.True(customFileExists);
            }

            // Cleanup
            Directory.Delete(TEMP_DIR, true);
        }
    }
}
