using Content.Shared.Whitelist;
using Robust.Shared.GameStates;

namespace Content.Shared.Warps;

/// <summary>
/// Allows ghosts etc to warp to this entity by name.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class WarpPointComponent : Component
{
    [DataField]
    public string? Location;

    // Monkestation addition start
    /// <summary>
    ///     Tags that determine what category this point will go into in the ghost's orbit menu
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField]
    public bool Mob;
    [ViewVariables(VVAccess.ReadWrite), DataField]
    public bool Ghost;
    [ViewVariables(VVAccess.ReadWrite), DataField]
    public bool Antagonist;
    // Monkestation addition end

    /// <summary>
    /// If true, ghosts warping to this entity will begin following it.
    /// </summary>
    [DataField]
    public bool Follow;

    /// <summary>
    /// What points should be excluded?
    /// Useful where you want things like a ghost to reach only like CentComm
    /// </summary>
    [DataField]
    public EntityWhitelist? Blacklist;
}
