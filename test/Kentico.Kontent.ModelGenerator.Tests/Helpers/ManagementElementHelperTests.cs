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
            var elementCodename = "codename";
            var snippetCodename = "snippet_codename_";

            var snippets = new List<ContentTypeSnippetModel>
            {
                new ContentTypeSnippetModel
                {
                    Codename = snippetCodename,
                    Elements = new List<ElementMetadataBase>()
                }
            };

            var snippetElement = TestHelper.GenerateElementMetadataBase(Guid.NewGuid(), elementCodename, ElementMetadataType.ContentTypeSnippet);

            Assert.Throws<ArgumentException>(() => ManagementElementHelper.GetManagementContentTypeSnippetElements(snippetElement, snippets));
        }

        [Fact]
        public void GetManagementContentTypeSnippetElements_NoMatchingSnippetElements_Throws()
        {
            var elementCodename = "codename";
            var snippetCodename = "snippet_codename_";

            var snippetElement = TestHelper.GenerateElementMetadataBase(Guid.NewGuid(), elementCodename, ElementMetadataType.ContentTypeSnippet);

            var snippets = new List<ContentTypeSnippetModel>
            {
                new ContentTypeSnippetModel
                {
                    Codename = snippetCodename,
                    Elements = new List<ElementMetadataBase>
                    {
                        TestHelper.GenerateElementMetadataBase(Guid.NewGuid(), $"{snippetCodename}other"),
                        TestHelper.GenerateElementMetadataBase(Guid.NewGuid(), $"{snippetCodename}other2", ElementMetadataType.Number)
                    }
                }
            };

            Assert.Throws<ArgumentException>(() => ManagementElementHelper.GetManagementContentTypeSnippetElements(snippetElement, snippets));
        }

        [Fact]
        public void GetManagementContentTypeSnippetElements_NoSnippets_Throws()
        {
            var elementCodename = "codename";

            var snippetElement = TestHelper.GenerateElementMetadataBase(Guid.NewGuid(), elementCodename, ElementMetadataType.ContentTypeSnippet);

            Assert.Throws<ArgumentException>(() => ManagementElementHelper.GetManagementContentTypeSnippetElements(snippetElement, new List<ContentTypeSnippetModel>()));
        }

        [Fact]
        public void GetManagementContentTypeSnippetElements_NoMatchingSnippet_Throws()
        {
            var elementCodename = "codename";
            var snippetCodename = "snippet_codename_";
            var snippetElementCodename = $"{snippetCodename}{elementCodename}";

            var snippetElement = TestHelper.GenerateElementMetadataBase(Guid.NewGuid(), elementCodename, ElementMetadataType.ContentTypeSnippet);

            var snippets = new List<ContentTypeSnippetModel>
            {
                new ContentTypeSnippetModel
                {
                    Codename = "other_snippet_codename",
                    Elements = new List<ElementMetadataBase>
                    {
                        TestHelper.GenerateElementMetadataBase(Guid.NewGuid(), snippetElementCodename),
                        TestHelper.GenerateElementMetadataBase(Guid.NewGuid(), snippetElementCodename, ElementMetadataType.Number)
                    }
                }
            };

            Assert.Throws<ArgumentException>(() => ManagementElementHelper.GetManagementContentTypeSnippetElements(snippetElement, snippets));
        }

        [Fact]
        public void GetManagementContentTypeSnippetElements_NotManagementContentTypeSnippetElement_ReturnsNull()
        {
            var elementCodename = "codename";

            var element = TestHelper.GenerateElementMetadataBase(Guid.NewGuid(), elementCodename);

            var result = ManagementElementHelper.GetManagementContentTypeSnippetElements(element, new List<ContentTypeSnippetModel>());

            Assert.Null(result);
        }

        [Fact]
        public void GetManagementContentTypeSnippetElements_ManagementContentTypeSnippetElement_Returns()
        {
            var elementCodename = "codename";
            var expectedElementId = Guid.NewGuid();
            var expectedElement2Id = Guid.NewGuid();
            var snippetCodename = "snippet_codename_";
            var snippetElementCodename = $"{snippetCodename}{elementCodename}";

            var snippetElement = TestHelper.GenerateElementMetadataBase(Guid.NewGuid(), elementCodename, ElementMetadataType.ContentTypeSnippet);

            var snippets = new List<ContentTypeSnippetModel>
            {
                new ContentTypeSnippetModel
                {
                    Codename = snippetCodename,
                    Elements = new List<ElementMetadataBase>
                    {
                        TestHelper.GenerateElementMetadataBase(expectedElementId, snippetElementCodename),
                        TestHelper.GenerateElementMetadataBase(expectedElement2Id, $"{snippetCodename}other", ElementMetadataType.Number)
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
