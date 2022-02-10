[![Build status](https://ci.appveyor.com/api/projects/status/1pagb8wjfyeyicj1/branch/master?svg=true)](https://ci.appveyor.com/project/kentico/kontent-generators-net/branch/master)
[![NuGet](https://img.shields.io/nuget/v/Kentico.Kontent.ModelGenerator.svg)](https://www.nuget.org/packages/Kentico.Kontent.ModelGenerator)
[![Stack Overflow](https://img.shields.io/badge/Stack%20Overflow-ASK%20NOW-FE7A16.svg?logo=stackoverflow&logoColor=white)](https://stackoverflow.com/tags/kentico-kontent)

# Kontent model generator utility for .NET

This utility generates strongly-typed (POCO) models based on [content types](https://kontent.ai/learn/tutorials/manage-kontent/content-modeling/create-and-delete-content-types) in a Kontent project. You can choose one of the following:
- [Generate models compatible with the Kontent Delivery SDK for .NET](#how-to-use-for-delivery-sdk)
- [Generate models compatible with the Kontent Management SDK for .NET](#how-to-use-for-management-sdk). 

ℹ If you want to take a look at the next version supporting all features, checkout the vNext branch. ℹ

## How to use for [Delivery SDK](https://github.com/Kentico/kontent-delivery-sdk-net)

To fully understand all benefits of this approach, please read the [documentation](https://github.com/Kentico/kontent-delivery-sdk-net/wiki/Working-with-strongly-typed-models#customizing-the-strong-type-binding-logic).

### .NET Core Tool

The recommended way of obtaining this tool is installing it as a [.NET Core Tool](https://docs.microsoft.com/en-us/dotnet/core/tools/global-tools). You can install it as a global tool or per project as a local tool.

**Global Tool**

- `dotnet tool install -g Kentico.Kontent.ModelGenerator` 

**Local Tool**

- `dotnet new tool-manifest` to initialize the tools manifest (if you haven't done that already)
- `dotnet tool install Kentico.Kontent.ModelGenerator` (to install the latest version

Then you can start using the `KontentModelGenerator` command in the command-line right away.

```
dotnet tool run KontentModelGenerator --projectid "<projectid>" [--namespace "<custom-namespace>"] [--outputdir "<output-directory>"] [--withtypeprovider <True|False>] [--structuredmodel <True|False>] [--filenamesuffix "<suffix>"]
```

### Standalone app for Windows

Latest release: [Download](https://github.com/Kentico/kontent-generators-net/releases/latest)

Since the app is [self-contained](https://www.hanselman.com/blog/SelfcontainedNETCoreApplications.aspx), it's an ideal option for machines without .NET Core or .NET Core SDK installed.

**Usage:**

```
KontentModelGenerator.exe --projectid "<projectid>" [--namespace "<custom-namespace>"] [--outputdir "<output-directory>"] [--withtypeprovider <True|False>] [--structuredmodel <True|False>] [--filenamesuffix "<suffix>"]
```

### Linux, Mac OS and other platforms

* Clone the repository
* Navigate to the `kontent-generators-net\src\KontentModelGenerator` folder
* Run `dotnet build -r <RID>` to build the app
* Run `dotnet publish -c release -r <RID>` to publish the app

```
dotnet run --projectid "<projectid>" [--namespace "<custom-namespace>"] [--outputdir "<output-directory>"] [--withtypeprovider <True|False>] [--structuredmodel <True|False>] [--filenamesuffix "<suffix>"]
```

See the [list of all RIDs](https://docs.microsoft.com/en-us/dotnet/articles/core/rid-catalog).

### Parameters

| Short key            | Long key | Required  | Default value  | Description |
| --------------------- |:---------:|:---------:|:--------------:|:-----------:|
| `-p` | `--projectid` | True  | `null` | A GUID that can be found in [Kentico Kontent](https://app.kontent.ai) -> API keys -> Project ID |
| `-n` | `--namespace` | False | `KenticoKontentModels` | A name of the [C# namespace](https://msdn.microsoft.com/en-us/library/z2kcy19k.aspx) |
| `-o` | `--outputdir` | False | `\.` | An output folder path |
| `-g` | `--generatepartials` | False | `true` | Generates partial classes for customization. Partial classes are the best practice for customization so the recommended value is `true`. |
| `-t` | `--withtypeprovider` | False | `true` | Indicates whether the `CustomTypeProvider` class should be generated (see [Customizing the strong-type binding logic](https://github.com/Kentico/delivery-sdk-net/wiki/Working-with-Strongly-Typed-Models-(aka-Code-First-Approach)#customizing-the-strong-type-binding-logic) for more info) |
| `-s` | `--structuredmodel` | False | `false` | Generates `IRichTextContent` instead of `string` for rich-text elements. This enables utilizing [structured rich-text rendering](https://github.com/Kentico/delivery-sdk-net/wiki/Structured-Rich-text-rendering) |
| `-f` | `--filenamesuffix` | False | `null` | Adds a suffix to generated filenames (e.g., News.cs becomes News.Generated.cs) |
| `-b` | `--baseclass` | False | `null` | If provided, a base class type will be created and all generated classes will derive from that base class via partial extender classes |

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

| Short key          | Long key | Required  | Default value  | Description |
| --------------------- |:---------:|:---------:|:--------------:|:-----------:|
| `-p` | `--projectid` | True  | `null` | A GUID that can be found in [Kontent](https://app.kontent.ai) -> API keys -> Project ID |
| `-c` | `--contentmanagementapi` | True  | `false` | Indicates that models should be generated for [Management SDK](https://github.com/Kentico/content-management-sdk-net) |
| `-n` | `--namespace` | False | `KenticoKontentModels` | A name of the [C# namespace](https://msdn.microsoft.com/en-us/library/z2kcy19k.aspx) |
| `-o` | `--outputdir` | False | `\.` | An output folder path |
| `-f` | `--filenamesuffix` | False | `null` | Adds a suffix to generated filenames (e.g., News.cs becomes News.Generated.cs) |
| `-b` | `--baseclass` | False | `null` | If provided, a base class type will be created and all generated classes will derive from that base class via partial extender classes |

These parameters can also be set via the appSettings.json file located in the same directory as the executable file. Command-line parameters always take precedence.


### Example output

```csharp
using System;
using System.Collections.Generic;
using Kentico.Kontent.Management.Models.Assets;
using Kentico.Kontent.Management.Models.Items;
using Newtonsoft.Json;

namespace KenticoKontentModels
{
    public partial class CompleteContentType
    {
        public string Text { get; set; }
        public string RichText { get; set; }
        public decimal? Number { get; set; }
        public IEnumerable<MultipleChoiceOptionIdentifier> MultipleChoice { get; set; }
        public DateTime? DateTime { get; set; }
        public IEnumerable<AssetIdentifier> Asset { get; set; }
        public IEnumerable<ContentItemIdentifier> ModularContent { get; set; }
        public IEnumerable<TaxonomyTermIdentifier> Taxonomy { get; set; }
        public string UrlSlug { get; set; }
	public string CustomElement { get; set; }
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
