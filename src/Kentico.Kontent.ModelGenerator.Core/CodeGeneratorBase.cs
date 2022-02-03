using Kentico.Kontent.ModelGenerator.Core.Configuration;
using Microsoft.Extensions.Options;

namespace Kentico.Kontent.ModelGenerator.Core
{
    public abstract class CodeGeneratorBase
    {
        private string ProjectId => Options.ManagementApi ? Options.ManagementOptions.ProjectId : Options.DeliveryOptions.ProjectId;

        protected readonly CodeGeneratorOptions Options;
        protected readonly IOutputProvider OutputProvider;

        protected string NoContentTypeAvailableMessage =>
            $@"No content type available for the project ({ProjectId}). Please make sure you have the Delivery API enabled at https://app.kontent.ai/.";
        protected string FilenameSuffix => string.IsNullOrEmpty(Options.FileNameSuffix) ? "" : $".{Options.FileNameSuffix}";

        protected CodeGeneratorBase(IOptions<CodeGeneratorOptions> options, IOutputProvider outputProvider)
        {
            Options = options.Value;
            OutputProvider = outputProvider;
        }
    }
}
