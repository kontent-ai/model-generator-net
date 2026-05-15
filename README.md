# Kontent.ai model generator utility for .NET

[![NuGet][nuget-badge]][nuget-url]
[![License][license-badge]][license-url]
[![Build & Test][build-badge]][build-url]
[![codecov][codecov-badge]][codecov-url]
[![Contributors][contributors-badge]][contributors-url]
[![Last commit][last-commit-badge]][last-commit-url]
[![GitHub Issues][issues-badge]][issues-url]
[![Stack Overflow][stack-overflow-badge]][stack-overflow-url]
[![Discord][discord-badge]][discord-url]

This utility generates strongly-typed **record-based models** for:

- the [Kontent.ai Delivery SDK for .NET (v19+)](https://github.com/kontent-ai/delivery-sdk-net) — default mode, for reading content
- the [Kontent.ai Management SDK for .NET](https://github.com/kontent-ai/management-sdk-net) — opt-in mode (`-m` / `--management`), for CRUD workflows. Preview — coordinated with the upcoming `management-sdk-net` vnext release.

> [!IMPORTANT]
> Management mode emits code that references the future `IContentItem` marker, `[KontentContentType]` / `[KontentElement]` attributes, validator, and STJ converter, all shipped by the `management-sdk-net` vnext branch (phase 3). Until that release lands, generated management models won't compile against the published Management SDK (v8.2.0).
>
> If you need models for the legacy Delivery SDK (v18.x and earlier) or for Extended Delivery, use the [previous stable release](https://github.com/kontent-ai/model-generator-net/tree/9.0.0).

## What's New in Updated Delivery Models

The generated models use modern C# features and patterns:

- **Records** - Immutable `record` types with `{ get; init; }` accessors
- **Modern types** - `RichTextContent`, `Asset`, `TaxonomyTerm`, `IEmbeddedContent`
- **Partial records** - Easily extendable without modifying generated code
- **`ContentTypeCodename` attribute** - For source-generated TypeProvider discovery
- **`ContentTypeCodename` constant** - Access the content type codename at compile time (usable in `switch`/`case` labels, attribute arguments, and other contexts that require a compile-time constant) without reflection

> [!NOTE]
> If an element codename would produce a property or constant that collides with the built-in `ContentTypeCodename` constant (e.g., an element named `content_type_codename` or `content_type`), the element's member is automatically prefixed with an underscore (`_ContentTypeCodename`) to avoid conflicts. The `[JsonPropertyName]` attribute ensures deserialization still works correctly.

## Installation & Usage

### .NET Tool (Recommended)

The recommended way of obtaining this tool is installing it as a [.NET Tool](https://docs.microsoft.com/en-us/dotnet/core/tools/global-tools). You can install it as a global tool or per project as a local tool.

#### Global Tool

```bash
dotnet tool install -g Kontent.Ai.ModelGenerator
```

Delivery (default):

```bash
KontentModelGenerator --environmentId "<environmentId>" \
    [--namespace "<custom-namespace>"] \
    [--outputdir "<output-directory>"] \
    [--baseRecord "<base-record-name>"] \
    [--nullability strict|semantic]
```

Management (preview — see [Management Models](#management-models)):

```bash
KontentModelGenerator --management \
    --environmentId "<environmentId>" \
    --apiKey "<management-api-key>" \
    [--namespace "<custom-namespace>"] \
    [--outputdir "<output-directory>"]
```

#### Local Tool

```bash
dotnet new tool-manifest
dotnet tool install Kontent.Ai.ModelGenerator
```

```bash
dotnet tool run KontentModelGenerator --environmentId "<environmentId>" \
    [--namespace "<custom-namespace>"] \
    [--outputdir "<output-directory>"] \
    [--baseRecord "<base-record-name>"] \
    [--nullability strict|semantic]
```

### Standalone apps for Windows, Linux, macOS

[Self-contained apps](https://docs.microsoft.com/en-us/dotnet/core/deploying/#publish-self-contained) are an ideal choice for machines without any version of .NET installed.

Latest release: [Download](https://github.com/kontent-ai/model-generator-net/releases/latest)

<details>
<summary>Building a self-contained binary for a specific platform</summary>

* Clone the repository
* Navigate to `src/Kontent.Ai.ModelGenerator`
* `dotnet build -r <RID>` to build (see the [list of all RIDs](https://learn.microsoft.com/en-us/dotnet/core/rid-catalog))
* `dotnet publish -c release -r <RID>` to publish

</details>

### Parameters

| Short key | Long key | Required | Default value | Description |
| --- | --- | :---: | --- | --- |
| `-i` | `--environmentId` | Yes | `null` | A GUID that can be found in [Kontent.ai](https://app.kontent.ai) -> Environment settings -> Environment ID |
| `-m` | `--management` | No | `false` | Switches the generator to **Management mode**. Emits models for the Management SDK instead of Delivery. See [Management Models](#management-models). |
| `-k` | `--apiKey` | Mgmt only | `null` | Management API key (required when `-m` / `--management` is used). |
| `-n` | `--namespace` | No | `KontentAiModels` | A name of the [C# namespace](https://msdn.microsoft.com/en-us/library/z2kcy19k.aspx) |
| `-o` | `--outputdir` | No | `./` | An output folder path |
| `-t` | `--withtypeprovider` | No | `false` | **(Obsolete)** TypeProvider is now source-generated by the Delivery SDK. |
| `-b`, `-r` | `--baseRecord` | No | `null` | If provided, a base record will be created and all generated records will derive from it via partial extender records |
| | `--nullability` | No | `strict` | Either `strict` or `semantic`. Delivery mode only. See [Nullability mode](#nullability-mode). |

### CLI Syntax

Short keys such as `-n "MyModels"` are interchangeable with the long keys `--namespace "MyModels"`. Other possible syntax is `-n=MyModels` or `--namespace=MyModels`. Parameter values are case-insensitive. To see all aspects of the syntax, see the [MS docs](https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.configuration.commandlineconfigurationextensions.addcommandline).

### Config file

These parameters can also be set via the `appSettings.json` file located in the same directory as the executable file. Command-line parameters always take precedence.

### Advanced configuration (Preview API, Secure API)

There are two ways of configuring advanced Delivery SDK options (such as secure API access, preview API access, and [others](https://github.com/kontent-ai/delivery-sdk-net/blob/master/Kontent.Ai.Delivery.Abstractions/Configuration/DeliveryOptions.cs)):

1. Command-line arguments:
   ```bash
   --DeliveryOptions:UseSecureAccess true --DeliveryOptions:SecureAccessApiKey <SecuredApiKey>
   ```

2. [`appSettings.json`](./src/Kontent.Ai.ModelGenerator/appSettings.json) - suitable for the standalone app release

## Generated Model Example (Delivery)

For the Management-mode equivalent, jump to [Management Models](#management-models).

**Generated file: `Article.cs`**

```csharp
// <auto-generated>
// This code was generated by Kontent.ai model generator tool
// (see https://github.com/kontent-ai/model-generator-net).
//
// Changes to this file may cause incorrect behavior and will be lost if the code is regenerated.
// To extend this record, create a separate partial record with the same name.
// </auto-generated>

#nullable enable

using System.Collections.Generic;
using System.Text.Json.Serialization;
using Kontent.Ai.Delivery.Abstractions;
using Kontent.Ai.Delivery.Attributes;
using Kontent.Ai.Delivery.ContentItems;
using Kontent.Ai.Delivery.ContentItems.RichText;
using Kontent.Ai.Delivery.SharedModels;

namespace KontentAiModels;

[ContentTypeCodename("article")]
public partial record Article
{
    public const string BodyCopyCodename = "body_copy";
    public const string CustomTrackingCodeCodename = "custom_tracking_code";
    public const string PersonasCodename = "personas";
    public const string PostDateCodename = "post_date";
    public const string RelatedArticlesCodename = "related_articles";
    public const string TeaserImageCodename = "teaser_image";
    public const string TitleCodename = "title";
    public const string UrlPatternCodename = "url_pattern";

    public const string ContentTypeCodename = "article";

    [JsonPropertyName("body_copy")]
    public RichTextContent? BodyCopy { get; init; }
    [JsonPropertyName("custom_tracking_code")]
    public string? CustomTrackingCode { get; init; }
    [JsonPropertyName("personas")]
    public IEnumerable<TaxonomyTerm>? Personas { get; init; }
    [JsonPropertyName("post_date")]
    public DateTimeContent? PostDate { get; init; }
    [JsonPropertyName("related_articles")]
    public IEnumerable<IEmbeddedContent>? RelatedArticles { get; init; }
    [JsonPropertyName("teaser_image")]
    public IEnumerable<Asset>? TeaserImage { get; init; }
    [JsonPropertyName("title")]
    public string? Title { get; init; }
    [JsonPropertyName("url_pattern")]
    public string? UrlPattern { get; init; }
}
```

## Nullability mode

The generator supports two nullability strategies for element properties via the `--nullability` flag:

### `strict` (default)

Every element property is generated as a nullable type — `string?`, `RichTextContent?`, `IEnumerable<Asset>?`, etc. This is the conservative default. It's also useful if you use the Delivery SDK's [projection](https://kontent.ai/learn/docs/apis/openapi/delivery-api/#tag/Items-and-content-types/operation/list-content-items) features (`WithElements` / `WithoutElements`) and want the type system to distinguish "not fetched" (`null`) from "fetched and empty" — projected-away elements surface as `null` at runtime.

```csharp
public string? Title { get; init; }
public RichTextContent? BodyCopy { get; init; }
public IEnumerable<IEmbeddedContent>? RelatedArticles { get; init; }
```

### `semantic`

Element properties match the runtime semantics of the Delivery API: empty text, rich text and collection elements always come back populated, so they're generated as **non-nullable** with sensible default initializers. Numbers, dates and custom elements can be genuinely unset, so they remain nullable.

```csharp
public string Title { get; init; } = string.Empty;
public RichTextContent BodyCopy { get; init; } = RichTextContent.Empty;
public IEnumerable<IEmbeddedContent> RelatedArticles { get; init; } = [];
public double? Rating { get; init; }
public DateTimeContent? PostDate { get; init; }
public string? CustomTrackingCode { get; init; }
```

> [!NOTE]
> When combined with [projection](https://kontent.ai/learn/docs/apis/openapi/delivery-api/#tag/Items-and-content-types/operation/list-content-items) (`WithElements` / `WithoutElements`), an omitted element surfaces as the type's default (`""`, `[]`, `RichTextContent.Empty`) rather than `null` — so "not fetched" and "fetched and empty" look the same. That's fine if your code doesn't branch on that distinction; if it does, prefer `strict`.

> [!IMPORTANT]
> `--nullability semantic` requires Delivery SDK **19.2.0+** (for `RichTextContent.Empty`). It will become the **default in the next major version** of the model generator.

## Customizing Generated Models

Since the generated models are **partial records**, you can extend them by creating your own partial record file:

**Generated file: `Article.cs` (auto-generated)**
```csharp
namespace KontentAiModels;

[ContentTypeCodename("article")]
public partial record Article
{
    public const string TitleCodename = "title";
    // ... other constants and properties

    public const string ContentTypeCodename = "article";

    [JsonPropertyName("title")]
    public string? Title { get; init; }
}
```

**Your custom file: `Article.Custom.cs` (your customizations)**
```csharp
namespace KontentAiModels;

public partial record Article
{
    // Add computed properties
    public string Slug => Title?.ToLowerInvariant().Replace(" ", "-") ?? string.Empty;

    // Add custom methods
    public bool IsPublished() => PostDate is { DateTime: var dt } && dt <= DateTime.Now;

    // Add validation
    public bool IsValid() => !string.IsNullOrEmpty(Title) && BodyCopy != null;
}
```

The generator creates the base model, and you maintain customizations in separate files that won't be overwritten.

## Management Models

> [!IMPORTANT]
> Preview. The emitted code references types and attributes that ship with the upcoming
> `management-sdk-net` vnext release (phase 3) — `IContentItem`, `[KontentContentType]`,
> `[KontentElement]`, `RichTextElement`, `AssetReference`, `Reference`, the validator, and
> the System.Text.Json converter. Until that release lands, the generated models won't
> compile against the published `Kontent.Ai.Management` v8.2.0.

When you need to **write** content to Kontent.ai (create / update / delete / publish via the Management API), pass `-m` / `--management` to switch the generator from Delivery mode into Management mode. The emitter produces strongly-typed records you can construct with object-initializer syntax and pass to `IManagementClient`.

### CLI

```bash
KontentModelGenerator --management \
    --environmentId "<environmentId>" \
    --apiKey "<management-api-key>" \
    [--namespace "<custom-namespace>"] \
    [--outputdir "<output-directory>"]
```

### What's different from Delivery models

| | Delivery | Management |
| --- | --- | --- |
| Use case | Read content, frontend rendering | CRUD via the Management API |
| Marker interface | None | `IContentItem` (empty marker) |
| Element identity | `[JsonPropertyName("codename")]` | `[KontentElement(Codename, Id)]` — both required (codename for request serialization, ID for response deserialization) |
| Type-level metadata | `[ContentTypeCodename]` | `[KontentContentType(Codename)]` |
| Collections | `IEnumerable<T>?` | `IReadOnlyList<T>?` (always; single-asset becomes `[MaxElements(1)]`) |
| Element constraints | Implicit at API layer | `[StringLength]`, `[RegularExpression]`, `[MinElements]` / `[MaxElements]` / `[ExactElements]`, `[AllowedTypes]`, `[AllowedTaxonomyGroup]`, `[MaxAssetSize]`, `[AllowedAssetFileTypes]` — consumed by the SDK validator before send |
| Multiple-choice | `IEnumerable<MultipleChoiceOption>?` | Per-element enum + `IReadOnlyList<{ContentType}{Element}>?` |
| Snippets | Implicit; values come back flattened | Flattened at generation time; properties carry `{snippet}__{element}` codenames |
| Required elements | Not exposed | **Not enforced on the model.** `is_required` is a publish-workflow gate in MAPI, not an upsert-shape constraint — every property stays nullable so partial draft saves work. |

### Generated model example

**Generated file: `Article.cs`**

```csharp
// <auto-generated/>

#nullable enable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Kontent.Ai.Management.Annotations;
using Kontent.Ai.Management.Models;

namespace MyProject.Models;

[KontentContentType(Codename = "article")]
public sealed partial record Article : IContentItem
{
    [KontentElement(Codename = "body", Id = "7ed15846-...")]
    [AllowedTypes("banner", "quote")]
    [AllowedItemLinkTypes("article")]
    public RichTextElement? Body { get; init; }

    [KontentElement(Codename = "category", Id = "f6d310a3-...")]
    [MaxElements(1)]
    public IReadOnlyList<ArticleCategory>? Category { get; init; }

    [KontentElement(Codename = "featured_image", Id = "8d2c...")]
    [MaxElements(1)]
    [MaxAssetSize(5242880L)]
    [AllowedAssetFileTypes(AssetFileType.Adjustable)]
    public IReadOnlyList<AssetReference>? FeaturedImage { get; init; }

    [KontentElement(Codename = "priority", Id = "88ae3d9b-...")]
    public decimal? Priority { get; init; }

    [KontentElement(Codename = "related_teasers", Id = "a3155ec4-...")]
    [AllowedTypes("article", "blog_post")]
    [MaxElements(3)]
    public IReadOnlyList<IContentItem>? RelatedTeasers { get; init; }

    [KontentElement(Codename = "seo__meta_title", Id = "09398b24-...")]
    [StringLength(70)]
    public string? SeoMetaTitle { get; init; }

    [KontentElement(Codename = "tags", Id = "1314993e-...")]
    [AllowedTaxonomyGroup("content_tags")]
    public IReadOnlyList<Reference>? Tags { get; init; }

    [KontentElement(Codename = "title", Id = "a47451eb-...")]
    [StringLength(100)]
    public string? Title { get; init; }
}

public enum ArticleCategory
{
    [KontentEnumValue(Codename = "news", Id = "d65a2212-...")] News,
    [KontentEnumValue(Codename = "release", Id = "709b1208-...")] Release,
    [KontentEnumValue(Codename = "blog", Id = "ae79c5a6-...")] Blog,
}
```

### Notes

- **Models are environment-specific** by virtue of their element IDs. Cloning an environment via data-ops produces logically identical content models with different element IDs — regenerate after cloning.
- **Snippets are flattened**. If your content type uses an `seo` snippet that contributes `meta_title` and `meta_description`, the generated record has `SeoMetaTitle` and `SeoMetaDescription` properties; the `[KontentElement]` attributes carry the `seo__meta_title` / `seo__meta_description` codenames the API expects.
- **Validation runs at the SDK layer before the HTTP request**, surfaced via `IManagementResult` — same shape as remote validation errors.

## Need Legacy Delivery SDK or Extended Delivery Support?

> [!NOTE]
> For these use cases, use the [previous stable release](https://github.com/kontent-ai/model-generator-net/tree/9.0.0):
>
> - **Legacy Delivery SDK (v18.x and earlier)** models
> - **Extended Delivery** models

## Feedback & Contributing

Found a bug or have a feature request? [Open an issue](https://github.com/kontent-ai/model-generator-net/issues). Pull requests are welcome!

### Wall of Fame

We would like to express our thanks to the following people who contributed and made the project possible:

- [Drazen Janjicek](https://github.com/djanjicek) - [EXLRT](http://www.exlrt.com/)
- [Kashif Jamal Soofi](https://github.com/kashifsoofi)
- [Casey Brown](https://github.com/MajorGrits)

## License

[MIT](./LICENSE)

<!-- Badge references -->
[nuget-badge]: https://img.shields.io/nuget/v/Kontent.Ai.ModelGenerator?style=for-the-badge
[nuget-url]: https://www.nuget.org/packages/Kontent.Ai.ModelGenerator
[license-badge]: https://img.shields.io/github/license/kontent-ai/model-generator-net?style=for-the-badge
[license-url]: https://github.com/kontent-ai/model-generator-net/blob/master/LICENSE
[build-badge]: https://img.shields.io/github/actions/workflow/status/kontent-ai/model-generator-net/integrate.yml?style=for-the-badge&label=Build%20%26%20Test
[build-url]: https://github.com/kontent-ai/model-generator-net/actions/workflows/integrate.yml
[codecov-badge]: https://img.shields.io/codecov/c/gh/kontent-ai/model-generator-net?style=for-the-badge&token=9LvfJ7m8gT
[codecov-url]: https://codecov.io/gh/kontent-ai/model-generator-net
[contributors-badge]: https://img.shields.io/github/contributors/kontent-ai/model-generator-net?style=for-the-badge
[contributors-url]: https://github.com/kontent-ai/model-generator-net/graphs/contributors
[last-commit-badge]: https://img.shields.io/github/last-commit/kontent-ai/model-generator-net?style=for-the-badge
[last-commit-url]: https://github.com/kontent-ai/model-generator-net/commits
[issues-badge]: https://img.shields.io/github/issues/kontent-ai/model-generator-net?style=for-the-badge
[issues-url]: https://github.com/kontent-ai/model-generator-net/issues
[stack-overflow-badge]: https://img.shields.io/badge/Stack%20Overflow-ASK%20NOW-FE7A16.svg?style=for-the-badge&logo=stackoverflow&logoColor=white
[stack-overflow-url]: https://stackoverflow.com/tags/kontent-ai
[discord-badge]: https://img.shields.io/discord/821885171984891914?style=for-the-badge&color=%237289DA&label=Kontent%20Discord&logo=discord
[discord-url]: https://discord.gg/SKCxwPtevJ
