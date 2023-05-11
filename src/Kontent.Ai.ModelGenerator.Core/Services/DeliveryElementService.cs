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
        if (elementType == null)
        {
            throw new ArgumentNullException(nameof(elementType));
        }

        if (!Options.IsStructuredModelEnabled())
        {
            return elementType;
        }

        if (Options.StructuredModelFlags.HasFlag(StructuredModelFlags.DateTime) && Property.IsDateTimeElementType(elementType))
        {
            elementType += Property.StructuredSuffix;
        }
        else if (Options.IsStructuredModelRichText() && Property.IsRichTextElementType(elementType))
        {
            elementType += Property.StructuredSuffix;
        }
        else if (!Options.ExtendedDeliveryModels && Options.IsStructuredModelModularContent() && Property.IsModularContentElementType(elementType))
        {
            elementType += Property.StructuredSuffix;
        }

        return elementType;
    }

    private static void Validate(CodeGeneratorOptions options)
    {
        if (options == null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        if (options.ManagementApi)
        {
            throw new InvalidOperationException("Cannot generate structured model for the Management models.");
        }
    }
}
