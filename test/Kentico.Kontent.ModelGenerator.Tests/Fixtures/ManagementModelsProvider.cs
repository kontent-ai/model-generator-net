using System;
using System.Collections.Generic;
using System.IO;
using Kontent.Ai.Management.Models.Types;
using Kontent.Ai.Management.Models.TypeSnippets;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Kontent.Ai.ModelGenerator.Tests.Fixtures
{
    internal class ManagementModelsProvider
    {
        public IEnumerator<ContentTypeModel> ManagementContentTypeModels { get; }
        public IEnumerator<ContentTypeSnippetModel> ManagementContentTypeSnippetModels { get; }

        public ManagementModelsProvider()
        {
            ManagementContentTypeModels = GetModels<ContentTypeModel>("Fixtures/management_types.json");
            ManagementContentTypeSnippetModels = GetModels<ContentTypeSnippetModel>("Fixtures/management_snippets.json");
        }

        private static IEnumerator<T> GetModels<T>(string filePath)
        {
            var stringServerResponse = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, filePath));
            var jTokenServerResponse = JToken.ReadFrom(new JsonTextReader(new StringReader(stringServerResponse)));

            var objectTypesProperty = Activator.CreateInstance(typeof(T)) switch
            {
                ContentTypeModel => "types",
                ContentTypeSnippetModel => "snippets",
                _ => throw new NotSupportedException()
            };

            return jTokenServerResponse[objectTypesProperty]
                .ToObject<IEnumerable<T>>()
                .GetEnumerator();
        }
    }
}
