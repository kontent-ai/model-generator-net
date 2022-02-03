using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Kentico.Kontent.Delivery;
using Kentico.Kontent.Delivery.Abstractions;
using Kentico.Kontent.Delivery.Extensions;
using Kentico.Kontent.Management;
using Kentico.Kontent.ModelGenerator.Core;
using Kentico.Kontent.ModelGenerator.Core.Configuration;

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
                            .AddCommandLine(args, GetSwitchMappings(args))
                            .Build();

                // Fill the DI container
                services.Configure<CodeGeneratorOptions>(configuration);
                services.AddManagementClient(configuration);
                services.AddDeliveryClient(configuration);
                services.AddTransient<HttpClient>();
                services.AddTransient<IOutputProvider, FileSystemOutputProvider>();
                services.AddSingleton<ManagementCodeGenerator>();
                services.AddSingleton<DeliveryCodeGenerator>();

                // Build the DI container
                var serviceProvider = services.BuildServiceProvider();

                // Validate configuration of the Delivery Client
                var options = serviceProvider.GetService<IOptions<CodeGeneratorOptions>>().Value;
                options.Validate();

                // Code generator entry point
                return options.ManagementApi
                    ? await serviceProvider.GetService<ManagementCodeGenerator>().RunAsync()
                    : await serviceProvider.GetService<DeliveryCodeGenerator>().RunAsync();
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

        internal static IDictionary<string, string> GetSwitchMappings(string[] args)
        {
            var generalMappings = new Dictionary<string, string>
            {
                { "-n", nameof(CodeGeneratorOptions.Namespace) },
                { "-o", nameof(CodeGeneratorOptions.OutputDir) },
                { "-f", nameof(CodeGeneratorOptions.FileNameSuffix) },
                { "-g", nameof(CodeGeneratorOptions.GeneratePartials) },
                { "-b", nameof(CodeGeneratorOptions.BaseClass) }
            };

            var deliveryMappings = new Dictionary<string, string>
            {
                { "-p", $"{nameof(DeliveryOptions)}:{nameof(DeliveryOptions.ProjectId)}" },
                { "-s", nameof(CodeGeneratorOptions.StructuredModel) },
                { "-t", nameof(CodeGeneratorOptions.WithTypeProvider) }
            };

            var managementMappings = new Dictionary<string, string>
            {
                { "-p", $"{nameof(ManagementOptions)}:{nameof(ManagementOptions.ProjectId)}" },
                { "-m", nameof(CodeGeneratorOptions.ManagementApi) },
                { "-k", $"{nameof(ManagementOptions)}:{nameof(ManagementOptions.ApiKey)}" }
            };

            return generalMappings
                .Union(ContainsManageApiArg() ? managementMappings : deliveryMappings)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            bool ContainsManageApiArg() =>
                args.Where((value, index) => value == "-m" && index + 1 < args.Length && args[index + 1] == "true")
                    .Any();
        }
    }
}
