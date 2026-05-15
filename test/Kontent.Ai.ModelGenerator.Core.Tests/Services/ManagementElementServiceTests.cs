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

    #region Text

    [Fact]
    public void Text_NoConstraints_EmitsOnlyKontentElement()
    {
        var input = new TextElementInput("title", "abc-123");

        var result = _sut.Build(input);

        result.Property.Codename.Should().Be("title");
        result.Property.Id.Should().Be("abc-123");
        result.Property.Identifier.Should().Be("Title");
        result.Property.TypeName.Should().Be("string?");
        result.Property.Attributes.Should().HaveCount(1);
        AssertIsKontentElement(result.Property.Attributes[0], "title", "abc-123");
        result.Enums.Should().BeEmpty();
    }

    [Fact]
    public void Text_WithMaxCharacters_EmitsStringLength()
    {
        var input = new TextElementInput("title", "abc-123", MaximumCharacters: 100);

        var result = _sut.Build(input);

        result.Property.Attributes.Should().HaveCount(2);
        AssertIsKontentElement(result.Property.Attributes[0], "title", "abc-123");
        result.Property.Attributes[1].Name.Should().Be("StringLength");
        result.Property.Attributes[1].Arguments.Should().ContainSingle()
            .Which.Value.Should().Be(100);
        result.Property.Attributes[1].Arguments[0].Name.Should().BeNull();
    }

    [Fact]
    public void Text_WithRegex_EmitsRegularExpression()
    {
        var input = new TextElementInput("slug", "id", Regex: "^[a-z0-9-]+$");

        var result = _sut.Build(input);

        result.Property.Attributes.Should().HaveCount(2);
        AssertIsKontentElement(result.Property.Attributes[0], "slug", "id");
        result.Property.Attributes[1].Name.Should().Be("RegularExpression");
        result.Property.Attributes[1].Arguments.Should().ContainSingle()
            .Which.Value.Should().Be("^[a-z0-9-]+$");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Text_BlankOrNullRegex_NotEmitted(string regex)
    {
        var input = new TextElementInput("title", "id", Regex: regex);

        var result = _sut.Build(input);

        result.Property.Attributes.Should().ContainSingle();
    }

    [Fact]
    public void Text_BothConstraints_EmitsAllAttributesInOrder()
    {
        var input = new TextElementInput("title", "id", MaximumCharacters: 50, Regex: ".*");

        var result = _sut.Build(input);

        result.Property.Attributes.Select(a => a.Name).Should().Equal(
            "KontentElement", "StringLength", "RegularExpression");
    }

    #endregion

    [Fact]
    public void Number_EmitsDecimalNullable()
    {
        var result = _sut.Build(new NumberElementInput("priority", "n-id"));

        result.Property.TypeName.Should().Be("decimal?");
        result.Property.Attributes.Should().ContainSingle();
        AssertIsKontentElement(result.Property.Attributes[0], "priority", "n-id");
    }

    [Fact]
    public void DateTime_EmitsDateTimeOffsetNullable()
    {
        var result = _sut.Build(new DateTimeElementInput("published_at", "d-id"));

        result.Property.TypeName.Should().Be("DateTimeOffset?");
        result.Property.Attributes.Should().ContainSingle();
        AssertIsKontentElement(result.Property.Attributes[0], "published_at", "d-id");
    }

    [Fact]
    public void Custom_EmitsStringNullable()
    {
        var result = _sut.Build(new CustomElementInput("color_picker", "c-id"));

        result.Property.TypeName.Should().Be("string?");
        result.Property.Attributes.Should().ContainSingle();
        AssertIsKontentElement(result.Property.Attributes[0], "color_picker", "c-id");
    }

    #region UrlSlug

    [Fact]
    public void UrlSlug_NoRegex_EmitsOnlyKontentElement()
    {
        var result = _sut.Build(new UrlSlugElementInput("url_slug", "u-id"));

        result.Property.TypeName.Should().Be("string?");
        result.Property.Attributes.Should().ContainSingle();
        AssertIsKontentElement(result.Property.Attributes[0], "url_slug", "u-id");
    }

    [Fact]
    public void UrlSlug_WithRegex_EmitsRegularExpression()
    {
        var result = _sut.Build(new UrlSlugElementInput("url_slug", "u-id", Regex: "^[a-z]+$"));

        result.Property.Attributes.Should().HaveCount(2);
        result.Property.Attributes[1].Name.Should().Be("RegularExpression");
        result.Property.Attributes[1].Arguments[0].Value.Should().Be("^[a-z]+$");
    }

    #endregion

    #region MultipleChoice

    [Fact]
    public void MultipleChoice_Single_EmitsListPropertyWithMaxElementsOne()
    {
        var input = new MultipleChoiceElementInput(
            Codename: "category",
            Id: "mc-id",
            EnumTypeName: "ArticleCategory",
            IsSingleSelect: true,
            Options:
            [
                new MultipleChoiceOptionInput("news", "opt-1"),
                new MultipleChoiceOptionInput("release", "opt-2"),
            ]);

        var result = _sut.Build(input);

        result.Property.TypeName.Should().Be("IReadOnlyList<ArticleCategory>?");
        result.Property.Attributes.Select(a => a.Name).Should().Equal("KontentElement", "MaxElements");
        result.Property.Attributes[1].Arguments[0].Value.Should().Be(1);
    }

    [Fact]
    public void MultipleChoice_Multiple_NoCountAttribute()
    {
        var input = new MultipleChoiceElementInput(
            Codename: "tags",
            Id: "mc-id",
            EnumTypeName: "ArticleTags",
            IsSingleSelect: false,
            Options:
            [
                new MultipleChoiceOptionInput("a", "opt-a"),
                new MultipleChoiceOptionInput("b", "opt-b"),
            ]);

        var result = _sut.Build(input);

        result.Property.TypeName.Should().Be("IReadOnlyList<ArticleTags>?");
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
            IsSingleSelect: true,
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
            IsSingleSelect: true,
            Options: [new MultipleChoiceOptionInput("news", "opt-1")]);

        var call = () => _sut.Build(input);

        call.Should().Throw<ArgumentException>().WithMessage("*EnumTypeName*");
    }

    #endregion

    #region LinkedItems / Subpages

    [Fact]
    public void LinkedItems_NoConstraints_EmitsOnlyKontentElement()
    {
        var result = _sut.Build(new LinkedItemsElementInput("related", "li-id"));

        result.Property.TypeName.Should().Be("IReadOnlyList<Reference>?");
        result.Property.Attributes.Should().ContainSingle();
        AssertIsKontentElement(result.Property.Attributes[0], "related", "li-id");
    }

    [Fact]
    public void LinkedItems_WithAllowedTypes_EmitsAllowedTypesAttribute()
    {
        var result = _sut.Build(new LinkedItemsElementInput("related", "li-id",
            AllowedTypeCodenames: ["article", "blog_post"]));

        result.Property.Attributes.Should().HaveCount(2);
        result.Property.Attributes[1].Name.Should().Be("AllowedTypes");
        result.Property.Attributes[1].Arguments.Select(a => a.Value)
            .Should().Equal("article", "blog_post");
        result.Property.Attributes[1].Arguments.Should().AllSatisfy(a => a.Name.Should().BeNull());
    }

    [Theory]
    [InlineData(CountLimitMode.AtLeast, "MinElements")]
    [InlineData(CountLimitMode.AtMost, "MaxElements")]
    [InlineData(CountLimitMode.Exactly, "ExactElements")]
    public void LinkedItems_CountLimit_DispatchesToCorrectAttribute(CountLimitMode mode, string expectedAttrName)
    {
        var result = _sut.Build(new LinkedItemsElementInput("related", "li-id",
            ItemCount: new CountLimit(3, mode)));

        result.Property.Attributes.Should().HaveCount(2);
        result.Property.Attributes[1].Name.Should().Be(expectedAttrName);
        result.Property.Attributes[1].Arguments[0].Value.Should().Be(3);
    }

    [Fact]
    public void LinkedItems_AllConstraints_EmitsAllAttributesInOrder()
    {
        var result = _sut.Build(new LinkedItemsElementInput("related", "li-id",
            AllowedTypeCodenames: ["article"],
            ItemCount: new CountLimit(5, CountLimitMode.AtMost)));

        result.Property.Attributes.Select(a => a.Name)
            .Should().Equal("KontentElement", "AllowedTypes", "MaxElements");
    }

    [Fact]
    public void Subpages_SameShapeAsLinkedItems()
    {
        var result = _sut.Build(new SubpagesElementInput("children", "sp-id",
            AllowedTypeCodenames: ["page"],
            ItemCount: new CountLimit(10, CountLimitMode.AtMost)));

        result.Property.TypeName.Should().Be("IReadOnlyList<Reference>?");
        result.Property.Attributes.Select(a => a.Name)
            .Should().Equal("KontentElement", "AllowedTypes", "MaxElements");
    }

    [Fact]
    public void LinkedItems_EmptyAllowedTypes_NoAttribute()
    {
        var result = _sut.Build(new LinkedItemsElementInput("related", "li-id",
            AllowedTypeCodenames: []));

        result.Property.Attributes.Should().ContainSingle()
            .Which.Name.Should().Be("KontentElement");
    }

    #endregion

    #region Taxonomy

    [Fact]
    public void Taxonomy_NoConstraints_EmitsOnlyKontentElement()
    {
        var result = _sut.Build(new TaxonomyElementInput("tags", "tx-id"));

        result.Property.TypeName.Should().Be("IReadOnlyList<Reference>?");
        result.Property.Attributes.Should().ContainSingle();
        AssertIsKontentElement(result.Property.Attributes[0], "tags", "tx-id");
    }

    [Fact]
    public void Taxonomy_WithGroupCodename_EmitsAllowedTaxonomyGroup()
    {
        var result = _sut.Build(new TaxonomyElementInput("tags", "tx-id",
            TaxonomyGroup: "content_tags"));

        result.Property.Attributes.Should().HaveCount(2);
        result.Property.Attributes[1].Name.Should().Be("AllowedTaxonomyGroup");
        result.Property.Attributes[1].Arguments[0].Value.Should().Be("content_tags");
    }

    [Fact]
    public void Taxonomy_WithCountLimit_EmitsCountAttribute()
    {
        var result = _sut.Build(new TaxonomyElementInput("tags", "tx-id",
            TaxonomyGroup: "content_tags",
            TermCount: new CountLimit(1, CountLimitMode.AtLeast)));

        result.Property.Attributes.Select(a => a.Name)
            .Should().Equal("KontentElement", "AllowedTaxonomyGroup", "MinElements");
        result.Property.Attributes[2].Arguments[0].Value.Should().Be(1);
    }

    [Fact]
    public void Taxonomy_BlankGroup_NoAttribute()
    {
        var result = _sut.Build(new TaxonomyElementInput("tags", "tx-id", TaxonomyGroup: "   "));

        result.Property.Attributes.Should().ContainSingle();
    }

    #endregion

    #region RichText

    [Fact]
    public void RichText_NoConstraints_EmitsOnlyKontentElement()
    {
        var result = _sut.Build(new RichTextElementInput("body", "rt-id"));

        result.Property.TypeName.Should().Be("RichTextElement?");
        result.Property.Attributes.Should().ContainSingle();
        AssertIsKontentElement(result.Property.Attributes[0], "body", "rt-id");
    }

    [Fact]
    public void RichText_WithAllowedTypesAndLinkTypes_EmitsBothAttributes()
    {
        var result = _sut.Build(new RichTextElementInput("body", "rt-id",
            AllowedTypeCodenames: ["banner", "quote"],
            AllowedItemLinkTypeCodenames: ["article"]));

        result.Property.Attributes.Select(a => a.Name)
            .Should().Equal("KontentElement", "AllowedTypes", "AllowedItemLinkTypes");
        result.Property.Attributes[1].Arguments.Select(a => a.Value).Should().Equal("banner", "quote");
        result.Property.Attributes[2].Arguments.Select(a => a.Value).Should().Equal("article");
    }

    [Fact]
    public void RichText_WithMaxCharacters_EmitsStringLength()
    {
        var result = _sut.Build(new RichTextElementInput("body", "rt-id", MaximumCharacters: 5000));

        result.Property.Attributes.Select(a => a.Name)
            .Should().Equal("KontentElement", "StringLength");
        result.Property.Attributes[1].Arguments[0].Value.Should().Be(5000);
    }

    #endregion

    #region Asset

    [Fact]
    public void Asset_NoConstraints_EmitsOnlyKontentElement()
    {
        var result = _sut.Build(new AssetElementInput("featured_image", "a-id"));

        result.Property.TypeName.Should().Be("IReadOnlyList<AssetReference>?");
        result.Property.Attributes.Should().ContainSingle();
        AssertIsKontentElement(result.Property.Attributes[0], "featured_image", "a-id");
    }

    [Fact]
    public void Asset_WithMaxFileSize_EmitsMaxAssetSizeBytes()
    {
        var result = _sut.Build(new AssetElementInput("featured_image", "a-id",
            MaximumFileSizeBytes: 5_242_880L));

        result.Property.Attributes.Select(a => a.Name)
            .Should().Equal("KontentElement", "MaxAssetSize");
        result.Property.Attributes[1].Arguments[0].Value.Should().Be(5_242_880L);
    }

    [Fact]
    public void Asset_WithAdjustableConstraint_EmitsAllowedFileTypesAsRawEnumExpression()
    {
        var result = _sut.Build(new AssetElementInput("featured_image", "a-id",
            AllowedFileType: AssetFileType.Adjustable));

        result.Property.Attributes.Select(a => a.Name)
            .Should().Equal("KontentElement", "AllowedAssetFileTypes");
        // Raw-code marker — emitter parses it as a C# expression rather than a string literal.
        result.Property.Attributes[1].Arguments[0].Value
            .Should().BeOfType<RawCodeAttributeValue>()
            .Which.Expression.Should().Be("AssetFileType.Adjustable");
    }

    [Fact]
    public void Asset_AllConstraints_EmitsAllAttributesInOrder()
    {
        var result = _sut.Build(new AssetElementInput("gallery", "a-id",
            AssetCount: new CountLimit(10, CountLimitMode.AtMost),
            MaximumFileSizeBytes: 1_000_000L,
            AllowedFileType: AssetFileType.Adjustable));

        result.Property.Attributes.Select(a => a.Name).Should().Equal(
            "KontentElement", "MaxElements", "MaxAssetSize", "AllowedAssetFileTypes");
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
