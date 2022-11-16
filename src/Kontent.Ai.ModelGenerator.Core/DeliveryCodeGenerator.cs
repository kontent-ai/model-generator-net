using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kontent.Ai.Delivery.Abstractions;
using Kontent.Ai.ModelGenerator.Core.Common;
using Kontent.Ai.ModelGenerator.Core.Configuration;
using Kontent.Ai.ModelGenerator.Core.Generators;
using Kontent.Ai.ModelGenerator.Core.Generators.Class;
using Kontent.Ai.ModelGenerator.Core.Helpers;
using Microsoft.Extensions.Options;

namespace Kontent.Ai.ModelGenerator.Core;

public class DeliveryCodeGenerator : CodeGeneratorBase
{
    private readonly IDeliveryClient _deliveryClient;

    public DeliveryCodeGenerator(IOptions<CodeGeneratorOptions> options, IOutputProvider outputProvider, IDeliveryClient deliveryClient)
        : base(options, outputProvider)
    {
        if (options.Value.ManagementApi)
        {
            throw new InvalidOperationException("Cannot create Delivery models with Management API options.");
        }

        _deliveryClient = deliveryClient;
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

    protected static new void AddProperty(Property property, ref ClassDefinition classDefinition)
    {
        if (property is not DisplayTimezoneProperty)
        {
            classDefinition.AddPropertyCodenameConstant(property.Codename);
        }

        classDefinition.AddProperty(property);
    }

    internal ClassCodeGenerator GetClassCodeGenerator(IContentType contentType)
    {
        var classDefinition = new ClassDefinition(contentType.System.Codename);

        foreach (var element in contentType.Elements.Values)
        {
            try
            {
                var elementType = DeliveryElementHelper.GetElementType(Options, element.Type);
                var property = Property.FromContentTypeElement(element.Codename, elementType);
                AddProperty(property, ref classDefinition);

                if (elementType == "date_time")
                {
                    var displayTimezoneProperty = DisplayTimezoneProperty.FromContentTypeElement(element.Codename, elementType);
                    AddProperty(displayTimezoneProperty, ref classDefinition);
                }
            }
            catch (Exception e)
            {
                WriteConsoleErrorMessage(e, element.Codename, element.Type, classDefinition.ClassName);
            }
        }

        TryAddSystemProperty(classDefinition);

        var classFilename = GetFileClassName(classDefinition.ClassName);

        return ClassCodeGeneratorFactory.CreateClassCodeGenerator(Options, classDefinition, classFilename);
    }

    private async Task GenerateTypeProvider()
    {
        var classCodeGenerators = await GetClassCodeGenerators();

        if (!classCodeGenerators.Any())
        {
            Console.WriteLine(NoContentTypeAvailableMessage);
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

    private static void TryAddSystemProperty(ClassDefinition classDefinition)
    {
        try
        {
            classDefinition.AddSystemProperty();
        }
        catch (InvalidOperationException)
        {
            Console.WriteLine(
                $"Warning: Can't add 'System' property. It's in collision with existing element in Content Type '{classDefinition.ClassName}'.");
        }
    }
}
