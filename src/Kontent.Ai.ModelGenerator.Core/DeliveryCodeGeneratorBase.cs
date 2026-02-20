using Kontent.Ai.ModelGenerator.Core.Configuration;
using Kontent.Ai.ModelGenerator.Core.Generators;
using Microsoft.Extensions.Options;
using System.Linq;
using System.Threading.Tasks;
using Kontent.Ai.ModelGenerator.Core.Contract;

namespace Kontent.Ai.ModelGenerator.Core;

public abstract class DeliveryCodeGeneratorBase(
    IOptions<CodeGeneratorOptions> options,
    IOutputProvider outputProvider,
    IClassCodeGeneratorFactory classCodeGeneratorFactory,
    IClassDefinitionFactory classDefinitionFactory,
    IDeliveryElementService deliveryElementService,
    IUserMessageLogger logger) : CodeGeneratorBase(options, outputProvider, classCodeGeneratorFactory, classDefinitionFactory, logger)
{
    protected readonly IDeliveryElementService DeliveryElementService = deliveryElementService;

    public new async Task<int> RunAsync()
    {
        await base.RunAsync();

#pragma warning disable CS0618 // Type or member is obsolete
        if (Options.WithTypeProvider)
        {
            Logger.LogWarning("WithTypeProvider is obsolete. TypeProvider is now generated via source generation in Delivery SDK 19.0.0-rc1+. Consider setting WithTypeProvider to false.");
            await GenerateTypeProvider();
        }
#pragma warning restore CS0618

        return 0;
    }

#pragma warning disable CS0618 // Type or member is obsolete
    private async Task GenerateTypeProvider()
    {
        var classCodeGenerators = await GetClassCodeGenerators();

        if (classCodeGenerators.Count == 0)
        {
            return;
        }

        var typeProviderCodeGenerator = new TypeProviderCodeGenerator(Options.Namespace);

        foreach (var codeGenerator in classCodeGenerators)
        {
            typeProviderCodeGenerator.AddContentType(codeGenerator.ClassDefinition.Codename, codeGenerator.ClassDefinition.ClassName);
        }

        var typeProviderCode = typeProviderCodeGenerator.GenerateCode();
        WriteToOutputProvider(typeProviderCode, TypeProviderCodeGenerator.ClassName, true);
    }
#pragma warning restore CS0618
}
