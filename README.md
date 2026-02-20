# Kontent.ai model generator utility for .NET

[![Build & Test](https://github.com/kontent-ai/model-generator-net/actions/workflows/integrate.yml/badge.svg)](https://github.com/kontent-ai/model-generator-net/actions/workflows/integrate.yml)
[![codecov](https://codecov.io/gh/kontent-ai/model-generator-net/branch/master/graph/badge.svg?token=9LvfJ7m8gT)](https://codecov.io/gh/kontent-ai/model-generator-net)
[![Stack Overflow](https://img.shields.io/badge/Stack%20Overflow-ASK%20NOW-FE7A16.svg?logo=stackoverflow&logoColor=white)](https://stackoverflow.com/tags/kontent-ai)
[![Discord](https://img.shields.io/discord/821885171984891914?color=%237289DA&label=Kontent%20Discord&logo=discord)](https://discord.gg/SKCxwPtevJ)

## ⚠️ BETA VERSION - Modern Delivery SDK ONLY

**This beta version ONLY works with the modernized Kontent.ai Delivery SDK for .NET (19.0.0-rc1 and higher).**

### What this version supports:
- ✅ **Modern Delivery SDK (v19+)** - Generates record-based models with modern types

### What this version does NOT support:
- ❌ **Legacy Delivery SDK (v18.x and earlier)** - Use [previous stable release](https://github.com/kontent-ai/model-generator-net/releases) instead
- ❌ **Management SDK models** - Use [previous stable release](https://github.com/kontent-ai/model-generator-net/releases) instead
- ❌ **Extended Delivery models** - Not yet updated for modern SDK

> **Need the old version?** If you're using the legacy Delivery SDK (v18.x), Management SDK, or Extended Delivery models, please use the [previous stable release](https://github.com/kontent-ai/model-generator-net/releases) from the releases page.

---

This utility generates strongly-typed **record-based models** for the modern [Kontent.ai Delivery SDK for .NET (v19+)](https://github.com/kontent-ai/delivery-sdk-net).

## What's New in Modern Delivery Models

The generated models use modern C# features and patterns:

- ✅ **Records** - Immutable `record` types with `{ get; init; }` accessors
- ✅ **File-scoped namespaces** - Clean, modern C# syntax
- ✅ **JSON attributes** - `[JsonPropertyName]` for explicit property mapping
- ✅ **Modern types** - `RichTextContent`, `Asset`, `TaxonomyTerm`, `IEmbeddedContent`
- ✅ **Single file per model** - No `.Generated.cs` split files
- ✅ **Partial records** - Easily extendable without modifying generated code
- ✅ **`ContentTypeCodename` attribute** - For source-generated TypeProvider discovery

## Installation & Usage

### .NET Tool (Recommended)

The recommended way of obtaining this tool is installing it as a [.NET Tool](https://docs.microsoft.com/en-us/dotnet/core/tools/global-tools). You can install it as a global tool or per project as a local tool.

#### Global Tool

- `dotnet tool install -g Kontent.Ai.ModelGenerator`
- `KontentModelGenerator --environmentId "<environmentId>" [--namespace "<custom-namespace>"] [--outputdir "<output-directory>"] [--withtypeprovider <True|False>] [--baseclass "<base-class-name>"]`

#### Local Tool

- `dotnet new tool-manifest` to initialize the tools manifest (if you haven't done that already)
- `dotnet tool install Kontent.Ai.ModelGenerator` (to install the latest version
- `dotnet tool run KontentModelGenerator --environmentId "<environmentId>" [--namespace "<custom-namespace>"] [--outputdir "<output-directory>"] [--withtypeprovider <True|False>] [--baseclass "<base-class-name>"]`

### Standalone apps for Windows 🗔, Linux 🐧, macOS 🍎

[Self-contained apps](https://docs.microsoft.com/en-us/dotnet/core/deploying/#publish-self-contained) are an ideal choice for machines without any version of .NET installed.

Latest release: [Download](https://github.com/kontent-ai/model-generator-net/releases/latest)

- `KontentModelGenerator --environmentId "<environmentId>" [--namespace "<custom-namespace>"] [--outputdir "<output-directory>"] [--withtypeprovider <True|False>] [--baseclass "<base-class-name>"]`

To learn how to generate executables for your favorite target platform, follow the steps in the [docs](./docs/build-and-run.md).

### Parameters

| Short key |        Long key        | Required |   Default value   | Description                                                                                                                                                                      |
| --------- | :--------------------: | :------: | :---------------: | :------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `-i`      |   `--environmentId`    |   True   |      `null`       | A GUID that can be found in [Kontent.ai](https://app.kontent.ai) -> Environment settings -> Environment ID                                                                       |
| `-n`      |     `--namespace`      |  False   | `KontentAiModels` | A name of the [C# namespace](https://msdn.microsoft.com/en-us/library/z2kcy19k.aspx)                                                                                             |
| `-o`      |     `--outputdir`      |  False   |       `\.`        | An output folder path                                                                                                                                                            |
| `-t`      |  `--withtypeprovider`  |  False   |      `false`      | **(Obsolete)** Indicates whether the `CustomTypeProvider` class should be generated. TypeProvider is now source-generated by the Delivery SDK.                                  |
| `-b`      |     `--baseclass`      |  False   |      `null`       | If provided, a base class type will be created and all generated classes will derive from that base class via partial extender classes                                          |

### CLI Syntax

Short keys such as `-t true` are interchangable with the long keys `--withtypeprovider true`. Other possible syntax is `-t=true` or `--withtypeprovider=true`. Parameter values are case-insensitive, so you can use both `-t=true` and `-t=True`. To see all aspects of the syntax, see the [MS docs](https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.configuration.commandlineconfigurationextensions.addcommandline).

### Config file

These parameters can also be set via the appSettings.json file located in the same directory as the executable file. Command-line parameters always take precedence.

### Advanced configuration (Preview API, Secure API)

There are two ways of configuring advanced Delivery SDK options (such as secure API access, preview API access, and [others](https://github.com/kontent-ai/delivery-sdk-net/blob/master/Kontent.Ai.Delivery.Abstractions/Configuration/DeliveryOptions.cs)):

1. Command-line arguments `--DeliveryOptions:UseSecureAccess true --DeliveryOptions:SecureAccessApiKey <SecuredApiKey>` ([syntax](https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.configuration.commandlineconfigurationextensions.addcommandline))

2. [`appSettings.json`](./src/Kontent.Ai.ModelGenerator/appSettings.json) - suitable for the standalone app release

## Generated Model Example

**Generated file: `Article.cs`**

```csharp
// <auto-generated>
// This code was generated by Kontent.ai model generator tool
// (see https://github.com/kontent-ai/model-generator-net).
//
// Changes to this file may cause incorrect behavior and will be lost if the code is regenerated.
// To extend this record, create a separate partial record with the same name.
// </auto-generated>

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
    [JsonPropertyName("title")]
    public string Title { get; init; } = default!;

    [JsonPropertyName("body_copy")]
    public RichTextContent BodyCopy { get; init; } = default!;

    [JsonPropertyName("post_date")]
    public DateTimeContent PostDate { get; init; }

    [JsonPropertyName("teaser_image")]
    public IEnumerable<Asset> TeaserImage { get; init; } = default!;

    [JsonPropertyName("related_articles")]
    public IEnumerable<IEmbeddedContent> RelatedArticles { get; init; } = default!;

    [JsonPropertyName("personas")]
    public IEnumerable<TaxonomyTerm> Personas { get; init; } = default!;

    [JsonPropertyName("url_pattern")]
    public string UrlPattern { get; init; } = default!;

    [JsonPropertyName("custom_tracking_code")]
    public string? CustomTrackingCode { get; init; }
}
```

## Customizing Generated Models

Since the generated models are **partial records**, you can extend them by creating your own partial record file:

**Generated file: `Article.cs` (auto-generated)**
```csharp
namespace KontentAiModels;

public partial record Article
{
    [JsonPropertyName("title")]
    public string Title { get; init; } = default!;
    // ... other properties
}
```

**Your custom file: `Article.Custom.cs` (your customizations)**
```csharp
namespace KontentAiModels;

public partial record Article
{
    // Add computed properties
    public string Slug => Title.ToLowerInvariant().Replace(" ", "-");

    // Add custom methods
    public bool IsPublished() => PostDate.HasValue && PostDate.Value <= DateTime.Now;

    // Add validation
    public bool IsValid() => !string.IsNullOrEmpty(Title) && BodyCopy != null;
}
```

The generator creates the base model, and you maintain customizations in separate files that won't be overwritten.

## Need Management SDK or Legacy Delivery SDK Support?

This beta version **only supports the modern Delivery SDK (v19+)**.

For other use cases, please use the [previous stable release](https://github.com/kontent-ai/model-generator-net/releases):
- **Legacy Delivery SDK (v18.x and earlier)** models
- **Management SDK** models
- **Extended Delivery** models

## Feedback & Contributing

Check out the [contributing](./CONTRIBUTING.md) page to see the best places to file issues, start discussions and begin contributing.

### Wall of Fame

We would like to express our thanks to the following people who contributed and made the project possible:

- [Dražen Janjiček](https://github.com/djanjicek) - [EXLRT](http://www.exlrt.com/)
- [Kashif Jamal Soofi](https://github.com/kashifsoofi)
- [Casey Brown](https://github.com/MajorGrits)

Would you like to become a hero too? Pick an [issue](https://github.com/kontent-ai/model-generator-net/issues) and send us a pull request!
