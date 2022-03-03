﻿using Kentico.Kontent.Management;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Kentico.Kontent.ModelGenerator
{
    internal static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddManagementClient(this IServiceCollection services, IConfiguration configuration, string configurationSectionName = "ManagementOptions")
            => services
                .LoadOptionsConfiguration(configuration, configurationSectionName)
                .RegisterDependencies();

        private static IServiceCollection RegisterDependencies(this IServiceCollection services)
        {
            services.AddSingleton<IManagementClient, ManagementClient>();
            return services;
        }

        private static IServiceCollection LoadOptionsConfiguration(this IServiceCollection services, IConfiguration configuration, string configurationSectionName)
        {
            var managementOptions = new ManagementOptions();
            configuration.GetSection(configurationSectionName).Bind(managementOptions);
            services.AddSingleton(managementOptions);

            return services;
        }
    }
}
