using System;
using Kontent.Ai.Delivery.Abstractions;
using Kontent.Ai.ModelGenerator.Core.Configuration;

namespace Kontent.Ai.ModelGenerator.Options;

/// <summary>
/// Extension methods meant for validation.
/// </summary>
public static class ValidationExtensions
{
    /// <summary>
    /// Validates that CodeGeneratorOptions are initialized for Delivery SDK.
    /// </summary>
    /// <param name="codeGeneratorOptions">CodeGeneratorOptions object to be validated</param>
    public static void Validate(this CodeGeneratorOptions codeGeneratorOptions)
    {
        if (codeGeneratorOptions.DeliveryOptions == null)
        {
            throw new Exception($"You have to provide the '{nameof(DeliveryOptions.EnvironmentId)}' argument. See http://bit.ly/k-params for more details on configuration.");
        }

        // Basic validation - DeliveryOptions.Validate requires ValidationContext in SDK v19
        if (string.IsNullOrWhiteSpace(codeGeneratorOptions.DeliveryOptions.EnvironmentId))
        {
            throw new Exception($"You have to provide the '{nameof(DeliveryOptions.EnvironmentId)}' argument. See http://bit.ly/k-params for more details on configuration.");
        }
    }
}
