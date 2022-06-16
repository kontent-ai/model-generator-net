﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Kentico.Kontent.Delivery.Abstractions;
using Kentico.Kontent.Management.Models.LanguageVariants.Elements;
using Kentico.Kontent.Management.Models.Types.Elements;
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

        internal static readonly Dictionary<string, string> DeliverElementTypesDictionary = new Dictionary<string, string>
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

        internal static readonly Dictionary<string, string> ExtendedDeliverElementTypesDictionary = new Dictionary<string, string>
        {
            { ElementMetadataType.Text.ToString(), "string" },
            { ElementMetadataType.RichText.ToString(), "string" },
            { ElementMetadataType.RichText + StructuredSuffix, nameof(IRichTextContent)},
            { ElementMetadataType.Number.ToString(), "decimal?" },
            { ElementMetadataType.MultipleChoice.ToString(), $"{nameof(IEnumerable)}<{nameof(IMultipleChoiceOption)}>"},
            { ElementMetadataType.DateTime.ToString(), "DateTime?" },
            { ElementMetadataType.Asset.ToString(), $"{nameof(IEnumerable)}<{nameof(IAsset)}>" },
            { ElementMetadataType.LinkedItems.ToString(), null },
            { ElementMetadataType.Taxonomy.ToString(), $"{nameof(IEnumerable)}<{nameof(ITaxonomyTerm)}>" },
            { ElementMetadataType.UrlSlug.ToString(), "string" },
            { ElementMetadataType.Custom.ToString(), "string" }
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

        public static bool IsContentTypeSupported(string elementType, bool extendedDeliverModels)
        {
            return extendedDeliverModels
                ? ExtendedDeliverElementTypesDictionary.ContainsKey(elementType)
                : DeliverElementTypesDictionary.ContainsKey(elementType);
        }

        public static bool IsContentTypeSupported(ElementMetadataType elementType)
        {
            return ManagementElementTypesDictionary.ContainsKey(elementType);
        }

        public static Property FromContentTypeElement(string codename, string elementType)
        {
            if (IsContentTypeSupported(elementType, false))
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

        public static Property FromContentTypeElement(ElementMetadataBase element, string elementType)
        {
            return FromContentTypeElement(element, elementType, element.Codename);
        }

        public static Property FromContentTypeElement(ElementMetadataBase element, string elementType, string finalPropertyName)
        {
            if (IsContentTypeSupported(element.Type.ToString(), true))
            {
                var resultElementType = element.Type == ElementMetadataType.LinkedItems
                    ? elementType
                    : ExtendedDeliverElementTypesDictionary[elementType];

                return new Property(finalPropertyName, resultElementType);
            }

            if (element.Type == ElementMetadataType.Guidelines)
            {
                throw new UnsupportedTypeException();
            }

            throw new ArgumentException($"Unknown Content Type {element.Type}", nameof(element));
        }
    }
}
