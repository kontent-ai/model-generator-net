using System.Reflection;
using Kontent.Ai.Management;
using Kontent.Ai.Management.Models.Shared;
using Kontent.Ai.Management.Models.TypeSnippets;
using Kontent.Ai.Management.Models.Types;
using Kontent.Ai.Management.Models.Types.Elements;
using LimitType = Kontent.Ai.Management.Models.Types.LimitType;
using Kontent.Ai.ModelGenerator.Core.Common;
using Kontent.Ai.ModelGenerator.Core.Configuration;
using Kontent.Ai.ModelGenerator.Core.Contract;
using Kontent.Ai.ModelGenerator.Core.Services;
using Microsoft.Extensions.Options;
using Moq;

namespace Kontent.Ai.ModelGenerator.Core.Tests;

public class ManagementCodeGeneratorTests
{
    private readonly Mock<IManagementClient> _client = new();
    private readonly Mock<IOutputProvider> _output = new();
    private readonly Mock<IUserMessageLogger> _logger = new();
    private readonly ClassDefinitionFactory _classDefinitionFactory = new();
    private readonly ClassCodeGeneratorFactory _classCodeGeneratorFactory = new();
    private readonly ManagementElementService _elementService = new();

    [Fact]
    public async Task RunAsync_TinyType_WritesOneFile()
    {
        SetupClientWithTypes(BuildArticleType());

        var sut = CreateGenerator(@namespace: "MyProject.Models");
        await sut.RunAsync();

        _output.Verify(
            o => o.Output(It.IsAny<string>(), "Article", true),
            Times.Once);
    }

    [Fact]
    public async Task RunAsync_EmittedCode_ContainsKontentTypeAttribute()
    {
        var typeId = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");
        var type = BuildArticleType(id: typeId);
        SetupClientWithTypes(type);
        string emitted = null;
        _output
            .Setup(o => o.Output(It.IsAny<string>(), "Article", true))
            .Callback<string, string, bool>((content, _, _) => emitted = content);

        await CreateGenerator(@namespace: "MyProject.Models").RunAsync();

        emitted.Should().NotBeNull();
        // Both args emitted: codename + id (positional).
        emitted.Should().Contain($"[KontentType(\"article\", \"{typeId}\")]");
        emitted.Should().Contain(": IElementsModel");
        emitted.Should().Contain("namespace MyProject.Models;");
    }

    [Fact]
    public async Task RunAsync_EmittedCode_HasOnePropertyPerSupportedElement()
    {
        SetupClientWithTypes(BuildArticleType());
        string emitted = null;
        _output
            .Setup(o => o.Output(It.IsAny<string>(), "Article", true))
            .Callback<string, string, bool>((content, _, _) => emitted = content);

        await CreateGenerator().RunAsync();

        emitted.Should().Contain("public string? Title { get; init; }");
        emitted.Should().Contain("public decimal? Priority { get; init; }");
        emitted.Should().Contain("public DateTimeValue? PublishedAt { get; init; }");
        // The title element carries a MAPI character limit; it must not surface as a client-side attribute.
        emitted.Should().NotContain("[StringLength");
    }

