using Kontent.Ai.Management.Models.Types;
using Kontent.Ai.Management.Models.TypeSnippets;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Kontent.Ai.ModelGenerator.Core.Tests.Fixtures;

internal class ManagementModelsProvider
{
    public IEnumerable<ContentTypeModel> ManagementContentTypeModels { get; }
    public IEnumerable<ContentTypeSnippetModel> ManagementContentTypeSnippetModels { get; }

    public ManagementModelsProvider()
    {
        ManagementContentTypeModels = GetModels<ContentTypeModel>("Fixtures/management_types.json");
        ManagementContentTypeSnippetModels = GetModels<ContentTypeSnippetModel>("Fixtures/management_snippets.json");
    }

    private static IEnumerable<T> GetModels<T>(string filePath)
    {
        var stringServerResponse = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, filePath));
        var jTokenServerResponse = JToken.ReadFrom(new JsonTextReader(new StringReader(stringServerResponse)));

        var objectTypesProperty = Activator.CreateInstance(typeof(T)) switch
        {
            ContentTypeModel => "types",
            ContentTypeSnippetModel => "snippets",
            _ => throw new NotSupportedException()
        };

        return jTokenServerResponse[objectTypesProperty].ToObject<IEnumerable<T>>();
    }
}
