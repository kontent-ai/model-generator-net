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

        public ManagementCodeGenerator(IOptions<CodeGeneratorOptions> options, IOutputProvider outputProvider, IManagementClient managementClient)
            : base(options, outputProvider)
        {
            if (!options.Value.ManagementApi)
            {
                throw new InvalidOperationException("Cannot create Management models with Delivery API options.");
            }

            _managementClient = managementClient;
        }

        protected override async Task<ICollection<ClassCodeGenerator>> GetClassCodeGenerators()
        {
            var managementTypesListingResponse = await _managementClient.ListContentTypesAsync().ConfigureAwait(false);
            var managementSnippetsListingResponse = await _managementClient.ListContentTypeSnippetsAsync().ConfigureAwait(false);
            var managementTypes = await GetAllContentModelsAsync(managementTypesListingResponse).ConfigureAwait(false);
            var managementSnippets = await GetAllContentModelsAsync(managementSnippetsListingResponse).ConfigureAwait(false);

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
                        codeGenerators.Add(GetCustomClassCodeGenerator(contentType.Codename));
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

        internal ClassCodeGenerator GetClassCodeGenerator(ContentTypeModel contentType, IEnumerable<ContentTypeSnippetModel> managementSnippets)
        {
            var classDefinition = new ClassDefinition(contentType.Codename);

            foreach (var element in contentType.Elements)
            {
                try
                {
                    if (element.Type != ElementMetadataType.ContentTypeSnippet)
                    {
                        AddProperty(Property.FromContentTypeElement(element), ref classDefinition);
                    }
                    else
                    {
                        var snippetElements = ManagementElementHelper.GetManagementContentTypeSnippetElements(element, managementSnippets);
                        foreach (var snippetElement in snippetElements)
                        {
                            AddProperty(Property.FromContentTypeElement(snippetElement), ref classDefinition);
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

                response = await response.GetNextPage().ConfigureAwait(false);
            }

            return contentModels.ToList();
        }
    }
}
