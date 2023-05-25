using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Kontent.Ai.Delivery;
using Kontent.Ai.Delivery.Extensions;
using Kontent.Ai.ModelGenerator.Core;
using Kontent.Ai.ModelGenerator.Core.Common;
using Kontent.Ai.ModelGenerator.Core.Configuration;
using Kontent.Ai.ModelGenerator.Options;
using Kontent.Ai.ModelGenerator.Core.Contract;
using Kontent.Ai.ModelGenerator.Core.Services;

namespace Kontent.Ai.ModelGenerator;

internal class Program
{
    public static async Task<int> Main(string[] args)
    {
        var logger = new UserMessageLogger();
        try
        {
            // Create an instance of a DI container
            var services = new ServiceCollection();

            var argValidationResult = ArgHelpers.ContainsValidArgs(args);
            if (argValidationResult.HasUnsupportedParams)
            {
                await logger.LogErrorAsync("Failed to run due to invalid configuration.");
                foreach (var unsupportedArg in argValidationResult.UnsupportedArgs)
                {
                    await logger.LogErrorAsync($"Unsupported parameter: {unsupportedArg}");
                }

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
            services.AddSingleton<IUserMessageLogger>(logger);
            services.AddSingleton<IClassCodeGeneratorFactory, ClassCodeGeneratorFactory>();
            services.AddSingleton<IClassDefinitionFactory, ClassDefinitionFactory>();
            services.AddSingleton<IDeliveryElementService, DeliveryElementService>();
            services.AddSingleton<ManagementCodeGenerator>();
            services.AddSingleton<DeliveryCodeGenerator>();
            services.AddSingleton<ExtendedDeliveryCodeGenerator>();

            // Build the DI container
            var serviceProvider = services.BuildServiceProvider();

            // Validate configuration of the Delivery Client
            var options = serviceProvider.GetService<IOptions<CodeGeneratorOptions>>().Value;
            options.Validate();

            await PrintSdkVersion(options);

            // Code generator entry point
            return options.GetDesiredModelsType() switch
            {
                DesiredModelsType.Delivery => await serviceProvider.GetService<DeliveryCodeGenerator>().RunAsync(),
                DesiredModelsType.ExtendedDelivery => await serviceProvider.GetService<ExtendedDeliveryCodeGenerator>().RunAsync(),
                DesiredModelsType.Management => await serviceProvider.GetService<ManagementCodeGenerator>().RunAsync(),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
        catch (AggregateException aex)
        {
            if ((aex.InnerExceptions.Count == 1) && aex.InnerException is DeliveryException)
            {
                // Return a friendlier message
                await logger.LogErrorAsync(aex.InnerException.Message);
            }
            return 1;
        }
        catch (Exception ex)
        {
            await logger.LogErrorAsync(ex.Message);
            return 1;
        }

        async Task PrintSdkVersion(CodeGeneratorOptions codeGeneratorOptions)
        {
            var usedSdkInfo = ArgHelpers.GetUsedSdkInfo(codeGeneratorOptions.GetDesiredModelsType());
            await logger.LogErrorAsync($"Models were generated for {usedSdkInfo.Name} version {usedSdkInfo.Version}");
        }
    }
}
