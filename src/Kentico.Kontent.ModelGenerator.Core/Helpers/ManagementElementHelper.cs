using System;
using System.Collections.Generic;
using System.Linq;
using Kentico.Kontent.Delivery.Abstractions;
using Kentico.Kontent.Management.Models.Types;
using Kentico.Kontent.Management.Models.Types.Elements;
using Kentico.Kontent.Management.Models.TypeSnippets;

namespace Kentico.Kontent.ModelGenerator.Core.Helpers
{
    public static class ManagementElementHelper
    {
        public static IEnumerable<ElementMetadataBase> GetManagementContentTypeSnippetElements(
            ElementMetadataBase element,
            IEnumerable<ContentTypeSnippetModel> managementSnippets)
        {
            Validate(element, managementSnippets);

            if (element.Type != ElementMetadataType.ContentTypeSnippet)
            {
                return null;
            }

            var managementSnippet = managementSnippets.FirstOrDefault(s => element.Codename == s.Codename);
            if (managementSnippet == null)
            {
                throw new ArgumentException($"{nameof(managementSnippet)} shouldn't be null.");
            }

            return managementSnippet.Elements;
        }

        private static void Validate(
            ElementMetadataBase element,
            IEnumerable<ContentTypeSnippetModel> managementSnippets)
        {
            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }

            if (managementSnippets == null)
            {
                throw new ArgumentNullException(nameof(managementSnippets));
            }
        }
    }
}
