using System;
using Kentico.Kontent.Management.Models.Types.Elements;
using Kentico.Kontent.ModelGenerator.Core.Configuration;
using Kentico.Kontent.ModelGenerator.Core.Helpers;
using Xunit;

namespace Kentico.Kontent.ModelGenerator.Tests.Helpers
{
    public class ElementTypeHelperTests
    {
        [Fact]
        public void GetElementType_OptionsIsNull_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() => ElementTypeHelper.GetElementType(null, "type", new TextElementMetadataModel()));
        }

        [Fact]
        public void GetElementType_ElementTypeIsNull_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() => ElementTypeHelper.GetElementType(new CodeGeneratorOptions(), null, new TextElementMetadataModel()));
        }

        [Fact]
        public void GetElementType_CmApiIsTrue_ManagementElementIsNull_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() => ElementTypeHelper.GetElementType(new CodeGeneratorOptions { ContentManagementApi = true }, "type", null));
        }

        [Fact]
        public void GetElementType_SubpagesElement_ReturnsSubpagesType()
        {
            var result = ElementTypeHelper.GetElementType(new CodeGeneratorOptions { ContentManagementApi = true }, "modular_content", new SubpagesElementMetadataModel());

            Assert.Equal("subpages", result);
        }

        [Fact]
        public void GetElementType_StructuredModel_ReturnsStructuredElementType()
        {
            var result = ElementTypeHelper.GetElementType(new CodeGeneratorOptions
            {
                ContentManagementApi = false,
                StructuredModel = true
            }, "rich_text", new RichTextElementMetadataModel());

            Assert.Equal("rich_text(structured)", result);
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(true, false)]
        [InlineData(false, false)]
        [InlineData(false, true)]
        public void GetElementType_Returns(bool contentManagementApi, bool structuredModel)
        {
            var result = ElementTypeHelper.GetElementType(new CodeGeneratorOptions
            {
                ContentManagementApi = contentManagementApi,
                StructuredModel = structuredModel
            }, "text", new TextElementMetadataModel());

            Assert.Equal("text", result);
        }
    }
}
