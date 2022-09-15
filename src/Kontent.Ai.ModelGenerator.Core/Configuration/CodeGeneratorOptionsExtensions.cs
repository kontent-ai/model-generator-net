namespace Kontent.Ai.ModelGenerator.Core.Configuration;

public static class CodeGeneratorOptionsExtensions
{
    public static bool ManagementApi(this CodeGeneratorOptions options) =>
        options.ManagementApi && !options.ExtendedDeliverModels && !options.ExtendedDeliverPreviewModels;

    public static bool ExtendedDeliveryModels(this CodeGeneratorOptions options) =>
        !options.ManagementApi && (options.ExtendedDeliverModels || options.ExtendedDeliverPreviewModels);

    public static bool DeliveryApi(this CodeGeneratorOptions options) => !options.ManagementApi() && !options.ExtendedDeliveryModels();

    public static UsedMappingsType GetUsedMappingsType(this CodeGeneratorOptions options)
    {
        if (options.ManagementApi())
        {
            return UsedMappingsType.Management;
        }

        return options.ExtendedDeliveryModels() ? UsedMappingsType.ExtendedDelivery : UsedMappingsType.Delivery;
    }
}
