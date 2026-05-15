using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Kontent.Ai.Delivery;
using Kontent.Ai.Delivery.Extensions;
using Kontent.Ai.Management;
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
            if (!ArgHelpers.ContainsValidArgs(args))
            {
                await WriteErrorMessageAsync("Failed to run due to invalid configuration.");
                return 1;
            }

            var managementMode = ArgHelpers.IsManagementMode(args);
            // Mode-switch flags don't bind to a config property — strip them before they reach
            // Microsoft.Extensions.Configuration.AddCommandLine, which would otherwise complain.
            var bindableArgs = ArgHelpers.StripModeSwitches(args);

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Environment.CurrentDirectory)
                .AddJsonFile("appSettings.json", true)
                .AddCommandLine(bindableArgs, ArgHelpers.GetSwitchMappings(args))
                .Build();

            var services = new ServiceCollection();
            services.Configure<CodeGeneratorOptions>(configuration);

            var generatorServiceType = managementMode
                ? ConfigureManagementMode(services, configuration)
                : ConfigureDeliveryMode(services, configuration);

            var serviceProvider = services.BuildServiceProvider();

            var options = serviceProvider.GetRequiredService<IOptions<CodeGeneratorOptions>>().Value;
            if (managementMode)
            {
                options.ValidateManagement();
            }
            else
            {
                options.Validate();
            }

            PrintSdkVersion(managementMode);

            return await ((CodeGeneratorBase)serviceProvider.GetRequiredService(generatorServiceType)).RunAsync();
        }
        catch (AggregateException aex)
        {
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

    private static Type ConfigureDeliveryMode(IServiceCollection services, IConfiguration configuration)
    {
        services.AddDeliveryClient(configuration);
        services.AddTransient<HttpClient>();
        services.AddTransient<IOutputProvider, FileSystemOutputProvider>();
        services.AddSingleton<IUserMessageLogger, UserMessageLogger>();
        services.AddSingleton<IClassCodeGeneratorFactory, ClassCodeGeneratorFactory>();
        services.AddSingleton<IClassDefinitionFactory, ClassDefinitionFactory>();
        services.AddSingleton<IDeliveryElementService, DeliveryElementService>();
        services.AddSingleton<DeliveryCodeGenerator>();
        return typeof(DeliveryCodeGenerator);
    }

    private static Type ConfigureManagementMode(IServiceCollection services, IConfiguration configuration)
    {
        // 8.2.0 doesn't ship an AddManagementClient DI extension yet; build the client manually
        // from the bound options. ManagementClient owns its HttpClient; the DI container disposes
        // the singleton when the process exits.
        services.AddSingleton<IManagementClient>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<CodeGeneratorOptions>>().Value;
            return new ManagementClient(options.ManagementOptions);
        });

        services.AddTransient<IOutputProvider, FileSystemOutputProvider>();
        services.AddSingleton<IUserMessageLogger, UserMessageLogger>();
        services.AddSingleton<IClassCodeGeneratorFactory, ClassCodeGeneratorFactory>();
        services.AddSingleton<IClassDefinitionFactory, ClassDefinitionFactory>();
        services.AddSingleton<IManagementElementService, ManagementElementService>();
        services.AddSingleton<ManagementCodeGenerator>();
        return typeof(ManagementCodeGenerator);
    }

    private static async Task WriteErrorMessageAsync(string message) => await Console.Error.WriteLineAsync(message);

    private static void PrintSdkVersion(bool managementMode)
    {
        var usedSdkInfo = ArgHelpers.GetUsedSdkInfo(managementMode);
        Console.WriteLine($"Models were generated for {usedSdkInfo.Name} version {usedSdkInfo.Version}");
    }
}
