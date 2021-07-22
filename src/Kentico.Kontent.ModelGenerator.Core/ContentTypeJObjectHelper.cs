using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Kentico.Kontent.ModelGenerator.Core
{
    public static class ContentTypeJObjectHelper
    {
        public static string GetElementIdFromContentType(JObject managementContentType, string elementCodename)
        {
            return managementContentType["elements"].ToObject<List<JObject>>()
                .FirstOrDefault(el => el["codename"].ToObject<string>() == elementCodename)["id"].ToObject<string>();
        }
    }
}
