using Robust.Shared.GameStates;

namespace Content.Shared.Warps;

/// <summary>
/// Allows ghosts etc to warp to this entity by name.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class WarpPointComponent : Component
{
    [DataField]
    public LocId? Location;

    // Monkestation addition start
    /// <summary>
    ///     Tags that determine what category this point will go into in the ghost's orbit menu
    /// </summary>
    [DataField]
    public bool Mob;
    [DataField]
    public bool Ghost;
    [DataField]
    public bool Antagonist;
    // Monkestation addition end

    /// <summary>
    /// If true, ghosts warping to this entity will begin following it.
    /// </summary>
    [DataField]
    public bool Follow;
}
