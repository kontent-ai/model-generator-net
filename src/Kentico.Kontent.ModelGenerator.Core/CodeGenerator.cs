using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kentico.Kontent.Delivery.Abstractions;
using Kentico.Kontent.Management.Models.Types;
using Kentico.Kontent.Management.Models.TypeSnippets;
using Kentico.Kontent.ModelGenerator.Core.Configuration;
using Microsoft.Extensions.Options;

namespace Kentico.Kontent.ModelGenerator.Core
{
    public class CodeGenerator
    {
        private readonly CodeGeneratorOptions _options;
        private readonly IDeliveryClient _client;
        private readonly IManagementClient _managementClient;
        private readonly IOutputProvider _outputProvider;

        private string FilenameSuffix => string.IsNullOrEmpty(_options.FileNameSuffix) ? "" : $".{_options.FileNameSuffix}";
        private string NoContentTypeAvailableMessage =>
            $@"No content type available for the project ({_options.DeliveryOptions.ProjectId}). Please make sure you have the Delivery API enabled at https://app.kontent.ai/.";

        public CodeGenerator(IOptions<CodeGeneratorOptions> options, IDeliveryClient deliveryClient, IOutputProvider outputProvider, IManagementClient managementClient)
        {
            _options = options.Value;
            _client = deliveryClient;
            _outputProvider = outputProvider;
            _managementClient = managementClient;
        }

        public async Task<int> RunAsync()
        {
            await GenerateContentTypeModels();

            if (!_options.ContentManagementApi && _options.WithTypeProvider)
            {
                await GenerateTypeProvider();
            }

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

        internal async Task GenerateTypeProvider()
        {
            var classCodeGenerators = await GetClassCodeGenerators();

            if (!classCodeGenerators.Any())
            {
                Console.WriteLine(NoContentTypeAvailableMessage);
                return;
            }

            var typeProviderCodeGenerator = new TypeProviderCodeGenerator(_options.Namespace);

            foreach (var codeGenerator in classCodeGenerators)
            {
                typeProviderCodeGenerator.AddContentType(codeGenerator.ClassDefinition.Codename, codeGenerator.ClassDefinition.ClassName);
            }

            var typeProviderCode = typeProviderCodeGenerator.GenerateCode();
            WriteToOutputProvider(typeProviderCode, TypeProviderCodeGenerator.ClassName, true);
        }

        internal async Task<ICollection<ClassCodeGenerator>> GetClassCodeGenerators()
        {
            var deliveryTypes = (await _client.GetTypesAsync()).Types;
            var managementTypes = await _managementClient.GetAllContentTypesAsync(_options);
            var managementSnippets = await _managementClient.GetAllSnippetsAsync(_options);

            var codeGenerators = new List<ClassCodeGenerator>();
            if (deliveryTypes == null)
            {
                return codeGenerators;
            }

            foreach (var contentType in deliveryTypes)
            {
                try
                {
                    if (_options.GeneratePartials)
                    {
                        codeGenerators.Add(GetCustomClassCodeGenerator(contentType));
                    }

                    var managementContentType = _options.ContentManagementApi
                        ? managementTypes.FirstOrDefault(managementType => managementType.Codename == contentType.System.Codename)
                        : null;

                    codeGenerators.Add(GetClassCodeGenerator(contentType, _options.StructuredModel, managementSnippets, managementContentType));
                }
                catch (InvalidIdentifierException)
                {
                    Console.WriteLine($"Warning: Skipping Content Type '{contentType.System.Codename}'. Can't create valid C# identifier from its name.");
                }
            }

            return codeGenerators;
        }

        internal ClassCodeGenerator GetClassCodeGenerator(IContentType contentType, bool structuredModel, ICollection<SnippetModel> managementSnippets, ContentTypeModel managementContentType = null)
        {
            var classDefinition = new ClassDefinition(contentType.System.Codename);

            foreach (var element in contentType.Elements.Values)
            {
                try
                {
                    var elementType = element.Type;
                    if (structuredModel && Property.IsContentTypeSupported(elementType + Property.StructuredSuffix, _options.ContentManagementApi))
                    {
                        elementType += Property.StructuredSuffix;
                    }

                    var elementId = ElementIdHelper.GetElementId(_options.ContentManagementApi, managementSnippets, managementContentType, element);

                    var property = Property.FromContentType(element.Codename, elementType, _options.ContentManagementApi, elementId);
                    classDefinition.AddPropertyCodenameConstant(element);
                    classDefinition.AddProperty(property);
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

        internal ClassCodeGenerator GetCustomClassCodeGenerator(IContentType contentType)
        {
            var classDefinition = new ClassDefinition(contentType.System.Codename);
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
            if (_options.ContentManagementApi)
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
    }
}
