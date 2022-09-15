using System;
using Kontent.Ai.Delivery.Abstractions;
using Kontent.Ai.Delivery.Configuration;
using Kontent.Ai.Management.Configuration;
using Kontent.Ai.ModelGenerator.Core.Configuration;

namespace Kontent.Ai.ModelGenerator.Options;

/// <summary>
/// Extension methods meant for validation.
/// </summary>
public static class ValidationExtensions
{
    /// <summary>
    /// Validates that CodeGeneratorOptions are initialized and performs some extra integrity validations.
    /// </summary>
    /// <param name="codeGeneratorOptions">CodeGeneratorOptions object to be validated</param>
    public static void Validate(this CodeGeneratorOptions codeGeneratorOptions)
    {
        var desiredModelsType = codeGeneratorOptions.GetDesiredModelsType();
        if (desiredModelsType == DesiredModelsType.Delivery)
        {
            codeGeneratorOptions.DeliveryOptionsValidate();
        }
        else
        {
            codeGeneratorOptions.ManagementOptionsValidate();
        }
    }

    /// <summary>
    /// Validates that ManagementOptions are initialized
    /// </summary>
    /// <param name="codeGeneratorOptions">CodeGeneratorOptions object including ManagementOptions object to be validated</param>
    /// <exception cref="Exception"></exception>
    private static void ManagementOptionsValidate(this CodeGeneratorOptions codeGeneratorOptions)
    {
        if (codeGeneratorOptions.ManagementOptions?.ProjectId == null)
        {
            throw new Exception(ExceptionMessage(codeGeneratorOptions, nameof(ManagementOptions.ProjectId)));
        }

        if (string.IsNullOrWhiteSpace(codeGeneratorOptions.ManagementOptions.ApiKey))
        {
            throw new Exception(ExceptionMessage(codeGeneratorOptions, nameof(ManagementOptions.ApiKey)));
        }
    }

    /// <summary>
    /// Validates that DeliveryOptions are initialized and performs some extra integrity validations.
    /// </summary>
    /// <param name="codeGeneratorOptions">CodeGeneratorOptions object including DeliveryOptions object to be validated</param>
    private static void DeliveryOptionsValidate(this CodeGeneratorOptions codeGeneratorOptions)
    {
        if (codeGeneratorOptions.DeliveryOptions == null)
        {
            throw new Exception(ExceptionMessage(codeGeneratorOptions, nameof(DeliveryOptions.ProjectId)));
        }

        codeGeneratorOptions.DeliveryOptions.Validate();
    }

    private static string ExceptionMessage(CodeGeneratorOptions options, string argName)
    {
        var atLeastPrefix = options.DeliveryApi() ? " at least " : " ";
        var sdkNameInfo = $"to generate type for {(options.ExtendedDeliveryModels() ? "Delivery" : "Management")} SDK";
        var argInfo = options.DeliveryApi() ? "argument" : sdkNameInfo;
        var seePart = SeePart();

        return $"You have to provide{atLeastPrefix}the '{argName}' {argInfo}. {seePart}";

        string SeePart()
        {
            var paramsLink = options.GetDesiredModelsType() switch
            {
                DesiredModelsType.Management => "https://bit.ly/3rSMeDA",
                DesiredModelsType.Delivery => "http://bit.ly/k-params",
                DesiredModelsType.ExtendedDelivery => "https://bit.ly/3rSMeDA",//todo provide proper link to docs
                _ => throw new ArgumentOutOfRangeException()
            };

            return $"See {paramsLink} for more details on configuration.";
        }
    }
}
