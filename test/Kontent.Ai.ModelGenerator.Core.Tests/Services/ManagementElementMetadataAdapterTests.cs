using System.Reflection;
using Kontent.Ai.Management.Models.Shared;
using Kontent.Ai.Management.Models.Types;
using Kontent.Ai.Management.Models.Types.Elements;
using Kontent.Ai.ModelGenerator.Core.Common;
using Kontent.Ai.ModelGenerator.Core.Services;

namespace Kontent.Ai.ModelGenerator.Core.Tests.Services;

public class ManagementElementMetadataAdapterTests
{
    private static readonly Guid SampleId = Guid.Parse("11111111-2222-3333-4444-555555555555");

    [Fact]
    public void ToInput_TextElement_NoConstraints_MapsCodenameAndId()
    {
        var element = WithId(new TextElementMetadataModel { Codename = "title" }, SampleId);

        var input = (TextElementInput)ManagementElementMetadataAdapter.ToInput(element, "Article");

        input.Codename.Should().Be("title");
        input.Id.Should().Be(SampleId.ToString());
        input.MaximumCharacters.Should().BeNull();
        input.Regex.Should().BeNull();
    }

    [Fact]
    public void ToInput_TextElement_CharacterLimit_MapsToMaximumCharacters()
    {
        var element = WithId(
            new TextElementMetadataModel
            {
                Codename = "title",
                MaximumTextLength = new MaximumTextLengthModel { Value = 100, AppliesTo = TextLengthLimitType.Characters },
            },
            SampleId);

        var input = (TextElementInput)ManagementElementMetadataAdapter.ToInput(element, "Article");

        input.MaximumCharacters.Should().Be(100);
    }

    [Fact]
    public void ToInput_TextElement_WordLimit_Ignored()
    {
        // 'words' isn't expressible as [StringLength]; the adapter drops it per plan §3.
        var element = WithId(
            new TextElementMetadataModel
            {
                Codename = "title",
                MaximumTextLength = new MaximumTextLengthModel { Value = 100, AppliesTo = TextLengthLimitType.Words },
            },
            SampleId);

        var input = (TextElementInput)ManagementElementMetadataAdapter.ToInput(element, "Article");

        input.MaximumCharacters.Should().BeNull();
    }

    [Fact]
    public void ToInput_TextElement_ActiveRegex_MapsToRegex()
    {
        var element = WithId(
            new TextElementMetadataModel
            {
                Codename = "title",
                ValidationRegex = new ValidationRegexModel { Regex = "^[a-z]+$", IsActive = true },
            },
            SampleId);

        var input = (TextElementInput)ManagementElementMetadataAdapter.ToInput(element, "Article");

        input.Regex.Should().Be("^[a-z]+$");
    }

    [Fact]
    public void ToInput_TextElement_InactiveRegex_Ignored()
    {
        var element = WithId(
            new TextElementMetadataModel
            {
                Codename = "title",
                ValidationRegex = new ValidationRegexModel { Regex = "^[a-z]+$", IsActive = false },
            },
            SampleId);

        var input = (TextElementInput)ManagementElementMetadataAdapter.ToInput(element, "Article");

        input.Regex.Should().BeNull();
    }

    [Fact]
    public void ToInput_NumberElement_MapsCodenameAndId()
    {
        var element = WithId(new NumberElementMetadataModel { Codename = "priority" }, SampleId);

        var input = ManagementElementMetadataAdapter.ToInput(element, "Article");

        input.Should().BeOfType<NumberElementInput>();
        input.Codename.Should().Be("priority");
        input.Id.Should().Be(SampleId.ToString());
    }

    [Fact]
    public void ToInput_DateTimeElement_MapsCodenameAndId()
    {
        var element = WithId(new DateTimeElementMetadataModel { Codename = "published_at" }, SampleId);

        var input = ManagementElementMetadataAdapter.ToInput(element, "Article");

        input.Should().BeOfType<DateTimeElementInput>();
        input.Codename.Should().Be("published_at");
    }

    [Fact]
    public void ToInput_CustomElement_MapsCodenameAndId()
    {
        var element = WithId(new CustomElementMetadataModel { Codename = "color_picker" }, SampleId);

        var input = ManagementElementMetadataAdapter.ToInput(element, "Article");

        input.Should().BeOfType<CustomElementInput>();
    }

    [Fact]
    public void ToInput_UrlSlugElement_WithActiveRegex_MapsRegex()
    {
        var element = WithId(
            new UrlSlugElementMetadataModel
            {
                Codename = "slug",
                ValidationRegex = new ValidationRegexModel { Regex = "^[a-z-]+$", IsActive = true },
            },
            SampleId);

        var input = (UrlSlugElementInput)ManagementElementMetadataAdapter.ToInput(element, "Article");

        input.Regex.Should().Be("^[a-z-]+$");
    }

