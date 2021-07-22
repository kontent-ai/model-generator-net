using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Kentico.Kontent.ModelGenerator.Core.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Kentico.Kontent.ModelGenerator.Core
{
    public class ManagementClient : IManagementClient
    {
        private const int MilisecondsDelay = 1000;
        private readonly HttpClient _httpClient;

        public ManagementClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IList<JObject>> GetAllContentTypesAsync(CodeGeneratorOptions options)
        {
            var contentTypes = new List<JObject>();
            string continuationToken = null;
            do
            {
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", options.ManagementOptions.ApiKey);
                if (continuationToken != null)
                {
                    _httpClient.DefaultRequestHeaders.Add("x-continuation", continuationToken);
                }

                var response = await _httpClient.GetAsync(new Uri($"https://manage.kontent.ai/v2/projects/{options.ManagementOptions.ProjectId}/types"), HttpCompletionOption.ResponseContentRead);

                var responseStream = await response.Content.ReadAsStreamAsync();
                var responseObject = await JObject.ReadFromAsync(new JsonTextReader(new StreamReader(responseStream)));

                continuationToken = responseObject["pagination"]["continuation_token"].ToObject<string>();

                contentTypes.AddRange(responseObject["types"].ToObject<List<JObject>>());

                await Task.Delay(MilisecondsDelay);
            }
            while (continuationToken != null);

            return contentTypes;
        }
    }
}
