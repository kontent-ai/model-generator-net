using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kentico.Kontent.Management;
using Kentico.Kontent.Management.Extensions;
using Kentico.Kontent.Management.Models.Types;
using Kentico.Kontent.Management.Models.Types.Elements;
using Kentico.Kontent.Management.Models.TypeSnippets;
using Kentico.Kontent.ModelGenerator.Core.Common;
using Kentico.Kontent.ModelGenerator.Core.Configuration;
using Kentico.Kontent.ModelGenerator.Core.Generators;
using Kentico.Kontent.ModelGenerator.Core.Generators.Class;
using Kentico.Kontent.ModelGenerator.Core.Helpers;
using Microsoft.Extensions.Options;

namespace Kentico.Kontent.ModelGenerator.Core
{
    public class ExtendedDeliveryCodeGenerator : CodeGeneratorBase
    {
        private readonly IManagementClient _managementClient;

        public ExtendedDeliveryCodeGenerator(IOptions<CodeGeneratorOptions> options, IOutputProvider outputProvider, IManagementClient managementClient)
            : base(options, outputProvider)
        {
            _managementClient = managementClient;
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

                    codeGenerators.Add(GetClassCodeGenerator(contentType, managementSnippets, deliveryTypes));
                }
                catch (InvalidIdentifierException)
                {
                    WriteConsoleErrorMessage(contentType.Codename);
                }
            }

            return codeGenerators;
        }

        internal ClassCodeGenerator GetClassCodeGenerator(ContentTypeModel contentType, List<ContentTypeSnippetModel> managementSnippets, List<ContentTypeModel> contentTypes)
        {
            var classDefinition = new ClassDefinition(contentType.Codename);

            foreach (var element in contentType.Elements)
            {
                try
                {
                    if (element.Type != ElementMetadataType.ContentTypeSnippet)
                    {
                        AddProperty(element, ref classDefinition, contentTypes);
                    }
                    else
                    {
                        var snippetElements = ManagementElementHelper.GetManagementContentTypeSnippetElements(element, managementSnippets);
                        foreach (var snippetElement in snippetElements)
                        {
                            AddProperty(snippetElement, ref classDefinition, contentTypes);
                        }
                    }
                }
                catch (Exception e)
                {
                    WriteConsoleErrorMessage(e, element.Codename, element.Type.ToString(), classDefinition.ClassName);
                }
            }

            var classFilename = GetFileClassName(classDefinition.ClassName);
            return ClassCodeGeneratorFactory.CreateClassCodeGenerator(Options, classDefinition, classFilename);
        }

        private void AddProperty(ElementMetadataBase el, ref ClassDefinition classDefinition, List<ContentTypeModel> contentTypes)
        {
            var elementType = DeliveryElementHelper.GetElementType(Options, el.Type.ToString());
            if (elementType == ElementMetadataType.LinkedItems.ToString())
            {
                elementType = ContentTypeToDeliverTypeNameMapper.Map(el, contentTypes, Options);
            }

            var property = Property.FromContentTypeElement(el, elementType);

            AddProperty(property, ref classDefinition);
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
    }
}
