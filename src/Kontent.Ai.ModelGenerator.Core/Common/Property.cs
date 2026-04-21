using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using Kontent.Ai.ModelGenerator.Core.Helpers;

namespace Kontent.Ai.ModelGenerator.Core.Common;

public class Property(string codename, string typeName, string id = null)
{
    private const string RichTextElementType = "rich_text";
    private const string DateTimeElementType = "date_time";
    private const string ModularContentElementType = "modular_content";

    public static string ObjectType => nameof(Object).ToLower(CultureInfo.InvariantCulture);

    public string IdentifierOverride { get; init; }

    public string Identifier => IdentifierOverride ?? TextHelpers.GetValidPascalCaseIdentifierName(Codename);

    public string Codename { get; } = codename;

    public string Id { get; } = id;

    /// <summary>
    /// Returns return type of the property in a string format (e.g.: "string").
    /// </summary>
    public string TypeName { get; } = typeName;

    /// <summary>
    /// Gets whether this property is nullable and should not have a default! initializer.
    /// </summary>
    public bool IsNullable => TypeName.EndsWith('?');

    /// <summary>
    /// Gets whether this property requires a default! initializer (non-nullable reference type).
    /// </summary>
    public bool RequiresDefaultInitializer => !IsNullable;

    private static readonly IImmutableDictionary<string, string> DeliverElementTypesDictionary = new Dictionary<string, string>
    {
        { "text", "string?" },
        { RichTextElementType, "RichTextContent?" },
        { "number", "double?" },
        { DateTimeElementType, "DateTimeContent?" },
        { "multiple_choice", "IEnumerable<MultipleChoiceOption>?"},
        { "asset", "IEnumerable<Asset>?" },
        { ModularContentElementType, "IEnumerable<IEmbeddedContent>?" },
        { "taxonomy", "IEnumerable<TaxonomyTerm>?" },
        { "url_slug", "string?" },
        { "custom", "string?" }
    }.ToImmutableDictionary();

    public static bool IsDateTimeElementType(string elementType) => elementType == DateTimeElementType;

    public static bool IsRichTextElementType(string elementType) => elementType == RichTextElementType;

    public static bool IsModularContentElementType(string elementType) => elementType == ModularContentElementType;

    public static Property FromContentTypeElement(string codename, string elementType) => IsContentTypeSupported(elementType)
        ? new Property(codename, DeliverElementTypesDictionary[elementType])
        : throw new ArgumentException($"Unknown Content Type {elementType}", nameof(elementType));

    private static bool IsContentTypeSupported(string elementType) => DeliverElementTypesDictionary.ContainsKey(elementType);
}
