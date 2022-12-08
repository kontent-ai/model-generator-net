using System;
using Kontent.Ai.Delivery.Abstractions;
using Kontent.Ai.Delivery.Configuration;
using Kontent.Ai.Management.Configuration;
using Kontent.Ai.ModelGenerator.Core.Configuration;

namespace Kontent.Ai.ModelGenerator;

/// <summary>
/// Extension methods meant for validation.
/// </summary>
public static class ValidationExtensions
{
    private const string DeliveryParamsLink = "http://bit.ly/k-params";
    private const string ManagementParamsLink = "https://bit.ly/3rSMeDA";
    private static string SeePart(bool managementApi) => $"See {(managementApi ? ManagementParamsLink : DeliveryParamsLink)} for more details on configuration.";

    /// <summary>
    /// Validates that CodeGeneratorOptions are initialized and performs some extra integrity validations.
    /// </summary>
    /// <param name="codeGeneratorOptions">CodeGeneratorOptions object to be validated</param>
    public static void Validate(this CodeGeneratorOptions codeGeneratorOptions)
    {
        if (codeGeneratorOptions.ManagementApi)
        {
            codeGeneratorOptions.ManagementOptionsValidate();
        }
        else
        {
            codeGeneratorOptions.DeliveryOptionsValidate();
        }
    }

    /// <summary>
    /// Validates that ManagementOptions are initialized
    /// </summary>
    /// <param name="codeGeneratorOptions">CodeGeneratorOptions including ManagementOptions object to be validated</param>
    /// <exception cref="Exception"></exception>
    private static void ManagementOptionsValidate(this CodeGeneratorOptions codeGeneratorOptions)
    {
        var seePart = SeePart(true);
        if (codeGeneratorOptions.ManagementOptions?.ProjectId == null)
        {
            throw new Exception($"You have to provide the '{nameof(ManagementOptions.ProjectId)}' to generate type for Management SDK. {seePart}");
        }

        if (string.IsNullOrWhiteSpace(codeGeneratorOptions.ManagementOptions.ApiKey))
        {
            throw new Exception($"You have to provide the '{nameof(ManagementOptions.ApiKey)}' to generate type for Management SDK. {seePart}");
        }

        if (codeGeneratorOptions.ElementReferenceFlags.HasFlag(ElementReferenceType.Error) ||
            codeGeneratorOptions.ElementReferenceFlags.HasFlag(ElementReferenceType.Empty))
        {
            throw new Exception($"You have to provide the '{nameof(CodeGeneratorOptions.ElementReference)}' argument to element references for Management SDK. {seePart}");
        }
    }

    /// <summary>
    /// Validates that DeliveryOptions are initialized and performs some extra integrity validations.
    /// </summary>
    /// <param name="codeGeneratorOptions">CodeGeneratorOptions including DeliveryOptions object to be validated</param>
    private static void DeliveryOptionsValidate(this CodeGeneratorOptions codeGeneratorOptions)
    {
        if (codeGeneratorOptions.DeliveryOptions == null)
        {
            throw new Exception($"You have to provide at least the '{nameof(DeliveryOptions.ProjectId)}' argument. {SeePart(false)}");
        }

        codeGeneratorOptions.DeliveryOptions.Validate();
    }
}
