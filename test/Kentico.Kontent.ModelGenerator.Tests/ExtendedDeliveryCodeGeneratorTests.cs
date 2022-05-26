using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Kentico.Kontent.Management;
using Kentico.Kontent.Management.Configuration;
using Kentico.Kontent.Management.Models.Shared;
using Kentico.Kontent.Management.Models.Types;
using Kentico.Kontent.Management.Models.Types.Elements;
using Kentico.Kontent.Management.Models.TypeSnippets;
using Kentico.Kontent.ModelGenerator.Core;
using Kentico.Kontent.ModelGenerator.Core.Configuration;
using Kentico.Kontent.ModelGenerator.Core.Generators.Class;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Kentico.Kontent.ModelGenerator.Tests
{
    public class ExtendedDeliveryCodeGeneratorTests : CodeGeneratorTestsBase
    {
        /// <summary>
        /// represents count of elements in 'management_types.json'
        /// </summary>
        private const int NumberOfContentTypesWithDefaultContentItem = 14 + 1;
        private readonly IManagementClient _managementClient;
        protected override string TempDir => Path.Combine(Path.GetTempPath(), "ExtendedDeliveryCodeGeneratorIntegrationTests");

        public ExtendedDeliveryCodeGeneratorTests()
        {
            _managementClient = CreateManagementClient();
        }

        [Fact]
        public void Constructor_ManagementIsTrue_Throws()
        {
            var mockOptions = new Mock<IOptions<CodeGeneratorOptions>>();
            mockOptions.SetupGet(option => option.Value).Returns(new CodeGeneratorOptions
            {
                ManagementApi = true,
                ExtendedDeliverModels = true
            });

            var managementClient = new Mock<IManagementClient>();
            var outputProvider = new Mock<IOutputProvider>();

            Assert.Throws<InvalidOperationException>(() => new ExtendedDeliveryCodeGenerator(mockOptions.Object, outputProvider.Object, managementClient.Object));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Constructor_ExtendedDeliveryModelsIsFalse_Throws(bool extendedDeliverPreviewModels)
        {
            var mockOptions = new Mock<IOptions<CodeGeneratorOptions>>();
            mockOptions.SetupGet(option => option.Value).Returns(new CodeGeneratorOptions
            {
                ManagementApi = false,
                ExtendedDeliverModels = false,
                ExtendedDeliverPreviewModels = extendedDeliverPreviewModels
            });

            var managementClient = new Mock<IManagementClient>();
            var outputProvider = new Mock<IOutputProvider>();

            Assert.Throws<InvalidOperationException>(() => new ExtendedDeliveryCodeGenerator(mockOptions.Object, outputProvider.Object, managementClient.Object));
        }

        [Fact]
        public void GetClassCodeGenerator_ExtendedDeliverPreviewModelsIsFalse_Returns()
        {
            var mockOptions = new Mock<IOptions<CodeGeneratorOptions>>();
            mockOptions.SetupGet(option => option.Value).Returns(new CodeGeneratorOptions
            {
                ManagementApi = false,
                ExtendedDeliverModels = true,
                ExtendedDeliverPreviewModels = false
            });

            var outputProvider = new Mock<IOutputProvider>();
            var managementClient = new Mock<IManagementClient>();

            var heroContentTypeModel = new ContentTypeModel
            {
                Codename = "hero",
                Id = Guid.NewGuid()
            };

            var contentTypeCodename = "Contenttype";
            var elementCodename = "element_codename";

            var linkedElement =
                (LinkedItemsElementMetadataModel)TestHelper.GenerateElementMetadataBase(Guid.NewGuid(), elementCodename, ElementMetadataType.LinkedItems);
            linkedElement.AllowedTypes = new List<Reference> { Reference.ById(heroContentTypeModel.Id) };

            var contentType = new ContentTypeModel
            {
                Codename = contentTypeCodename,
                Elements = new List<ElementMetadataBase>
                {
                    linkedElement
                }
            };

            var contentTypes = new List<ContentTypeModel> { contentType, heroContentTypeModel };

            var codeGenerator = new ExtendedDeliveryCodeGenerator(mockOptions.Object, outputProvider.Object, managementClient.Object);

            var result = codeGenerator.GetClassCodeGenerator(contentType, new List<ContentTypeSnippetModel>(), contentTypes);

            Assert.Equal("IEnumerable<Hero>", result.ClassDefinition.Properties[0].TypeName);
            Assert.Equal(typeof(ExtendedDeliveryClassCodeGenerator), result.GetType());
            Assert.Equal($"{contentTypeCodename}.Generated", result.ClassFilename);
        }

        [Fact]
        public void GetClassCodeGenerator_ExtendedDeliverPreviewModelsIsFalse_MultipleAllowedTypes_Returns()
        {
            var mockOptions = new Mock<IOptions<CodeGeneratorOptions>>();
            mockOptions.SetupGet(option => option.Value).Returns(new CodeGeneratorOptions
            {
                ManagementApi = false,
                ExtendedDeliverModels = true,
                ExtendedDeliverPreviewModels = false
            });

            var outputProvider = new Mock<IOutputProvider>();
            var managementClient = new Mock<IManagementClient>();

            var contentTypeCodename = "Contenttype";
            var elementCodename = "element_codename";

            var linkedElement =
                (LinkedItemsElementMetadataModel)TestHelper.GenerateElementMetadataBase(Guid.NewGuid(), elementCodename, ElementMetadataType.LinkedItems);
            linkedElement.AllowedTypes = new List<Reference>
            {
                Reference.ById(Guid.NewGuid()),
                Reference.ById(Guid.NewGuid())
            };

            var contentType = new ContentTypeModel
            {
                Codename = contentTypeCodename,
                Elements = new List<ElementMetadataBase>
                {
                    linkedElement
                }
            };

            var contentTypes = new List<ContentTypeModel>
            {
                contentType,
                new ContentTypeModel
                {
                    Id = Guid.NewGuid()
                },
                new ContentTypeModel
                {
                    Id = Guid.NewGuid()
                }
            };

            var codeGenerator = new ExtendedDeliveryCodeGenerator(mockOptions.Object, outputProvider.Object, managementClient.Object);

            var result = codeGenerator.GetClassCodeGenerator(contentType, new List<ContentTypeSnippetModel>(), contentTypes);

            Assert.Equal($"IEnumerable<{ContentItemClassCodeGenerator.DefaultContentItemClassName}>", result.ClassDefinition.Properties[0].TypeName);
            Assert.Equal(typeof(ExtendedDeliveryClassCodeGenerator), result.GetType());
            Assert.Equal($"{contentTypeCodename}.Generated", result.ClassFilename);
        }

        [Fact]
        public void GetClassCodeGenerator_ExtendedDeliverPreviewModelsIsTrue_Returns()
        {
            var mockOptions = new Mock<IOptions<CodeGeneratorOptions>>();
            mockOptions.SetupGet(option => option.Value).Returns(new CodeGeneratorOptions
            {
                ManagementApi = false,
                ExtendedDeliverModels = true,
                ExtendedDeliverPreviewModels = true
            });

            var outputProvider = new Mock<IOutputProvider>();
            var managementClient = new Mock<IManagementClient>();

            var heroContentTypeModel = new ContentTypeModel { Codename = "hero" };

            var contentTypeCodename = "Contenttype";
            var elementCodename = "element_codename";

            var linkedElement =
                (LinkedItemsElementMetadataModel)TestHelper.GenerateElementMetadataBase(Guid.NewGuid(), elementCodename, ElementMetadataType.LinkedItems);
            linkedElement.AllowedTypes = new List<Reference> { Reference.ByCodename(heroContentTypeModel.Codename) };

            var contentType = new ContentTypeModel
            {
                Codename = contentTypeCodename,
                Elements = new List<ElementMetadataBase>
                {
                    linkedElement
                }
            };

            var contentTypes = new List<ContentTypeModel> { contentType, heroContentTypeModel };

            var codeGenerator = new ExtendedDeliveryCodeGenerator(mockOptions.Object, outputProvider.Object, managementClient.Object);

            var result = codeGenerator.GetClassCodeGenerator(contentType, new List<ContentTypeSnippetModel>(), contentTypes);

            Assert.Equal($"IEnumerable<{ContentItemClassCodeGenerator.DefaultContentItemClassName}>", result.ClassDefinition.Properties[0].TypeName);
            Assert.Equal(typeof(ExtendedDeliveryClassCodeGenerator), result.GetType());
            Assert.Equal($"{contentTypeCodename}.Generated", result.ClassFilename);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task IntegrationTest_RunAsync_CorrectFiles(bool extendedDeliverPreviewModels)
        {
            var mockOptions = new Mock<IOptions<CodeGeneratorOptions>>();
            mockOptions.Setup(x => x.Value).Returns(new CodeGeneratorOptions
            {
                ExtendedDeliverModels = true,
                Namespace = "CustomNamespace",
                OutputDir = TempDir,
                ManagementApi = false,
                GeneratePartials = false,
                WithTypeProvider = false,
                StructuredModel = false,
                ManagementOptions = new ManagementOptions { ApiKey = "apiKey", ProjectId = ProjectId },
                ExtendedDeliverPreviewModels = extendedDeliverPreviewModels
            });

            var codeGenerator = new ExtendedDeliveryCodeGenerator(mockOptions.Object, new FileSystemOutputProvider(mockOptions.Object), _managementClient);

            await codeGenerator.RunAsync();

            Assert.Equal(NumberOfContentTypesWithDefaultContentItem, Directory.GetFiles(Path.GetFullPath(TempDir)).Length);

            Assert.NotEmpty(Directory.EnumerateFiles(Path.GetFullPath(TempDir), "*.Generated.cs"));
            Assert.NotEmpty(Directory.EnumerateFiles(Path.GetFullPath(TempDir)).Where(p => !p.Contains("*.Generated.cs")));

            // Cleanup
            Directory.Delete(TempDir, true);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task IntegrationTest_RunAsync_GeneratedSuffix_CorrectFiles(bool extendedDeliverPreviewModels)
        {
            const string transformFilename = "CustomSuffix";
            var mockOptions = new Mock<IOptions<CodeGeneratorOptions>>();
            mockOptions.Setup(x => x.Value).Returns(new CodeGeneratorOptions
            {
                ExtendedDeliverModels = true,
                Namespace = "CustomNamespace",
                OutputDir = TempDir,
                ManagementApi = false,
                GeneratePartials = false,
                WithTypeProvider = false,
                StructuredModel = false,
                ManagementOptions = new ManagementOptions { ApiKey = "apiKey", ProjectId = ProjectId },
                FileNameSuffix = transformFilename,
                ExtendedDeliverPreviewModels = extendedDeliverPreviewModels
            });

            var codeGenerator = new ExtendedDeliveryCodeGenerator(mockOptions.Object, new FileSystemOutputProvider(mockOptions.Object), _managementClient);

            await codeGenerator.RunAsync();

            Assert.Equal(NumberOfContentTypesWithDefaultContentItem, Directory.GetFiles(Path.GetFullPath(TempDir)).Length);

            foreach (var filepath in Directory.EnumerateFiles(Path.GetFullPath(TempDir)).Where(f => !f.Contains($"{ContentItemClassCodeGenerator.DefaultContentItemClassName}.cs")))
            {
                Assert.EndsWith($".{transformFilename}.cs", Path.GetFileName(filepath));
            }

            // Cleanup
            Directory.Delete(TempDir, true);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task IntegrationTest_RunAsync_GeneratePartials_CorrectFiles(bool extendedDeliverPreviewModels)
        {
            const string transformFilename = "Generated";
            var mockOptions = new Mock<IOptions<CodeGeneratorOptions>>();
            mockOptions.Setup(x => x.Value).Returns(new CodeGeneratorOptions
            {
                ExtendedDeliverModels = true,
                Namespace = "CustomNamespace",
                OutputDir = TempDir,
                ManagementApi = false,
                GeneratePartials = true,
                WithTypeProvider = false,
                StructuredModel = false,
                ManagementOptions = new ManagementOptions { ApiKey = "apiKey", ProjectId = ProjectId },
                FileNameSuffix = transformFilename,
                ExtendedDeliverPreviewModels = extendedDeliverPreviewModels
            });

            var codeGenerator = new ExtendedDeliveryCodeGenerator(mockOptions.Object, new FileSystemOutputProvider(mockOptions.Object), _managementClient);

            await codeGenerator.RunAsync();

            var allFilesCount = Directory.GetFiles(Path.GetFullPath(TempDir), "*.cs").Length;
            var generatedCount = Directory.GetFiles(Path.GetFullPath(TempDir), $"*.{transformFilename}.cs").Length;
            Assert.Equal(allFilesCount, (generatedCount * 2) + 1);

            foreach (var filepath in Directory.EnumerateFiles(Path.GetFullPath(TempDir)).Where(f => !f.Contains($"{ContentItemClassCodeGenerator.DefaultContentItemClassName}.cs")))
            {
                var customFileExists = File.Exists(filepath.Replace($".{transformFilename}", ""));
                Assert.True(customFileExists);
            }

            // Cleanup
            Directory.Delete(TempDir, true);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task IntegrationTest_RunAsync_TypeProvider_CorrectFiles(bool extendedDeliverPreviewModels)
        {
            var mockOptions = new Mock<IOptions<CodeGeneratorOptions>>();
            mockOptions.Setup(x => x.Value).Returns(new CodeGeneratorOptions
            {
                ExtendedDeliverModels = true,
                Namespace = "CustomNamespace",
                OutputDir = TempDir,
                ManagementApi = false,
                GeneratePartials = false,
                WithTypeProvider = true,
                StructuredModel = false,
                ManagementOptions = new ManagementOptions { ApiKey = "apiKey", ProjectId = ProjectId },
                ExtendedDeliverPreviewModels = extendedDeliverPreviewModels
            });

            var codeGenerator = new ExtendedDeliveryCodeGenerator(mockOptions.Object, new FileSystemOutputProvider(mockOptions.Object), _managementClient);

            await codeGenerator.RunAsync();

            Assert.Equal(NumberOfContentTypesWithDefaultContentItem + 1, Directory.GetFiles(Path.GetFullPath(TempDir)).Length);

            Assert.NotEmpty(Directory.EnumerateFiles(Path.GetFullPath(TempDir), "*TypeProvider.cs"));

            // Cleanup
            Directory.Delete(TempDir, true);
        }
    }
}