    [Fact]
    public void ToInput_UrlSlugElement_NoRegex_RegexIsNull()
    {
        var element = WithId(new UrlSlugElementMetadataModel { Codename = "slug" }, SampleId);

        var input = (UrlSlugElementInput)ManagementElementMetadataAdapter.ToInput(element, "Article");

        input.Regex.Should().BeNull();
    }

    [Fact]
    public void ToInput_MultipleChoice_Single_MapsModeAndOptions()
    {
        var element = WithId(new MultipleChoiceElementMetadataModel
        {
            Codename = "category",
            Mode = MultipleChoiceMode.Single,
            Options =
            [
                new MultipleChoiceOptionModel { Codename = "news", Id = Guid.Parse("11111111-1111-1111-1111-111111111111") },
                new MultipleChoiceOptionModel { Codename = "release_note", Id = Guid.Parse("22222222-2222-2222-2222-222222222222") },
            ],
        }, SampleId);

        var input = (MultipleChoiceElementInput)ManagementElementMetadataAdapter.ToInput(element, "Article");

        input.IsSingleSelect.Should().BeTrue();
        input.EnumTypeName.Should().Be("ArticleCategory");
        input.Options.Should().HaveCount(2);
        input.Options[0].Codename.Should().Be("news");
        input.Options[0].Id.Should().Be("11111111-1111-1111-1111-111111111111");
    }

    [Fact]
    public void ToInput_MultipleChoice_Multiple_FlagsMultiSelect()
    {
        var element = WithId(new MultipleChoiceElementMetadataModel
        {
            Codename = "tags",
            Mode = MultipleChoiceMode.Multiple,
            Options = [new MultipleChoiceOptionModel { Codename = "x", Id = Guid.NewGuid() }],
        }, SampleId);

        var input = (MultipleChoiceElementInput)ManagementElementMetadataAdapter.ToInput(element, "Article");

        input.IsSingleSelect.Should().BeFalse();
        input.EnumTypeName.Should().Be("ArticleTags");
    }

    [Fact]
    public void ToInput_MultipleChoice_NullOptions_TreatedAsEmpty()
    {
        var element = WithId(new MultipleChoiceElementMetadataModel
        {
            Codename = "category",
            Mode = MultipleChoiceMode.Single,
            Options = null,
        }, SampleId);

        var input = (MultipleChoiceElementInput)ManagementElementMetadataAdapter.ToInput(element, "Article");

        input.Options.Should().BeEmpty();
    }

    #region LinkedItems / Subpages / Taxonomy

    [Fact]
    public void ToInput_LinkedItems_MapsAllowedTypesByCodename()
    {
        var element = WithId(new LinkedItemsElementMetadataModel
        {
            Codename = "related",
            AllowedTypes =
            [
                Reference.ByCodename("article"),
                Reference.ByCodename("blog_post"),
            ],
        }, SampleId);

        var input = (LinkedItemsElementInput)ManagementElementMetadataAdapter.ToInput(element, "Article");

        input.AllowedTypeCodenames.Should().Equal("article", "blog_post");
    }

    [Fact]
    public void ToInput_LinkedItems_IdOnlyRefs_FilteredOut()
    {
        var element = WithId(new LinkedItemsElementMetadataModel
        {
            Codename = "related",
            AllowedTypes =
            [
                Reference.ByCodename("article"),
                Reference.ById(Guid.NewGuid()),  // id-only — should be dropped
            ],
        }, SampleId);

        var input = (LinkedItemsElementInput)ManagementElementMetadataAdapter.ToInput(element, "Article");

        input.AllowedTypeCodenames.Should().Equal("article");
    }

    [Fact]
    public void ToInput_LinkedItems_AllIdOnly_NoResolver_DropsAllAndWarns()
    {
        var element = WithId(new LinkedItemsElementMetadataModel
        {
            Codename = "related",
            AllowedTypes = [Reference.ById(Guid.NewGuid())],
        }, SampleId);
        var warnings = new List<string>();

        var input = (LinkedItemsElementInput)ManagementElementMetadataAdapter.ToInput(
            element, "Article", warn: warnings.Add);

        input.AllowedTypeCodenames.Should().BeNull();
        warnings.Should().ContainSingle().Which.Should().Contain("allowed-type reference");
    }

