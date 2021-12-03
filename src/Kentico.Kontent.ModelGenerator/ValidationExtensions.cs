using System;
using Kentico.Kontent.Delivery.Abstractions;
using Kentico.Kontent.Delivery.Configuration;
using Kentico.Kontent.Management;
using Kentico.Kontent.ModelGenerator.Core.Configuration;

namespace Kentico.Kontent.ModelGenerator
{
    /// <summary>
    /// Extension methods meant for validation.
    /// </summary>
    public static class ValidationExtensions
    {
        private const string SeePart = "See http://bit.ly/k-params for more details on configuration.";

        /// <summary>
        /// Validates that CodeGeneratorOptions are initialized and performs some extra integrity validations.
        /// </summary>
        /// <param name="codeGeneratorOptions">CodeGeneratorOptions object to be validated</param>
        public static void Validate(this CodeGeneratorOptions codeGeneratorOptions)
        {
            codeGeneratorOptions.DeliveryOptions.Validate();

            if (codeGeneratorOptions.ContentManagementApi)
            {
                codeGeneratorOptions.ManagementOptions.ProjectId = codeGeneratorOptions.DeliveryOptions.ProjectId;
                if (codeGeneratorOptions.ManagementOptions == null || codeGeneratorOptions.ManagementOptions.ProjectId == null)
                {
                    throw new Exception($"You have to provide the '{nameof(ManagementOptions.ProjectId)}' to generate type for Content Management SDK. {SeePart}");
                }

                if (string.IsNullOrWhiteSpace(codeGeneratorOptions.ManagementOptions.ApiKey))
                {
                    throw new Exception($"You have to provide the '{nameof(ManagementOptions.ApiKey)}' to generate type for Content Management SDK. {SeePart}");
                }
            }
        }

        /// <summary>
        /// Validates that DeliveryOptions are initialized and performs some extra integrity validations.
        /// </summary>
        /// <param name="deliveryOptions">DeliveryOptions object to be validated</param>
        private static void Validate(this DeliveryOptions deliveryOptions)
        {
            if (deliveryOptions == null)
            {
                throw new Exception($"You have to provide at least the '{nameof(DeliveryOptions.ProjectId)}' argument. See http://bit.ly/k-params for more details on configuration.");
            }
            else
            {
                DeliveryOptionsValidator.Validate(deliveryOptions);
            }
        }
    }
}
