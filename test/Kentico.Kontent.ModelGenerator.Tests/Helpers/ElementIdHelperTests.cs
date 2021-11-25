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
    public class ElementIdHelperTests
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void GetElementId_ManagementSnippetsAreNull_ThrowsException(bool cmApi)
        {
            Assert.Throws<ArgumentNullException>(() => ElementIdHelper.GetElementId(cmApi, null, new ContentTypeModel(), new FakeContentElement()));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void GetElementId_ManagementContentTypeIsNull_ThrowsException(bool cmApi)
        {
            Assert.Throws<ArgumentNullException>(() => ElementIdHelper.GetElementId(cmApi, new List<SnippetModel>(), null, new FakeContentElement()));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void GetElementId_ElementIsNull_ThrowsException(bool cmApi)
        {
            Assert.Throws<ArgumentNullException>(() => ElementIdHelper.GetElementId(cmApi, new List<SnippetModel>(), new ContentTypeModel(), null));
        }

        [Fact]
        public void GetElementId_NotCmApi_ReturnsNull()
        {
            var result = ElementIdHelper.GetElementId(false, new List<SnippetModel>(), new ContentTypeModel(), new FakeContentElement());

            Assert.Null(result);
        }

        [Fact]
        public void GetElementId_CmApi_ContentTypeElement_Returns()
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

            var element = new FakeContentElement
            {
                Codename = elementCodename
            };

            var result = ElementIdHelper.GetElementId(true, new List<SnippetModel>(), contentTypeModel, element);

            Assert.NotNull(result);
            Assert.Equal(expectedElementId.ToString(), result);
        }

        [Fact]
        public void GetElementId_CmApi_ContentTypeSnippetElement_Returns()
        {
            var elementCodename = "codename";
            var expectedElementId = Guid.NewGuid();
            var snippetCodename = "snippet_codename_";
            var snippetElementCodename = $"{snippetCodename}{elementCodename}";

            var contentTypeModel = new ContentTypeModel
            {
                Elements = new List<ElementMetadataBase>
                {
                    GenerateElementMetadataBase(Guid.NewGuid(), snippetCodename, ElementMetadataType.Snippet),
                    GenerateElementMetadataBase(Guid.NewGuid(), "other")
                }
            };

            var snippets = new List<SnippetModel>
            {
                new SnippetModel
                {
                    Codename = snippetCodename,
                    Elements = new List<ElementMetadataBase>
                    {
                        GenerateElementMetadataBase(expectedElementId, snippetElementCodename),
                        GenerateElementMetadataBase(Guid.NewGuid(), $"{snippetCodename}other", ElementMetadataType.Number)
                    }
                }
            };

            var element = new FakeContentElement
            {
                Codename = snippetElementCodename
            };

            var result = ElementIdHelper.GetElementId(true, snippets, contentTypeModel, element);

            Assert.NotNull(result);
            Assert.Equal(expectedElementId.ToString(), result);
        }

        [Fact]
        public void GetElementId_CmApi_NoSnippetElements_Throws()
        {
            var elementCodename = "codename";
            var snippetCodename = "snippet_codename_";
            var snippetElementCodename = $"{snippetCodename}{elementCodename}";

            var contentTypeModel = new ContentTypeModel
            {
                Elements = new List<ElementMetadataBase>
                {
                    GenerateElementMetadataBase(Guid.NewGuid(), snippetCodename, ElementMetadataType.Snippet),
                    GenerateElementMetadataBase(Guid.NewGuid(), "other")
                }
            };

            var snippets = new List<SnippetModel>
            {
                new SnippetModel
                {
                    Codename = snippetCodename,
                    Elements = new List<ElementMetadataBase>()
                }
            };

            var element = new FakeContentElement
            {
                Codename = snippetElementCodename
            };

            Assert.Throws<ArgumentException>(() => ElementIdHelper.GetElementId(true, snippets, contentTypeModel, element));
        }

        [Fact]
        public void GetElementId_CmApi_NoMatchingSnippetElements_Throws()
        {
            var elementCodename = "codename";
            var snippetCodename = "snippet_codename_";
            var snippetElementCodename = $"{snippetCodename}{elementCodename}";

            var contentTypeModel = new ContentTypeModel
            {
                Elements = new List<ElementMetadataBase>
                {
                    GenerateElementMetadataBase(Guid.NewGuid(), snippetCodename, ElementMetadataType.Snippet),
                    GenerateElementMetadataBase(Guid.NewGuid(), "other")
                }
            };

            var snippets = new List<SnippetModel>
            {
                new SnippetModel
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

            Assert.Throws<ArgumentException>(() => ElementIdHelper.GetElementId(true, snippets, contentTypeModel, element));
        }

        [Fact]
        public void GetElementId_CmApi_NoSnippets_Throws()
        {
            var elementCodename = "codename";
            var snippetCodename = "snippet_codename_";
            var snippetElementCodename = $"{snippetCodename}{elementCodename}";

            var contentTypeModel = new ContentTypeModel
            {
                Elements = new List<ElementMetadataBase>
                {
                    GenerateElementMetadataBase(Guid.NewGuid(), snippetCodename, ElementMetadataType.Snippet),
                    GenerateElementMetadataBase(Guid.NewGuid(), "other")
                }
            };

            var element = new FakeContentElement
            {
                Codename = snippetElementCodename
            };

            Assert.Throws<ArgumentException>(() => ElementIdHelper.GetElementId(true, new List<SnippetModel>(), contentTypeModel, element));
        }

        [Fact]
        public void GetElementId_CmApi_NoMatchingSnippets_Throws()
        {
            var elementCodename = "codename";
            var snippetCodename = "snippet_codename_";
            var snippetElementCodename = $"{snippetCodename}{elementCodename}";

            var contentTypeModel = new ContentTypeModel
            {
                Elements = new List<ElementMetadataBase>
                {
                    GenerateElementMetadataBase(Guid.NewGuid(), snippetCodename, ElementMetadataType.Snippet),
                    GenerateElementMetadataBase(Guid.NewGuid(), "other")
                }
            };

            var snippets = new List<SnippetModel>
            {
                new SnippetModel
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

            Assert.Throws<ArgumentException>(() => ElementIdHelper.GetElementId(true, snippets, contentTypeModel, element));
        }

        private static ElementMetadataBase GenerateElementMetadataBase(Guid elementId, string elementCodename, ElementMetadataType type = ElementMetadataType.Text) =>
            JObject.FromObject(new
            {
                Id = elementId,
                Codename = elementCodename,
                type
            }).ToObject<ElementMetadataBase>();
    }

    internal class FakeContentElement : IContentElement
    {
        public string Codename { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
    }
}
