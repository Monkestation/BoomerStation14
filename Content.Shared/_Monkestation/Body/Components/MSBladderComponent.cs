using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.FixedPoint;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._Monkestation.Body.Components;

/// <summary>
/// This is a bladder, used to store piss.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, AutoGenerateComponentPause]
public sealed partial class MSBladderComponent : Component
{
    /// <summary>
    /// If enabled slowly fill with PissSolution. Enabled when inserted disabled when removed.
    /// </summary>
    [DataField]
    public bool Enabled;

    /// <summary>
    /// The solution inside of this bladder
    /// </summary>
    [ViewVariables]
    public Entity<SolutionComponent>? Solution;

    /// <summary>
    /// The name of the solution inside of this bladder
    /// </summary>
    [DataField]
    public string SolutionName = "bladder";

    /// <summary>
    /// The solution to fill the bladder with
    /// </summary>
    [DataField]
    public Solution? PissSolution;

    [AutoNetworkedField, AutoPausedField]
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan NextTick;

    /// <summary>
    /// How frequently the bladder is filled
    /// </summary>
    [DataField]
    public TimeSpan Frequency = TimeSpan.FromSeconds(1);

    /// <summary>
    /// Amount of piss to piss per piss
    /// </summary>
    [DataField]
    public FixedPoint2 PissAmount = 10;
}
