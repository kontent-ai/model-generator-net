using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Kentico.Kontent.Delivery;
using Kentico.Kontent.Delivery.Abstractions;
using Kentico.Kontent.Delivery.Extensions;
using Kentico.Kontent.ModelGenerator.Core;
using Kentico.Kontent.ModelGenerator.Core.Configuration;
using Kentico.Kontent.ModelGenerator.Core.ManagementClient;

namespace Kentico.Kontent.ModelGenerator
{
    internal class Program
    {
        public static async Task<int> Main(string[] args)
        {
            try
            {
                // Create an instance of a DI container
                var services = new ServiceCollection();

                // Build a configuration object from given sources
                var configuration = new ConfigurationBuilder()
                            .SetBasePath(Environment.CurrentDirectory)
                            .AddJsonFile("appSettings.json", true)
                            .AddCommandLine(args, GetSwitchMappings())
                            .Build();

                // Fill the DI container
                services.Configure<CodeGeneratorOptions>(configuration);
                services.AddDeliveryClient(configuration);
                services.AddTransient<HttpClient>();
                services.AddTransient<IManagementClient, ManagementClient>();
                services.AddTransient<IOutputProvider, FileSystemOutputProvider>();
                services.AddTransient<CodeGenerator>();

                // Build the DI container
                var serviceProvider = services.BuildServiceProvider();

                // Validate configuration of the Delivery Client
                serviceProvider.GetService<IOptions<CodeGeneratorOptions>>().Value.Validate();

                // Code generator entry point
                return await serviceProvider.GetService<CodeGenerator>().RunAsync();
            }
            catch (AggregateException aex)
            {
                if ((aex.InnerExceptions.Count == 1) && aex.InnerException is DeliveryException)
                {
                    // Return a friendlier message
                    Console.WriteLine(aex.InnerException.Message);
                }
                return 1;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return 1;
            }
        }

        private static IDictionary<string, string> GetSwitchMappings()
        {
            var mappings = new Dictionary<string, string>
            {
                {"-p", $"{nameof(DeliveryOptions)}:{nameof(DeliveryOptions.ProjectId)}" },
                {"--projectid", $"{nameof(DeliveryOptions)}:{nameof(DeliveryOptions.ProjectId)}" }, // Backwards compatibility
                {"-n", nameof(CodeGeneratorOptions.Namespace) },
                {"-o", nameof(CodeGeneratorOptions.OutputDir) },
                {"-f", nameof(CodeGeneratorOptions.FileNameSuffix) },
                {"-g", nameof(CodeGeneratorOptions.GeneratePartials) },
                {"-t", nameof(CodeGeneratorOptions.WithTypeProvider) },
                {"-s", nameof(CodeGeneratorOptions.StructuredModel) },
                {"-c", nameof(CodeGeneratorOptions.ContentManagementApi) },
                {"-k", $"{nameof(CodeGeneratorOptions.ManagementOptions)}:{nameof(CodeGeneratorOptions.ManagementOptions.ApiKey)}" },
                {"-b", nameof(CodeGeneratorOptions.BaseClass) }
            };
            return mappings;
        }
    }
}