    [Fact]
    public void ToInput_LinkedItems_IdOnly_ResolverHydratesCodename()
    {
        var typeId = Guid.NewGuid();
        var element = WithId(new LinkedItemsElementMetadataModel
        {
            Codename = "related",
            AllowedTypes = [Reference.ById(typeId)],
        }, SampleId);
        var warnings = new List<string>();
        Func<Guid, string> resolver = id => id == typeId ? "article" : null;

        var input = (LinkedItemsElementInput)ManagementElementMetadataAdapter.ToInput(
            element, "Article", resolveTypeCodename: resolver, warn: warnings.Add);

        input.AllowedTypeCodenames.Should().Equal("article");
        warnings.Should().BeEmpty();
    }

    [Fact]
    public void ToInput_LinkedItems_MixedRefs_ResolverFillsMissingCodenames()
    {
        // First ref has codename; second is id-only and resolver supplies the codename.
        var idOnly = Guid.NewGuid();
        var element = WithId(new LinkedItemsElementMetadataModel
        {
            Codename = "related",
            AllowedTypes = [Reference.ByCodename("article"), Reference.ById(idOnly)],
        }, SampleId);
        Func<Guid, string> resolver = id => id == idOnly ? "blog_post" : null;

        var input = (LinkedItemsElementInput)ManagementElementMetadataAdapter.ToInput(
            element, "Article", resolveTypeCodename: resolver);

        input.AllowedTypeCodenames.Should().Equal("article", "blog_post");
    }

    [Fact]
    public void ToInput_LinkedItems_UnresolvableId_DropsEntryAndWarns()
    {
        var element = WithId(new LinkedItemsElementMetadataModel
        {
            Codename = "related",
            AllowedTypes = [Reference.ByCodename("article"), Reference.ById(Guid.NewGuid())],
        }, SampleId);
        var warnings = new List<string>();
        Func<Guid, string> resolver = _ => null;  // type was deleted; ref is stale

        var input = (LinkedItemsElementInput)ManagementElementMetadataAdapter.ToInput(
            element, "Article", resolveTypeCodename: resolver, warn: warnings.Add);

        input.AllowedTypeCodenames.Should().Equal("article");
        warnings.Should().ContainSingle()
            .Which.Should().Contain("related");
    }

    [Theory]
    [InlineData(LimitType.AtLeast, CountLimitMode.AtLeast)]
    [InlineData(LimitType.AtMost, CountLimitMode.AtMost)]
    [InlineData(LimitType.Exactly, CountLimitMode.Exactly)]
    public void ToInput_LinkedItems_CountLimit_MapsConditionCorrectly(LimitType mapiType, CountLimitMode expectedMode)
    {
        var element = WithId(new LinkedItemsElementMetadataModel
        {
            Codename = "related",
            ItemCountLimit = new LimitModel { Value = 3, Condition = mapiType },
        }, SampleId);

        var input = (LinkedItemsElementInput)ManagementElementMetadataAdapter.ToInput(element, "Article");

        input.ItemCount.Should().NotBeNull();
        input.ItemCount.Value.Should().Be(3);
        input.ItemCount.Mode.Should().Be(expectedMode);
    }

    [Fact]
    public void ToInput_Subpages_MapsToSubpagesInput()
    {
        var element = WithId(new SubpagesElementMetadataModel
        {
            Codename = "children",
            AllowedContentTypes = [Reference.ByCodename("page")],
            ItemCountLimit = new LimitModel { Value = 10, Condition = LimitType.AtMost },
        }, SampleId);

        var input = ManagementElementMetadataAdapter.ToInput(element, "Article");

        input.Should().BeOfType<SubpagesElementInput>();
        var sp = (SubpagesElementInput)input;
        sp.AllowedTypeCodenames.Should().Equal("page");
        sp.ItemCount.Value.Should().Be(10);
        sp.ItemCount.Mode.Should().Be(CountLimitMode.AtMost);
    }

    [Fact]
    public void ToInput_Taxonomy_GroupCodenamePreferred()
    {
        var element = WithId(new TaxonomyElementMetadataModel
        {
            Codename = "tags",
            TaxonomyGroup = Reference.ByCodename("content_tags"),
        }, SampleId);

        var input = (TaxonomyElementInput)ManagementElementMetadataAdapter.ToInput(element, "Article");

        input.TaxonomyGroup.Should().Be("content_tags");
    }

    [Fact]
    public void ToInput_Taxonomy_IdOnlyGroup_FallsBackToIdString()
    {
        var groupId = Guid.Parse("f30c7f72-e9ab-8832-2a57-62944a038809");
        var element = WithId(new TaxonomyElementMetadataModel
        {
            Codename = "tags",
            TaxonomyGroup = Reference.ById(groupId),
        }, SampleId);

        var input = (TaxonomyElementInput)ManagementElementMetadataAdapter.ToInput(element, "Article");

        input.TaxonomyGroup.Should().Be(groupId.ToString());
    }

