using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Kentico.Kontent.Management;
using Kentico.Kontent.Management.Extensions;
using Kentico.Kontent.Management.Models.Types;
using Kentico.Kontent.Management.Models.Types.Elements;
using Kentico.Kontent.Management.Models.TypeSnippets;
using Kentico.Kontent.ModelGenerator.Core.Common;
using Kentico.Kontent.ModelGenerator.Core.Configuration;
using Kentico.Kontent.ModelGenerator.Core.Generators.Class;
using Kentico.Kontent.ModelGenerator.Core.Helpers;
using Microsoft.Extensions.Options;

namespace Kentico.Kontent.ModelGenerator.Core
{
    public class ExtendedDeliveryCodeGenerator : DeliveryCodeGeneratorBase
    {
        private const string TypedSuffixFileName = ".Typed";
        private readonly IManagementClient _managementClient;

        public ExtendedDeliveryCodeGenerator(IOptions<CodeGeneratorOptions> options, IOutputProvider outputProvider, IManagementClient managementClient)
            : base(options, outputProvider)
        {
            if (options.Value.ManagementApi)
            {
                throw new InvalidOperationException("Cannot create Extended Delivery models with Management API enabled.");
            }

            if (!options.Value.ExtendedDeliverModels)
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

            codeGenerators.Add(new ContentItemClassCodeGenerator(Options.Namespace));

            foreach (var contentType in deliveryTypes)
            {
                try
                {
                    if (Options.GeneratePartials)
                    {
                        codeGenerators.Add(GetCustomClassCodeGenerator(contentType.Codename));
                    }

                    codeGenerators.AddRange(GetClassCodeGenerator(contentType, managementSnippets, deliveryTypes));
                }
                catch (InvalidIdentifierException)
                {
                    WriteConsoleErrorMessage(contentType.Codename);
                }
            }

            return codeGenerators;
        }

        internal IEnumerable<ClassCodeGenerator> GetClassCodeGenerator(ContentTypeModel contentType, List<ContentTypeSnippetModel> managementSnippets, List<ContentTypeModel> contentTypes)
        {
            var classDefinition = new ClassDefinition(contentType.Codename);
            var typedClassDefinition = new ClassDefinition(contentType.Codename);

            foreach (var element in contentType.Elements)
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

            yield return new TypedExtendedDeliveryClassCodeGenerator(typedClassDefinition, GetFileClassName(classDefinition.ClassName + TypedSuffixFileName));
            yield return ClassCodeGeneratorFactory.CreateClassCodeGenerator(Options, classDefinition, GetFileClassName(classDefinition.ClassName));
        }

        private void AddProperty(ElementMetadataBase el, ref ClassDefinition classDefinition, ref ClassDefinition typedClassDefinition, List<ContentTypeModel> contentTypes)
        {
            var elementType = DeliveryElementHelper.GetElementType(Options, el.Type.ToString());
            if (elementType == ElementMetadataType.LinkedItems.ToString())
            {
                var typedProperty = ContentTypeToDeliverTypeNameMapper.Map(el, contentTypes, Options);
                if (typedProperty != null)
                {
                    AddProperty(typedProperty, ref typedClassDefinition);
                }

                elementType = $"{nameof(IEnumerable)}<{ContentItemClassCodeGenerator.DefaultContentItemClassName}>";
            }

            var property = Property.FromContentTypeElement(el, elementType!);

            AddProperty(property, ref classDefinition);
        }
    }
}
