using System.Collections;
using System.Reflection;
using Kontent.Ai.Management;
using Kontent.Ai.Management.Models.Shared;
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
    public async Task RunAsync_EmittedCode_ContainsKontentContentTypeAttribute()
    {
        SetupClientWithTypes(BuildArticleType());
        string emitted = null;
        _output
            .Setup(o => o.Output(It.IsAny<string>(), "Article", true))
            .Callback<string, string, bool>((content, _, _) => emitted = content);

        await CreateGenerator(@namespace: "MyProject.Models").RunAsync();

        emitted.Should().NotBeNull();
        emitted.Should().Contain("[KontentContentType(Codename = \"article\")]");
        emitted.Should().Contain(": IContentItem");
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
        emitted.Should().Contain("[StringLength(100)]");
        emitted.Should().Contain("public decimal? Priority { get; init; }");
        emitted.Should().Contain("public DateTimeOffset? PublishedAt { get; init; }");
    }

    [Fact]
    public async Task RunAsync_GuidelinesElement_SkippedSilently()
    {
        var type = new ContentTypeModel
        {
            Codename = "article",
            Elements =
            [
                WithId(new TextElementMetadataModel { Codename = "title" }, Guid.NewGuid()),
                new GuidelinesElementMetadataModel(),
            ],
        };
        SetupClientWithTypes(type);

        await CreateGenerator().RunAsync();

        // Guidelines is silently skipped — neither a warning nor an output.
        _logger.Verify(l => l.LogWarning(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task RunAsync_UnsupportedElementType_LogsWarningAndSkips()
    {
        // After slice 6, the only remaining "unsupported" element type is the snippet
        // (slice 7 will expand it inline rather than emit a property for it).
        var type = new ContentTypeModel
        {
            Codename = "article",
            Elements =
            [
                WithId(new TextElementMetadataModel { Codename = "title" }, Guid.NewGuid()),
                WithId(new ContentTypeSnippetElementMetadataModel(), Guid.NewGuid()),
            ],
        };
        SetupClientWithTypes(type);
        string emitted = null;
        _output
            .Setup(o => o.Output(It.IsAny<string>(), "Article", true))
            .Callback<string, string, bool>((content, _, _) => emitted = content);

        await CreateGenerator().RunAsync();

        _logger.Verify(
            l => l.LogWarning(It.Is<string>(s => s.Contains("ContentTypeSnippet"))),
            Times.Once);
        emitted.Should().Contain("Title");
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
            Codename = "article",
            Elements =
            [
                WithId(new MultipleChoiceElementMetadataModel
                {
                    Codename = "category",
                    Mode = MultipleChoiceMode.Single,
                    Options =
                    [
                        new MultipleChoiceOptionModel
                        {
                            Codename = "news",
                            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                        },
                        new MultipleChoiceOptionModel
                        {
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
        emitted.Should().Contain("public IReadOnlyList<ArticleCategory>? Category { get; init; }");
        emitted.Should().Contain("[MaxElements(1)]");
        emitted.Should().Contain("public enum ArticleCategory");
        emitted.Should().Contain("News");
        emitted.Should().Contain("ReleaseNote");
    }

    [Fact]
    public async Task RunAsync_LinkedItemsAndTaxonomy_EmitExpectedShapes()
    {
        var type = new ContentTypeModel
        {
            Codename = "article",
            Elements =
            [
                WithId(new LinkedItemsElementMetadataModel
                {
                    Codename = "related",
                    AllowedTypes = [Reference.ByCodename("article"), Reference.ByCodename("blog_post")],
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
        emitted.Should().Contain("public IReadOnlyList<IContentItem>? Related { get; init; }");
        emitted.Should().Contain("[AllowedTypes(\"article\", \"blog_post\")]");
        emitted.Should().Contain("[MaxElements(3)]");
        emitted.Should().Contain("public IReadOnlyList<Reference>? Tags { get; init; }");
        emitted.Should().Contain("[AllowedTaxonomyGroup(\"content_tags\")]");
        emitted.Should().Contain("[MinElements(1)]");
    }

    [Fact]
    public async Task RunAsync_RichTextAndAsset_EmitExpectedShapes()
    {
        var type = new ContentTypeModel
        {
            Codename = "article",
            Elements =
            [
                WithId(new RichTextElementMetadataModel
                {
                    Codename = "body",
                    AllowedTypes = [Reference.ByCodename("banner")],
                    AllowedItemLinkTypes = [Reference.ByCodename("article")],
                    MaximumTextLength = new MaximumTextLengthModel { Value = 5000, AppliesTo = TextLengthLimitType.Characters },
                }, Guid.NewGuid()),
                WithId(new AssetElementMetadataModel
                {
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
        emitted.Should().Contain("public RichTextElement? Body { get; init; }");
        emitted.Should().Contain("[AllowedTypes(\"banner\")]");
        emitted.Should().Contain("[AllowedItemLinkTypes(\"article\")]");
        emitted.Should().Contain("[StringLength(5000)]");
        emitted.Should().Contain("public IReadOnlyList<AssetReference>? FeaturedImage { get; init; }");
        emitted.Should().Contain("[MaxElements(1)]");
        emitted.Should().Contain("[MaxAssetSize(5242880L)]");
        emitted.Should().Contain("[AllowedAssetFileTypes(AssetFileType.Adjustable)]");
    }

    [Fact]
    public async Task RunAsync_PaginatedListing_WalksAllPages()
    {
        var page2 = BuildListingPage(hasNext: false, types: [BuildArticleType()]);
        var page1 = BuildListingPage(hasNext: true, types: [BuildArticleType("first")], nextPage: page2);
        _client.Setup(c => c.ListContentTypesAsync()).ReturnsAsync(page1);

        await CreateGenerator().RunAsync();

        _output.Verify(o => o.Output(It.IsAny<string>(), "First", true), Times.Once);
        _output.Verify(o => o.Output(It.IsAny<string>(), "Article", true), Times.Once);
    }

    private void SetupClientWithTypes(params ContentTypeModel[] types)
    {
        var page = BuildListingPage(hasNext: false, types: types);
        _client.Setup(c => c.ListContentTypesAsync()).ReturnsAsync(page);
    }

    private static IListingResponseModel<ContentTypeModel> BuildListingPage(
        bool hasNext,
        IEnumerable<ContentTypeModel> types,
        IListingResponseModel<ContentTypeModel> nextPage = null)
    {
        var mock = new Mock<IListingResponseModel<ContentTypeModel>>();
        var list = types.ToList();
        mock.Setup(m => m.GetEnumerator()).Returns(() => list.GetEnumerator());
        mock.As<IEnumerable>().Setup(m => m.GetEnumerator()).Returns(() => list.GetEnumerator());
        mock.Setup(m => m.HasNextPage()).Returns(hasNext);
        mock.Setup(m => m.GetNextPage()).ReturnsAsync(nextPage);
        return mock.Object;
    }

    private static ContentTypeModel BuildArticleType(string codename = "article") => new()
    {
        Codename = codename,
        Elements =
        [
            WithId(
                new TextElementMetadataModel
                {
                    Codename = "title",
                    MaximumTextLength = new MaximumTextLengthModel { Value = 100, AppliesTo = TextLengthLimitType.Characters },
                },
                Guid.NewGuid()),
            WithId(new NumberElementMetadataModel { Codename = "priority" }, Guid.NewGuid()),
            WithId(new DateTimeElementMetadataModel { Codename = "published_at" }, Guid.NewGuid()),
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
