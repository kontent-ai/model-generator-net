namespace CloudModelGenerator
{
    public class CodeGeneratorOptions
    {
        /// <summary>
        /// Kentico Cloud Project ID
        /// </summary>
        public string ProjectId { get; set; }

        /// <summary>
        /// Namespace name of the generated classes
        /// </summary>
        public string Namespace { get; set; }

        /// <summary>
        /// Output directory for the generated files
        /// </summary>
        public string OutputDir { get; set; }

        /// <summary>
        /// Optionally add suffix to the generated files
        /// </summary>
        public string FilenameSuffix { get; set; }

        /// <summary>
        /// Optionally generate partial classes for user customisation
        /// </summary>
        public bool GeneratePartials { get; set; } = false;

        /// <summary>
        /// Indicates whether the CustomTypeProvider class should be generated
        /// </summary>
        public bool WithTypeProvider { get; set; } = true;

        /// <summary>
        /// Indicates whether the classes should be generated with types that represent structured data model
        /// </summary>
        public bool StructuredModel { get; set; } = false;
    }
}
