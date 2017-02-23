using CommandLine;
using CommandLine.Text;

namespace KenticoCloudDotNetGenerators
{
    public class Options
    {
        [Option(
            shortName: 'p',
            longName: "projectid",
            Required = true,
            HelpText = "Kentico Cloud Project ID.")]
        public string ProjectId { get; set; }

        [Option(
            shortName: 'o',
            longName: "outputDir",
            Required = false,
            HelpText = "Output directory where files will be generated.",
            DefaultValue = ".")]
        public string OutputDir { get; set; }

        [Option(
            shortName: 'n',
            longName: "namespace",
            HelpText = "Namespace name of the generated classes.",
            DefaultValue = ClassCodeGenerator.DEFAULT_NAMESPACE)]
        public string Namespace { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            return HelpText.AutoBuild(this, (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
        }
    }
}
