using System;
using System.Collections.Generic;
using System.Linq;
using Kentico.Kontent.Delivery.Abstractions;
using Kentico.Kontent.Management.Models.Types;
using Kentico.Kontent.Management.Models.Types.Elements;
using Kentico.Kontent.Management.Models.TypeSnippets;

namespace Kentico.Kontent.ModelGenerator.Core
{
    public static class ElementIdHelper
    {
        public static string GetElementId(bool cmApi, ICollection<SnippetModel> managementSnippets, ContentTypeModel managementContentType, IContentElement element)
        {
            Validate(managementSnippets, managementContentType, element);

            if (!cmApi)
            {
                return null;
            }

            var contentTypeElement = managementContentType.Elements.FirstOrDefault(el => el.Codename == element.Codename);
            if (contentTypeElement != null)
            {
                return contentTypeElement.Id.ToString();
            }

            var snippet = managementSnippets.FirstOrDefault(s =>
                 managementContentType.Elements.FirstOrDefault(el =>
                     el.Type == ElementMetadataType.Snippet && el.Codename == s.Codename) != null);

            if (snippet == null)
            {
                throw new ArgumentException($"{nameof(snippet)} shouldn't be null.");
            }

            var snippetElement = snippet.Elements.FirstOrDefault(el => el.Codename == element.Codename);
            if (snippetElement == null)
            {
                throw new ArgumentException($"{nameof(snippetElement)} shouldn't be null.");
            }

            return snippetElement.Id.ToString();
        }

        private static void Validate(ICollection<SnippetModel> managementSnippets, ContentTypeModel managementContentType, IContentElement element)
        {
            if (managementSnippets == null)
            {
                throw new ArgumentNullException(nameof(managementSnippets));
            }

            if (managementContentType == null)
            {
                throw new ArgumentNullException(nameof(managementContentType));
            }

            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }
        }
    }
}
