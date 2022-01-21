[![Build & Test](https://github.com/Kentico/kontent-generators-net/actions/workflows/integrate.yml/badge.svg)](https://github.com/Kentico/kontent-generators-net/actions/workflows/integrate.yml)
[![codecov](https://codecov.io/gh/Kentico/kontent-generators-net/branch/master/graph/badge.svg?token=9LvfJ7m8gT)](https://codecov.io/gh/Kentico/kontent-generators-net)
[![Stack Overflow](https://img.shields.io/badge/Stack%20Overflow-ASK%20NOW-FE7A16.svg?logo=stackoverflow&logoColor=white)](https://stackoverflow.com/tags/kentico-kontent)
[![Discord](https://img.shields.io/discord/821885171984891914?color=%237289DA&label=Kontent%20Discord&logo=discord)](https://discord.gg/SKCxwPtevJ)

| Packages                       |                                                                     Version                                                                     |                                                                   Downloads                                                                   |                                                                  Compatibility                                                                   |     Documentation     |
| ------------------------------ | :---------------------------------------------------------------------------------------------------------------------------------------------: | :-------------------------------------------------------------------------------------------------------------------------------------------: | :----------------------------------------------------------------------------------------------------------------------------------------------: | :-------------------: |
| Kentico.Kontent.ModelGenerator | [![NuGet](https://img.shields.io/nuget/vpre/Kentico.Kontent.ModelGenerator.svg)](https://www.nuget.org/packages/Kentico.Kontent.ModelGenerator) | [![NuGet](https://img.shields.io/nuget/dt/Kentico.Kontent.ModelGenerator.svg)](https://www.nuget.org/packages/Kentico.Kontent.ModelGenerator) | [`net6.0`](https://dotnet.microsoft.com/download/dotnet/6.0) [`netstandard2.0`](https://docs.microsoft.com/en-us/dotnet/standard/net-standard)\* | [📖 Wiki](../../wiki) |

> \* We highly recommend targeting [`net6.0`](https://dotnet.microsoft.com/download/dotnet/6.0) in your projects. [`netstandard2.0`](https://docs.microsoft.com/en-us/dotnet/standard/net-standard) is supported to allow older projects to iteratively upgrade.

# Kontent model generator utility for .NET

This utility generates strongly-typed (POCO) models based on [Content Types](https://docs.kontent.ai/tutorials/set-up-projects/define-content-models/creating-and-deleting-content-types) in a [Kontent by Kentico](https://kontent.ai) project. You can choose one of the following:

- [Generate models compatible with the Kontent Delivery SDK for .NET](#how-to-use-for-delivery-sdk)
- [Generate models compatible with the Kontent Management SDK for .NET](#how-to-use-for-management-sdk).

⚠️ Please note that this tool uses [Delivery SDK](https://github.com/Kentico/kontent-delivery-sdk-net) and [Management SDK](https://github.com/Kentico/kontent-management-sdk-net).

ℹ If you want to take a look at the next version supporting all features, checkout the vNext branch. ℹ

## How to use for [Delivery SDK](https://github.com/Kentico/kontent-delivery-sdk-net)

To fully understand all benefits of this approach, please read the [documentation](https://github.com/Kentico/kontent-delivery-sdk-net/wiki/Working-with-strongly-typed-models#customizing-the-strong-type-binding-logic).

### .NET Tool

The recommended way of obtaining this tool is installing it as a [.NET Tool](https://docs.microsoft.com/en-us/dotnet/core/tools/global-tools). You can install it as a global tool or per project as a local tool.

**Global Tool**

- `dotnet tool install -g Kentico.Kontent.ModelGenerator`
- `KontentModelGenerator --projectid "<projectid>" [--namespace "<custom-namespace>"] [--outputdir "<output-directory>"] [--withtypeprovider <True|False>] [--structuredmodel <True|False>] [--filenamesuffix "<suffix>"]`

**Local Tool**

- `dotnet new tool-manifest` to initialize the tools manifest (if you haven't done that already)
- `dotnet tool install Kentico.Kontent.ModelGenerator` (to install the latest version
- `dotnet tool run KontentModelGenerator --projectid "<projectid>" [--namespace "<custom-namespace>"] [--outputdir "<output-directory>"] [--withtypeprovider <True|False>] [--structuredmodel <True|False>] [--filenamesuffix "<suffix>"]`

### Standalone apps for Windows 🗔, Linux 🐧, macOS 🍎

[Self-contained apps](https://docs.microsoft.com/en-us/dotnet/core/deploying/#publish-self-contained) are an ideal choice for machines without any version of .NET installed.

Latest release: [Download](https://github.com/Kentico/kontent-generators-net/releases/latest)

- `KontentModelGenerator --projectid "<projectid>" [--namespace "<custom-namespace>"] [--outputdir "<output-directory>"] [--withtypeprovider <True|False>] [--structuredmodel <True|False>] [--filenamesuffix "<suffix>"]`

To learn how to generate executables for your favorite target platform, follow the steps in the [wiki](https://github.com/Kentico/kontent-generators-net/wiki/Building-and-running-the-generator-for-your-target-platform).

### Parameters

| Short key |       Long key       | Required |     Default value      |                                                                                                                                 Description                                                                                                                                 |
| --------- | :------------------: | :------: | :--------------------: | :-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------: |
| `-p`      |    `--projectid`     |   True   |         `null`         |                                                                                           A GUID that can be found in [Kontent](https://app.kontent.ai) -> API keys -> Project ID                                                                                           |
| `-n`      |    `--namespace`     |  False   | `KenticoKontentModels` |                                                                                            A name of the [C# namespace](https://msdn.microsoft.com/en-us/library/z2kcy19k.aspx)                                                                                             |
| `-o`      |    `--outputdir`     |  False   |          `\.`          |                                                                                                                            An output folder path                                                                                                                            |
| `-g`      | `--generatepartials` |  False   |         `true`         |                                                                  Generates partial classes for customization. Partial classes are the best practice for customization so the recommended value is `true`.                                                                   |
| `-t`      | `--withtypeprovider` |  False   |         `true`         | Indicates whether the `CustomTypeProvider` class should be generated (see [Customizing the strong-type binding logic](https://github.com/Kentico/kontent-delivery-sdk-net/wiki/Working-with-Strongly-Typed-Models#customizing-the-strong-type-binding-logic) for more info) |
| `-s`      | `--structuredmodel`  |  False   |        `false`         |                              Generates `IRichTextContent` instead of `string` for rich-text elements. This enables utilizing [structured rich-text rendering](https://github.com/Kentico/delivery-sdk-net/wiki/Structured-Rich-text-rendering)                              |
| `-f`      |  `--filenamesuffix`  |  False   |         `null`         |                                                                                               Adds a suffix to generated filenames (e.g., News.cs becomes News.Generated.cs)                                                                                                |
| `-b`      |    `--baseclass`     |  False   |         `null`         |                                                                   If provided, a base class type will be created and all generated classes will derive from that base class via partial extender classes                                                                    |

### CLI Syntax

Short keys such as `-s true` are interchangable with the long keys `--structuredmodel true`. Other possible syntax is `-s=true` or `--structuredmodel=true`. Parameter values are case-insensitive, so you can use both `-s=true` and `-s=True`. To see all aspects of the syntax, see the [MS docs](https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.configuration.commandlineconfigurationextensions.addcommandline).

### Config file

These parameters can also be set via the appSettings.json file located in the same directory as the executable file. Command-line parameters always take precedence.

### Advanced configuration (Preview API, Secure API)

There are two ways of configuring advanced Delivery SDK options (such as secure API access, preview API access, and [others](https://github.com/Kentico/kontent-delivery-sdk-net/blob/master/Kentico.Kontent.Delivery/Configuration/DeliveryOptions.cs)):

1. Command-line arguments `--DeliveryOptions:UseSecureAccess true` ([syntax](https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.configuration.commandlineconfigurationextensions.addcommandline))

2. [`appSettings.json`](https://github.com/Kentico/kontent-generators-net/blob/master/src/Kentico.Kontent.ModelGenerator/appSettings.json) - suitable for the standalone app release

### Example output

```csharp
using System;
using System.Collections.Generic;
using Kentico.Kontent.Delivery.Abstractions;

namespace KenticoKontentModels
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

### Customizing models - Handling content element constraints

Currently, the generator is built on top of the Delivery API which doesn't provide information about content element constraints such as "Allowed Content Types" or "Limit number of items". In case you want your models to be more specific, this is the best practice on how to extend them:

Model.Generated.cs

```csharp
public partial class Home
{
    public IEnumerable<object> LinkedContentItems { get; set; }
}
```

Model.cs

```csharp
public partial class Home
{
    // Allowed Content Types == "Article"
    public IEnumerable<Article> Articles => LinkedContentItems.OfType<Article>();

    // Allowed Content Types == "Article" && Limit number of items == 1
    public Article Article => LinkedContentItems.OfType<Article>().FirstOrDefault();
}
```

## How to use for [Management SDK](https://github.com/Kentico/kontent-management-sdk-net)

**Usage:**

```
KontentModelGenerator.exe --projectid "<projectid>" --contentmanagementapi true [--namespace "<custom-namespace>"] [--outputdir "<output-directory>"] [--filenamesuffix "<suffix>"]
```

### Parameters

| Short key |         Long key         | Required |     Default value      |                                                              Description                                                               |
| --------- | :----------------------: | :------: | :--------------------: | :------------------------------------------------------------------------------------------------------------------------------------: |
| `-p`      |      `--projectid`       |   True   |         `null`         |                        A GUID that can be found in [Kontent](https://app.kontent.ai) -> API keys -> Project ID                         |
| `-c`      | `--contentmanagementapi` |   True   |        `false`         |     Indicates that models should be generated for [Content Management SDK](https://github.com/Kentico/content-management-sdk-net)      |
| `-n`      |      `--namespace`       |  False   | `KenticoKontentModels` |                          A name of the [C# namespace](https://msdn.microsoft.com/en-us/library/z2kcy19k.aspx)                          |
| `-o`      |      `--outputdir`       |  False   |          `\.`          |                                                         An output folder path                                                          |
| `-f`      |    `--filenamesuffix`    |  False   |         `null`         |                             Adds a suffix to generated filenames (e.g., News.cs becomes News.Generated.cs)                             |
| `-b`      |      `--baseclass`       |  False   |         `null`         | If provided, a base class type will be created and all generated classes will derive from that base class via partial extender classes |

These parameters can also be set via the appSettings.json file located in the same directory as the executable file. Command-line parameters always take precedence.

### Example output

```csharp
using Kentico.Kontent.Management.Models.LanguageVariants.Elements;
using Kentico.Kontent.Management.Modules.ModelBuilders;
using Newtonsoft.Json;

namespace KenticoKontentModels
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

## Feedback & Contributing

Check out the [contributing](https://github.com/Kentico/kontent-generators-net/blob/master/CONTRIBUTING.md) page to see the best places to file issues, start discussions and begin contributing.

### Wall of Fame

We would like to express our thanks to the following people who contributed and made the project possible:

- [Dražen Janjiček](https://github.com/djanjicek) - [EXLRT](http://www.exlrt.com/)
- [Kashif Jamal Soofi](https://github.com/kashifsoofi)
- [Casey Brown](https://github.com/MajorGrits)

Would you like to become a hero too? Pick an [issue](https://github.com/Kentico/kontent-generators-net/issues) and send us a pull request!
