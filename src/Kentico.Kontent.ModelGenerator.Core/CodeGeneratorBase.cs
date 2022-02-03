﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kentico.Kontent.ModelGenerator.Core.Common;
using Kentico.Kontent.ModelGenerator.Core.Configuration;
using Kentico.Kontent.ModelGenerator.Core.Generators.Class;
using Microsoft.Extensions.Options;

namespace Kentico.Kontent.ModelGenerator.Core
{
    public abstract class CodeGeneratorBase
    {
        private string ProjectId => Options.ManagementApi ? Options.ManagementOptions.ProjectId : Options.DeliveryOptions.ProjectId;

        protected readonly CodeGeneratorOptions Options;
        protected readonly IOutputProvider OutputProvider;

        protected string FilenameSuffix => string.IsNullOrEmpty(Options.FileNameSuffix) ? "" : $".{Options.FileNameSuffix}";
        protected string NoContentTypeAvailableMessage =>
            $@"No content type available for the project ({ProjectId}). Please make sure you have the Delivery API enabled at https://app.kontent.ai/.";

        protected CodeGeneratorBase(IOptions<CodeGeneratorOptions> options, IOutputProvider outputProvider)
        {
            Options = options.Value;
            OutputProvider = outputProvider;
        }

        public async Task<int> RunAsync()
        {
            await GenerateContentTypeModels();

            if (!string.IsNullOrEmpty(Options.BaseClass))
            {
                await GenerateBaseClass();
            }

            return 0;
        }

        protected string GetFileClassName(string className) => $"{className}{FilenameSuffix}";

        protected void WriteToOutputProvider(string content, string fileName, bool overwriteExisting)
        {
            if (string.IsNullOrEmpty(content))
            {
                return;
            }

            OutputProvider.Output(content, fileName, overwriteExisting);
            Console.WriteLine($"{fileName} class was successfully created.");
        }

        protected void WriteToOutputProvider(ICollection<ClassCodeGenerator> classCodeGenerators)
        {
            foreach (var codeGenerator in classCodeGenerators)
            {
                OutputProvider.Output(codeGenerator.GenerateCode(), codeGenerator.ClassFilename,
                    codeGenerator.OverwriteExisting);
            }

            Console.WriteLine($"{classCodeGenerators.Count} content type models were successfully created.");
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

        internal abstract Task<ICollection<ClassCodeGenerator>> GetClassCodeGenerators();

        public void WriteConsoleErrorMessage(Exception e, string elementCodename, string elementType, string className)
        {
            switch (e)
            {
                case InvalidOperationException:
                    Console.WriteLine($"Warning: Element '{elementCodename}' is already present in Content Type '{className}'.");
                    break;
                case InvalidIdentifierException:
                    Console.WriteLine($"Warning: Can't create valid C# Identifier from '{elementCodename}'. Skipping element.");
                    break;
                case ArgumentNullException or ArgumentException:
                    Console.WriteLine($"Warning: Skipping unknown Content Element type '{elementType}'. (Content Type: '{className}', Element Codename: '{elementCodename}').");
                    break;
            }
        }

        public void WriteConsoleErrorMessage(string codename)
        {
            Console.WriteLine($"Warning: Skipping Content Type '{codename}'. Can't create valid C# identifier from its name.");
        }
    }
}
