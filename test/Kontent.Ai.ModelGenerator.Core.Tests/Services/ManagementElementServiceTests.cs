using Kontent.Ai.ModelGenerator.Core.Common;
using Kontent.Ai.ModelGenerator.Core.Services;

namespace Kontent.Ai.ModelGenerator.Core.Tests.Services;

public class ManagementElementServiceTests
{
    private readonly ManagementElementService _sut = new();

    [Fact]
    public void Build_NullInput_Throws()
    {
        var call = () => _sut.Build(null);

        call.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [MemberData(nameof(SimpleElements))]
    public void Build_SimpleElement_EmitsTypedPropertyWithOnlyKontentElement(
        ManagementElementInput input, string expectedTypeName, string expectedIdentifier)
    {
        var result = _sut.Build(input);

        result.Property.Codename.Should().Be(input.Codename);
        result.Property.Id.Should().Be(input.Id);
        result.Property.Identifier.Should().Be(expectedIdentifier);
        result.Property.TypeName.Should().Be(expectedTypeName);
        result.Property.Attributes.Should().ContainSingle();
        AssertIsKontentElement(result.Property.Attributes[0], input.Codename, input.Id);
        result.Enums.Should().BeEmpty();
    }

    public static TheoryData<ManagementElementInput, string, string> SimpleElements() => new()
    {
        { new TextElementInput("title", "t-id"), "string?", "Title" },
        { new NumberElementInput("priority", "n-id"), "decimal?", "Priority" },
        { new DateTimeElementInput("published_at", "d-id"), "DateTimeValue?", "PublishedAt" },
        { new CustomElementInput("color_picker", "c-id"), "CustomValue?", "ColorPicker" },
        { new UrlSlugElementInput("url_slug", "u-id"), "UrlSlugValue?", "UrlSlug" },
        { new LinkedItemsElementInput("related", "li-id"), "IEnumerable<Reference>?", "Related" },
        { new SubpagesElementInput("children", "sp-id"), "IEnumerable<Reference>?", "Children" },
        { new TaxonomyElementInput("tags", "tx-id"), "IEnumerable<Reference>?", "Tags" },
        { new RichTextElementInput("body", "rt-id"), "RichTextValue?", "Body" },
        { new AssetElementInput("featured_image", "a-id"), "IEnumerable<AssetReference>?", "FeaturedImage" },
    };

    #region MultipleChoice

    [Fact]
    public void MultipleChoice_EmitsListPropertyWithOnlyKontentElement()
    {
        var input = new MultipleChoiceElementInput(
            Codename: "category",
            Id: "mc-id",
            EnumTypeName: "ArticleCategory",
            Options:
            [
                new MultipleChoiceOptionInput("news", "opt-1"),
                new MultipleChoiceOptionInput("release", "opt-2"),
            ]);

        var result = _sut.Build(input);

        result.Property.TypeName.Should().Be("IEnumerable<ArticleCategory>?");
        result.Property.Attributes.Should().ContainSingle()
            .Which.Name.Should().Be("KontentElement");
    }

    [Fact]
    public void MultipleChoice_EmitsEnumWithPascalCaseMembers()
    {
        var input = new MultipleChoiceElementInput(
            Codename: "category",
            Id: "mc-id",
            EnumTypeName: "ArticleCategory",
            Options:
            [
                new MultipleChoiceOptionInput("news", "opt-1"),
                new MultipleChoiceOptionInput("release_note", "opt-2"),
                new MultipleChoiceOptionInput("n3", "opt-3"),
            ]);

        var result = _sut.Build(input);

        result.Enums.Should().ContainSingle();
        var enumDef = result.Enums[0];
        enumDef.Name.Should().Be("ArticleCategory");
        enumDef.Members.Select(m => m.Identifier).Should().Equal("News", "ReleaseNote", "N3");

        var newsAttr = enumDef.Members[0].Attributes.Should().ContainSingle().Subject;
        newsAttr.Name.Should().Be("KontentEnumValue");
        newsAttr.Arguments[0].Name.Should().BeNull();
        newsAttr.Arguments[0].Value.Should().Be("news");
        newsAttr.Arguments[1].Name.Should().BeNull();
        newsAttr.Arguments[1].Value.Should().Be("opt-1");
    }

    [Fact]
    public void MultipleChoice_EmptyEnumTypeName_Throws()
    {
        var input = new MultipleChoiceElementInput(
            Codename: "category",
            Id: "mc-id",
            EnumTypeName: "",
            Options: [new MultipleChoiceOptionInput("news", "opt-1")]);

        var call = () => _sut.Build(input);

        call.Should().Throw<ArgumentException>().WithMessage("*EnumTypeName*");
    }

    #endregion

    private static void AssertIsKontentElement(AttributeSpec attr, string codename, string id)
    {
        attr.Name.Should().Be("KontentElement");
        attr.Arguments.Should().HaveCount(2);
        attr.Arguments[0].Name.Should().BeNull();
        attr.Arguments[0].Value.Should().Be(codename);
        attr.Arguments[1].Name.Should().BeNull();
        attr.Arguments[1].Value.Should().Be(id);
    }
}
