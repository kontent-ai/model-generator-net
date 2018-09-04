namespace CloudModelGenerator
{
    public class CodeGeneratorOptions
    {
        public static bool DefaultGeneratePartials = false;
        public static bool DefaultWithTypeProvider = true;
        public static bool DefaultStructuredModel = false;
        public static bool DefaultContentManagementApi = false;

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
        public string FileNameSuffix { get; set; }

        /// <summary>
        /// Optionally generate partial classes for user customization
        /// </summary>
        public bool GeneratePartials { get; set; } = DefaultGeneratePartials;

        /// <summary>
        /// Indicates whether the CustomTypeProvider class should be generated
        /// </summary>
        public bool WithTypeProvider { get; set; } = DefaultWithTypeProvider;

        /// <summary>
        /// Indicates whether the classes should be generated with types that represent structured data model
        /// </summary>
        public bool StructuredModel { get; set; } = DefaultStructuredModel;

        /// <summary>
        /// Indicates whether the classes should be generated for CM API SDK
        /// </summary>
        public bool ContentManagementApi { get; set; } = DefaultContentManagementApi;

        /// <summary>
        /// Indicates whether a base class should be created and all ouput classes should derive from it using a partial class
        /// </summary>
        public string BaseClass { get; set; }

    }
}
