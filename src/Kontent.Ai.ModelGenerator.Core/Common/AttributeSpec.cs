using System;
using System.Collections.Generic;

namespace Kontent.Ai.ModelGenerator.Core.Common;

/// <summary>
/// Describes a single C# attribute to apply to a generated property.
/// Held as a data structure (not a Roslyn syntax node) so element-mapping logic
/// stays decoupled from emission.
/// </summary>
public sealed class AttributeSpec
{
    /// <summary>
    /// Attribute name without the <c>[]</c> brackets and without a trailing <c>Attribute</c> suffix
    /// (e.g. <c>"KontentElement"</c>, <c>"StringLength"</c>).
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Ordered list of arguments to pass to the attribute. Positional arguments come first;
    /// named arguments may follow.
    /// </summary>
    public IReadOnlyList<AttributeArg> Arguments { get; }

    public AttributeSpec(string name, IReadOnlyList<AttributeArg> arguments = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Name = name;
        Arguments = arguments ?? Array.Empty<AttributeArg>();
    }
}

/// <summary>
/// A single argument to an <see cref="AttributeSpec"/>.
/// </summary>
public sealed class AttributeArg
{
    /// <summary>
    /// Argument name, or <c>null</c> for a positional argument.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The literal value the emitter will render. Supported runtime types: <c>string</c>,
    /// <c>int</c>, <c>long</c>, <c>bool</c>, <c>double</c>, <c>string[]</c>. Other types are rendered
    /// via their <see cref="object.ToString"/> as raw C# code (e.g. <c>"AssetFileType.Image"</c>).
    /// </summary>
    public object Value { get; }

    public AttributeArg(object value, string name = null)
    {
        Value = value;
        Name = name;
    }

    public static AttributeArg Positional(object value) => new(value);

    public static AttributeArg Named(string name, object value) => new(value, name);

    /// <summary>
    /// Positional argument rendered as a raw C# expression (member access, enum value, etc.)
    /// rather than as a string literal. Use for things like <c>AssetFileType.Adjustable</c>.
    /// </summary>
    public static AttributeArg PositionalRawCode(string expression) =>
        new(new RawCodeAttributeValue(expression));

    /// <summary>
    /// Named argument rendered as a raw C# expression. See <see cref="PositionalRawCode"/>.
    /// </summary>
    public static AttributeArg NamedRawCode(string name, string expression) =>
        new(new RawCodeAttributeValue(expression), name);
}

/// <summary>
/// Marker wrapping a raw C# expression string. The emitter recognizes this type and parses
/// the wrapped string as an expression instead of rendering it as a literal — used for things
/// like enum member access (<c>AssetFileType.Adjustable</c>) where a literal would be wrong.
/// </summary>
public sealed record RawCodeAttributeValue(string Expression);
