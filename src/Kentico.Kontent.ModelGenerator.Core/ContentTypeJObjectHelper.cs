using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Kentico.Kontent.ModelGenerator.Core
{
    public static class ContentTypeJObjectHelper
    {
        public static string GetElementIdFromContentType(JObject managementContentType, string elementCodename)
        {
            if (!managementContentType.TryGetValue("elements", out var elements))
            {
                throw new InvalidIdException($"Unable to create a valid Id for '{elementCodename}', couldn't find {nameof(elements)}.");
            }

            if (elements is not { Type: JTokenType.Array })
            {
                throw new InvalidIdException($"Unable to create a valid Id for '{elementCodename}', {nameof(elements)} has invalid type.");
            }

            var element = elements.ToObject<List<JObject>>().FirstOrDefault(el =>
            {
                if (!el.TryGetValue("codename", out var codename))
                {
                    throw new InvalidIdException($"Unable to create a valid Id for '{elementCodename}', couldn't find {nameof(codename)}.");
                }

                if (codename is not { Type: JTokenType.String })
                {
                    throw new InvalidIdException($"Unable to create a valid Id for '{elementCodename}', {nameof(elements)} has invalid type.");
                }

                return codename.ToObject<string>() == elementCodename;
            });

            if (element == null)
            {
                throw new InvalidIdException($"Unable to create a valid Id for '{elementCodename}', missing element.");
            }

            if (!element.TryGetValue("id", out var elementId))
            {
                throw new InvalidIdException($"Unable to create a valid Id for '{elementCodename}', couldn't find {nameof(elementId)}.");
            }

            if (elementId is not { Type: JTokenType.String })
            {
                throw new InvalidIdException($"Unable to create a valid Id for '{elementCodename}', {nameof(elementId)} has invalid type.");
            }

            return elementId.ToObject<string>();
        }
    }
}
