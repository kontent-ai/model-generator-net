using Kentico.Kontent.ModelGenerator.Core.Configuration;
using RichardSzalay.MockHttp;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using ManagementClientImpl = Kentico.Kontent.ModelGenerator.Core.ManagementClient.ManagementClient;

namespace Kentico.Kontent.ModelGenerator.Tests.ManagementClient
{
    public class ManagementClientTests
    {
        private static HttpClient _httpClient;
        public ManagementClientTests()
        {
            var mockHttp = new MockHttpMessageHandler();
            _httpClient = mockHttp.ToHttpClient();
        }

        [Fact]
        public void GetAllContentTypesAsync_OptionsIsNull_Throws()
        {
            var client = new ManagementClientImpl(_httpClient);

            var exception = Assert.ThrowsAsync<ArgumentNullException>(() => client.GetAllContentTypesAsync(null));
            Assert.Equal("options", exception.Result.ParamName);
        }

        [Fact]
        public void GetAllContentTypesAsync_ManagementOptionsIsNull_Throws()
        {
            var client = new ManagementClientImpl(_httpClient);

            var options = new CodeGeneratorOptions
            {
                ManagementOptions = null
            };

            var exception = Assert.ThrowsAsync<ArgumentException>(() => client.GetAllContentTypesAsync(options));
            Assert.Equal("ManagementClient ManagementOptions cannot be null.", exception.Result.Message);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void GetAllContentTypesAsync_ManagementOptionsApiKeyIsNullOrEmpty_Throws(string apiKey)
        {
            var client = new ManagementClientImpl(_httpClient);

            var options = new CodeGeneratorOptions
            {
                ManagementOptions = new Management.ManagementOptions
                {
                    ProjectId = "139985ac-4aa5-436b-96a2-94c2d4fbbedc",
                    ApiKey = apiKey
                }
            };

            var exception = Assert.ThrowsAsync<ArgumentException>(() => client.GetAllContentTypesAsync(options));
            Assert.Equal("ManagementClient ApiKey cannot be null.", exception.Result.Message);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void GetAllContentTypesAsync_ManagementOptionsProjectIdIsNull_Throws(string projectId)
        {
            var client = new ManagementClientImpl(_httpClient);

            var options = new CodeGeneratorOptions
            {
                ManagementOptions = new Management.ManagementOptions
                {
                    ProjectId = projectId
                }
            };

            var exception = Assert.ThrowsAsync<ArgumentException>(() => client.GetAllContentTypesAsync(options));
            Assert.Equal("ManagementClient ProjectId cannot be null.", exception.Result.Message);
        }

        [Fact]
        public async Task GetAllContentTypesAsync_Returns()
        {
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When("https://manage.kontent.ai/v2/projects/*/types")
                .Respond("application/json", await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "Fixtures/management_types.json")));
            var httpClient = mockHttp.ToHttpClient();

            var client = new ManagementClientImpl(httpClient);

            var options = new CodeGeneratorOptions
            {
                ManagementOptions = new Management.ManagementOptions
                {
                    ProjectId = "139985ac-4aa5-436b-96a2-94c2d4fbbedc",
                    ApiKey = "apiKey"
                }
            };

            var result = await client.GetAllContentTypesAsync(options);

            Assert.NotNull(result);
            Assert.Equal(14, result.Count);
        }

        [Fact]
        public void GetAllSnippetsAsync_OptionsIsNull_Throws()
        {
            var client = new ManagementClientImpl(_httpClient);

            var exception = Assert.ThrowsAsync<ArgumentNullException>(() => client.GetAllSnippetsAsync(null));
            Assert.Equal("options", exception.Result.ParamName);
        }

        [Fact]
        public void GetAllSnippetsAsync_ManagementOptionsIsNull_Throws()
        {
            var client = new ManagementClientImpl(_httpClient);

            var options = new CodeGeneratorOptions
            {
                ManagementOptions = null
            };

            var exception = Assert.ThrowsAsync<ArgumentException>(() => client.GetAllSnippetsAsync(options));
            Assert.Equal("ManagementClient ManagementOptions cannot be null.", exception.Result.Message);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void GetAllSnippetsAsync_ManagementOptionsApiKeyIsNullOrEmpty_Throws(string apiKey)
        {
            var client = new ManagementClientImpl(_httpClient);

            var options = new CodeGeneratorOptions
            {
                ManagementOptions = new Management.ManagementOptions
                {
                    ProjectId = "139985ac-4aa5-436b-96a2-94c2d4fbbedc",
                    ApiKey = apiKey
                }
            };

            var exception = Assert.ThrowsAsync<ArgumentException>(() => client.GetAllSnippetsAsync(options));
            Assert.Equal("ManagementClient ApiKey cannot be null.", exception.Result.Message);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void GetAllSnippetsAsync_ManagementOptionsProjectIdIsNull_Throws(string projectId)
        {
            var client = new ManagementClientImpl(_httpClient);

            var options = new CodeGeneratorOptions
            {
                ManagementOptions = new Management.ManagementOptions
                {
                    ProjectId = projectId
                }
            };

            var exception = Assert.ThrowsAsync<ArgumentException>(() => client.GetAllSnippetsAsync(options));
            Assert.Equal("ManagementClient ProjectId cannot be null.", exception.Result.Message);
        }

        [Fact]
        public async Task GetAllSnippetsAsync_Returns()
        {
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When("https://manage.kontent.ai/v2/projects/*/snippets")
                .Respond("application/json", await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "Fixtures/management_snippets.json")));
            var httpClient = mockHttp.ToHttpClient();

            var client = new ManagementClientImpl(httpClient);

            var options = new CodeGeneratorOptions
            {
                ManagementOptions = new Management.ManagementOptions
                {
                    ProjectId = "139985ac-4aa5-436b-96a2-94c2d4fbbedc",
                    ApiKey = "apiKey"
                }
            };

            var result = await client.GetAllSnippetsAsync(options);

            Assert.NotNull(result);
            Assert.Equal(1, result.Count);
        }
    }
}
