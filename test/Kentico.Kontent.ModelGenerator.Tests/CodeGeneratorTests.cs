using System;
using System.Collections.Generic;
using System.IO;
using Kentico.Kontent.Delivery.Abstractions;
using Kentico.Kontent.Management;
using Kentico.Kontent.Management.Models.Types;
using Kentico.Kontent.Management.Models.TypeSnippets;
using Kentico.Kontent.ModelGenerator.Core;
using Kentico.Kontent.ModelGenerator.Core.Configuration;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Kentico.Kontent.ModelGenerator.Tests
{
    public class CodeGeneratorTests
    {
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
        [InlineData(true)]
        [InlineData(false)]
        public void GetClassCodeGenerator_Returns(bool contentManagementApi)
        {
            var mockOptions = new Mock<IOptions<CodeGeneratorOptions>>();
            mockOptions.SetupGet(option => option.Value).Returns(new CodeGeneratorOptions
            {
                ContentManagementApi = contentManagementApi,
                StructuredModel = true
            });

            var deliveryClient = new Mock<IDeliveryClient>();
            var outputProvider = new Mock<IOutputProvider>();
            var managementClient = new Mock<IManagementClient>();

            var elementCodename = "element_codename";
            var contentElement = new Mock<IContentElement>();
            contentElement.SetupGet(element => element.Type).Returns("text");
            contentElement.SetupGet(element => element.Codename).Returns(elementCodename);

            var contentType = new Mock<IContentType>();
            var contentTypeCodename = "Contenttype";
            contentType.SetupGet(type => type.System.Codename).Returns(contentTypeCodename);
            contentType.SetupGet(type => type.Elements).Returns(new Dictionary<string, IContentElement> { { elementCodename, contentElement.Object } });

            var codeGenerator = new CodeGenerator(mockOptions.Object, deliveryClient.Object, outputProvider.Object, managementClient.Object);

            var result = codeGenerator.GetClassCodeGenerator(contentType.Object, new List<ContentTypeSnippetModel>(), new ContentTypeModel());

            Assert.Equal($"{contentTypeCodename}.Generated", result.ClassFilename);
        }
    }
}
