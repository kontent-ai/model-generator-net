using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kentico.Kontent.Management;
using Kentico.Kontent.Management.Models.Shared;
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
    public class ManagementCodeGenerator : CodeGeneratorBase
    {
        private readonly IManagementClient _managementClient;

        public ManagementCodeGenerator(IOptions<CodeGeneratorOptions> options, IManagementClient managementClient, IOutputProvider outputProvider)
            : base(options, outputProvider)
        {
            if (!options.Value.ManagementApi)
            {
                throw new InvalidOperationException("Cannot create Management models with Delivery API options.");
            }

            _managementClient = managementClient;
        }

        internal override async Task<ICollection<ClassCodeGenerator>> GetClassCodeGenerators()
        {
            var managementTypes = await GetAllContentModelsAsync(await _managementClient.ListContentTypesAsync());
            var managementSnippets = await GetAllContentModelsAsync(await _managementClient.ListContentTypeSnippetsAsync());

            var codeGenerators = new List<ClassCodeGenerator>();
            if (managementTypes == null || !managementTypes.Any())
            {
                return codeGenerators;
            }

            foreach (var contentType in managementTypes)
            {
                try
                {
                    if (Options.GeneratePartials)
                    {
                        codeGenerators.Add(GetCustomClassCodeGenerator(contentType));
                    }

                    codeGenerators.Add(GetClassCodeGenerator(contentType, managementSnippets));
                }
                catch (InvalidIdentifierException)
                {
                    WriteConsoleErrorMessage(contentType.Codename);
                }
            }

            return codeGenerators;
        }

        internal ClassCodeGenerator GetClassCodeGenerator(ContentTypeModel contentType, IEnumerable<ContentTypeSnippetModel> managementSnippets = null)
        {
            var classDefinition = new ClassDefinition(contentType.Codename);

            foreach (var element in contentType.Elements)
            {
                try
                {
                    var snippetElements = ManagementElementHelper.GetManagementContentTypeSnippetElements(element, managementSnippets);
                    if (snippetElements == null)
                    {
                        AddProperty(element, ref classDefinition);
                    }
                    else
                    {
                        foreach (var snippetElement in snippetElements)
                        {
                            AddProperty(snippetElement, ref classDefinition);
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

        internal ClassCodeGenerator GetCustomClassCodeGenerator(ContentTypeModel contentType)
        {
            var classDefinition = new ClassDefinition(contentType.Codename);
            var classFilename = $"{classDefinition.ClassName}";

            return ClassCodeGeneratorFactory.CreateClassCodeGenerator(Options, classDefinition, classFilename, true);
        }

        private static void AddProperty(ElementMetadataBase element, ref ClassDefinition classDefinition)
        {
            var property = Property.FromContentTypeElement(element);

            classDefinition.AddPropertyCodenameConstant(element.Codename);
            classDefinition.AddProperty(property);
        }

        private static async Task<IEnumerable<T>> GetAllContentModelsAsync<T>(IListingResponseModel<T> response)
        {
            var contentModels = new List<T>();
            while (true)
            {
                foreach (var model in response)
                {
                    contentModels.Add(model);
                }

                if (!response.HasNextPage())
                {
                    break;
                }

                response = await response.GetNextPage();
            }

            return contentModels.ToList();
        }
    }
}
