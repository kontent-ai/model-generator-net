using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Kentico.Kontent.Delivery.Abstractions;

namespace Kentico.Kontent.ModelGenerator.Core
{
    public class Property
    {
        public const string STRUCTURED_SUFFIX = "(structured)";

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
            { "rich_text" + STRUCTURED_SUFFIX, nameof(IRichTextContent)},
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
            { "text", "string" },
            { "rich_text", "string" },
            { "number", "decimal?" },
            { "multiple_choice", "IEnumerable<MultipleChoiceOptionIdentifier>" },
            { "date_time", "DateTime?" },
            { "asset", "IEnumerable<AssetIdentifier>" },
            { "modular_content", "IEnumerable<ContentItemIdentifier>" },
            { "taxonomy", "IEnumerable<TaxonomyTermIdentifier>" },
            { "url_slug", "string" },
            { "custom", "string" }
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
            if (!IsContentTypeSupported(contentType, cmApi))
            {
                throw new ArgumentException($"Unknown Content Type {contentType}", nameof(contentType));
            }

            return new Property(codename, ContentTypeToTypeName(cmApi)[contentType], id);
        }
    }
}
