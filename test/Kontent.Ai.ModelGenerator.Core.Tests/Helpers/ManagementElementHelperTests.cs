using FluentAssertions;
using Kontent.Ai.Management.Models.Types.Elements;
using Kontent.Ai.Management.Models.TypeSnippets;
using Kontent.Ai.ModelGenerator.Core.Helpers;
using Kontent.Ai.ModelGenerator.Core.Tests.TestHelpers;

namespace Kontent.Ai.ModelGenerator.Core.Tests.Helpers;

public class ManagementElementHelperTests
{
    [Fact]
    public void GetManagementContentTypeSnippetElements_ManagementSnippetsAreNull_ThrowsException()
    {
        var getManagementContentTypeSnippetElementsCall = () =>
            ManagementElementHelper.GetManagementContentTypeSnippetElements(new TextElementMetadataModel(), null);

        getManagementContentTypeSnippetElementsCall.Should().ThrowExactly<ArgumentNullException>();
    }

    [Fact]
    public void GetManagementContentTypeSnippetElements_ManagementContentTypeIsNull_ThrowsException()
    {
        var getManagementContentTypeSnippetElementsCall = () =>
            ManagementElementHelper.GetManagementContentTypeSnippetElements(null, new List<ContentTypeSnippetModel>());

        getManagementContentTypeSnippetElementsCall.Should().ThrowExactly<ArgumentNullException>();
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

        var snippetElement = TestDataGenerator.GenerateElementMetadataBase(Guid.NewGuid(), contentTypeElementCodename, ElementMetadataType.ContentTypeSnippet);

        var result = ManagementElementHelper.GetManagementContentTypeSnippetElements(snippetElement, snippets);

        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public void GetManagementContentTypeSnippetElements_NoSnippets_Throws()
    {
        var contentTypeElementCodename = "codename";

        var snippetElement = TestDataGenerator.GenerateElementMetadataBase(Guid.NewGuid(), contentTypeElementCodename, ElementMetadataType.ContentTypeSnippet);

        var getManagementContentTypeSnippetElementsCall =
            () => ManagementElementHelper.GetManagementContentTypeSnippetElements(snippetElement, new List<ContentTypeSnippetModel>());

        getManagementContentTypeSnippetElementsCall.Should().ThrowExactly<ArgumentException>();
    }

    [Fact]
    public void GetManagementContentTypeSnippetElements_NoMatchingSnippet_Throws()
    {
        var contentTypeElementCodename = "codename";
        var snippetCodename = "other_snippet_codename";

        var snippetElement = TestDataGenerator.GenerateElementMetadataBase(Guid.NewGuid(), contentTypeElementCodename, ElementMetadataType.ContentTypeSnippet);

        var snippets = new List<ContentTypeSnippetModel>
        {
            new ContentTypeSnippetModel
            {
                Codename = snippetCodename,
                Elements = new List<ElementMetadataBase>
                {
                    TestDataGenerator.GenerateElementMetadataBase(Guid.NewGuid(), $"{snippetCodename}el"),
                    TestDataGenerator.GenerateElementMetadataBase(Guid.NewGuid(), $"{snippetCodename}el2", ElementMetadataType.Number)
                }
            }
        };

        var getManagementContentTypeSnippetElementsCall = () => ManagementElementHelper.GetManagementContentTypeSnippetElements(snippetElement, snippets);

        getManagementContentTypeSnippetElementsCall.Should().ThrowExactly<ArgumentException>();
    }

    [Fact]
    public void GetManagementContentTypeSnippetElements_NotManagementContentTypeSnippetElement_ReturnsNull()
    {
        var snippetCodename = "codename";

        var element = TestDataGenerator.GenerateElementMetadataBase(Guid.NewGuid(), snippetCodename);

        var result = ManagementElementHelper.GetManagementContentTypeSnippetElements(element, new List<ContentTypeSnippetModel>());

        result.Should().BeNull();
    }

    [Fact]
    public void GetManagementContentTypeSnippetElements_Returns()
    {
        var expectedElementId = Guid.NewGuid();
        var expectedElement2Id = Guid.NewGuid();
        var snippetCodename = "snippet_codename";

        var snippetElement = TestDataGenerator.GenerateElementMetadataBase(Guid.NewGuid(), snippetCodename, ElementMetadataType.ContentTypeSnippet);

        var snippets = new List<ContentTypeSnippetModel>
        {
            new ContentTypeSnippetModel
            {
                Codename = snippetCodename,
                Elements = new List<ElementMetadataBase>
                {
                    TestDataGenerator.GenerateElementMetadataBase(expectedElementId, $"{snippetCodename}_el"),
                    TestDataGenerator.GenerateElementMetadataBase(expectedElement2Id, $"{snippetCodename}_el2", ElementMetadataType.Number)
                }
            }
        };

        var result = ManagementElementHelper.GetManagementContentTypeSnippetElements(snippetElement, snippets).ToList();

        result.Should().NotBeNullOrEmpty();
        result.Count.Should().Be(2);
        result.Should().ContainSingle(el => el.Id == expectedElementId);
        result.Should().ContainSingle(el => el.Id == expectedElement2Id);
    }
}