    [Fact]
    public async Task RunAsync_GuidelinesElement_SkippedSilently()
    {
        var type = new ContentTypeModel
        {
            Id = Guid.NewGuid(),
            LastModified = default,
            Name = "Article",
            ContentGroups = [],
            Codename = "article",
            Elements =
            [
                WithId(new TextElementMetadataModel { Name = "n", Codename = "title" }, Guid.NewGuid()),
                new GuidelinesElementMetadataModel { Guidelines = "g" },
            ],
        };
        SetupClientWithTypes(type);

        await CreateGenerator().RunAsync();

        // Guidelines is silently skipped — neither a warning nor an output.
        _logger.Verify(l => l.LogWarning(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task RunAsync_UnresolvableSnippetReference_LogsWarningAndSkips()
    {
        // After slice 7, every MAPI element type is handled. The closest "warn-and-skip" path
        // is a snippet element whose reference doesn't point at any snippet the generator fetched.
        var type = new ContentTypeModel
        {
            Id = Guid.NewGuid(),
            LastModified = default,
            Name = "Article",
            ContentGroups = [],
            Codename = "article",
            Elements =
            [
                WithId(new TextElementMetadataModel { Name = "n", Codename = "title" }, Guid.NewGuid()),
                WithId(
                    new ContentTypeSnippetElementMetadataModel
                    {
                        SnippetIdentifier = Reference.ByCodename("ghost_snippet"),
                    },
                    Guid.NewGuid()),
            ],
        };
        SetupClientWithTypes(type);

        await CreateGenerator().RunAsync();

        _logger.Verify(
            l => l.LogWarning(It.Is<string>(s => s.Contains("snippet"))),
            Times.Once);
    }

    [Fact]
    public async Task RunAsync_NoTypes_NoOutput()
    {
        SetupClientWithTypes();

        await CreateGenerator().RunAsync();

        _output.Verify(o => o.Output(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
    }

    [Fact]
    public async Task RunAsync_MultipleChoiceElement_EmitsPropertyAndSiblingEnum()
    {
        var type = new ContentTypeModel
        {
            Id = Guid.NewGuid(),
            LastModified = default,
            Name = "Article",
            ContentGroups = [],
            Codename = "article",
            Elements =
            [
                WithId(new MultipleChoiceElementMetadataModel
                {
                    Name = "n",
                    Codename = "category",
                    Mode = MultipleChoiceMode.Single,
                    Options =
                    [
                        new MultipleChoiceOptionModel
                        {
                            Name = "n",
                            Codename = "news",
                            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                        },
                        new MultipleChoiceOptionModel
                        {
                            Name = "n",
                            Codename = "release_note",
                            Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                        },
                    ],
                }, Guid.NewGuid()),
            ],
        };
        SetupClientWithTypes(type);
        string emitted = null;
        _output
            .Setup(o => o.Output(It.IsAny<string>(), "Article", true))
            .Callback<string, string, bool>((content, _, _) => emitted = content);

        await CreateGenerator().RunAsync();

        emitted.Should().NotBeNull();
        emitted.Should().Contain("public IEnumerable<ArticleCategory>? Category { get; init; }");
        emitted.Should().Contain("public enum ArticleCategory");
        emitted.Should().Contain("News");
        emitted.Should().Contain("ReleaseNote");
        // Single-select is a server-side rule, not a generated [MaxElements(1)].
        emitted.Should().NotContain("[MaxElements");
    }

    [Fact]
    public async Task RunAsync_LinkedItemsAndTaxonomy_EmitExpectedShapes()
    {
        var type = new ContentTypeModel
        {
            Id = Guid.NewGuid(),
            LastModified = default,
            Name = "Article",
            ContentGroups = [],
            Codename = "article",
            Elements =
            [
                WithId(new LinkedItemsElementMetadataModel
                {
                    Name = "n",
                    Codename = "related",
                    AllowedContentTypes = [Reference.ByCodename("article"), Reference.ByCodename("blog_post")],
                    ItemCountLimit = new LimitModel { Value = 3, Condition = LimitType.AtMost },
                }, Guid.NewGuid()),
                WithId(new TaxonomyElementMetadataModel
                {
                    Codename = "tags",
                    TaxonomyGroup = Reference.ByCodename("content_tags"),
                    TermCountLimit = new LimitModel { Value = 1, Condition = LimitType.AtLeast },
                }, Guid.NewGuid()),
            ],
        };
        SetupClientWithTypes(type);
        string emitted = null;
        _output
            .Setup(o => o.Output(It.IsAny<string>(), "Article", true))
            .Callback<string, string, bool>((content, _, _) => emitted = content);

        await CreateGenerator().RunAsync();

        emitted.Should().NotBeNull();
        emitted.Should().Contain("public IEnumerable<Reference>? Related { get; init; }");
        emitted.Should().Contain("public IEnumerable<Reference>? Tags { get; init; }");
        // Allowed types, count limits, and taxonomy group are all server-enforced — none emitted.
        emitted.Should().NotContain("[AllowedTypes");
        emitted.Should().NotContain("[MaxElements");
        emitted.Should().NotContain("[AllowedTaxonomyGroup");
        emitted.Should().NotContain("[MinElements");
    }

    [Fact]
    public async Task RunAsync_RichTextAndAsset_EmitExpectedShapes()
    {
        var type = new ContentTypeModel
        {
            Id = Guid.NewGuid(),
            LastModified = default,
            Name = "Article",
            ContentGroups = [],
            Codename = "article",
            Elements =
            [
                WithId(new RichTextElementMetadataModel
                {
                    Name = "n",
                    Codename = "body",
                    AllowedContentTypes = [Reference.ByCodename("banner")],
                    AllowedItemLinkTypes = [Reference.ByCodename("article")],
                    MaximumTextLength = new MaximumTextLengthModel { Value = 5000, AppliesTo = TextLengthLimitType.Characters },
                }, Guid.NewGuid()),
                WithId(new AssetElementMetadataModel
                {
                    Name = "n",
                    Codename = "featured_image",
                    AssetCountLimit = new LimitModel { Value = 1, Condition = LimitType.AtMost },
                    MaximumFileSize = 5_242_880L,
                    AllowedFileTypes = FileType.Adjustable,
                }, Guid.NewGuid()),
            ],
        };
        SetupClientWithTypes(type);
        string emitted = null;
        _output
            .Setup(o => o.Output(It.IsAny<string>(), "Article", true))
            .Callback<string, string, bool>((content, _, _) => emitted = content);

        await CreateGenerator().RunAsync();

        emitted.Should().NotBeNull();
        emitted.Should().Contain("public RichTextValue? Body { get; init; }");
        emitted.Should().Contain("public IEnumerable<AssetReference>? FeaturedImage { get; init; }");
        // Every rich-text / asset constraint is server-enforced — only the identity attribute is emitted.
        emitted.Should().NotContain("[AllowedTypes");
        emitted.Should().NotContain("[AllowedItemLinkTypes");
        emitted.Should().NotContain("[StringLength");
        emitted.Should().NotContain("[MaxElements");
        emitted.Should().NotContain("[MaxAssetSize");
        emitted.Should().NotContain("[AllowedAssetFileTypes");
    }

    [Fact]
    public async Task RunAsync_SnippetReference_InlinesPrefixedElements()
    {
        var seoSnippet = new ContentTypeSnippetModel
        {
            Id = Guid.NewGuid(),
            LastModified = default,
            Name = "SEO",
            Codename = "seo",
            // MAPI returns snippet element codenames already prefixed with the snippet codename.
            Elements =
            [
                WithId(new TextElementMetadataModel { Name = "n", Codename = "seo__meta_title" }, Guid.NewGuid()),
                WithId(new TextElementMetadataModel { Name = "n", Codename = "seo__meta_description" }, Guid.NewGuid()),
            ],
        };
        var type = new ContentTypeModel
        {
            Id = Guid.NewGuid(),
            LastModified = default,
            Name = "Article",
            ContentGroups = [],
            Codename = "article",
            Elements =
            [
                WithId(new TextElementMetadataModel { Name = "n", Codename = "title" }, Guid.NewGuid()),
                WithId(
                    new ContentTypeSnippetElementMetadataModel { SnippetIdentifier = Reference.ById(seoSnippet.Id) },
                    Guid.NewGuid()),
            ],
        };
        SetupClientWith(types: [type], snippets: [seoSnippet]);

        string emitted = null;
        _output
            .Setup(o => o.Output(It.IsAny<string>(), "Article", true))
            .Callback<string, string, bool>((content, _, _) => emitted = content);

        await CreateGenerator().RunAsync();

        emitted.Should().NotBeNull();
        emitted.Should().Contain("public string? Title { get; init; }");
        // Snippet-contributed codename arrives from MAPI already `seo__`-prefixed and passes
        // through verbatim — not re-prefixed (regression guard against double `seo__seo__`).
        emitted.Should().Contain("public string? SeoMetaTitle { get; init; }");
        emitted.Should().Contain("[KontentElement(\"seo__meta_title\"");
        emitted.Should().Contain("public string? SeoMetaDescription { get; init; }");
        emitted.Should().Contain("[KontentElement(\"seo__meta_description\"");
    }

    [Fact]
    public async Task RunAsync_SnippetReferenceByCodename_ResolvesCorrectly()
    {
        var snippet = new ContentTypeSnippetModel
        {
            Id = Guid.NewGuid(),
            LastModified = default,
            Name = "SEO",
            Codename = "seo",
            Elements = [WithId(new TextElementMetadataModel { Name = "n", Codename = "seo__meta_title" }, Guid.NewGuid())],
        };
        var type = new ContentTypeModel
        {
            Id = Guid.NewGuid(),
            LastModified = default,
            Name = "Article",
            ContentGroups = [],
            Codename = "article",
            Elements =
            [
                WithId(
                    new ContentTypeSnippetElementMetadataModel { SnippetIdentifier = Reference.ByCodename("seo") },
                    Guid.NewGuid()),
            ],
        };
        SetupClientWith(types: [type], snippets: [snippet]);

        string emitted = null;
        _output
            .Setup(o => o.Output(It.IsAny<string>(), "Article", true))
            .Callback<string, string, bool>((content, _, _) => emitted = content);

        await CreateGenerator().RunAsync();

        emitted.Should().Contain("SeoMetaTitle");
        _logger.Verify(l => l.LogWarning(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task RunAsync_MultipleTypes_WritesFileForEach()
    {
        // Listings are materialized (the modern client returns the whole IReadOnlyList in one
        // IManagementResult — no continuation-token paging to walk).
        SetupClientWithTypes(BuildArticleType("first"), BuildArticleType());

        await CreateGenerator().RunAsync();

        _output.Verify(o => o.Output(It.IsAny<string>(), "First", true), Times.Once);
        _output.Verify(o => o.Output(It.IsAny<string>(), "Article", true), Times.Once);
    }

    [Fact]
    public async Task RunAsync_FailedTypeListing_Throws()
    {
        _client.Setup(c => c.ListContentTypeSnippetsAsync())
            .ReturnsAsync(SuccessListing<IReadOnlyList<ContentTypeSnippetModel>>([]));
        _client.Setup(c => c.ListContentTypesAsync())
            .ReturnsAsync(FailedListing<IReadOnlyList<ContentTypeModel>>("Invalid API key."));

        var act = async () => await CreateGenerator().RunAsync();

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*content types*Invalid API key.*");
    }

    private void SetupClientWithTypes(params ContentTypeModel[] types)
    {
        SetupClientWith(types, snippets: []);
    }

    private void SetupClientWith(
        IEnumerable<ContentTypeModel> types,
        IEnumerable<ContentTypeSnippetModel> snippets)
    {
        _client.Setup(c => c.ListContentTypesAsync())
            .ReturnsAsync(SuccessListing<IReadOnlyList<ContentTypeModel>>(types.ToList()));
        _client.Setup(c => c.ListContentTypeSnippetsAsync())
            .ReturnsAsync(SuccessListing<IReadOnlyList<ContentTypeSnippetModel>>(snippets.ToList()));
    }

    private static IManagementResult<T> SuccessListing<T>(T value)
    {
        var mock = new Mock<IManagementResult<T>>();
        mock.SetupGet(m => m.IsSuccess).Returns(true);
        mock.SetupGet(m => m.Value).Returns(value);
        return mock.Object;
    }

    private static IManagementResult<T> FailedListing<T>(string message)
    {
        var error = new Mock<IError>();
        error.SetupGet(e => e.Message).Returns(message);

        var mock = new Mock<IManagementResult<T>>();
        mock.SetupGet(m => m.IsSuccess).Returns(false);
        mock.SetupGet(m => m.Error).Returns(error.Object);
        return mock.Object;
    }

    private static ContentTypeModel BuildArticleType(string codename = "article", Guid? id = null) => new()
    {
        // Guid.Empty by default — the generator only emits the type id attribute when Id != Empty.
        Id = id ?? Guid.Empty,
        LastModified = default,
        Name = codename,
        ContentGroups = [],
        Codename = codename,
        Elements =
        [
            WithId(
                new TextElementMetadataModel
                {
                    Name = "n",
                    Codename = "title",
                    MaximumTextLength = new MaximumTextLengthModel { Value = 100, AppliesTo = TextLengthLimitType.Characters },
                },
                Guid.NewGuid()),
            WithId(new NumberElementMetadataModel { Name = "n", Codename = "priority" }, Guid.NewGuid()),
            WithId(new DateTimeElementMetadataModel { Name = "n", Codename = "published_at" }, Guid.NewGuid()),
        ],
    };

    private ManagementCodeGenerator CreateGenerator(string @namespace = null)
    {
        var options = Microsoft.Extensions.Options.Options.Create(new CodeGeneratorOptions
        {
            Namespace = @namespace,
        });

        return new ManagementCodeGenerator(
            options,
            _output.Object,
            _client.Object,
            _classCodeGeneratorFactory,
            _classDefinitionFactory,
            _elementService,
            _logger.Object);
    }

    private static T WithId<T>(T element, Guid id) where T : ElementMetadataBase
    {
        typeof(ElementMetadataBase)
            .GetProperty(nameof(ElementMetadataBase.Id), BindingFlags.Public | BindingFlags.Instance)!
            .SetValue(element, id);
        return element;
    }
}
