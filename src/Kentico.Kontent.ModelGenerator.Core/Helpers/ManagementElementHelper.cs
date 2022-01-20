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
        public static ElementMetadataBase GetManagementElement(
            bool managementApi,
            IContentElement deliverElement,
            IEnumerable<ContentTypeSnippetModel> managementSnippets,
            ContentTypeModel managementContentType)
        {
            if (!managementApi)
            {
                return null;
            }

            Validate(deliverElement, managementSnippets, managementContentType);

            var managementContentTypeElement = managementContentType.Elements.FirstOrDefault(el => el.Codename == deliverElement.Codename);
            if (managementContentTypeElement != null)
            {
                return managementContentTypeElement;
            }

            var managementSnippet = managementSnippets.FirstOrDefault(s =>
                managementContentType.Elements.FirstOrDefault(el =>
                    el.Type == ElementMetadataType.ContentTypeSnippet && el.Codename == s.Codename) != null);
            if (managementSnippet == null)
            {
                throw new ArgumentException($"{nameof(managementSnippet)} shouldn't be null.");
            }

            var managementSnippetElement = managementSnippet.Elements.FirstOrDefault(el => el.Codename == deliverElement.Codename);
            if (managementSnippetElement == null)
            {
                throw new ArgumentException($"{nameof(managementSnippetElement)} shouldn't be null.");
            }

            return managementSnippetElement;
        }

        private static void Validate(
            IContentElement deliverElement,
            IEnumerable<ContentTypeSnippetModel> managementSnippets,
            ContentTypeModel managementContentType)
        {
            if (deliverElement == null)
            {
                throw new ArgumentNullException(nameof(deliverElement));
            }

            if (managementSnippets == null)
            {
                throw new ArgumentNullException(nameof(managementSnippets));
            }

            if (managementContentType == null)
            {
                throw new ArgumentNullException(nameof(managementContentType));
            }
        }
    }
}
