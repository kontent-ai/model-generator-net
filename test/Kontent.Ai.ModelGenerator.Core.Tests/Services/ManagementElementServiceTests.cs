using Kontent.Ai.ModelGenerator.Core.Common;
using Kontent.Ai.ModelGenerator.Core.Services;

namespace Kontent.Ai.ModelGenerator.Core.Tests.Services;

public class ManagementElementServiceTests
{
    private readonly ManagementElementService _sut = new();

    [Fact]
    public void BuildProperty_NullInput_Throws()
    {
        var call = () => _sut.BuildProperty(null);

        call.Should().Throw<ArgumentNullException>();
    }

    #region Text

    [Fact]
    public void Text_NoConstraints_EmitsOnlyKontentElement()
    {
        var input = new TextElementInput("title", "abc-123");

        var result = _sut.BuildProperty(input);

        result.Codename.Should().Be("title");
        result.Id.Should().Be("abc-123");
        result.Identifier.Should().Be("Title");
        result.TypeName.Should().Be("string?");
        result.Attributes.Should().HaveCount(1);
        AssertIsKontentElement(result.Attributes[0], "title", "abc-123");
    }

    [Fact]
    public void Text_WithMaxCharacters_EmitsStringLength()
    {
        var input = new TextElementInput("title", "abc-123", MaximumCharacters: 100);

        var result = _sut.BuildProperty(input);

        result.Attributes.Should().HaveCount(2);
        AssertIsKontentElement(result.Attributes[0], "title", "abc-123");
        result.Attributes[1].Name.Should().Be("StringLength");
        result.Attributes[1].Arguments.Should().ContainSingle()
            .Which.Value.Should().Be(100);
        result.Attributes[1].Arguments[0].Name.Should().BeNull();
    }

    [Fact]
    public void Text_WithRegex_EmitsRegularExpression()
    {
        var input = new TextElementInput("slug", "id", Regex: "^[a-z0-9-]+$");

        var result = _sut.BuildProperty(input);

        result.Attributes.Should().HaveCount(2);
        AssertIsKontentElement(result.Attributes[0], "slug", "id");
        result.Attributes[1].Name.Should().Be("RegularExpression");
        result.Attributes[1].Arguments.Should().ContainSingle()
            .Which.Value.Should().Be("^[a-z0-9-]+$");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Text_BlankOrNullRegex_NotEmitted(string regex)
    {
        var input = new TextElementInput("title", "id", Regex: regex);

        var result = _sut.BuildProperty(input);

        result.Attributes.Should().ContainSingle();
    }

    [Fact]
    public void Text_BothConstraints_EmitsAllAttributesInOrder()
    {
        var input = new TextElementInput("title", "id", MaximumCharacters: 50, Regex: ".*");

        var result = _sut.BuildProperty(input);

        result.Attributes.Select(a => a.Name).Should().Equal(
            "KontentElement", "StringLength", "RegularExpression");
    }

    #endregion

    [Fact]
    public void Number_EmitsDecimalNullable()
    {
        var result = _sut.BuildProperty(new NumberElementInput("priority", "n-id"));

        result.TypeName.Should().Be("decimal?");
        result.Attributes.Should().ContainSingle();
        AssertIsKontentElement(result.Attributes[0], "priority", "n-id");
    }

    [Fact]
    public void DateTime_EmitsDateTimeOffsetNullable()
    {
        var result = _sut.BuildProperty(new DateTimeElementInput("published_at", "d-id"));

        result.TypeName.Should().Be("DateTimeOffset?");
        result.Attributes.Should().ContainSingle();
        AssertIsKontentElement(result.Attributes[0], "published_at", "d-id");
    }

    [Fact]
    public void Custom_EmitsStringNullable()
    {
        var result = _sut.BuildProperty(new CustomElementInput("color_picker", "c-id"));

        result.TypeName.Should().Be("string?");
        result.Attributes.Should().ContainSingle();
        AssertIsKontentElement(result.Attributes[0], "color_picker", "c-id");
    }

    #region UrlSlug

    [Fact]
    public void UrlSlug_NoRegex_EmitsOnlyKontentElement()
    {
        var result = _sut.BuildProperty(new UrlSlugElementInput("url_slug", "u-id"));

        result.TypeName.Should().Be("string?");
        result.Attributes.Should().ContainSingle();
        AssertIsKontentElement(result.Attributes[0], "url_slug", "u-id");
    }

    [Fact]
    public void UrlSlug_WithRegex_EmitsRegularExpression()
    {
        var result = _sut.BuildProperty(new UrlSlugElementInput("url_slug", "u-id", Regex: "^[a-z]+$"));

        result.Attributes.Should().HaveCount(2);
        result.Attributes[1].Name.Should().Be("RegularExpression");
        result.Attributes[1].Arguments[0].Value.Should().Be("^[a-z]+$");
    }

    #endregion

    private static void AssertIsKontentElement(AttributeSpec attr, string codename, string id)
    {
        attr.Name.Should().Be("KontentElement");
        attr.Arguments.Should().HaveCount(2);
        attr.Arguments[0].Name.Should().Be("Codename");
        attr.Arguments[0].Value.Should().Be(codename);
        attr.Arguments[1].Name.Should().Be("Id");
        attr.Arguments[1].Value.Should().Be(id);
    }
}
