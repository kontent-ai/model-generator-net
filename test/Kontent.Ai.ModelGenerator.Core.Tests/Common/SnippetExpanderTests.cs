using System.Reflection;
using Kontent.Ai.Management.Models.Shared;
using Kontent.Ai.Management.Models.TypeSnippets;
using Kontent.Ai.Management.Models.Types.Elements;
using Kontent.Ai.ModelGenerator.Core.Common;

namespace Kontent.Ai.ModelGenerator.Core.Tests.Common;

public class SnippetExpanderTests
{
    private readonly List<string> _warnings = [];

    [Fact]
    public void Expand_NullElements_YieldsNothing()
    {
        var result = SnippetExpander.Expand(null, _ => null, _warnings.Add).ToList();

        result.Should().BeEmpty();
        _warnings.Should().BeEmpty();
    }

    [Fact]
    public void Expand_NonSnippetElement_PassesThroughUnchanged()
    {
        var element = WithId(new TextElementMetadataModel { Codename = "title" }, Guid.NewGuid());

        var result = SnippetExpander.Expand([element], _ => null, _warnings.Add).ToList();

        result.Should().ContainSingle()
            .Which.Should().BeSameAs(element);
        _warnings.Should().BeEmpty();
    }

    [Fact]
    public void Expand_SnippetElement_InlinesInnerElementsWithMapiCodenamesUnchanged()
    {
        // MAPI returns snippet element codenames already prefixed with the snippet codename.
        var metaTitle = WithId(new TextElementMetadataModel { Codename = "seo__meta_title" }, Guid.NewGuid());
        var metaDescription = WithId(new TextElementMetadataModel { Codename = "seo__meta_description" }, Guid.NewGuid());
        var snippet = new ContentTypeSnippetModel
        {
            Codename = "seo",
            Elements = [metaTitle, metaDescription],
        };
        var snippetEl = WithId(
            new ContentTypeSnippetElementMetadataModel { SnippetIdentifier = Reference.ByCodename("seo") },
            Guid.NewGuid());

        var result = SnippetExpander.Expand([snippetEl], _ => snippet, _warnings.Add).ToList();

        result.Should().HaveCount(2);
        // Inlined by reference; codenames pass through verbatim — NOT re-prefixed to seo__seo__*.
        result[0].Should().BeSameAs(metaTitle);
        result[0].Codename.Should().Be("seo__meta_title");
        result[1].Should().BeSameAs(metaDescription);
        result[1].Codename.Should().Be("seo__meta_description");
        _warnings.Should().BeEmpty();
    }

    [Fact]
    public void Expand_MixedListWithSnippetAndOwnElements_PreservesOrderAndFlattens()
    {
        var title = WithId(new TextElementMetadataModel { Codename = "title" }, Guid.NewGuid());
        var snippetEl = WithId(
            new ContentTypeSnippetElementMetadataModel { SnippetIdentifier = Reference.ByCodename("seo") },
            Guid.NewGuid());
        var body = WithId(new TextElementMetadataModel { Codename = "body" }, Guid.NewGuid());

        var metaTitle = WithId(new TextElementMetadataModel { Codename = "seo__meta_title" }, Guid.NewGuid());
        var snippet = new ContentTypeSnippetModel
        {
            Codename = "seo",
            Elements = [metaTitle],
        };

        var result = SnippetExpander.Expand(
            [title, snippetEl, body],
            _ => snippet,
            _warnings.Add).ToList();

        result.Should().HaveCount(3);
        result[0].Should().BeSameAs(title);
        result[1].Should().BeSameAs(metaTitle);
        result[1].Codename.Should().Be("seo__meta_title");
        result[2].Should().BeSameAs(body);
        _warnings.Should().BeEmpty();
    }

    [Fact]
    public void Expand_UnresolvableSnippetReference_WarnsAndSkips()
    {
        var snippetEl = WithId(
            new ContentTypeSnippetElementMetadataModel { SnippetIdentifier = Reference.ByCodename("missing") },
            Guid.NewGuid());

        var result = SnippetExpander.Expand([snippetEl], _ => null, _warnings.Add).ToList();

        result.Should().BeEmpty();
        _warnings.Should().ContainSingle()
            .Which.Should().Contain("Could not resolve snippet reference");
    }

    [Fact]
    public void Expand_SnippetWithNullIdentifier_WarnsAndSkips()
    {
        var snippetEl = WithId(new ContentTypeSnippetElementMetadataModel(), Guid.NewGuid());

        var result = SnippetExpander.Expand([snippetEl], _ => null, _warnings.Add).ToList();

        result.Should().BeEmpty();
        _warnings.Should().ContainSingle();
    }

    [Fact]
    public void Expand_NestedSnippet_WarnsAndSkipsInnerSnippetElement()
    {
        var x = WithId(new TextElementMetadataModel { Codename = "outer__x" }, Guid.NewGuid());
        var outerSnippet = new ContentTypeSnippetModel
        {
            Codename = "outer",
            Elements =
            [
                x,
                // MAPI shouldn't allow this but defend if it ever appears.
                WithId(new ContentTypeSnippetElementMetadataModel(), Guid.NewGuid()),
            ],
        };
        var snippetEl = WithId(
            new ContentTypeSnippetElementMetadataModel { SnippetIdentifier = Reference.ByCodename("outer") },
            Guid.NewGuid());

        var result = SnippetExpander.Expand([snippetEl], _ => outerSnippet, _warnings.Add).ToList();

        result.Should().ContainSingle()
            .Which.Should().BeSameAs(x);
        _warnings.Should().ContainSingle()
            .Which.Should().Contain("nested snippet");
    }

    [Fact]
    public void Expand_GuidelinesInsideSnippet_SilentlyDropped()
    {
        var metaTitle = WithId(new TextElementMetadataModel { Codename = "seo__meta_title" }, Guid.NewGuid());
        var snippet = new ContentTypeSnippetModel
        {
            Codename = "seo",
            Elements =
            [
                metaTitle,
                new GuidelinesElementMetadataModel(),
            ],
        };
        var snippetEl = WithId(
            new ContentTypeSnippetElementMetadataModel { SnippetIdentifier = Reference.ByCodename("seo") },
            Guid.NewGuid());

        var result = SnippetExpander.Expand([snippetEl], _ => snippet, _warnings.Add).ToList();

        result.Should().ContainSingle()
            .Which.Should().BeSameAs(metaTitle);
        _warnings.Should().BeEmpty();
    }

    [Fact]
    public void Expand_NullResolver_Throws()
    {
        var call = () => SnippetExpander.Expand([], null, _warnings.Add).ToList();

        call.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Expand_NullWarn_Throws()
    {
        var call = () => SnippetExpander.Expand([], _ => null, null).ToList();

        call.Should().Throw<ArgumentNullException>();
    }

    private static T WithId<T>(T element, Guid id) where T : ElementMetadataBase
    {
        typeof(ElementMetadataBase)
            .GetProperty(nameof(ElementMetadataBase.Id), BindingFlags.Public | BindingFlags.Instance)!
            .SetValue(element, id);
        return element;
    }
}
