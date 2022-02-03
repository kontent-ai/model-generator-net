using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kentico.Kontent.Delivery.Abstractions;
using Kentico.Kontent.Management.Models.Shared;
using Kentico.Kontent.ModelGenerator.Core.Common;
using Kentico.Kontent.ModelGenerator.Core.Configuration;
using Kentico.Kontent.ModelGenerator.Core.Generators;
using Kentico.Kontent.ModelGenerator.Core.Generators.Class;
using Kentico.Kontent.ModelGenerator.Core.Helpers;
using Microsoft.Extensions.Options;

namespace Kentico.Kontent.ModelGenerator.Core
{
    public class DeliveryCodeGenerator : CodeGeneratorBase
    {
        private readonly IDeliveryClient _client;

        public DeliveryCodeGenerator(IOptions<CodeGeneratorOptions> options, IOutputProvider outputProvider, IDeliveryClient deliveryClient)
            : base(options, outputProvider)
        {
            if (options.Value.ManagementApi)
            {
                throw new InvalidOperationException("Cannot create Delivery models with Management API options.");
            }

            _client = deliveryClient;
        }

        public async Task<int> RunAsync()
        {
            await GenerateContentTypeModels();

            if (Options.WithTypeProvider)
            {
                await GenerateTypeProvider();
            }

            if (!string.IsNullOrEmpty(Options.BaseClass))
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

            var typeProviderCodeGenerator = new TypeProviderCodeGenerator(Options.Namespace);

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
                        codeGenerators.Add(GetCustomClassCodeGenerator(contentType));
                    }

                    codeGenerators.Add(GetClassCodeGenerator(contentType));
                }
                catch (InvalidIdentifierException)
                {
                    Console.WriteLine($"Warning: Skipping Content Type '{contentType.System.Codename}'. Can't create valid C# identifier from its name.");
                }
            }

            return codeGenerators;
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

                    classDefinition.AddPropertyCodenameConstant(element.Codename);
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

            return ClassCodeGeneratorFactory.CreateClassCodeGenerator(Options, classDefinition, classFilename);
        }

        internal ClassCodeGenerator GetCustomClassCodeGenerator(IContentType contentType)
        {
            var classDefinition = new ClassDefinition(contentType.System.Codename);
            var classFilename = $"{classDefinition.ClassName}";

            return ClassCodeGeneratorFactory.CreateClassCodeGenerator(Options, classDefinition, classFilename, true);
        }

        internal async Task GenerateBaseClass()
        {
            var classCodeGenerators = await GetClassCodeGenerators();

            if (!classCodeGenerators.Any())
            {
                Console.WriteLine(NoContentTypeAvailableMessage);
                return;
            }

            var baseClassCodeGenerator = new BaseClassCodeGenerator(Options.BaseClass, Options.Namespace);

            foreach (var codeGenerator in classCodeGenerators)
            {
                baseClassCodeGenerator.AddClassNameToExtend(codeGenerator.ClassDefinition.ClassName);
            }

            var baseClassCode = baseClassCodeGenerator.GenerateBaseClassCode();
            WriteToOutputProvider(baseClassCode, Options.BaseClass, false);

            var baseClassExtenderCode = baseClassCodeGenerator.GenereateExtenderCode();
            WriteToOutputProvider(baseClassExtenderCode, baseClassCodeGenerator.ExtenderClassName, true);
        }

        private void TryAddSystemProperty(ClassDefinition classDefinition)
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
}
