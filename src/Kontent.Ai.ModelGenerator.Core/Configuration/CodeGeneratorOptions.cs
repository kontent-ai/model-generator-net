using System;
using Kontent.Ai.Delivery.Abstractions;

namespace Kontent.Ai.ModelGenerator.Core.Configuration;

/// <summary>
/// Configuration options for generating modern Delivery SDK model classes (records).
/// This is a beta version that only supports the modern Delivery SDK (v19+).
/// </summary>
public class CodeGeneratorOptions
{
    private const bool DefaultWithTypeProvider = false;

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
    [Obsolete("TypeProvider is now generated via source generation in Delivery SDK 19.0.0-rc1+. This option will be removed in a future version.")]
    public bool WithTypeProvider { get; set; } = DefaultWithTypeProvider;

    /// <summary>
    /// Indicates whether a base record should be created and all output records should derive from it using a partial record.
    /// </summary>
    public string BaseRecord { get; set; }

    /// <summary>
    /// Use <see cref="BaseRecord"/> instead.
    /// </summary>
    [Obsolete("Use BaseRecord instead. This property will be removed in a future version.")]
    public string BaseClass
    {
        get => BaseRecord;
        set => BaseRecord = value;
    }
}
