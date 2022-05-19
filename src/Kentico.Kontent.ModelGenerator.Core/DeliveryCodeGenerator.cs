using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kentico.Kontent.Delivery.Abstractions;
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
                }
                catch (Exception e)
                {
                    WriteConsoleErrorMessage(e, element.Codename, element.Type, classDefinition.ClassName);
                }
            }

            classDefinition.TryAddSystemProperty();

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
    }
}
