﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Kontent.Ai.Delivery;
using Kontent.Ai.Delivery.Extensions;
using Kontent.Ai.ModelGenerator.Core;
using Kontent.Ai.ModelGenerator.Core.Configuration;

namespace Kontent.Ai.ModelGenerator
{
    internal class Program
    {
        public static async Task<int> Main(string[] args)
        {
            try
            {
                // Create an instance of a DI container
                var services = new ServiceCollection();

                if (!ArgHelpers.ContainsValidArgs(args))
                {
                    await WriteErrorMessageAsync("Failed to run due to invalid configuration.");
                    return 1;
                }

                // Build a configuration object from given sources
                var configuration = new ConfigurationBuilder()
                            .SetBasePath(Environment.CurrentDirectory)
                            .AddJsonFile("appSettings.json", true)
                            .AddCommandLine(args, ArgHelpers.GetSwitchMappings(args))
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

                PrintSdkVersion(options);

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
                    await WriteErrorMessageAsync(aex.InnerException.Message);
                }
                return 1;
            }
            catch (Exception ex)
            {
                await WriteErrorMessageAsync(ex.Message);
                return 1;
            }
        }

        private static async Task WriteErrorMessageAsync(string message) => await Console.Error.WriteLineAsync(message);

        private static void PrintSdkVersion(CodeGeneratorOptions options)
        {
            var usedSdkInfo = ArgHelpers.GetUsedSdkInfo(options.ManagementApi);
            Console.WriteLine($"Models were generated for {usedSdkInfo.Name} version {usedSdkInfo.Version}");
        }
    }
}
