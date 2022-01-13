using System;
using System.Collections.Generic;
using Kentico.Kontent.Delivery.Abstractions;
using Kentico.Kontent.Management.Models.Types;
using Kentico.Kontent.Management.Models.Types.Elements;
using Kentico.Kontent.Management.Models.TypeSnippets;
using Kentico.Kontent.ModelGenerator.Core.Helpers;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Kentico.Kontent.ModelGenerator.Tests.Helpers
{
    public class ManagementElementHelperTests
    {
        [Fact]
        public void GetManagementElement_DeliverElementIsNull_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                ManagementElementHelper.GetManagementElement(true, null, new List<ContentTypeSnippetModel>(), new ContentTypeModel()));
        }

        [Fact]
        public void GetManagementElement_ManagementSnippetsAreNull_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                ManagementElementHelper.GetManagementElement(true, new FakeContentElement(), null, new ContentTypeModel()));
        }

        [Fact]
        public void GetManagementElement_ManagementContentTypeIsNull_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                ManagementElementHelper.GetManagementElement(true, new FakeContentElement(), new List<ContentTypeSnippetModel>(), null));
        }

        [Fact]
        public void GetManagementElement_NoSnippetElements_Throws()
        {
            var elementCodename = "codename";
            var snippetCodename = "snippet_codename_";
            var snippetElementCodename = $"{snippetCodename}{elementCodename}";

            var contentTypeModel = new ContentTypeModel
            {
                Elements = new List<ElementMetadataBase>
                {
                    GenerateElementMetadataBase(Guid.NewGuid(), snippetCodename, ElementMetadataType.ContentTypeSnippet),
                    GenerateElementMetadataBase(Guid.NewGuid(), "other")
                }
            };

            var snippets = new List<ContentTypeSnippetModel>
            {
                new ContentTypeSnippetModel
                {
                    Codename = snippetCodename,
                    Elements = new List<ElementMetadataBase>()
                }
            };

            var element = new FakeContentElement
            {
                Codename = snippetElementCodename
            };

            Assert.Throws<ArgumentException>(() => ManagementElementHelper.GetManagementElement(true, element, snippets, contentTypeModel));
        }

        [Fact]
        public void GetManagementElement_NoMatchingSnippetElements_Throws()
        {
            var elementCodename = "codename";
            var snippetCodename = "snippet_codename_";
            var snippetElementCodename = $"{snippetCodename}{elementCodename}";

            var contentTypeModel = new ContentTypeModel
            {
                Elements = new List<ElementMetadataBase>
                {
                    GenerateElementMetadataBase(Guid.NewGuid(), snippetCodename, ElementMetadataType.ContentTypeSnippet),
                    GenerateElementMetadataBase(Guid.NewGuid(), "other")
                }
            };

            var snippets = new List<ContentTypeSnippetModel>
            {
                new ContentTypeSnippetModel
                {
                    Codename = snippetCodename,
                    Elements = new List<ElementMetadataBase>
                    {
                        GenerateElementMetadataBase(Guid.NewGuid(), $"{snippetCodename}other"),
                        GenerateElementMetadataBase(Guid.NewGuid(), $"{snippetCodename}other2", ElementMetadataType.Number)
                    }
                }
            };

            var element = new FakeContentElement
            {
                Codename = snippetElementCodename
            };

            Assert.Throws<ArgumentException>(() => ManagementElementHelper.GetManagementElement(true, element, snippets, contentTypeModel));
        }

        [Fact]
        public void GetManagementElement_NoSnippets_Throws()
        {
            var elementCodename = "codename";
            var snippetCodename = "snippet_codename_";
            var snippetElementCodename = $"{snippetCodename}{elementCodename}";

            var contentTypeModel = new ContentTypeModel
            {
                Elements = new List<ElementMetadataBase>
                {
                    GenerateElementMetadataBase(Guid.NewGuid(), snippetCodename, ElementMetadataType.ContentTypeSnippet),
                    GenerateElementMetadataBase(Guid.NewGuid(), "other")
                }
            };

            var element = new FakeContentElement
            {
                Codename = snippetElementCodename
            };

            Assert.Throws<ArgumentException>(() => ManagementElementHelper.GetManagementElement(true, element, new List<ContentTypeSnippetModel>(), contentTypeModel));
        }

        [Fact]
        public void GetManagementElement_NoMatchingSnippet_Throws()
        {
            var elementCodename = "codename";
            var snippetCodename = "snippet_codename_";
            var snippetElementCodename = $"{snippetCodename}{elementCodename}";

            var contentTypeModel = new ContentTypeModel
            {
                Elements = new List<ElementMetadataBase>
                {
                    GenerateElementMetadataBase(Guid.NewGuid(), snippetCodename, ElementMetadataType.ContentTypeSnippet),
                    GenerateElementMetadataBase(Guid.NewGuid(), "other")
                }
            };

            var snippets = new List<ContentTypeSnippetModel>
            {
                new ContentTypeSnippetModel
                {
                    Codename = "other_snippet_codename",
                    Elements = new List<ElementMetadataBase>
                    {
                        GenerateElementMetadataBase(Guid.NewGuid(), snippetElementCodename),
                        GenerateElementMetadataBase(Guid.NewGuid(), snippetElementCodename, ElementMetadataType.Number)
                    }
                }
            };

            var element = new FakeContentElement
            {
                Codename = snippetElementCodename
            };

            Assert.Throws<ArgumentException>(() => ManagementElementHelper.GetManagementElement(true, element, snippets, contentTypeModel));
        }

        [Fact]
        public void GetManagementElement_CmApiIsFalse_DeliverElementIsNull_ThrowsException()
        {
            var result = ManagementElementHelper.GetManagementElement(false, null, new List<ContentTypeSnippetModel>(), new ContentTypeModel());

            Assert.Null(result);
        }

        [Fact]
        public void GetManagementElement_CmApiIsFalse_ReturnsNull()
        {
            var result = ManagementElementHelper.GetManagementElement(false, new FakeContentElement(), new List<ContentTypeSnippetModel>(), new ContentTypeModel());

            Assert.Null(result);
        }

        [Fact]
        public void GetManagementElement_CmApiIsFalse_ManagementSnippetsAreNull_ReturnsNull()
        {
            var result = ManagementElementHelper.GetManagementElement(false, new FakeContentElement(), null, new ContentTypeModel());

            Assert.Null(result);
        }

        [Fact]
        public void GetManagementElement_CmApiIsFalse_ManagementContentTypeIsNull_ReturnsNull()
        {
            var result = ManagementElementHelper.GetManagementElement(false, new FakeContentElement(), new List<ContentTypeSnippetModel>(), null);

            Assert.Null(result);
        }

        [Fact]
        public void GetManagementElement_ManagementContentTypeElement_Returns()
        {
            var elementCodename = "codename";
            var expectedElementId = Guid.NewGuid();

            var contentTypeModel = new ContentTypeModel
            {
                Elements = new List<ElementMetadataBase>
                {
                    GenerateElementMetadataBase(expectedElementId, elementCodename),
                    GenerateElementMetadataBase(Guid.NewGuid(), "other")
                }
            };

            var element = new ManagementElementHelperTests.FakeContentElement
            {
                Codename = elementCodename
            };

            var result = ManagementElementHelper.GetManagementElement(true, element, new List<ContentTypeSnippetModel>(), contentTypeModel);

            Assert.NotNull(result);
            Assert.Equal(expectedElementId, result.Id);
        }

        [Fact]
        public void GetManagementElement_ManagementContentTypeSnippetElement_Returns()
        {
            var elementCodename = "codename";
            var expectedElementId = Guid.NewGuid();
            var snippetCodename = "snippet_codename_";
            var snippetElementCodename = $"{snippetCodename}{elementCodename}";

            var contentTypeModel = new ContentTypeModel
            {
                Elements = new List<ElementMetadataBase>
                {
                    GenerateElementMetadataBase(Guid.NewGuid(), snippetCodename, ElementMetadataType.ContentTypeSnippet),
                    GenerateElementMetadataBase(Guid.NewGuid(), "other")
                }
            };

            var snippets = new List<ContentTypeSnippetModel>
            {
                new ContentTypeSnippetModel
                {
                    Codename = snippetCodename,
                    Elements = new List<ElementMetadataBase>
                    {
                        GenerateElementMetadataBase(expectedElementId, snippetElementCodename),
                        GenerateElementMetadataBase(Guid.NewGuid(), $"{snippetCodename}other", ElementMetadataType.Number)
                    }
                }
            };

            var element = new ManagementElementHelperTests.FakeContentElement
            {
                Codename = snippetElementCodename
            };

            var result = ManagementElementHelper.GetManagementElement(true, element, snippets, contentTypeModel);

            Assert.NotNull(result);
            Assert.Equal(expectedElementId, result.Id);
        }

        private static ElementMetadataBase GenerateElementMetadataBase(Guid elementId, string elementCodename, ElementMetadataType type = ElementMetadataType.Text) =>
            JObject.FromObject(new
            {
                Id = elementId,
                Codename = elementCodename,
                type
            }).ToObject<ElementMetadataBase>();

        internal class FakeContentElement : IContentElement
        {
            public string Codename { get; set; }
            public string Name { get; set; }
            public string Type { get; set; }
        }
    }
}
