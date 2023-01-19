using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Kontent.Ai.Delivery.Abstractions;
using Kontent.Ai.Management.Models.LanguageVariants.Elements;
using Kontent.Ai.Management.Models.Types.Elements;
using Kontent.Ai.ModelGenerator.Core.Helpers;

namespace Kontent.Ai.ModelGenerator.Core.Common;

public class Property
{
    private const string RichTextElementType = "rich_text";
    private const string DateTimeElementType = "date_time";
    public const string StructuredSuffix = "(structured)";

    public string Identifier => TextHelpers.GetValidPascalCaseIdentifierName(Codename);

    public string Codename { get; }

    public string Id { get; }

    /// <summary>
    /// Returns return type of the property in a string format (e.g.: "string").
    /// </summary>
    public string TypeName { get; }

    private static readonly Dictionary<string, string> DeliverElementTypesDictionary = new Dictionary<string, string>
    {
        { "text", "string" },
        { RichTextElementType, "string" },
        { $"{RichTextElementType}{StructuredSuffix}", nameof(IRichTextContent)},
        { "number", "decimal?" },
        { "multiple_choice", $"{nameof(IEnumerable)}<{nameof(IMultipleChoiceOption)}>"},
        { DateTimeElementType, "DateTime?" },
        { $"{DateTimeElementType}{StructuredSuffix}", nameof(IDateTimeContent) },
        { "asset", $"{nameof(IEnumerable)}<{nameof(IAsset)}>" },
        { "modular_content", $"{nameof(IEnumerable)}<{nameof(Object).ToLower(CultureInfo.InvariantCulture)}>" },
        { "taxonomy", $"{nameof(IEnumerable)}<{nameof(ITaxonomyTerm)}>" },
        { "url_slug", "string" },
        { "custom", "string" }
    };

    private static readonly Dictionary<ElementMetadataType, string> ManagementElementTypesDictionary = new Dictionary<ElementMetadataType, string>
    {
        { ElementMetadataType.Text, nameof(TextElement) },
        { ElementMetadataType.RichText, nameof(RichTextElement) },
        { ElementMetadataType.Number, nameof(NumberElement) },
        { ElementMetadataType.MultipleChoice, nameof(MultipleChoiceElement) },
        { ElementMetadataType.DateTime, nameof(DateTimeElement)},
        { ElementMetadataType.Asset, nameof(AssetElement) },
        { ElementMetadataType.LinkedItems, nameof(LinkedItemsElement) },
        { ElementMetadataType.Subpages, nameof(SubpagesElement) },
        { ElementMetadataType.Taxonomy, nameof(TaxonomyElement) },
        { ElementMetadataType.UrlSlug,nameof(UrlSlugElement) },
        { ElementMetadataType.Custom, nameof(CustomElement) }
    };

    public Property(string codename, string typeName, string id = null)
    {
        Codename = codename;
        TypeName = typeName;
        Id = id;
    }

    public static bool IsDateTimeElementType(string elementType) => elementType == DateTimeElementType;

    public static bool IsRichTextElementType(string elementType) => elementType == RichTextElementType;

    private static bool IsContentTypeSupported(string elementType)
    {
        return DeliverElementTypesDictionary.ContainsKey(elementType);
    }

    private static bool IsContentTypeSupported(ElementMetadataType elementType)
    {
        return ManagementElementTypesDictionary.ContainsKey(elementType);
    }

    public static Property FromContentTypeElement(string codename, string elementType)
    {
        if (IsContentTypeSupported(elementType))
        {
            return new Property(codename, DeliverElementTypesDictionary[elementType]);
        }

        throw new ArgumentException($"Unknown Content Type {elementType}", nameof(elementType));
    }

    public static Property FromContentTypeElement(ElementMetadataBase element)
    {
        if (IsContentTypeSupported(element.Type))
        {
            return new Property(element.Codename, ManagementElementTypesDictionary[element.Type], element.Id.ToString());
        }

        if (element.Type == ElementMetadataType.Guidelines)
        {
            throw new UnsupportedTypeException();
        }

        throw new ArgumentException($"Unknown Content Type {element.Type}", nameof(element));
    }
}
