namespace Kontent.Ai.ModelGenerator.Core.Configuration;

public static class CodeGeneratorOptionsExtensions
{
    public static bool ManagementApi(this CodeGeneratorOptions options) =>
        options.ManagementApi && !options.ExtendedDeliverModels;

    public static bool ExtendedDeliveryModels(this CodeGeneratorOptions options) =>
        !options.ManagementApi && options.ExtendedDeliverModels;

    public static bool DeliveryApi(this CodeGeneratorOptions options) => !options.ManagementApi() && !options.ExtendedDeliveryModels();

    public static DesiredModelsType GetDesiredModelsType(this CodeGeneratorOptions options)
    {
        if (options.ManagementApi())
        {
            return DesiredModelsType.Management;
        }

        return options.ExtendedDeliveryModels() ? DesiredModelsType.ExtendedDelivery : DesiredModelsType.Delivery;
    }
}
