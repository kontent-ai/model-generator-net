using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Kentico.Kontent.Management;
using Kentico.Kontent.Management.Models.Shared;
using Kentico.Kontent.Management.Models.Types;
using Kentico.Kontent.Management.Models.Types.Elements;
using Kentico.Kontent.Management.Models.TypeSnippets;
using Kentico.Kontent.ModelGenerator.Core;
using Kentico.Kontent.ModelGenerator.Core.Configuration;
using Kentico.Kontent.ModelGenerator.Tests.Fixtures;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Kentico.Kontent.ModelGenerator.Tests
{
    public class ManagementCodeGeneratorTests : CodeGeneratorTestsBase
    {
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

            Assert.Throws<InvalidOperationException>(() => new ManagementCodeGenerator(mockOptions.Object, outputProvider.Object, _managementClient));
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
                    TestHelper.GenerateElementMetadataBase(Guid.NewGuid(), elementCodename)
                }
            };

            var codeGenerator = new ManagementCodeGenerator(mockOptions.Object, outputProvider.Object, managementClient.Object);

            var result = codeGenerator.GetClassCodeGenerator(contentType, new List<ContentTypeSnippetModel>());

            Assert.Equal($"{contentTypeCodename}.Generated", result.ClassFilename);
        }

        [Fact]
        public async Task IntegrationTest()
        {
            var mockOptions = new Mock<IOptions<CodeGeneratorOptions>>();
            mockOptions.Setup(x => x.Value).Returns(new CodeGeneratorOptions
            {
                Namespace = "CustomNamespace",
                OutputDir = TempDir,
                ManagementApi = true,
                ManagementOptions = new ManagementOptions { ApiKey = "apiKey", ProjectId = ProjectId }
            });

            var codeGenerator = new ManagementCodeGenerator(mockOptions.Object, new FileSystemOutputProvider(mockOptions.Object), _managementClient);

            await codeGenerator.GenerateContentTypeModels();

            Assert.True(Directory.GetFiles(Path.GetFullPath(TempDir)).Length > 10);

            Assert.NotEmpty(Directory.EnumerateFiles(Path.GetFullPath(TempDir), "*.Generated.cs"));
            Assert.NotEmpty(Directory.EnumerateFiles(Path.GetFullPath(TempDir)).Where(p => !p.Contains("*.Generated.cs")));

            // Cleanup
            Directory.Delete(TempDir, true);
        }

        [Fact]
        public async Task IntegrationTestWithGeneratedSuffix()
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

            await codeGenerator.GenerateContentTypeModels();

            Assert.True(Directory.GetFiles(Path.GetFullPath(TempDir)).Length > 10);

            foreach (var filepath in Directory.EnumerateFiles(Path.GetFullPath(TempDir)))
            {
                Assert.EndsWith($".{transformFilename}.cs", Path.GetFileName(filepath));
            }

            // Cleanup
            Directory.Delete(TempDir, true);
        }

        [Fact]
        public async Task IntegrationTestWithGeneratePartials()
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

        private IManagementClient CreateManagementClient()
        {
            var managementModelsProvider = new ManagementModelsProvider();
            var managementClientMock = new Mock<IManagementClient>();

            var contentTypeListingResponseModel = new Mock<IListingResponseModel<ContentTypeModel>>();
            contentTypeListingResponseModel
                .Setup(c => c.GetEnumerator())
                .Returns(managementModelsProvider.ManagementContentTypeModels);

            var contentTypeSnippetListingResponseModel = new Mock<IListingResponseModel<ContentTypeSnippetModel>>();
            contentTypeSnippetListingResponseModel
                .Setup(c => c.GetEnumerator())
                .Returns(managementModelsProvider.ManagementContentTypeSnippetModels);

            managementClientMock.Setup(client => client.ListContentTypeSnippetsAsync())
                .Returns(Task.FromResult(contentTypeSnippetListingResponseModel.Object));
            managementClientMock.Setup(client => client.ListContentTypesAsync())
                .Returns(Task.FromResult(contentTypeListingResponseModel.Object));

            return managementClientMock.Object;
        }
    }
}
