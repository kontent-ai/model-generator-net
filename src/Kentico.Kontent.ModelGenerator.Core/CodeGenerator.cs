using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kentico.Kontent.Delivery.Abstractions;
using Kentico.Kontent.Management.Models.Types;
using Kentico.Kontent.ModelGenerator.Core.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;

namespace Kentico.Kontent.ModelGenerator.Core
{
    public class CodeGenerator
    {
        private readonly CodeGeneratorOptions _options;
        private readonly IDeliveryClient _client;
        private readonly IManagementClient _managementClient;
        private readonly IOutputProvider _outputProvider;

        public CodeGenerator(IOptions<CodeGeneratorOptions> options, IDeliveryClient deliveryClient, IOutputProvider outputProvider, IManagementClient managementClient)
        {
            _options = options.Value;
            _client = deliveryClient;
            _outputProvider = outputProvider;
            _managementClient = managementClient;
        }

        public async Task<int> RunAsync()
        {
            await GenerateContentTypeModels(_options.StructuredModel);

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

        internal async Task GenerateContentTypeModels(bool structuredModel = false)
        {
            var classCodeGenerators = await GetClassCodeGenerators(structuredModel);

            if (classCodeGenerators.Any())
            {
                foreach (var codeGenerator in classCodeGenerators)
                {
                    _outputProvider.Output(codeGenerator.GenerateCode(), codeGenerator.ClassFilename, codeGenerator.OverwriteExisting);
                }

                Console.WriteLine($"{classCodeGenerators.Count()} content type models were successfully created.");
            }
            else
            {
                Console.WriteLine($@"No content type available for the project ({_options.DeliveryOptions.ProjectId}). Please make sure you have the Delivery API enabled at https://app.kontent.ai/.");
            }
        }

        internal async Task GenerateTypeProvider()
        {
            var classCodeGenerators = await GetClassCodeGenerators();

            if (classCodeGenerators.Any())
            {
                var typeProviderCodeGenerator = new TypeProviderCodeGenerator(_options.Namespace);

                foreach (var codeGenerator in classCodeGenerators)
                {
                    typeProviderCodeGenerator.AddContentType(codeGenerator.ClassDefinition.Codename, codeGenerator.ClassDefinition.ClassName);
                }

                var typeProviderCode = typeProviderCodeGenerator.GenerateCode();
                if (!string.IsNullOrEmpty(typeProviderCode))
                {
                    _outputProvider.Output(typeProviderCode, TypeProviderCodeGenerator.ClassName, true);
                    Console.WriteLine($"{TypeProviderCodeGenerator.ClassName} class was successfully created.");
                }
            }
            else
            {
                Console.WriteLine($@"No content type available for the project ({_options.DeliveryOptions.ProjectId}). Please make sure you have the Delivery API enabled at https://app.kontent.ai/.");
            }
        }

        internal async Task<ICollection<ClassCodeGenerator>> GetClassCodeGenerators(bool structuredModel = false)
        {
            var deliveryTypes = (await _client.GetTypesAsync()).Types;
            var managementTypes = await _managementClient.GetAllContentTypesAsync(_options);

            var codeGenerators = new List<ClassCodeGenerator>();
            if (deliveryTypes != null)
            {
                foreach (var contentType in deliveryTypes)
                {
                    try
                    {
                        if (_options.GeneratePartials)
                        {
                            codeGenerators.Add(GetCustomClassCodeGenerator(contentType));
                        }

                        var managementContentType = _options.ContentManagementApi
                            ? managementTypes.FirstOrDefault(ct => ct["codename"].ToObject<string>() == contentType.System.Codename)
                            : null;

                        codeGenerators.Add(GetClassCodeGenerator(contentType, structuredModel, managementContentType));
                    }
                    catch (InvalidIdentifierException)
                    {
                        Console.WriteLine($"Warning: Skipping Content Type '{contentType.System.Codename}'. Can't create valid C# identifier from its name.");
                    }
                }
            }
            return codeGenerators;
        }

        internal ClassCodeGenerator GetClassCodeGenerator(IContentType contentType, bool structuredModel, JObject managementContentType = null)
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

                    var elementId = _options.ContentManagementApi
                        ? managementContentType.ToObject<ContentTypeModel>().Id.ToString()
                        : null;

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
                catch (ArgumentException)
                {
                    Console.WriteLine($"Warning: Skipping unknown Content Element type '{element.Type}'. (Content Type: '{classDefinition.ClassName}', Element Codename: '{element.Codename}').");
                }
            }

            if (!_options.ContentManagementApi)
            {
                try
                {
                    classDefinition.AddSystemProperty();
                }
                catch (InvalidOperationException)
                {
                    Console.WriteLine($"Warning: Can't add 'System' property. It's in collision with existing element in Content Type '{classDefinition.ClassName}'.");
                }
            }

            string suffix = string.IsNullOrEmpty(_options.FileNameSuffix) ? "" : $".{_options.FileNameSuffix}";
            string classFilename = $"{classDefinition.ClassName}{suffix}";

            return ClassCodeGeneratorFactory.CreateClassCodeGenerator(_options, classDefinition, classFilename);
        }

        internal ClassCodeGenerator GetCustomClassCodeGenerator(IContentType contentType)
        {
            var classDefinition = new ClassDefinition(contentType.System.Codename);
            string classFilename = $"{classDefinition.ClassName}";

            return ClassCodeGeneratorFactory.CreateClassCodeGenerator(_options, classDefinition, classFilename, true);
        }

        internal async Task GenerateBaseClass()
        {
            var classCodeGenerators = await GetClassCodeGenerators();

            if (classCodeGenerators.Any())
            {
                var baseClassCodeGenerator = new BaseClassCodeGenerator(_options.BaseClass, _options.Namespace);

                foreach (var codeGenerator in classCodeGenerators)
                {
                    baseClassCodeGenerator.AddClassNameToExtend(codeGenerator.ClassDefinition.ClassName);
                }

                var baseClassCode = baseClassCodeGenerator.GenerateBaseClassCode();
                if (!string.IsNullOrEmpty(baseClassCode))
                {
                    _outputProvider.Output(baseClassCode, _options.BaseClass, false);
                    Console.WriteLine($"{_options.BaseClass} class was successfully created.");
                }

                var baseClassExtenderCode = baseClassCodeGenerator.GenereateExtenderCode();
                if (!string.IsNullOrEmpty(baseClassExtenderCode))
                {
                    _outputProvider.Output(baseClassExtenderCode, baseClassCodeGenerator.ExtenderClassName, true);
                    Console.WriteLine($"{baseClassCodeGenerator.ExtenderClassName} class was successfully created.");
                }
            }
            else
            {
                Console.WriteLine($@"No content type available for the project ({_options.DeliveryOptions.ProjectId}). Please make sure you have the Delivery API enabled at https://app.kontent.ai/.");
            }
        }
    }
}
