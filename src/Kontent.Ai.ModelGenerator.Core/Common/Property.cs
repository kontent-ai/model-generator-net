using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using Kontent.Ai.ModelGenerator.Core.Configuration;
using Kontent.Ai.ModelGenerator.Core.Helpers;

namespace Kontent.Ai.ModelGenerator.Core.Common;

public class Property(string codename, string typeName, string id = null, string initializer = null)
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
    /// Optional initializer expression (e.g. <c>string.Empty</c>, <c>[]</c>, <c>RichTextContent.Empty</c>).
    /// When set, the generator emits <c>= {initializer};</c> on the property.
    /// </summary>
    public string Initializer { get; } = initializer;

    /// <summary>
    /// Gets whether this property is nullable.
    /// </summary>
    public bool IsNullable => TypeName.EndsWith('?');

    /// <summary>
    /// Gets whether this property has an explicit initializer expression.
    /// </summary>
    public bool HasInitializer => !string.IsNullOrEmpty(Initializer);

    /// <summary>
    /// Gets whether this property requires a default! initializer (non-nullable reference type).
    /// </summary>
    [Obsolete("Replaced by HasInitializer, which carries the actual initializer expression. This property will be removed in a future version.")]
    public bool RequiresDefaultInitializer => !IsNullable;

    private sealed record DeliveryElementMapping(string StrictTypeName, string SemanticTypeName, string SemanticInitializer);

    private static readonly IImmutableDictionary<string, DeliveryElementMapping> DeliverElementTypesDictionary =
        new Dictionary<string, DeliveryElementMapping>
        {
            { "text", new("string?", "string", "string.Empty") },
            { RichTextElementType, new("RichTextContent?", "RichTextContent", "RichTextContent.Empty") },
            { "number", new("double?", "double?", null) },
            { DateTimeElementType, new("DateTimeContent?", "DateTimeContent?", null) },
            { "multiple_choice", new("IEnumerable<MultipleChoiceOption>?", "IEnumerable<MultipleChoiceOption>", "[]") },
            { "asset", new("IEnumerable<Asset>?", "IEnumerable<Asset>", "[]") },
            { ModularContentElementType, new("IEnumerable<IEmbeddedContent>?", "IEnumerable<IEmbeddedContent>", "[]") },
            { "taxonomy", new("IEnumerable<TaxonomyTerm>?", "IEnumerable<TaxonomyTerm>", "[]") },
            { "url_slug", new("string?", "string", "string.Empty") },
            { "custom", new("string?", "string?", null) }
        }.ToImmutableDictionary();

    public static bool IsDateTimeElementType(string elementType) => elementType == DateTimeElementType;

    public static bool IsRichTextElementType(string elementType) => elementType == RichTextElementType;

    public static bool IsModularContentElementType(string elementType) => elementType == ModularContentElementType;

    public static Property FromContentTypeElement(string codename, string elementType) =>
        FromContentTypeElement(codename, elementType, NullabilityMode.Strict);

    public static Property FromContentTypeElement(string codename, string elementType, NullabilityMode nullabilityMode)
    {
        if (!IsContentTypeSupported(elementType))
        {
            throw new ArgumentException($"Unknown Content Type {elementType}", nameof(elementType));
        }

        var mapping = DeliverElementTypesDictionary[elementType];
        return nullabilityMode == NullabilityMode.Semantic
            ? new Property(codename, mapping.SemanticTypeName, id: null, initializer: mapping.SemanticInitializer)
            : new Property(codename, mapping.StrictTypeName);
    }

    private static bool IsContentTypeSupported(string elementType) => DeliverElementTypesDictionary.ContainsKey(elementType);
}
