using System;
using Kontent.Ai.ModelGenerator.Core.Common;
using Kontent.Ai.ModelGenerator.Core.Configuration;
using Kontent.Ai.ModelGenerator.Core.Contract;
using Microsoft.Extensions.Options;

namespace Kontent.Ai.ModelGenerator.Core.Services;

public class DeliveryElementService : IDeliveryElementService
{
    protected readonly CodeGeneratorOptions Options;

    public DeliveryElementService(IOptions<CodeGeneratorOptions> options)
    {
        Validate(options.Value);
        Options = options.Value;
    }

    public string GetElementType(string elementType)
    {
        ArgumentNullException.ThrowIfNull(elementType);

        // Modern delivery models are always structured - no suffix needed
        // Extended delivery models have their own type resolution logic
        return elementType;
    }

    private static void Validate(CodeGeneratorOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        // Modern beta version only supports Delivery SDK models
    }
}
