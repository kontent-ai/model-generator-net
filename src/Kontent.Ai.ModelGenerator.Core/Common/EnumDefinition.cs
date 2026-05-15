using System;
using System.Collections.Generic;

namespace Kontent.Ai.ModelGenerator.Core.Common;

/// <summary>
/// A C# enum type to be emitted alongside a content-type record. Used by the Management
/// emission path for multiple-choice elements — each multiple-choice element produces one
/// enum, name-spaced per consuming type to avoid cross-type collisions.
/// </summary>
public sealed class EnumDefinition
{
    /// <summary>
    /// The enum's C# identifier (e.g. <c>ArticleCategory</c>).
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Enum members in declaration order.
    /// </summary>
    public IReadOnlyList<EnumMember> Members { get; }

    public EnumDefinition(string name, IReadOnlyList<EnumMember> members)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(members);

        Name = name;
        Members = members;
    }
}

/// <summary>
/// A single member of an <see cref="EnumDefinition"/>. Each member maps to one option of a
/// multiple-choice element and carries its codename + id metadata via <see cref="Attributes"/>
/// (typically a single <c>[KontentEnumValue(...)]</c>).
/// </summary>
public sealed class EnumMember
{
    /// <summary>
    /// The C# identifier for this member (PascalCased option codename).
    /// </summary>
    public string Identifier { get; }

    /// <summary>
    /// Attributes to emit on this enum member.
    /// </summary>
    public IReadOnlyList<AttributeSpec> Attributes { get; }

    public EnumMember(string identifier, IReadOnlyList<AttributeSpec> attributes)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(identifier);
        ArgumentNullException.ThrowIfNull(attributes);

        Identifier = identifier;
        Attributes = attributes;
    }
}
