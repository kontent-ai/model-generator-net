using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Kentico.Kontent.Delivery.Abstractions;
using Kentico.Kontent.Management.Models.LanguageVariants.Elements;
using Kentico.Kontent.ModelGenerator.Core.Helpers;

namespace Kentico.Kontent.ModelGenerator.Core.Common
{
    public class Property
    {
        public const string StructuredSuffix = "(structured)";

        public string Identifier => TextHelpers.GetValidPascalCaseIdentifierName(Codename);

        public string Codename { get; }

        public string Id { get; }

        /// <summary>
        /// Returns return type of the property in a string format (e.g.: "string").
        /// </summary>
        public string TypeName { get; }

        private static readonly Dictionary<string, string> DeliverTypes = new Dictionary<string, string>
        {
            { "text", "string" },
            { "rich_text", "string" },
            { "rich_text" + StructuredSuffix, nameof(IRichTextContent)},
            { "number", "decimal?" },
            { "multiple_choice", $"{nameof(IEnumerable)}<{nameof(IMultipleChoiceOption)}>"},
            { "date_time", "DateTime?" },
            { "asset", $"{nameof(IEnumerable)}<{nameof(IAsset)}>" },
            { "modular_content", $"{nameof(IEnumerable)}<{nameof(Object).ToLower(CultureInfo.InvariantCulture)}>" },
            { "taxonomy", $"{nameof(IEnumerable)}<{nameof(ITaxonomyTerm)}>" },
            { "url_slug", "string" },
            { "custom", "string" }
        };

        private static readonly Dictionary<string, string> ContentManagementTypes = new Dictionary<string, string>
        {
            { "text", nameof(TextElement) },
            { "rich_text", nameof(RichTextElement) },
            { "number", nameof(NumberElement) },
            { "multiple_choice", nameof(MultipleChoiceElement) },
            { "date_time", nameof(DateTimeElement)},
            { "asset", nameof(AssetElement) },
            { "modular_content", nameof(LinkedItemsElement) },
            { "subpages", nameof(SubpagesElement) },
            { "taxonomy", nameof(TaxonomyElement) },
            { "url_slug",nameof(UrlSlugElement) },
            { "custom", nameof(CustomElement) }
        };

        private static Dictionary<string, string> ContentTypeToTypeName(bool cmApi)
            => cmApi ? ContentManagementTypes : DeliverTypes;

        public Property(string codename, string typeName, string id = null)
        {
            Codename = codename;
            TypeName = typeName;
            Id = id;
        }

        public static bool IsContentTypeSupported(string contentType, bool cmApi = false)
        {
            return ContentTypeToTypeName(cmApi).ContainsKey(contentType);
        }

        public static Property FromContentType(string codename, string contentType, bool cmApi = false, string id = null)
        {
            if (IsContentTypeSupported(contentType, cmApi))
            {
                return new Property(codename, ContentTypeToTypeName(cmApi)[contentType], id);
            }

            throw new ArgumentException($"Unknown Content Type {contentType}", nameof(contentType));
        }
    }
}
