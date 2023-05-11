using Kontent.Ai.ModelGenerator.Core.Configuration;
using Kontent.Ai.ModelGenerator.Core.Generators;
using Microsoft.Extensions.Options;
using System.Linq;
using System.Threading.Tasks;
using Kontent.Ai.ModelGenerator.Core.Contract;

namespace Kontent.Ai.ModelGenerator.Core;

public abstract class DeliveryCodeGeneratorBase : CodeGeneratorBase
{
    protected readonly IDeliveryElementService DeliveryElementService;

    protected DeliveryCodeGeneratorBase(
        IOptions<CodeGeneratorOptions> options,
        IOutputProvider outputProvider,
        IClassCodeGeneratorFactory classCodeGeneratorFactory,
        IDeliveryElementService deliveryElementService,
        IUserMessageLogger logger)
        : base(options, outputProvider, classCodeGeneratorFactory, logger)
    {
        DeliveryElementService = deliveryElementService;
    }

    public new async Task<int> RunAsync()
    {
        await base.RunAsync();

        if (Options.WithTypeProvider)
        {
            await GenerateTypeProvider();
        }

        return 0;
    }

    private async Task GenerateTypeProvider()
    {
        var classCodeGenerators = await GetClassCodeGenerators();

        if (!classCodeGenerators.Any())
        {
            Logger.Log(NoContentTypeAvailableMessage);
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
}
