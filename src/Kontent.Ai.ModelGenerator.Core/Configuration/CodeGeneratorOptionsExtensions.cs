﻿namespace Kontent.Ai.ModelGenerator.Core.Configuration;

public static class CodeGeneratorOptionsExtensions
{
    public static bool ManagementApi(this CodeGeneratorOptions options) =>
        options.ManagementApi && !options.ExtendedDeliveryModels;

    public static bool ExtendedDeliveryModels(this CodeGeneratorOptions options) =>
        !options.ManagementApi && options.ExtendedDeliveryModels;

    public static bool DeliveryApi(this CodeGeneratorOptions options) => !options.ManagementApi() && !options.ExtendedDeliveryModels();

    public static DesiredModelsType GetDesiredModelsType(this CodeGeneratorOptions options)
    {
        if (options.ManagementApi())
        {
            return DesiredModelsType.Management;
        }

        return options.ExtendedDeliveryModels() ? DesiredModelsType.ExtendedDelivery : DesiredModelsType.Delivery;
    }

    public static string GetEnvironmentId(this CodeGeneratorOptions options) =>
        options.ManagementApi || options.ExtendedDeliveryModels
            ? options.ManagementOptions.EnvironmentId
            : options.DeliveryOptions.EnvironmentId;

    public static bool IsStructuredModelModularContent(this CodeGeneratorOptions options) =>
        options.StructuredModelFlags.HasFlag(StructuredModelFlags.ModularContent);

    public static bool IsStructuredModelEnabled(this CodeGeneratorOptions options) =>
        options.StructuredModelFlags.HasFlag(StructuredModelFlags.RichText) ||
        options.StructuredModelFlags.HasFlag(StructuredModelFlags.True) ||
        options.StructuredModelFlags.HasFlag(StructuredModelFlags.ModularContent) ||
        options.StructuredModelFlags.HasFlag(StructuredModelFlags.DateTime);

    public static bool IsStructuredModelRichText(this CodeGeneratorOptions options) =>
        options.StructuredModelFlags.HasFlag(StructuredModelFlags.RichText) ||
        options.StructuredModelFlags.HasFlag(StructuredModelFlags.True);
}
