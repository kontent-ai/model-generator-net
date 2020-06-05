using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Kentico.Kontent.Delivery.Configuration.DeliveryOptions;
using Kentico.Kontent.Delivery.Extensions;
using Kentico.Kontent.ModelGenerator.Configuration;

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
                services.AddTransient<CodeGenerator>();

                // Build the DI container
                var serviceProvider = services.BuildServiceProvider();

                // Validate configuration of the Delivery Client
                serviceProvider.GetService<IOptions<CodeGeneratorOptions>>().Value.DeliveryOptions.Validate();

                // Code generator entry point
                return await serviceProvider.GetService<CodeGenerator>().RunAsync();
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
                {"-b", nameof(CodeGeneratorOptions.BaseClass) }
            };
            return mappings;
        }
    }
}
