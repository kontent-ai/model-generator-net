using Kontent.Ai.Delivery.Abstractions;

namespace Kontent.Ai.ModelGenerator.Core.Configuration;

/// <summary>
/// Configuration options for generating modern Delivery SDK model classes (records).
/// This is a beta version that only supports the modern Delivery SDK (v19+).
/// </summary>
public class CodeGeneratorOptions
{
    private const bool DefaultWithTypeProvider = true;

    /// <summary>
    /// Delivery Client configuration.
    /// </summary>
    public DeliveryOptions DeliveryOptions { get; set; }

    /// <summary>
    /// Namespace name of the generated classes
    /// </summary>
    public string Namespace { get; set; }

    /// <summary>
    /// Output directory for the generated files
    /// </summary>
    public string OutputDir { get; set; }

    /// <summary>
    /// Indicates whether the CustomTypeProvider class should be generated
    /// </summary>
    public bool WithTypeProvider { get; set; } = DefaultWithTypeProvider;

    /// <summary>
    /// Indicates whether a base class should be created and all output classes should derive from it using a partial class
    /// </summary>
    public string BaseClass { get; set; }
}
