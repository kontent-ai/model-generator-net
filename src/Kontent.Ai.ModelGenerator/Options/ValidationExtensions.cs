using System;
using Kontent.Ai.Delivery.Abstractions;
using Kontent.Ai.Management.Configuration;
using Kontent.Ai.ModelGenerator.Core.Configuration;

namespace Kontent.Ai.ModelGenerator.Options;

/// <summary>
/// Extension methods meant for validation.
/// </summary>
public static class ValidationExtensions
{
    /// <summary>
    /// Validates that <see cref="CodeGeneratorOptions"/> are initialized for the Delivery SDK.
    /// </summary>
    public static void Validate(this CodeGeneratorOptions codeGeneratorOptions)
    {
        if (codeGeneratorOptions.DeliveryOptions == null)
        {
            throw new Exception($"You have to provide the '{nameof(DeliveryOptions.EnvironmentId)}' argument. See http://bit.ly/k-params for more details on configuration.");
        }

        if (string.IsNullOrWhiteSpace(codeGeneratorOptions.DeliveryOptions.EnvironmentId))
        {
            throw new Exception($"You have to provide the '{nameof(DeliveryOptions.EnvironmentId)}' argument. See http://bit.ly/k-params for more details on configuration.");
        }
    }

    /// <summary>
    /// Validates that <see cref="CodeGeneratorOptions"/> are initialized for the Management SDK.
    /// Both <c>EnvironmentId</c> and <c>ApiKey</c> are required.
    /// </summary>
    public static void ValidateManagement(this CodeGeneratorOptions codeGeneratorOptions)
    {
        if (codeGeneratorOptions.ManagementOptions == null
            || string.IsNullOrWhiteSpace(codeGeneratorOptions.ManagementOptions.EnvironmentId))
        {
            throw new Exception(
                $"You have to provide the '{nameof(ManagementOptions.EnvironmentId)}' argument when using management mode.");
        }

        if (string.IsNullOrWhiteSpace(codeGeneratorOptions.ManagementOptions.ApiKey))
        {
            throw new Exception(
                $"You have to provide the '{nameof(ManagementOptions.ApiKey)}' (or '-k') argument when using management mode.");
        }
    }
}
