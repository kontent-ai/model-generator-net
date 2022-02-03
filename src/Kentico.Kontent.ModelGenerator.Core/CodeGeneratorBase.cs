using System;
using System.Collections.Generic;
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
    }
}
