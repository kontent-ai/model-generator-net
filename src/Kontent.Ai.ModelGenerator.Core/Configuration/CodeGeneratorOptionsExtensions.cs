namespace Kontent.Ai.ModelGenerator.Core.Configuration;

public static class CodeGeneratorOptionsExtensions
{
    public static bool IsStructuredModelEnabled(this CodeGeneratorOptions options) =>
        options.StructuredModelFlags.HasFlag(StructuredModelFlags.RichText) ||
        options.StructuredModelFlags.HasFlag(StructuredModelFlags.True) ||
        options.StructuredModelFlags.HasFlag(StructuredModelFlags.DateTime);

    public static bool IsStructuredModelRichText(this CodeGeneratorOptions options) =>
        options.StructuredModelFlags.HasFlag(StructuredModelFlags.RichText) ||
        options.StructuredModelFlags.HasFlag(StructuredModelFlags.True);
}
