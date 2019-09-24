[![Build status](https://ci.appveyor.com/api/projects/status/1pagb8wjfyeyicj1/branch/master?svg=true)](https://ci.appveyor.com/project/kentico/kontent-generators-net/branch/master)
[![NuGet](https://img.shields.io/nuget/v/Kentico.Kontent.ModelGenerator.svg)](https://www.nuget.org/packages/Kentico.Kontent.ModelGenerator)
[![Stack Overflow](https://img.shields.io/badge/Stack%20Overflow-ASK%20NOW-FE7A16.svg?logo=stackoverflow&logoColor=white)](https://stackoverflow.com/tags/kentico-kontent)

# Kentico Kontent model generator utility for .NET

This utility generates strongly-typed models based on Content Types in a Kentico Kontent project. The models are supposed to be used together with the [Kentico Kontent Delivery SDK for .NET](https://github.com/Kentico/delivery-sdk-net) or [Kentico Kontent Content Management SDK for .NET](https://github.com/Kentico/content-management-sdk-net). Please read the [documentation](https://github.com/Kentico/delivery-sdk-net/wiki/Working-with-Strongly-Typed-Models-(aka-Code-First-Approach)#customizing-the-strong-type-binding-logic) to see all benefits of this approach.


## Get the tool

### .NET Core Global Tool

The recommended way of obtaining this tool is installing it as a [.NET Core Global Tool](https://docs.microsoft.com/en-us/dotnet/core/tools/global-tools#install-a-global-tool).

- Run `dotnet tool install -g Kentico.Kontent.ModelGenerator` 

Then you can start using the `KontentModelGenerator` command in the command-line right away.

### Windows

Latest release: [Download](https://github.com/Kentico/kontent-generators-net/releases/latest)

Note: The application is [self-contained](https://www.hanselman.com/blog/SelfcontainedNETCoreApplications.aspx). There's no need to install any version of .NET on your machine.

### Linux, Mac OS and other platforms

* Clone the repository
* Navigate to the `kontent-generators-net\src\KontentModelGenerator` folder
* Run `dotnet build -r <RID>` to build the app
* Run `dotnet publish -c release -r <RID>` to publish the app

See the [list of all RIDs](https://docs.microsoft.com/en-us/dotnet/articles/core/rid-catalog).

## How to use for Delivery SDK

### Windows

```
KontentModelGenerator.exe --projectid "<projectid>" [--namespace "<custom-namespace>"] [--outputdir "<output-directory>"] [--withtypeprovider] [--structuredmodel] [--filenamesuffix "<suffix>"]
```

### Linux, Mac OS and other platforms
```
dotnet run --projectid "<projectid>" [--namespace "<custom-namespace>"] [--outputdir "<output-directory>"] [--withtypeprovider] [--structuredmodel] [--filenamesuffix "<suffix>"]
```

### Parameters

| Parameter (single character variant)            | Keyword variant | Required  | Default value  | Description |
| --------------------- |:---------:|:---------:|:--------------:|:-----------:|
| `-p` | `--projectid` | True  | `null` | A GUID that can be found in [Kentico Kontent](https://app.kontent.ai) -> API keys -> Project ID |
| `-n` | `--namespace` | False | `KenticoKontentModels` | A name of the [C# namespace](https://msdn.microsoft.com/en-us/library/z2kcy19k.aspx) |
| `-o` | `--outputdir` | False | `\.` | An output folder path |
| `-g` | `--generatepartials` | False | `null` | Generates partial classes for customizationfilenames |
| `-t` | `--withtypeprovider` | False | `true` | Indicates whether the `CustomTypeProvider` class should be generated (see [Customizing the strong-type binding logic](https://github.com/Kentico/delivery-sdk-net/wiki/Working-with-Strongly-Typed-Models-(aka-Code-First-Approach)#customizing-the-strong-type-binding-logic) for more info) |
| `-s` | `--structuredmodel` | False | `false` | Generates `IRichTextContent` instead of `string` for rich-text elements. This enables utilizing [structured rich-text rendering](https://github.com/Kentico/delivery-sdk-net/wiki/Structured-Rich-text-rendering) |
| `-f` | `--filenamesuffix` | False | `null` | Adds a suffix to generated filenames (e.g., News.cs becomes News.Generated.cs) |
| `-b` | `--baseclass` | False | `null` | If provided, a base class type will be created and all generated classes will derive from that base class via partial extender classes |

These parameters can also be set via the appSettings.json file located in the same directory as the executable file. Command-line parameters always take precedence.

### Example output

```csharp
using System;
using System.Collections.Generic;
using Kentico.Kontent.Delivery;

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

## How to use for Content Management SDK

### Windows

```
KontentModelGenerator.exe --projectid "<projectid>" --contentmanagementapi [--namespace "<custom-namespace>"] [--outputdir "<output-directory>"] [--filenamesuffix "<suffix>"]
```

### Linux, Mac OS and other platforms
```
dotnet run --projectid "<projectid>" --contentmanagementapi [--namespace "<custom-namespace>"] [--outputdir "<output-directory>"] [--filenamesuffix "<suffix>"]
```

### Parameters

| Parameter (single character variant)            | Keyword variant | Required  | Default value  | Description |
| --------------------- |:---------:|:---------:|:--------------:|:-----------:|
| `-p` | `--projectid` | True  | `null` | A GUID that can be found in [Kentico Kontent](https://app.kontent.ai) -> API keys -> Project ID |
| `-c` | `--contentmanagementapi` | True  | `false` | Indicates that models should be generated for  [Content Management SDK](https://github.com/Kentico/content-management-sdk-net) |
| `-n` | `--namespace` | False | `KenticoKontentModels` | A name of the [C# namespace](https://msdn.microsoft.com/en-us/library/z2kcy19k.aspx) |
| `-o` | `--outputdir` | False | `\.` | An output folder path |
| `-f` | `--filenamesuffix` | False | `null` | Adds a suffix to generated filenames (e.g., News.cs becomes News.Generated.cs) |
| `-b` | `--baseclass` | False | `null` | If provided, a base class type will be created and all generated classes will derive from that base class via partial extender classes |

These parameters can also be set via the appSettings.json file located in the same directory as the executable file. Command-line parameters always take precedence.


### Example output

```csharp
using System;
using System.Collections.Generic;
using KenticoCloud.ContentManagement.Models.Assets;
using KenticoCloud.ContentManagement.Models.Items;
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

![Analytics](https://kentico-ga-beacon.azurewebsites.net/api/UA-69014260-4/Kentico/kontent-generators-net?pixel)
