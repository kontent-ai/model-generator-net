using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using Kontent.Ai.Delivery.Abstractions;
using Kontent.Ai.Management.Models.LanguageVariants.Elements;
using Kontent.Ai.Management.Models.Types.Elements;
using Kontent.Ai.ModelGenerator.Core.Helpers;

namespace Kontent.Ai.ModelGenerator.Core.Common;

public class Property(string codename, string typeName, string id = null)
{
    private const string RichTextElementType = "rich_text";
    private const string DateTimeElementType = "date_time";
    private const string ModularContentElementType = "modular_content";

    public const string StructuredSuffix = "(structured)";

    public static string ObjectType => nameof(Object).ToLower(CultureInfo.InvariantCulture);

    public string Identifier => TextHelpers.GetValidPascalCaseIdentifierName(Codename);

    public string Codename { get; } = codename;

    public string Id { get; } = id;

    /// <summary>
    /// Returns return type of the property in a string format (e.g.: "string").
    /// </summary>
    public string TypeName { get; } = typeName;

    private static readonly IImmutableDictionary<string, string> DeliverElementTypesDictionary = new Dictionary<string, string>
    {
        { "text", "string" },
        { RichTextElementType, "string" },
        { $"{RichTextElementType}{StructuredSuffix}", nameof(IRichTextContent)},
        { "number", "decimal?" },
        { DateTimeElementType, "DateTime?" },
        { $"{DateTimeElementType}{StructuredSuffix}", nameof(IDateTimeContent) },
        { "multiple_choice", TextHelpers.GetEnumerableType(nameof(IMultipleChoiceOption))},
        { "asset", TextHelpers.GetEnumerableType(nameof(IAsset)) },
        { ModularContentElementType, TextHelpers.GetEnumerableType(ObjectType) },
        { $"{ModularContentElementType}{StructuredSuffix}", TextHelpers.GetEnumerableType(nameof(IContentItem)) },
        { "taxonomy", TextHelpers.GetEnumerableType(nameof(ITaxonomyTerm)) },
        { "url_slug", "string" },
        { "custom", "string" }
    }.ToImmutableDictionary();

    private static readonly IImmutableDictionary<string, string> ExtendedDeliverElementTypesDictionary = new Dictionary<string, string>
    {
        { ElementMetadataType.Text.ToString(), "string" },
        { ElementMetadataType.RichText.ToString(), "string" },
        { $"{ElementMetadataType.RichText}{StructuredSuffix}", nameof(IRichTextContent)},
        { ElementMetadataType.Number.ToString(), "decimal?" },
        { ElementMetadataType.MultipleChoice.ToString(), TextHelpers.GetEnumerableType(nameof(IMultipleChoiceOption))},
        { ElementMetadataType.DateTime.ToString(), "DateTime?" },
        { $"{ElementMetadataType.DateTime}{StructuredSuffix}", nameof(IDateTimeContent) },
        { ElementMetadataType.Asset.ToString(), TextHelpers.GetEnumerableType(nameof(IAsset)) },
        { ElementMetadataType.LinkedItems.ToString(), null },
        { ElementMetadataType.Subpages.ToString(), null },
        { ElementMetadataType.Taxonomy.ToString(), TextHelpers.GetEnumerableType(nameof(ITaxonomyTerm)) },
        { ElementMetadataType.UrlSlug.ToString(), "string" },
        { ElementMetadataType.Custom.ToString(), "string" }
    }.ToImmutableDictionary();

    private static readonly IImmutableDictionary<ElementMetadataType, string> ManagementElementTypesDictionary = new Dictionary<ElementMetadataType, string>
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
    }.ToImmutableDictionary();

    public static bool IsDateTimeElementType(string elementType) => elementType == DateTimeElementType;

    public static bool IsRichTextElementType(string elementType) => elementType == RichTextElementType;

    public static bool IsModularContentElementType(string elementType) => elementType == ModularContentElementType;

    public static Property FromContentTypeElement(string codename, string elementType) => IsContentTypeSupported(elementType)
        ? new Property(codename, DeliverElementTypesDictionary[elementType])
        : throw new ArgumentException($"Unknown Content Type {elementType}", nameof(elementType));

    public static Property FromContentTypeElement(ElementMetadataBase element)
    {
        if (IsContentTypeSupported(element.Type))
        {
            return new Property(element.Codename, ManagementElementTypesDictionary[element.Type], element.Id.ToString());
        }

        throw new ArgumentException($"Unknown Content Type Element {element.Type}", nameof(element));
    }

    public static Property FromContentTypeElement(ElementMetadataBase element, string elementType) =>
        FromContentTypeElement(element, elementType, element.Codename);

    public static Property FromContentTypeElement(ElementMetadataBase element, string elementType, string finalPropertyName)
    {
        if (IsContentTypeSupported(element.Type.ToString(), true))
        {
            var resultElementType = element.Type is ElementMetadataType.LinkedItems or ElementMetadataType.Subpages
                ? elementType
                : ExtendedDeliverElementTypesDictionary[elementType];

            return new Property(finalPropertyName, resultElementType);
        }

        throw new ArgumentException($"Unknown Content Type Element {element.Type}", nameof(element));
    }

    private static bool IsContentTypeSupported(string elementType, bool extendedDeliveryModels) => extendedDeliveryModels
        ? ExtendedDeliverElementTypesDictionary.ContainsKey(elementType)
        : DeliverElementTypesDictionary.ContainsKey(elementType);

    private static bool IsContentTypeSupported(string elementType) => DeliverElementTypesDictionary.ContainsKey(elementType);

    private static bool IsContentTypeSupported(ElementMetadataType elementType) =>
        ManagementElementTypesDictionary.ContainsKey(elementType);
}
