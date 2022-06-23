using System;
using System.Linq;
using System.Threading.Tasks;
using Kentico.Kontent.ModelGenerator.Core.Configuration;
using Kentico.Kontent.ModelGenerator.Core.Generators;
using Kentico.Kontent.ModelGenerator.Core.Generators.Class;
using Microsoft.Extensions.Options;

namespace Kentico.Kontent.ModelGenerator.Core
{
    public abstract class DeliveryCodeGeneratorBase : CodeGeneratorBase
    {
        protected DeliveryCodeGeneratorBase(IOptions<CodeGeneratorOptions> options, IOutputProvider outputProvider) : base(options, outputProvider)
        {
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
                var className = codeGenerator is ContentItemClassCodeGenerator ? codeGenerator.ClassDefinition.Codename : codeGenerator.ClassDefinition.ClassName;
                typeProviderCodeGenerator.AddContentType(codeGenerator.ClassDefinition.Codename, className);
            }

            var typeProviderCode = typeProviderCodeGenerator.GenerateCode();
            WriteToOutputProvider(typeProviderCode, TypeProviderCodeGenerator.ClassName, true);
        }
    }
}
