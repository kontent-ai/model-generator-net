using Kontent.Ai.ModelGenerator.Core.Common;

namespace Kontent.Ai.ModelGenerator.Core.Contract;

/// <summary>
/// Maps a Management API element (described by a <see cref="ManagementElementInput"/>) to the
/// artifacts the code emitter consumes — a <see cref="ManagementProperty"/> and, for
/// multiple-choice elements, an associated sidecar <c>EnumDefinition</c>. Pure function —
/// no IO, no state.
/// </summary>
public interface IManagementElementService
{
    /// <summary>
    /// Build a <see cref="ManagementElementOutput"/> (property + any sidecar enums)
    /// for the given element input.
    /// </summary>
    ManagementElementOutput Build(ManagementElementInput input);
}
