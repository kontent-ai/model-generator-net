[![Build status](https://ci.appveyor.com/api/projects/status/t6dgpiamopwugu8v/branch/master?svg=true)](https://ci.appveyor.com/project/kentico/cloud-generators-net/branch/master)
[![Forums](https://img.shields.io/badge/chat-on%20forums-orange.svg)](https://forums.kenticocloud.com)

# Kentico Cloud dotnet models generator utility

This utility will generate strongly typed models based on your Content Types in your Kentico Cloud project.

## Get the tool

Latest successful build: [Download](https://ci.appveyor.com/api/projects/kentico/cloud-generators-net/artifacts/artifacts/CloudModelGenerator.zip)

## How to use

```
CloudModelGenerator.exe --projectid "<projectid>" [--namespace "<custom-namespace>"] [--outputdir "<output-directory>"]
```

## Example output

```csharp
using System;
using System.Collections.Generic;
using KenticoCloud.Delivery;

namespace KenticoCloudModels
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
        public ContentItemSystemAttributes System { get; set; }
    }
}
```

## Feedback & Contributing
Check out the [contributing](https://github.com/Kentico/cloud-generators-net/blob/master/CONTRIBUTING.md) page to see the best places to file issues, start discussions and begin contributing.
