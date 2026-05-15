namespace Kontent.Ai.ModelGenerator.Core.Common;

/// <summary>
/// Input shape for <see cref="Services.ManagementElementService"/>. One concrete subtype
/// per Management API element type the generator emits. The orchestrator (slice 3) adapts
/// MAPI's <c>ElementMetadataBase</c> hierarchy into these records; tests construct them
/// directly without going through MAPI serialization.
/// </summary>
public abstract record ManagementElementInput(string Codename, string Id);

/// <summary>
/// Text element. <paramref name="MaximumCharacters"/> is the character-count constraint
/// (null when not set, or when the MAPI constraint applied to words rather than characters
/// — <c>words</c> is not currently emittable; see plan §3 in management-models-plan.md).
/// <paramref name="Regex"/> is the validation pattern (null when not set or inactive).
/// </summary>
public sealed record TextElementInput(
    string Codename,
    string Id,
    int? MaximumCharacters = null,
    string Regex = null) : ManagementElementInput(Codename, Id);

/// <summary>Number element. Has no consumer-facing constraints beyond identity.</summary>
public sealed record NumberElementInput(string Codename, string Id)
    : ManagementElementInput(Codename, Id);

/// <summary>Date-time element. Has no consumer-facing constraints beyond identity.</summary>
public sealed record DateTimeElementInput(string Codename, string Id)
    : ManagementElementInput(Codename, Id);

/// <summary>
/// Custom element. The value is an opaque string from the generator's perspective;
/// <c>source_url</c> / <c>json_parameters</c> / <c>allowed_elements</c> are owner-of-custom-element
/// concerns, not emitted on the consuming type.
/// </summary>
public sealed record CustomElementInput(string Codename, string Id)
    : ManagementElementInput(Codename, Id);

/// <summary>
/// URL slug element. <paramref name="Regex"/> is the validation pattern (null when not set
/// or inactive). The <c>depends_on</c> source-element reference is deferred per plan.
/// </summary>
public sealed record UrlSlugElementInput(
    string Codename,
    string Id,
    string Regex = null) : ManagementElementInput(Codename, Id);

/// <summary>
/// Multiple-choice element — both single-select (mode=single) and multi-select (mode=multiple)
/// use the same input shape. The wire value is always an array of option references, so the
/// generator always emits <c>IReadOnlyList&lt;TEnum&gt;?</c>; <see cref="IsSingleSelect"/> only
/// controls whether a <c>[MaxElements(1)]</c> attribute is emitted.
/// <para>
/// <see cref="EnumTypeName"/> is set by the orchestrator (typically <c>{ContentTypeClassName}{PascalElementCodename}</c>)
/// so the same multiple-choice element on two content types produces two distinct, collision-free enum types.
/// </para>
/// </summary>
public sealed record MultipleChoiceElementInput(
    string Codename,
    string Id,
    string EnumTypeName,
    bool IsSingleSelect,
    System.Collections.Generic.IReadOnlyList<MultipleChoiceOptionInput> Options)
    : ManagementElementInput(Codename, Id);

/// <summary>
/// A single option of a multiple-choice element.
/// </summary>
public sealed record MultipleChoiceOptionInput(string Codename, string Id);
