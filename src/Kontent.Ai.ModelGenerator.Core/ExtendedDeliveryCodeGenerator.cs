using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Kontent.Ai.Delivery.Abstractions;
using Kontent.Ai.Management;
using Kontent.Ai.Management.Extensions;
using Kontent.Ai.Management.Models.Types;
using Kontent.Ai.Management.Models.Types.Elements;
using Kontent.Ai.Management.Models.TypeSnippets;
using Kontent.Ai.ModelGenerator.Core.Common;
using Kontent.Ai.ModelGenerator.Core.Configuration;
using Kontent.Ai.ModelGenerator.Core.Contract;
using Kontent.Ai.ModelGenerator.Core.Generators.Class;
using Kontent.Ai.ModelGenerator.Core.Helpers;
using Microsoft.Extensions.Options;

namespace Kontent.Ai.ModelGenerator.Core;

public class ExtendedDeliveryCodeGenerator : DeliveryCodeGeneratorBase
{
    public const string TypedSuffixFileName = ".Typed";
    private readonly IManagementClient _managementClient;

    public ExtendedDeliveryCodeGenerator(
        IOptions<CodeGeneratorOptions> options,
        IOutputProvider outputProvider,
        IManagementClient managementClient,
        IClassCodeGeneratorFactory classCodeGeneratorFactory,
        IDeliveryElementService deliveryElementService,
        IUserMessageLogger logger)
        : base(options, outputProvider, classCodeGeneratorFactory, deliveryElementService, logger)
    {
        if (options.Value.ManagementApi())
        {
            throw new InvalidOperationException("Cannot create Extended Delivery models with Management API enabled.");
        }

        if (!options.Value.ExtendedDeliveryModels())
        {
            throw new InvalidOperationException("Cannot create Extended Delivery models.");
        }

        _managementClient = managementClient;
    }

    protected override async Task<ICollection<ClassCodeGenerator>> GetClassCodeGenerators()
    {
        var deliveryTypes = await _managementClient.ListContentTypesAsync().GetAllAsync();
        var managementSnippets = await _managementClient.ListContentTypeSnippetsAsync().GetAllAsync();

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
                    codeGenerators.Add(GetCustomClassCodeGenerator(contentType.Codename));
                }

                codeGenerators.AddRange(GetClassCodeGenerators(contentType, managementSnippets, deliveryTypes));
            }
            catch (InvalidIdentifierException)
            {
                WriteConsoleErrorMessage(contentType.Codename);
            }
        }

        return codeGenerators;
    }

    internal IEnumerable<ClassCodeGenerator> GetClassCodeGenerators(ContentTypeModel contentType, List<ContentTypeSnippetModel> managementSnippets, List<ContentTypeModel> contentTypes)
    {
        var classDefinition = new ClassDefinition(contentType.Codename);
        var typedClassDefinition = new ClassDefinition(contentType.Codename);

        foreach (var element in contentType.Elements.ExcludeGuidelines())
        {
            try
            {
                if (element.Type != ElementMetadataType.ContentTypeSnippet)
                {
                    AddProperty(element, ref classDefinition, ref typedClassDefinition, contentTypes);
                }
                else
                {
                    var snippetElements = ManagementElementHelper.GetManagementContentTypeSnippetElements(element, managementSnippets);
                    foreach (var snippetElement in snippetElements)
                    {
                        AddProperty(snippetElement, ref classDefinition, ref typedClassDefinition, contentTypes);
                    }
                }
            }
            catch (Exception e)
            {
                WriteConsoleErrorMessage(e, element.Codename, element.Type.ToString(), classDefinition.ClassName);
            }
        }

        return new List<ClassCodeGenerator>
        {
            new TypedExtendedDeliveryClassCodeGenerator(typedClassDefinition, GetFileClassName(classDefinition.ClassName + TypedSuffixFileName)),
            ClassCodeGeneratorFactory.CreateClassCodeGenerator(Options, classDefinition, GetFileClassName(classDefinition.ClassName))
        };
    }

    private void AddProperty(ElementMetadataBase el, ref ClassDefinition classDefinition, ref ClassDefinition typedClassDefinition, List<ContentTypeModel> contentTypes)
    {
        var elementType = DeliveryElementService.GetElementType(el.Type.ToString());
        if (elementType == ElementMetadataType.LinkedItems.ToString() || elementType == ElementMetadataType.Subpages.ToString())
        {
            if (TypedDeliveryPropertyMapper.TryMap(el, contentTypes, Options, out var typedProperty))
            {
                AddProperty(typedProperty, ref typedClassDefinition);
            }

            var linkedObjectType = Options.IsStructuredModelModularContent()
                ? nameof(IContentItem)
                : Property.ObjectType;

            elementType = TextHelpers.GetEnumerableType(linkedObjectType);
        }

        var property = Property.FromContentTypeElement(el, elementType!);

        AddProperty(property, ref classDefinition);
    }
}
