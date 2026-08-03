using System.Reflection;
using Kontent.Ai.Management.Models.Shared;
using Kontent.Ai.Management.Models.Types.Elements;
using Kontent.Ai.ModelGenerator.Core.Common;
using Kontent.Ai.ModelGenerator.Core.Services;

namespace Kontent.Ai.ModelGenerator.Core.Tests.Services;

public class ManagementElementMetadataAdapterTests
{
    private static readonly Guid SampleId = Guid.Parse("11111111-2222-3333-4444-555555555555");

    [Fact]
    public void ToInput_TextElement_MapsToTextInput()
    {
        var element = WithId(new TextElementMetadataModel { Codename = "title", Name = "n" }, SampleId);

        var input = ManagementElementMetadataAdapter.ToInput(element, "Article");

        input.Should().BeOfType<TextElementInput>();
        input.Codename.Should().Be("title");
        input.Id.Should().Be(SampleId.ToString());
    }

    [Fact]
    public void ToInput_NumberElement_MapsToNumberInput()
    {
        var element = WithId(new NumberElementMetadataModel { Codename = "priority", Name = "n" }, SampleId);

        var input = ManagementElementMetadataAdapter.ToInput(element, "Article");

        input.Should().BeOfType<NumberElementInput>();
        input.Codename.Should().Be("priority");
        input.Id.Should().Be(SampleId.ToString());
    }

    [Fact]
    public void ToInput_DateTimeElement_MapsToDateTimeInput()
    {
        var element = WithId(new DateTimeElementMetadataModel { Codename = "published_at", Name = "n" }, SampleId);

        ManagementElementMetadataAdapter.ToInput(element, "Article").Should().BeOfType<DateTimeElementInput>();
    }

    [Fact]
    public void ToInput_CustomElement_MapsToCustomInput()
    {
        var element = WithId(new CustomElementMetadataModel { Codename = "color_picker", Name = "n", SourceUrl = "https://example.com" }, SampleId);

        ManagementElementMetadataAdapter.ToInput(element, "Article").Should().BeOfType<CustomElementInput>();
    }

    [Fact]
    public void ToInput_UrlSlugElement_MapsToUrlSlugInput()
    {
        var element = WithId(new UrlSlugElementMetadataModel { Codename = "slug", Name = "n", DependsOn = null }, SampleId);

        ManagementElementMetadataAdapter.ToInput(element, "Article").Should().BeOfType<UrlSlugElementInput>();
    }

    [Fact]
    public void ToInput_LinkedItems_MapsToLinkedItemsInput()
    {
        var element = WithId(new LinkedItemsElementMetadataModel { Codename = "related", Name = "n" }, SampleId);

        ManagementElementMetadataAdapter.ToInput(element, "Article").Should().BeOfType<LinkedItemsElementInput>();
    }

    [Fact]
    public void ToInput_Subpages_MapsToSubpagesInput()
    {
        var element = WithId(new SubpagesElementMetadataModel { Codename = "children", Name = "n" }, SampleId);

        ManagementElementMetadataAdapter.ToInput(element, "Article").Should().BeOfType<SubpagesElementInput>();
    }

    [Fact]
    public void ToInput_Taxonomy_MapsToTaxonomyInput()
    {
        var element = WithId(new TaxonomyElementMetadataModel { Codename = "tags", TaxonomyGroup = Reference.ByCodename("content_tags") }, SampleId);

        ManagementElementMetadataAdapter.ToInput(element, "Article").Should().BeOfType<TaxonomyElementInput>();
    }

    [Fact]
    public void ToInput_RichText_MapsToRichTextInput()
    {
        var element = WithId(new RichTextElementMetadataModel { Codename = "body", Name = "n" }, SampleId);

        ManagementElementMetadataAdapter.ToInput(element, "Article").Should().BeOfType<RichTextElementInput>();
    }

    [Fact]
    public void ToInput_Asset_MapsToAssetInput()
    {
        var element = WithId(new AssetElementMetadataModel { Codename = "featured_image", Name = "n" }, SampleId);

        ManagementElementMetadataAdapter.ToInput(element, "Article").Should().BeOfType<AssetElementInput>();
    }

    #region MultipleChoice

    [Fact]
    public void ToInput_MultipleChoice_MapsEnumNameAndOptions()
    {
        var element = WithId(new MultipleChoiceElementMetadataModel
        {
            Codename = "category",
            Name = "n",
            Mode = MultipleChoiceMode.Single,
            Options =
            [
                new MultipleChoiceOptionModel { Codename = "news", Name = "n", Id = Guid.Parse("11111111-1111-1111-1111-111111111111") },
                new MultipleChoiceOptionModel { Codename = "release_note", Name = "n", Id = Guid.Parse("22222222-2222-2222-2222-222222222222") },
            ],
        }, SampleId);

        var input = (MultipleChoiceElementInput)ManagementElementMetadataAdapter.ToInput(element, "Article");

        input.EnumTypeName.Should().Be("ArticleCategory");
        input.Options.Should().HaveCount(2);
        input.Options[0].Codename.Should().Be("news");
        input.Options[0].Id.Should().Be("11111111-1111-1111-1111-111111111111");
    }

    [Fact]
    public void ToInput_MultipleChoice_EnumNameDerivedFromContentTypeAndCodename()
    {
        var element = WithId(new MultipleChoiceElementMetadataModel
        {
            Codename = "tags",
            Name = "n",
            Mode = MultipleChoiceMode.Multiple,
            Options = [new MultipleChoiceOptionModel { Codename = "x", Name = "n", Id = Guid.NewGuid() }],
        }, SampleId);

        var input = (MultipleChoiceElementInput)ManagementElementMetadataAdapter.ToInput(element, "Article");

        input.EnumTypeName.Should().Be("ArticleTags");
    }

    [Fact]
    public void ToInput_MultipleChoice_NullOptions_TreatedAsEmpty()
    {
        var element = WithId(new MultipleChoiceElementMetadataModel
        {
            Codename = "category",
            Name = "n",
            Mode = MultipleChoiceMode.Single,
            Options = null,
        }, SampleId);

        var input = (MultipleChoiceElementInput)ManagementElementMetadataAdapter.ToInput(element, "Article");

        input.Options.Should().BeEmpty();
    }

    #endregion

    [Fact]
    public void ToInput_UnsupportedElementType_ReturnsNull()
    {
        // ContentTypeSnippet is expanded inline by the orchestrator rather than emitted as a property;
        // guidelines are dropped earlier in the orchestrator.
        var element = WithId(new ContentTypeSnippetElementMetadataModel { Snippet = null }, SampleId);

        ManagementElementMetadataAdapter.ToInput(element, "Article").Should().BeNull();
    }

    /// <summary>
    /// MAPI's <see cref="ElementMetadataBase.Id"/> has a private setter (only Newtonsoft can
    /// populate it during deserialization). Tests reach in via reflection.
    /// </summary>
    private static T WithId<T>(T element, Guid id) where T : ElementMetadataBase
    {
        typeof(ElementMetadataBase)
            .GetProperty(nameof(ElementMetadataBase.Id), BindingFlags.Public | BindingFlags.Instance)!
            .SetValue(element, id);
        return element;
    }
}
