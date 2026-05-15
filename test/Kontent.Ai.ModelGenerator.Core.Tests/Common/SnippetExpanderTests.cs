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
    public void Expand_NonSnippetElement_PassesThroughWithNoOverride()
    {
        var element = WithId(new TextElementMetadataModel { Codename = "title" }, Guid.NewGuid());

        var result = SnippetExpander.Expand([element], _ => null, _warnings.Add).ToList();

        result.Should().ContainSingle();
        result[0].Element.Should().BeSameAs(element);
        result[0].CodenameOverride.Should().BeNull();
    }

    [Fact]
    public void Expand_SnippetElement_InlinesInnerElementsWithPrefixedCodenames()
    {
        var snippet = new ContentTypeSnippetModel
        {
            Codename = "seo",
            Elements =
            [
                WithId(new TextElementMetadataModel { Codename = "meta_title" }, Guid.NewGuid()),
                WithId(new TextElementMetadataModel { Codename = "meta_description" }, Guid.NewGuid()),
            ],
        };
        var snippetEl = WithId(
            new ContentTypeSnippetElementMetadataModel { SnippetIdentifier = Reference.ByCodename("seo") },
            Guid.NewGuid());

        var result = SnippetExpander.Expand([snippetEl], _ => snippet, _warnings.Add).ToList();

        result.Should().HaveCount(2);
        result[0].CodenameOverride.Should().Be("seo__meta_title");
        result[1].CodenameOverride.Should().Be("seo__meta_description");
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

        var snippet = new ContentTypeSnippetModel
        {
            Codename = "seo",
            Elements = [WithId(new TextElementMetadataModel { Codename = "meta_title" }, Guid.NewGuid())],
        };

        var result = SnippetExpander.Expand(
            [title, snippetEl, body],
            _ => snippet,
            _warnings.Add).ToList();

        result.Should().HaveCount(3);
        result[0].CodenameOverride.Should().BeNull();
        result[1].CodenameOverride.Should().Be("seo__meta_title");
        result[2].CodenameOverride.Should().BeNull();
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
        var outerSnippet = new ContentTypeSnippetModel
        {
            Codename = "outer",
            Elements =
            [
                WithId(new TextElementMetadataModel { Codename = "x" }, Guid.NewGuid()),
                // MAPI shouldn't allow this but defend if it ever appears.
                WithId(new ContentTypeSnippetElementMetadataModel(), Guid.NewGuid()),
            ],
        };
        var snippetEl = WithId(
            new ContentTypeSnippetElementMetadataModel { SnippetIdentifier = Reference.ByCodename("outer") },
            Guid.NewGuid());

        var result = SnippetExpander.Expand([snippetEl], _ => outerSnippet, _warnings.Add).ToList();

        result.Should().ContainSingle()
            .Which.CodenameOverride.Should().Be("outer__x");
        _warnings.Should().ContainSingle()
            .Which.Should().Contain("nested snippet");
    }

    [Fact]
    public void Expand_GuidelinesInsideSnippet_SilentlyDropped()
    {
        var snippet = new ContentTypeSnippetModel
        {
            Codename = "seo",
            Elements =
            [
                WithId(new TextElementMetadataModel { Codename = "meta_title" }, Guid.NewGuid()),
                new GuidelinesElementMetadataModel(),
            ],
        };
        var snippetEl = WithId(
            new ContentTypeSnippetElementMetadataModel { SnippetIdentifier = Reference.ByCodename("seo") },
            Guid.NewGuid());

        var result = SnippetExpander.Expand([snippetEl], _ => snippet, _warnings.Add).ToList();

        result.Should().ContainSingle()
            .Which.CodenameOverride.Should().Be("seo__meta_title");
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
