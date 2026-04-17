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
            services.AddDeliveryClient(configuration);
            services.AddTransient<HttpClient>();
            services.AddTransient<IOutputProvider, FileSystemOutputProvider>();
            services.AddSingleton<IUserMessageLogger, UserMessageLogger>();
            services.AddSingleton<IClassCodeGeneratorFactory, ClassCodeGeneratorFactory>();
            services.AddSingleton<IClassDefinitionFactory, ClassDefinitionFactory>();
            services.AddSingleton<IDeliveryElementService, DeliveryElementService>();
            services.AddSingleton<DeliveryCodeGenerator>();

            // Build the DI container
            var serviceProvider = services.BuildServiceProvider();

            // Validate configuration of the Delivery Client
            var options = serviceProvider.GetService<IOptions<CodeGeneratorOptions>>().Value;
            options.Validate();

            PrintSdkVersion();

            // Code generator entry point
            return await serviceProvider.GetService<DeliveryCodeGenerator>().RunAsync();
        }
        catch (AggregateException aex)
        {
            // Return a friendlier message for exceptions
            if (aex.InnerExceptions.Count == 1 && aex.InnerException != null)
            {
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

    private static void PrintSdkVersion()
    {
        var usedSdkInfo = ArgHelpers.GetUsedSdkInfo();
        Console.WriteLine($"Models were generated for {usedSdkInfo.Name} version {usedSdkInfo.Version}");
    }
}
