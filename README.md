# Kentico Cloud dotnet models generator utility

This utility will generate strongly typed models based on your Content Types in your Kentico Cloud project.

## Get the tool

TODO: add download link.

## How to use

```
CloudModelGenerator.exe --projectid "<projectid>" --namespace "<custom-namespace>" --outputdir "<output-directory>"
```

## Example output

```
using System;
using System.Collections.Generic;
using KenticoCloud.Delivery;

namespace KenticoCloudModels
{
    public class CompleteContentType
    {
        public string Text { get; set; }
        public string RichText { get; set; }
        public decimal? Number { get; set; }
        public IEnumerable<MultipleChoiceOption> MultipleChoice { get; set; }
        public DateTime? DateTime { get; set; }
        public IEnumerable<Asset> Asset { get; set; }
        public IEnumerable<ContentItem> ModularContent { get; set; }
        public IEnumerable<TaxonomyTerm> Taxonomy { get; set; }
        public ContentItemSystemAttributes System { get; set; }
    }
}
```

## Contribution guidelines

TODO
