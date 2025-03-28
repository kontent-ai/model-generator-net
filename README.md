# Kontent.ai model generator utility for .NET

[![Build & Test](https://github.com/kontent-ai/model-generator-net/actions/workflows/integrate.yml/badge.svg)](https://github.com/kontent-ai/model-generator-net/actions/workflows/integrate.yml)
[![codecov](https://codecov.io/gh/kontent-ai/model-generator-net/branch/master/graph/badge.svg?token=9LvfJ7m8gT)](https://codecov.io/gh/kontent-ai/model-generator-net)
[![Stack Overflow](https://img.shields.io/badge/Stack%20Overflow-ASK%20NOW-FE7A16.svg?logo=stackoverflow&logoColor=white)](https://stackoverflow.com/tags/kontent-ai)
[![Discord](https://img.shields.io/discord/821885171984891914?color=%237289DA&label=Kontent%20Discord&logo=discord)](https://discord.gg/SKCxwPtevJ)

| Packages                  |                                                                Version                                                                |                                                              Downloads                                                              |                        Compatibility                         |        Documentation        |
| ------------------------- | :-----------------------------------------------------------------------------------------------------------------------------------: | :---------------------------------------------------------------------------------------------------------------------------------: | :----------------------------------------------------------: | :-------------------------: |
| Kontent.Ai.ModelGenerator | [![NuGet](https://img.shields.io/nuget/vpre/Kontent.Ai.ModelGenerator.svg)](https://www.nuget.org/packages/Kontent.Ai.ModelGenerator) | [![NuGet](https://img.shields.io/nuget/dt/Kontent.Ai.ModelGenerator.svg)](https://www.nuget.org/packages/Kontent.Ai.ModelGenerator) | [`net8.0`](https://dotnet.microsoft.com/download/dotnet/8.0) | [📖 Docs](./docs/README.md) |

This utility generates strongly-typed (POCO) models based on [content types](https://kontent.ai/learn/tutorials/manage-kontent/content-modeling/create-and-delete-content-types) in a Kontent.ai project environment. You can choose one of the following:

- [Generate models compatible with the Kontent.ai Delivery SDK for .NET](#how-to-use-for-delivery-sdk)
- [Generate models compatible with the Kontent.ai Management SDK for .NET](#how-to-use-for-management-sdk).

⚠️ Please note that this tool uses [Delivery SDK](https://github.com/kontent-ai/delivery-sdk-net) and [Management SDK](https://github.com/kontent-ai/management-sdk-net).

## How to use for [Delivery SDK](https://github.com/kontent-ai/delivery-sdk-net)

To fully understand all benefits of this approach, please read the [documentation](https://github.com/kontent-ai/delivery-sdk-net/blob/master/docs/customization-and-extensibility/strongly-typed-models.md#customizing-the-strong-type-binding-logic).

### .NET Tool

The recommended way of obtaining this tool is installing it as a [.NET Tool](https://docs.microsoft.com/en-us/dotnet/core/tools/global-tools). You can install it as a global tool or per project as a local tool.

#### Global Tool

- `dotnet tool install -g Kontent.Ai.ModelGenerator`
- `KontentModelGenerator --environmentid "<environmentId>" [--namespace "<custom-namespace>"] [--outputdir "<output-directory>"] [--withtypeprovider <True|False>] [--structuredmodel "<structured_model>"] [--filenamesuffix "<suffix>"]`

#### Local Tool

- `dotnet new tool-manifest` to initialize the tools manifest (if you haven't done that already)
- `dotnet tool install Kontent.Ai.ModelGenerator` (to install the latest version
- `dotnet tool run KontentModelGenerator --environmentId "<environmentId>" [--namespace "<custom-namespace>"] [--outputdir "<output-directory>"] [--withtypeprovider <True|False>] [--structuredmodel "<structured_model>"] [--filenamesuffix "<suffix>"]`

### Standalone apps for Windows 🗔, Linux 🐧, macOS 🍎

[Self-contained apps](https://docs.microsoft.com/en-us/dotnet/core/deploying/#publish-self-contained) are an ideal choice for machines without any version of .NET installed.

Latest release: [Download](https://github.com/kontent-ai/model-generator-net/releases/latest)

- `KontentModelGenerator --environmentId "<environmentId>" [--namespace "<custom-namespace>"] [--outputdir "<output-directory>"] [--withtypeprovider <True|False>] [--structuredmodel "<structured_model>"] [--filenamesuffix "<suffix>"]`

To learn how to generate executables for your favorite target platform, follow the steps in the [docs](./docs/build-and-run.md).

### Delivery API parameters

| Short key |                Long key                  | Required |   Default value   | Description |
| --------- | :--------------------------------------: | :------: | :---------------: | :--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------: |
| `-i`      |            `--environmentId`             |   True   |      `null`       |                                                                                                                                                  A GUID that can be found in [Kontent.ai](https://app.kontent.ai) -> Environment settings -> Environment ID                                                                                                                                                      |
| `-n`      |              `--namespace`               |  False   | `KontentAiModels` |                                                                                                                                                    A name of the [C# namespace](https://msdn.microsoft.com/en-us/library/z2kcy19k.aspx)                                                                                                                                                        |
| `-o`      |              `--outputdir`               |  False   |       `\.`        |                                                                                                                                                                                    An output folder path                                                                                                                                                                                       |
| `-g`      |           `--generatepartials`           |  False   |      `true`       |                                                                                                                          Generates partial classes for customization. Partial classes are the best practice for customization so the recommended value is `true`.                                                                                                                              |
| `-t`      |           `--withtypeprovider`           |  False   |      `true`       |                                          Indicates whether the `CustomTypeProvider` class should be generated (see [Customizing the strong-type binding logic](https://github.com/kontent-ai/delivery-sdk-net/blob/master/docs/customization-and-extensibility/strongly-typed-models.md#customizing-the-strong-type-binding-logic) for more info)                                              |
| `-s`      |           `--structuredmodel`            |  False   |      `null`       | Allowed values [`RichText`, `DateTime`, `True`, `ModularContent`], as a separator you should use `,`. ⚠️ `True` parameter is **obsolete** and interprets the same value as `RichText`. For further details see [structured models rendering](https://github.com/kontent-ai/delivery-sdk-net/blob/master/docs/customization-and-extensibility/structured-models/structured-models-rendering.md) |
| `-f`      |           `--filenamesuffix`             |  False   |      `null`       |                                                                                                                                                        Adds a suffix to generated filenames (e.g., News.cs becomes News.Generated.cs)                                                                                                                                                          |
| `-b`      |              `--baseclass`               |  False   |      `null`       |                                                                                                                            If provided, a base class type will be created and all generated classes will derive from that base class via partial extender classes                                                                                                                              |
| `-e`      |        `--extendeddeliverymodels`        |  False   |     `false`       |                                                                                                                                                                 Indicates whether extended deliver models should be generated                                                                                                                                                                  |
| `-k`      |                 `--apikey`               |   True   |      `null`       |                                                                                                                                           Can be used with the extended delivery models. For details please see [Management API parameters section](#management-api-parameters)                                                                                                               |

For advanced configuration please see [Advanced configuration (Preview API, Secure API)](#advanced-configuration-preview-api-secure-api)

### CLI Syntax

Short keys such as `-t true` are interchangable with the long keys `--withtypeprovider true`. Other possible syntax is `-t=true` or `--withtypeprovider=true`. Parameter values are case-insensitive, so you can use both `-t=true` and `-t=True`. To see all aspects of the syntax, see the [MS docs](https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.configuration.commandlineconfigurationextensions.addcommandline).

### Config file

These parameters can also be set via the appSettings.json file located in the same directory as the executable file. Command-line parameters always take precedence.

### Advanced configuration (Preview API, Secure API)

There are two ways of configuring advanced Delivery SDK options (such as secure API access, preview API access, and [others](https://github.com/kontent-ai/delivery-sdk-net/blob/master/Kontent.Ai.Delivery.Abstractions/Configuration/DeliveryOptions.cs)):

1. Command-line arguments `--DeliveryOptions:UseSecureAccess true --DeliveryOptions:SecureAccessApiKey <SecuredApiKey>` ([syntax](https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.configuration.commandlineconfigurationextensions.addcommandline))

2. [`appSettings.json`](./src/Kontent.Ai.ModelGenerator/appSettings.json) - suitable for the standalone app release

### Delivery API example output

```csharp
using System;
using System.Collections.Generic;
using Kontent.Ai.Delivery.Abstractions;

namespace KontentAiModels
{
    public partial class CompleteContentType
    {
        public string Text { get; set; }
        public string RichText { get; set; }
        public decimal? Number { get; set; }
        public IEnumerable<MultipleChoiceOption> MultipleChoice { get; set; }
        public DateTime? DateTime { get; set; }
        public IEnumerable<Asset> Asset { get; set; }
        public IEnumerable<object> ModularContent { get; set; }
        public IEnumerable<object> Subpages { get; set; }
        public IEnumerable<TaxonomyTerm> Taxonomy { get; set; }
        public string UrlSlug { get; set; }
        public string CustomElement { get; set; }
        public ContentItemSystemAttributes System { get; set; }
    }
}
```

### Customizing models - Extended delivery models
Provides support to customize generated models based on content linked/subpages element constraints. This feature uses [Management SDK](https://github.com/kontent-ai/management-sdk-net) thus you'll need to provide api key as well.

`KontentModelGenerator --environmentId "<environmentId>" -e true -k "<apikey>"`

#### Extended delivery models example output
Model is generated using structured model option ModularContent.
Model.Generated.cs

```csharp
public partial class Home : IContentItem
{
    public const string SingleAllowedTypeAtMostOneLinkedContentItemCodename = "single_allowed_type_at_most_one_linked_content_item";
    public const string SingleAllowedTypeSingleLinkedContentItemCodename = "single_allowed_type_single_linked_content_item";
    public const string SingleAllowedTypeMultiLinkedContentItemsCodename = "single_allowed_type_multi_linked_content_items";
    public const string MultiAllowedTypesSingleLinkedContentItemCodename = "multi_allowed_types_single_linked_content_item";
    public const string MultiAllowedTypesAtMostSingleLinkedContentItemCodename = "multi_allowed_types_at-most_single_linked_content_item";
    public const string MultiAllowedTypesMultiLinkedContentItemsCodename = "multi_allowed_types_multi_linked_content_items";

    // Allowed Content Types == "Article" && Limit number of items <= 1
    public IEnumerable<IContentItem> SingleAllowedTypeAtMostOneLinkedContentItem { get; set; }
    
    // Allowed Content Types == "Article" && Limit number of items == 1
    public IEnumerable<IContentItem> SingleAllowedTypeSingleLinkedContentItem { get; set; }
    
    // Allowed Content Types == "Article" && Limit number of items > 1
    public IEnumerable<IContentItem> SingleAllowedTypeMultiLinkedContentItems { get; set; }

    // Allowed Content Types number > 1 && Limit number of items == 1
    public IEnumerable<IContentItem> MultiAllowedTypesExactlySingleLinkedContentItem { get; set; }

    // Allowed Content Types number > 1 && Limit number of items <= 1
    public IEnumerable<IContentItem> MultiAllowedTypesAtMostSingleLinkedContentItem { get; set; }

    // Allowed Content Types number > 1 && Limit number of items > 1
    public IEnumerable<IContentItem> MultiAllowedTypesMultiLinkedContentItems { get; set; }
}
```

Model.Typed.Generated.cs

```csharp
public partial class Home
{
    public Article SingleAllowedTypeAtMostOneLinkedContentItemSingle => SingleAllowedTypeAtMostOneLinkedContentItem.OfType<Article>().FirstOrDefault();
    
    public Article SingleAllowedTypeSingleLinkedContentItemSingle => SingleAllowedTypeSingleLinkedContentItem.OfType<Article>().FirstOrDefault();
    
    public IEnumerable<Article> SingleAllowedTypeMultiLinkedContentItemsArticleTyped => SingleAllowedTypeMultiLinkedContentItems.OfType<Article>();
}
```

## How to use for [Management SDK](https://github.com/kontent-ai/management-sdk-net)

### Usage

```sh
KontentModelGenerator.exe --environmentId "<environmentId>" --managementapi true --apikey "<apikey>" [--namespace "<custom-namespace>"] [--outputdir "<output-directory>"] [--filenamesuffix "<suffix>"]
```

### Management API parameters

| Short key |      Long key      | Required |   Default value   |                                                              Description                                                               |
| --------- | :----------------: | :------: | :---------------: | :------------------------------------------------------------------------------------------------------------------------------------: |
| `-p`      | `--environmentId`  |   True   |      `null`       |                        A GUID that can be found in [Kontent.ai](https://app.kontent.ai) -> Environment settings -> Environment ID                         |
| `-m`      | `--managementapi`  |   True   |      `false`      |        Indicates that models should be generated for [Content Management SDK](https://github.com/kontent-ai/management-sdk-net)        |
| `-k`      |     `--apikey`     |   True   |      `null`       |                     An api key that can be found in [Kontent.ai](https://app.kontent.ai) -> Project settings -> API keys -> Management API keys                     |
| `-n`      |   `--namespace`    |  False   | `KontentAiModels` |                          A name of the [C# namespace](https://msdn.microsoft.com/en-us/library/z2kcy19k.aspx)                          |
| `-o`      |   `--outputdir`    |  False   |       `\.`        |                                                         An output folder path                                                          |
| `-f`      | `--filenamesuffix` |  False   |      `null`       |                             Adds a suffix to generated filenames (e.g., News.cs becomes News.Generated.cs)                             |
| `-b`      |   `--baseclass`    |  False   |      `null`       | If provided, a base class type will be created and all generated classes will derive from that base class via partial extender classes |

These parameters can also be set via the appSettings.json file located in the same directory as the executable file. Command-line parameters always take precedence.

### Management API example output

> `JsonProperty`'s attribute value is being generated from element codename (not from the type) and `KontentElementId` attribute value is element's ID.

```csharp
using Kontent.Ai.Management.Models.LanguageVariants.Elements;
using Kontent.Ai.Management.Modules.ModelBuilders;
using Newtonsoft.Json;

namespace KontentAiModels
{
    public partial class CompleteContentType
    {
        [JsonProperty("text")]
        [KontentElementId("487f9540-0120-49dc-afb2-ee9bccb0c1d7")]
        public TextElement Text { get; set; }
        [JsonProperty("rich_text")]
        [KontentElementId("4517b6da-ed36-48f2-9c8e-00cd6a4cb0ec")]
        public RichTextElement RichText { get; set; }
        [JsonProperty("number")]
        [KontentElementId("4ea37483-c6b1-4b8a-8452-6046f4140923")]
        public NumberElement Number { get; set; }
        [JsonProperty("multiple_choice")]
        [KontentElementId("8fc9a86f-d256-4786-a8f6-c8c90f6ca4e3")]
        public MultipleChoiceElement MultipleChoice { get; set; }
        [JsonProperty("date_time")]
        [KontentElementId("d46fa45c-a1be-4bc7-8b8e-ed3c5521f83c")]
        public DateTimeElement DateTime { get; set; }
        [JsonProperty("asset")]
        [KontentElementId("eb1d611d-b145-4ae3-b22e-ef3609572df0")]
        public AssetElement Asset { get; set; }
        [JsonProperty("modular_content")]
        [KontentElementId("9e520c61-6879-4e83-bcc6-ee6e3e8ce9b4")]
        public LinkedItemsElement ModularContent { get; set; }
        [JsonProperty("subpages")]
        [KontentElementId("fddd89e8-c370-4f9e-9b7d-9daa64d8a252")]
        public LinkedItemsElement Subpages { get; set; }
        [JsonProperty("taxonomy")]
        [KontentElementId("a684d81c-68a7-40e1-85f9-2d22a71bebff")]
        public TaxonomyElement Taxonomy { get; set; }
        [JsonProperty("url_slug")]
        [KontentElementId("1c724f49-b15f-42f5-aab4-4127aa5cf7be")]
        public UrlSlugElement UrlSlug { get; set; }
        [JsonProperty("custom_element")]
        [KontentElementId("cb3b9df0-20df-461c-a0f7-4abb44b83c95")]
        public CustomElement CustomElement { get; set; }
    }
}
```

⚠️ Please note that _Guidelines_ element is not supported, thus it will not be included in the generated model.

## Feedback & Contributing

Check out the [contributing](./CONTRIBUTING.md) page to see the best places to file issues, start discussions and begin contributing.

### Wall of Fame

We would like to express our thanks to the following people who contributed and made the project possible:

- [Dražen Janjiček](https://github.com/djanjicek) - [EXLRT](http://www.exlrt.com/)
- [Kashif Jamal Soofi](https://github.com/kashifsoofi)
- [Casey Brown](https://github.com/MajorGrits)

Would you like to become a hero too? Pick an [issue](https://github.com/kontent-ai/model-generator-net/issues) and send us a pull request!
