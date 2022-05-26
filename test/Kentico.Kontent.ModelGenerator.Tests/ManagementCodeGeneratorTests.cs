﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Kentico.Kontent.Management;
using Kentico.Kontent.Management.Configuration;
using Kentico.Kontent.Management.Models.Types;
using Kentico.Kontent.Management.Models.Types.Elements;
using Kentico.Kontent.Management.Models.TypeSnippets;
using Kentico.Kontent.ModelGenerator.Core;
using Kentico.Kontent.ModelGenerator.Core.Configuration;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Kentico.Kontent.ModelGenerator.Tests
{
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

            Assert.Throws<InvalidOperationException>(() => new ManagementCodeGenerator(mockOptions.Object, outputProvider.Object, _managementClient));
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

            Assert.Equal(NumberOfContentTypes, Directory.GetFiles(Path.GetFullPath(TempDir)).Length);

            Assert.NotEmpty(Directory.EnumerateFiles(Path.GetFullPath(TempDir), "*.Generated.cs"));
            Assert.NotEmpty(Directory.EnumerateFiles(Path.GetFullPath(TempDir)).Where(p => !p.Contains("*.Generated.cs")));

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
