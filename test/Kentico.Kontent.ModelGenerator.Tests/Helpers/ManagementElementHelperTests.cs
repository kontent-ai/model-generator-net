using System;
using System.Collections.Generic;
using System.Linq;
using Kentico.Kontent.Management.Models.Types.Elements;
using Kentico.Kontent.Management.Models.TypeSnippets;
using Kentico.Kontent.ModelGenerator.Core.Helpers;
using Xunit;

namespace Kentico.Kontent.ModelGenerator.Tests.Helpers
{
    public class ManagementElementHelperTests
    {
        [Fact]
        public void GetManagementContentTypeSnippetElements_ManagementSnippetsAreNull_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                ManagementElementHelper.GetManagementContentTypeSnippetElements(new TextElementMetadataModel(), null));
        }

        [Fact]
        public void GetManagementContentTypeSnippetElements_ManagementContentTypeIsNull_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                ManagementElementHelper.GetManagementContentTypeSnippetElements(null, new List<ContentTypeSnippetModel>()));
        }

        [Fact]
        public void GetManagementContentTypeSnippetElements_NoSnippetElements_Throws()
        {
            var contentTypeElementCodename = "codename";

            var snippets = new List<ContentTypeSnippetModel>
            {
                new ContentTypeSnippetModel
                {
                    Codename = contentTypeElementCodename,
                    Elements = new List<ElementMetadataBase>()
                }
            };

            var snippetElement = TestHelper.GenerateElementMetadataBase(Guid.NewGuid(), contentTypeElementCodename, ElementMetadataType.ContentTypeSnippet);

            var result = ManagementElementHelper.GetManagementContentTypeSnippetElements(snippetElement, snippets);

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public void GetManagementContentTypeSnippetElements_NoSnippets_Throws()
        {
            var contentTypeElementCodename = "codename";

            var snippetElement = TestHelper.GenerateElementMetadataBase(Guid.NewGuid(), contentTypeElementCodename, ElementMetadataType.ContentTypeSnippet);

            Assert.Throws<ArgumentException>(() => ManagementElementHelper.GetManagementContentTypeSnippetElements(snippetElement, new List<ContentTypeSnippetModel>()));
        }

        [Fact]
        public void GetManagementContentTypeSnippetElements_NoMatchingSnippet_Throws()
        {
            var contentTypeElementCodename = "codename";
            var snippetCodename = "other_snippet_codename";

            var snippetElement = TestHelper.GenerateElementMetadataBase(Guid.NewGuid(), contentTypeElementCodename, ElementMetadataType.ContentTypeSnippet);

            var snippets = new List<ContentTypeSnippetModel>
            {
                new ContentTypeSnippetModel
                {
                    Codename = snippetCodename,
                    Elements = new List<ElementMetadataBase>
                    {
                        TestHelper.GenerateElementMetadataBase(Guid.NewGuid(), $"{snippetCodename}el"),
                        TestHelper.GenerateElementMetadataBase(Guid.NewGuid(), $"{snippetCodename}el2", ElementMetadataType.Number)
                    }
                }
            };

            Assert.Throws<ArgumentException>(() => ManagementElementHelper.GetManagementContentTypeSnippetElements(snippetElement, snippets));
        }

        [Fact]
        public void GetManagementContentTypeSnippetElements_NotManagementContentTypeSnippetElement_ReturnsNull()
        {
            var snippetCodename = "codename";

            var element = TestHelper.GenerateElementMetadataBase(Guid.NewGuid(), snippetCodename);

            var result = ManagementElementHelper.GetManagementContentTypeSnippetElements(element, new List<ContentTypeSnippetModel>());

            Assert.Null(result);
        }

        [Fact]
        public void GetManagementContentTypeSnippetElements_Returns()
        {
            var expectedElementId = Guid.NewGuid();
            var expectedElement2Id = Guid.NewGuid();
            var snippetCodename = "snippet_codename";

            var snippetElement = TestHelper.GenerateElementMetadataBase(Guid.NewGuid(), snippetCodename, ElementMetadataType.ContentTypeSnippet);

            var snippets = new List<ContentTypeSnippetModel>
            {
                new ContentTypeSnippetModel
                {
                    Codename = snippetCodename,
                    Elements = new List<ElementMetadataBase>
                    {
                        TestHelper.GenerateElementMetadataBase(expectedElementId, $"{snippetCodename}_el"),
                        TestHelper.GenerateElementMetadataBase(expectedElement2Id, $"{snippetCodename}_el2", ElementMetadataType.Number)
                    }
                }
            };

            var result = ManagementElementHelper.GetManagementContentTypeSnippetElements(snippetElement, snippets).ToList();

            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.Contains(result, el => el.Id == expectedElementId);
            Assert.Contains(result, el => el.Id == expectedElement2Id);
        }
    }
}
