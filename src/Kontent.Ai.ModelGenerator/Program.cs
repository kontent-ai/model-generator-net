using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Kontent.Ai.Delivery;
using Kontent.Ai.Delivery.Abstractions;
using Kontent.Ai.Delivery.Extensions;
using Kontent.Ai.Management.Configuration;
using Kontent.Ai.ModelGenerator.Core;
using Kontent.Ai.ModelGenerator.Core.Configuration;

namespace Kontent.Ai.ModelGenerator
{
    internal class Program
    {
        private static readonly OptionsTypeData ManagementOptionsTypeData = new OptionsTypeData(typeof(ManagementOptions), "management-sdk-net");
        private static readonly OptionsTypeData DeliveryOptionsTypeData = new OptionsTypeData(typeof(DeliveryOptions), "delivery-sdk-net");

        public static async Task<int> Main(string[] args)
        {
            try
            {
                // Create an instance of a DI container
                var services = new ServiceCollection();

                var switchMappings = GetSwitchMappings(args);
                if (ContainsContainsUnsupportedArg(args, switchMappings))
                {
                    Console.WriteLine("Failed to run due to invalid configuration.");
                    return 1;
                }

                // Build a configuration object from given sources
                var configuration = new ConfigurationBuilder()
                            .SetBasePath(Environment.CurrentDirectory)
                            .AddJsonFile("appSettings.json", true)
                            .AddCommandLine(args, switchMappings)
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

                PrintSdkVersion(options.ManagementApi);

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
                { "-p", $"{DeliveryOptionsTypeData.OptionsName}:{nameof(DeliveryOptions.ProjectId)}" },
                {"--projectid", $"{DeliveryOptionsTypeData.OptionsName}:{nameof(DeliveryOptions.ProjectId)}" }, // Backwards compatibility
                { "-s", nameof(CodeGeneratorOptions.StructuredModel) },
                { "-t", nameof(CodeGeneratorOptions.WithTypeProvider) }
            };

            var managementMappings = new Dictionary<string, string>
            {
                { "-p", $"{ManagementOptionsTypeData.OptionsName}:{nameof(ManagementOptions.ProjectId)}" },
                {"--projectid", $"{ManagementOptionsTypeData.OptionsName}:{nameof(ManagementOptions.ProjectId)}" }, // Backwards compatibility
                { "-m", nameof(CodeGeneratorOptions.ManagementApi) },
                { "-k", $"{ManagementOptionsTypeData.OptionsName}:{nameof(ManagementOptions.ApiKey)}" },
                { "--apikey", $"{ManagementOptionsTypeData.OptionsName}:{nameof(ManagementOptions.ApiKey)}" } // Backwards compatibility
            };

            return generalMappings
                .Union(ContainsManageApiArg() ? managementMappings : deliveryMappings)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            bool ContainsManageApiArg() =>
                args.Where((value, index) => (value is "-m" or "--managementapi") && index + 1 < args.Length && args[index + 1] == "true").Any();
        }

        internal static bool ContainsContainsUnsupportedArg(string[] args, IDictionary<string, string> usedMappings)
        {
            var containsUnsupportedArg = false;
            var codeGeneratorOptionsProperties = typeof(CodeGeneratorOptions).GetProperties()
                .Where(p => p.PropertyType != ManagementOptionsTypeData.Type && p.PropertyType != DeliveryOptionsTypeData.Type)
                .Select(p => p.Name.ToLower())
                .ToList();

            foreach (var arg in args.Where(a => a.StartsWith('-')))
            {
                if (!usedMappings.ContainsKey(arg) &&
                    IsOptionPropertyUnsupported(ManagementOptionsTypeData, arg) &&
                    IsOptionPropertyUnsupported(DeliveryOptionsTypeData, arg) &&
                    IsOptionPropertyUnsupported(codeGeneratorOptionsProperties, arg))
                {
                    Console.WriteLine($"Unsupported parameter: {arg}");
                    containsUnsupportedArg = true;
                }
            }

            return containsUnsupportedArg;


        }

        private static bool IsOptionPropertyUnsupported(OptionsTypeData optionsTypeData, string arg) =>
            IsOptionPropertyUnsupported(optionsTypeData.OptionProperties.Select(prop => $"{optionsTypeData.OptionsName}:{prop.Name}"), arg);

        private static bool IsOptionPropertyUnsupported(IEnumerable<string> optionProperties, string arg) =>
            optionProperties.All(prop => $"--{prop}" != arg);

        private static void PrintSdkVersion(bool managementApi)
        {
            var optionsTypeData = managementApi ? ManagementOptionsTypeData : DeliveryOptionsTypeData;
            Console.WriteLine($"Models were generated for {optionsTypeData.SdkName} version {optionsTypeData.SdkVersion}");
        }

        private class OptionsTypeData
        {
            public IEnumerable<PropertyInfo> OptionProperties { get; }
            public string OptionsName { get; }
            public string SdkName { get; }
            public string SdkVersion { get; }
            public Type Type { get; }

            public OptionsTypeData(Type type, string sdkName)
            {
                SdkName = sdkName;
                SdkVersion = Assembly.GetAssembly(type).GetName().Version.ToString(3);
                Type = type;
                OptionProperties = type.GetProperties();
                OptionsName = type.Name;
            }
        }
    }
}
