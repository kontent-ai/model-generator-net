namespace Kontent.Ai.ModelGenerator.Core.Common;

/// <summary>
/// Input shape for <see cref="Services.ManagementElementService"/>. One concrete subtype per
/// Management API element type the generator emits — the subtype selects the C# value type the
/// element projects to. The orchestrator adapts MAPI's <c>ElementMetadataBase</c> hierarchy into
/// these records; tests construct them directly without going through MAPI serialization.
/// </summary>
public abstract record ManagementElementInput(string Codename, string Id);

/// <summary>Text element → <c>string?</c>.</summary>
public sealed record TextElementInput(string Codename, string Id)
    : ManagementElementInput(Codename, Id);

/// <summary>Number element → <c>decimal?</c>.</summary>
public sealed record NumberElementInput(string Codename, string Id)
    : ManagementElementInput(Codename, Id);

/// <summary>Date-time element → <c>DateTimeValue?</c>.</summary>
public sealed record DateTimeElementInput(string Codename, string Id)
    : ManagementElementInput(Codename, Id);

/// <summary>
/// Custom element → <c>CustomValue?</c>. The value is an opaque string from the generator's
/// perspective; <c>source_url</c> / <c>json_parameters</c> / <c>allowed_elements</c> are
/// owner-of-custom-element concerns, not emitted on the consuming type.
/// </summary>
public sealed record CustomElementInput(string Codename, string Id)
    : ManagementElementInput(Codename, Id);

/// <summary>URL slug element → <c>UrlSlugValue?</c>.</summary>
public sealed record UrlSlugElementInput(string Codename, string Id)
    : ManagementElementInput(Codename, Id);

/// <summary>
/// Multiple-choice element (single- and multi-select alike) → <c>IEnumerable&lt;TEnum&gt;?</c>; the
/// wire value is always an array of option references.
/// <para>
/// <see cref="EnumTypeName"/> is set by the orchestrator (typically <c>{ContentTypeClassName}{PascalElementCodename}</c>)
/// so the same multiple-choice element on two content types produces two distinct, collision-free enum types.
/// </para>
/// </summary>
public sealed record MultipleChoiceElementInput(
    string Codename,
    string Id,
    string EnumTypeName,
    System.Collections.Generic.IReadOnlyList<MultipleChoiceOptionInput> Options)
    : ManagementElementInput(Codename, Id);

/// <summary>A single option of a multiple-choice element.</summary>
public sealed record MultipleChoiceOptionInput(string Codename, string Id);

/// <summary>Linked items element → <c>IEnumerable&lt;Reference&gt;?</c> (an array of item references, not inlined models).</summary>
public sealed record LinkedItemsElementInput(string Codename, string Id)
    : ManagementElementInput(Codename, Id);

/// <summary>Subpages element (Web Spotlight) → <c>IEnumerable&lt;Reference&gt;?</c>. Same wire shape as linked items; different MAPI element type.</summary>
public sealed record SubpagesElementInput(string Codename, string Id)
    : ManagementElementInput(Codename, Id);

/// <summary>Taxonomy element → <c>IEnumerable&lt;Reference&gt;?</c>.</summary>
public sealed record TaxonomyElementInput(string Codename, string Id)
    : ManagementElementInput(Codename, Id);

/// <summary>Rich text element → <c>RichTextValue?</c>.</summary>
public sealed record RichTextElementInput(string Codename, string Id)
    : ManagementElementInput(Codename, Id);

/// <summary>Asset element → <c>IEnumerable&lt;AssetReference&gt;?</c>.</summary>
public sealed record AssetElementInput(string Codename, string Id)
    : ManagementElementInput(Codename, Id);
