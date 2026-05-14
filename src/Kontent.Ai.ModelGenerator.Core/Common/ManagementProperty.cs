using System;
using System.Collections.Generic;

namespace Kontent.Ai.ModelGenerator.Core.Common;

/// <summary>
/// A <see cref="Property"/> that additionally carries a list of <see cref="AttributeSpec"/>
/// to emit on the generated property. Used by the Management emission path; the Delivery
/// path keeps using plain <see cref="Property"/>.
/// </summary>
public sealed class ManagementProperty : Property
{
    /// <summary>
    /// Attributes to emit on the generated property, in declaration order.
    /// Always includes at least the element-identity attribute (<c>[KontentElement]</c>);
    /// constraint attributes (<c>[StringLength]</c>, <c>[AllowedTypes]</c>, ...) follow.
    /// </summary>
    public IReadOnlyList<AttributeSpec> Attributes { get; }

    public ManagementProperty(
        string codename,
        string typeName,
        string id,
        IReadOnlyList<AttributeSpec> attributes)
        : base(codename, typeName, id)
    {
        ArgumentNullException.ThrowIfNull(attributes);
        Attributes = attributes;
    }
}
