using Content.Shared.Atmos;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared._Monkestation.Body.Components;

/// <summary>
/// This is a bladder, used to store piss.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class MSButtComponent : Component
{
    /// <summary>
    /// The gas to fart
    /// </summary>
    [DataField]
    public GasMixture FartGas { get; set; } = new();

    /// <summary>
    /// The sound to play when farting
    /// </summary>
    [DataField]
    public SoundSpecifier? FartSound;

    /// <summary>
    /// The percent chance to lose your rear each fart
    /// </summary>
    [DataField]
    public int FartInstability;
}
