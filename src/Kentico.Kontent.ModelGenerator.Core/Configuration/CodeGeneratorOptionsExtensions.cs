namespace Kentico.Kontent.ModelGenerator.Core.Configuration
{
    public static class CodeGeneratorOptionsExtensions
    {
        public static bool ManagementApi(this CodeGeneratorOptions options) => options.ManagementApi && !options.ExtendedDeliverModels;
    }
}
