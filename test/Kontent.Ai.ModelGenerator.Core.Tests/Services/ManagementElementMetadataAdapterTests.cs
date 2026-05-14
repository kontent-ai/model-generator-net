using System.Reflection;
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

        var input = (TextElementInput)ManagementElementMetadataAdapter.ToInput(element);

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

        var input = (TextElementInput)ManagementElementMetadataAdapter.ToInput(element);

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

        var input = (TextElementInput)ManagementElementMetadataAdapter.ToInput(element);

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

        var input = (TextElementInput)ManagementElementMetadataAdapter.ToInput(element);

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

        var input = (TextElementInput)ManagementElementMetadataAdapter.ToInput(element);

        input.Regex.Should().BeNull();
    }

    [Fact]
    public void ToInput_NumberElement_MapsCodenameAndId()
    {
        var element = WithId(new NumberElementMetadataModel { Codename = "priority" }, SampleId);

        var input = ManagementElementMetadataAdapter.ToInput(element);

        input.Should().BeOfType<NumberElementInput>();
        input.Codename.Should().Be("priority");
        input.Id.Should().Be(SampleId.ToString());
    }

    [Fact]
    public void ToInput_DateTimeElement_MapsCodenameAndId()
    {
        var element = WithId(new DateTimeElementMetadataModel { Codename = "published_at" }, SampleId);

        var input = ManagementElementMetadataAdapter.ToInput(element);

        input.Should().BeOfType<DateTimeElementInput>();
        input.Codename.Should().Be("published_at");
    }

    [Fact]
    public void ToInput_CustomElement_MapsCodenameAndId()
    {
        var element = WithId(new CustomElementMetadataModel { Codename = "color_picker" }, SampleId);

        var input = ManagementElementMetadataAdapter.ToInput(element);

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

        var input = (UrlSlugElementInput)ManagementElementMetadataAdapter.ToInput(element);

        input.Regex.Should().Be("^[a-z-]+$");
    }

    [Fact]
    public void ToInput_UrlSlugElement_NoRegex_RegexIsNull()
    {
        var element = WithId(new UrlSlugElementMetadataModel { Codename = "slug" }, SampleId);

        var input = (UrlSlugElementInput)ManagementElementMetadataAdapter.ToInput(element);

        input.Regex.Should().BeNull();
    }

    [Fact]
    public void ToInput_UnsupportedElementType_ReturnsNull()
    {
        // Linked items / asset / multiple choice / rich text / taxonomy / subpages / snippets /
        // guidelines aren't handled in slice 3; the orchestrator turns null into warn-and-skip.
        var element = WithId(new LinkedItemsElementMetadataModel { Codename = "related" }, SampleId);

        var input = ManagementElementMetadataAdapter.ToInput(element);

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
