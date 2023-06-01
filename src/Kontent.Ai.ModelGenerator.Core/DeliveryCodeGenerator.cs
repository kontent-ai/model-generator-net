using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Kontent.Ai.Delivery.Abstractions;
using Kontent.Ai.ModelGenerator.Core.Common;
using Kontent.Ai.ModelGenerator.Core.Configuration;
using Kontent.Ai.ModelGenerator.Core.Contract;
using Kontent.Ai.ModelGenerator.Core.Generators.Class;
using Microsoft.Extensions.Options;

namespace Kontent.Ai.ModelGenerator.Core;

public class DeliveryCodeGenerator : DeliveryCodeGeneratorBase
{
    private readonly IDeliveryClient _deliveryClient;

    public DeliveryCodeGenerator(
        IOptions<CodeGeneratorOptions> options,
        IOutputProvider outputProvider,
        IDeliveryClient deliveryClient,
        IClassCodeGeneratorFactory classCodeGeneratorFactory,
        IClassDefinitionFactory classDefinitionFactory,
        IDeliveryElementService deliveryElementService,
        IUserMessageLogger logger)
        : base(options, outputProvider, classCodeGeneratorFactory, classDefinitionFactory, deliveryElementService, logger)
    {
        if (options.Value.ManagementApi)
        {
            throw new InvalidOperationException("Cannot create Delivery models with Management API options.");
        }

        _deliveryClient = deliveryClient;
    }

    protected override async Task<ICollection<ClassCodeGenerator>> GetClassCodeGenerators()
    {
        var deliveryTypes = (await _deliveryClient.GetTypesAsync()).Types;

        var codeGenerators = new List<ClassCodeGenerator>();
        if (deliveryTypes == null)
        {
            return codeGenerators;
        }

        foreach (var contentType in deliveryTypes)
        {
            try
            {
                if (Options.GeneratePartials)
                {
                    codeGenerators.Add(GetCustomClassCodeGenerator(contentType.System.Codename));
                }

                codeGenerators.Add(GetClassCodeGenerator(contentType));
            }
            catch (InvalidIdentifierException)
            {
                WriteConsoleErrorMessage(contentType.System.Codename);
            }
        }

        return codeGenerators;
    }

    internal ClassCodeGenerator GetClassCodeGenerator(IContentType contentType)
    {
        var classDefinition = ClassDefinitionFactory.CreateClassDefinition(contentType.System.Codename);

        foreach (var element in contentType.Elements.Values)
        {
            try
            {
                var elementType = DeliveryElementService.GetElementType(element.Type);
                var property = Property.FromContentTypeElement(element.Codename, elementType);
                AddProperty(property, ref classDefinition);
            }
            catch (Exception e)
            {
                WriteConsoleErrorMessage(e, element.Codename, element.Type, classDefinition.ClassName);
            }
        }

        TryAddSystemProperty(classDefinition);

        var classFilename = GetFileClassName(classDefinition.ClassName);

        return ClassCodeGeneratorFactory.CreateClassCodeGenerator(Options, classDefinition, classFilename, Logger);
    }

    private void TryAddSystemProperty(ClassDefinition classDefinition)
    {
        try
        {
            classDefinition.AddSystemProperty();
        }
        catch (InvalidOperationException)
        {
            Logger.LogWarning(
                $"Can't add 'System' property. It's in collision with existing element in Content Type '{classDefinition.ClassName}'.");
        }
    }
}
