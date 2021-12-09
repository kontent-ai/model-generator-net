using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Kentico.Kontent.Management.Models.Types;
using Kentico.Kontent.Management.Models.TypeSnippets;
using Kentico.Kontent.ModelGenerator.Core.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Kentico.Kontent.ModelGenerator.Core.ManagementClient
{
    public class ManagementClient : IManagementClient
    {
        private const int MilisecondsDelay = 1000;
        private readonly HttpClient _httpClient;

        public ManagementClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IEnumerable<ContentTypeModel>> GetAllContentTypesAsync(CodeGeneratorOptions options)
        {
            return await GetObjects<ContentTypeModel>(options, "types");
        }

        public async Task<IEnumerable<ContentTypeSnippetModel>> GetAllSnippetsAsync(CodeGeneratorOptions options)
        {
            return await GetObjects<ContentTypeSnippetModel>(options, "snippets");
        }

        private async Task<IEnumerable<T>> GetObjects<T>(CodeGeneratorOptions options, string modelType)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (modelType == null)
            {
                throw new ArgumentNullException(nameof(modelType));
            }

            if (options.ManagementOptions == null)
            {
                throw new ArgumentException($"ManagementClient {nameof(options.ManagementOptions)} cannot be null.");
            }

            if (string.IsNullOrEmpty(options.ManagementOptions.ProjectId))
            {
                throw new ArgumentException($"ManagementClient {nameof(options.ManagementOptions.ProjectId)} cannot be null.");
            }

            if (string.IsNullOrEmpty(options.ManagementOptions.ApiKey))
            {
                throw new ArgumentException($"ManagementClient {nameof(options.ManagementOptions.ApiKey)} cannot be null.");
            }

            var models = new List<T>();
            string continuationToken = null;
            do
            {
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", options.ManagementOptions.ApiKey);
                if (continuationToken != null)
                {
                    _httpClient.DefaultRequestHeaders.Add("x-continuation", continuationToken);
                }

                var response = await _httpClient.GetAsync(new Uri($"https://manage.kontent.ai/v2/projects/{options.ManagementOptions.ProjectId}/{modelType}"), HttpCompletionOption.ResponseContentRead);

                var responseStream = await response.Content.ReadAsStreamAsync();
                var responseObject = await JToken.ReadFromAsync(new JsonTextReader(new StreamReader(responseStream)));

                continuationToken = responseObject["pagination"]?["continuation_token"]?.ToObject<string>();

                models.AddRange(responseObject[modelType]?.ToObject<List<T>>()!);

                await Task.Delay(MilisecondsDelay);
            }
            while (continuationToken != null);

            return models;
        }
    }
}
