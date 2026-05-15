using System;
using System.Collections.Generic;

namespace Kontent.Ai.ModelGenerator.Core.Common;

/// <summary>
/// Result of mapping one Management API element to emittable artifacts. Most element types
/// produce only a <see cref="Property"/>; multiple-choice additionally produces one
/// <see cref="EnumDefinition"/> in <see cref="Enums"/>.
/// </summary>
public sealed class ManagementElementOutput
{
    public ManagementProperty Property { get; }

    /// <summary>
    /// Sidecar enum definitions to emit alongside the consuming content-type record.
    /// </summary>
    public IReadOnlyList<EnumDefinition> Enums { get; }

    public ManagementElementOutput(ManagementProperty property, IReadOnlyList<EnumDefinition> enums = null)
    {
        ArgumentNullException.ThrowIfNull(property);

        Property = property;
        Enums = enums ?? Array.Empty<EnumDefinition>();
    }
}
