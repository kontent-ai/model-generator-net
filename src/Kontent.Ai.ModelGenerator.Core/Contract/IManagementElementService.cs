using Kontent.Ai.ModelGenerator.Core.Common;

namespace Kontent.Ai.ModelGenerator.Core.Contract;

/// <summary>
/// Maps a Management API element (described by a <see cref="ManagementElementInput"/>) to the
/// <see cref="ManagementProperty"/> the code emitter consumes. Pure function — no IO, no state.
/// </summary>
public interface IManagementElementService
{
    /// <summary>
    /// Build a <see cref="ManagementProperty"/> for the given element input.
    /// </summary>
    ManagementProperty BuildProperty(ManagementElementInput input);
}
