using System;
using System.Collections.Generic;

namespace CloudModelGenerator
{
    public class Property
    {
        public string Identifier { get; private set; }

        /// <summary>
        /// Returns return type of the property in a string format (e.g.: "string").
        /// </summary>
        public string TypeName { get; private set; }

        private static Dictionary<string, string> contentTypeToTypeName = new Dictionary<string, string>()
        {
            { "text", "string" },
            { "rich_text", "string" },
            { "number", "decimal?" },
            { "multiple_choice", "IEnumerable<MultipleChoiceOption>" },
            { "date_time", "DateTime?" },
            { "asset", "IEnumerable<Asset>" },
            { "modular_content", "IEnumerable<object>" },
            { "taxonomy", "IEnumerable<TaxonomyTerm>" },
        };

        public Property(string codename, string typeName)
        {
            Identifier = TextHelpers.GetValidPascalCaseIdentifierName(codename);
            TypeName = typeName;
        }

        public static Property FromContentType(string codename, string contentType)
        {
            if (!contentTypeToTypeName.ContainsKey(contentType))
            {
                throw new ArgumentException($"Unknown Content Type {contentType}", nameof(contentType));
            }

            return new Property(codename, null)
            {
                Identifier = TextHelpers.GetValidPascalCaseIdentifierName(codename),
                TypeName = contentTypeToTypeName[contentType],
            };
        }
    }
}
