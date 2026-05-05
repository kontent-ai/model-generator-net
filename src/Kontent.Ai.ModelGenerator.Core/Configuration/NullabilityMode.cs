namespace Kontent.Ai.ModelGenerator.Core.Configuration;

/// <summary>
/// Controls how nullability is expressed on generated element properties.
/// </summary>
public enum NullabilityMode
{
    /// <summary>
    /// All element properties are nullable. Safe for projection (<c>WithElements</c> /
    /// <c>WithoutElements</c>): any element omitted from a projected response surfaces as
    /// <c>null</c>, which the type system makes explicit.
    /// </summary>
    Strict,

    /// <summary>
    /// Element properties match the runtime semantics of the Delivery API: elements that
    /// always come back populated (text, rich text, collections) are non-nullable with
    /// sensible defaults; elements that can be genuinely unset (number, date_time, custom)
    /// remain nullable. Not recommended when using projection — projected-away elements
    /// become indistinguishable from "fetched and empty".
    /// </summary>
    Semantic
}
