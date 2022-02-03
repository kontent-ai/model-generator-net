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
    public class ManagementCodeGenerator
    {
        private readonly CodeGeneratorOptions _options;
        private readonly IManagementClient _managementClient;
        private readonly IOutputProvider _outputProvider;

        private string FilenameSuffix => string.IsNullOrEmpty(_options.FileNameSuffix) ? "" : $".{_options.FileNameSuffix}";
        private string NoContentTypeAvailableMessage =>
            $@"No content type available for the project ({_options.DeliveryOptions.ProjectId}). Please make sure you have the Delivery API enabled at https://app.kontent.ai/.";

        public ManagementCodeGenerator(IOptions<CodeGeneratorOptions> options, IManagementClient managementClient, IOutputProvider outputProvider)
        {
            if (!options.Value.ManagementApi)
            {
                throw new InvalidOperationException("Cannot create Management models with Delivery API options.");
            }

            _options = options.Value;
            _outputProvider = outputProvider;
            _managementClient = managementClient;
        }

        public async Task<int> RunAsync()
        {
            await GenerateContentTypeModels();

            if (!string.IsNullOrEmpty(_options.BaseClass))
            {
                await GenerateBaseClass();
            }

            return 0;
        }

        internal async Task GenerateContentTypeModels()
        {
            var classCodeGenerators = await GetClassCodeGenerators();

            if (!classCodeGenerators.Any())
            {
                Console.WriteLine(NoContentTypeAvailableMessage);
                return;
            }

            WriteToOutputProvider(classCodeGenerators);
        }

        internal async Task<ICollection<ClassCodeGenerator>> GetClassCodeGenerators()
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
                    if (_options.GeneratePartials)
                    {
                        codeGenerators.Add(GetCustomClassCodeGenerator(contentType));
                    }

                    codeGenerators.Add(GetClassCodeGenerator(contentType, managementSnippets));
                }
                catch (InvalidIdentifierException)
                {
                    Console.WriteLine($"Warning: Skipping Content Type '{contentType.Codename}'. Can't create valid C# identifier from its name.");
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
                catch (InvalidOperationException)
                {
                    Console.WriteLine($"Warning: Element '{element.Codename}' is already present in Content Type '{classDefinition.ClassName}'.");
                }
                catch (InvalidIdentifierException)
                {
                    Console.WriteLine($"Warning: Can't create valid C# Identifier from '{element.Codename}'. Skipping element.");
                }
                catch (Exception e) when (e is ArgumentNullException or ArgumentException)
                {
                    Console.WriteLine($"Warning: Skipping unknown Content Element type '{element.Type}'. (Content Type: '{classDefinition.ClassName}', Element Codename: '{element.Codename}').");
                }
            }

            TryAddSystemProperty(classDefinition);

            var classFilename = $"{classDefinition.ClassName}{FilenameSuffix}";

            return ClassCodeGeneratorFactory.CreateClassCodeGenerator(_options, classDefinition, classFilename);
        }

        private void AddProperty(ElementMetadataBase element, ref ClassDefinition classDefinition)
        {
            var property = Property.FromContentTypeElement(element);

            classDefinition.AddPropertyCodenameConstant(element.Codename);
            classDefinition.AddProperty(property);
        }

        internal ClassCodeGenerator GetCustomClassCodeGenerator(ContentTypeModel contentType)
        {
            var classDefinition = new ClassDefinition(contentType.Codename);
            var classFilename = $"{classDefinition.ClassName}";

            return ClassCodeGeneratorFactory.CreateClassCodeGenerator(_options, classDefinition, classFilename, true);
        }

        internal async Task GenerateBaseClass()
        {
            var classCodeGenerators = await GetClassCodeGenerators();

            if (!classCodeGenerators.Any())
            {
                Console.WriteLine(NoContentTypeAvailableMessage);
                return;
            }

            var baseClassCodeGenerator = new BaseClassCodeGenerator(_options.BaseClass, _options.Namespace);

            foreach (var codeGenerator in classCodeGenerators)
            {
                baseClassCodeGenerator.AddClassNameToExtend(codeGenerator.ClassDefinition.ClassName);
            }

            var baseClassCode = baseClassCodeGenerator.GenerateBaseClassCode();
            WriteToOutputProvider(baseClassCode, _options.BaseClass, false);

            var baseClassExtenderCode = baseClassCodeGenerator.GenereateExtenderCode();
            WriteToOutputProvider(baseClassExtenderCode, baseClassCodeGenerator.ExtenderClassName, true);
        }

        private void TryAddSystemProperty(ClassDefinition classDefinition)
        {
            if (_options.ManagementApi)
            {
                return;
            }

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

        private void WriteToOutputProvider(string content, string fileName, bool overwriteExisting)
        {
            if (string.IsNullOrEmpty(content))
            {
                return;
            }

            _outputProvider.Output(content, fileName, overwriteExisting);
            Console.WriteLine($"{fileName} class was successfully created.");
        }

        private void WriteToOutputProvider(ICollection<ClassCodeGenerator> classCodeGenerators)
        {
            foreach (var codeGenerator in classCodeGenerators)
            {
                _outputProvider.Output(codeGenerator.GenerateCode(), codeGenerator.ClassFilename,
                    codeGenerator.OverwriteExisting);
            }

            Console.WriteLine($"{classCodeGenerators.Count} content type models were successfully created.");
        }

        private async Task<IEnumerable<T>> GetAllContentModelsAsync<T>(IListingResponseModel<T> response)
        {
            if (!_options.ManagementApi)
            {
                return null;
            }

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

            return contentModels;
        }
    }
}