    [Fact]
    public void ToInput_Taxonomy_NoGroup_GroupIsNull()
    {
        var element = WithId(new TaxonomyElementMetadataModel { Codename = "tags" }, SampleId);

        var input = (TaxonomyElementInput)ManagementElementMetadataAdapter.ToInput(element, "Article");

        input.TaxonomyGroup.Should().BeNull();
    }

    [Fact]
    public void ToInput_Taxonomy_WithTermCount_MapsCountLimit()
    {
        var element = WithId(new TaxonomyElementMetadataModel
        {
            Codename = "tags",
            TermCountLimit = new LimitModel { Value = 2, Condition = LimitType.Exactly },
        }, SampleId);

        var input = (TaxonomyElementInput)ManagementElementMetadataAdapter.ToInput(element, "Article");

        input.TermCount.Should().NotBeNull();
        input.TermCount.Value.Should().Be(2);
        input.TermCount.Mode.Should().Be(CountLimitMode.Exactly);
    }

    #endregion

    #region RichText / Asset

    [Fact]
    public void ToInput_RichText_MapsAllowedTypesAndItemLinkTypes()
    {
        var element = WithId(new RichTextElementMetadataModel
        {
            Codename = "body",
            AllowedTypes = [Reference.ByCodename("banner"), Reference.ByCodename("quote")],
            AllowedItemLinkTypes = [Reference.ByCodename("article")],
        }, SampleId);

        var input = (RichTextElementInput)ManagementElementMetadataAdapter.ToInput(element, "Article");

        input.AllowedTypeCodenames.Should().Equal("banner", "quote");
        input.AllowedItemLinkTypeCodenames.Should().Equal("article");
    }

    [Fact]
    public void ToInput_RichText_CharacterLimit_MapsMaximumCharacters()
    {
        var element = WithId(new RichTextElementMetadataModel
        {
            Codename = "body",
            MaximumTextLength = new MaximumTextLengthModel { Value = 5000, AppliesTo = TextLengthLimitType.Characters },
        }, SampleId);

        var input = (RichTextElementInput)ManagementElementMetadataAdapter.ToInput(element, "Article");

        input.MaximumCharacters.Should().Be(5000);
    }

    [Fact]
    public void ToInput_RichText_WordLimit_IgnoredLikeText()
    {
        var element = WithId(new RichTextElementMetadataModel
        {
            Codename = "body",
            MaximumTextLength = new MaximumTextLengthModel { Value = 5000, AppliesTo = TextLengthLimitType.Words },
        }, SampleId);

        var input = (RichTextElementInput)ManagementElementMetadataAdapter.ToInput(element, "Article");

        input.MaximumCharacters.Should().BeNull();
    }

    [Fact]
    public void ToInput_Asset_MapsCountLimitAndFileSize()
    {
        var element = WithId(new AssetElementMetadataModel
        {
            Codename = "featured_image",
            AssetCountLimit = new LimitModel { Value = 1, Condition = LimitType.AtMost },
            MaximumFileSize = 5_242_880L,
        }, SampleId);

        var input = (AssetElementInput)ManagementElementMetadataAdapter.ToInput(element, "Article");

        input.AssetCount.Value.Should().Be(1);
        input.AssetCount.Mode.Should().Be(CountLimitMode.AtMost);
        input.MaximumFileSizeBytes.Should().Be(5_242_880L);
    }

    [Theory]
    [InlineData(FileType.Any, null)]
    [InlineData(FileType.Adjustable, AssetFileType.Adjustable)]
    public void ToInput_Asset_FileTypeMapping(FileType mapiType, AssetFileType? expected)
    {
        var element = WithId(new AssetElementMetadataModel
        {
            Codename = "featured_image",
            AllowedFileTypes = mapiType,
        }, SampleId);

        var input = (AssetElementInput)ManagementElementMetadataAdapter.ToInput(element, "Article");

        input.AllowedFileType.Should().Be(expected);
    }

    #endregion

    [Fact]
    public void ToInput_UnsupportedElementType_ReturnsNull()
    {
        // After slice 6, only ContentTypeSnippet remains unsupported (slice 7 expands it inline
        // rather than emitting it as a property). Guidelines are dropped earlier in the orchestrator.
        var element = WithId(new ContentTypeSnippetElementMetadataModel(), SampleId);

        var input = ManagementElementMetadataAdapter.ToInput(element, "Article");

        input.Should().BeNull();
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
