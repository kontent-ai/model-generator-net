namespace Kontent.Ai.ModelGenerator.Core.Configuration;

/// <summary>
/// Extension methods for CodeGeneratorOptions.
/// Modern beta version only supports Delivery SDK models.
/// </summary>
public static class CodeGeneratorOptionsExtensions
{
    /// <summary>
    /// Gets the environment ID from the Delivery options, or <c>null</c> if not configured
    /// (e.g. when the generator is running in a non-Delivery mode that doesn't populate
    /// <see cref="CodeGeneratorOptions.DeliveryOptions"/>).
    /// </summary>
    public static string GetEnvironmentId(this CodeGeneratorOptions options) =>
        options.DeliveryOptions?.EnvironmentId;
}
