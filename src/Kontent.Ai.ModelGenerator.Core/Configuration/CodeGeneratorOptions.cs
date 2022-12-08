using System;
using System.Linq;
using Kontent.Ai.Delivery.Abstractions;
using Kontent.Ai.Management.Configuration;

namespace Kontent.Ai.ModelGenerator.Core.Configuration;

public class CodeGeneratorOptions
{
    private const bool DefaultGeneratePartials = true;
    private const bool DefaultWithTypeProvider = true;
    private const bool DefaultStructuredModel = false;
    private const bool DefaultManagementApi = false;
    private const string DefaultFileNameSuffix = "Generated";
    private const char ElementReferenceSeparator = ',';
    private static readonly string DefaultElementReference = $"{ElementReferenceType.Codename}{ElementReferenceSeparator}{ElementReferenceType.Id}";

    /// <summary>
    /// Delivery Client configuration.
    /// </summary>
    public DeliveryOptions DeliveryOptions { get; set; }

    /// <summary>
    /// Management Client configuration.
    /// </summary>
    public ManagementOptions ManagementOptions { get; set; }

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
    public string FileNameSuffix { get; set; } = DefaultFileNameSuffix;

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
    public bool ManagementApi { get; set; } = DefaultManagementApi;

    /// <summary>
    /// Indicates whether a base class should be created and all output classes should derive from it using a partial class
    /// </summary>
    public string BaseClass { get; set; }

    /// <summary>
    /// Indicates whether the classes should be generated with desired element reference
    /// </summary>
    public string ElementReference { private get; set; } = DefaultElementReference;

    /// <summary>
    /// Indicates selected element references
    /// </summary>
    public ElementReferenceType ElementReferenceFlags
    {
        get
        {
            if (string.IsNullOrWhiteSpace(ElementReference))
            {
                return ElementReferenceType.Empty;
            }

            var splitElementReferences = ElementReference.Split(ElementReferenceSeparator);

            return splitElementReferences.Any()
                ? splitElementReferences
                    .Select(elementReference =>
                        Enum.TryParse<ElementReferenceType>(elementReference, true, out var parsed)
                            ? parsed
                            : ElementReferenceType.Error)
                    .Aggregate((result, next) => result | next)
                : ElementReferenceType.Empty;
        }
    }
}
