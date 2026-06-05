using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.FixedPoint;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.Dictionary;

namespace Content.Shared._Monkestation.Body.Components;

/// <summary>
/// This is a bladder, used to store piss.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, AutoGenerateComponentPause]
public sealed partial class MSBladderComponent : Component
{
    /// <summary>
    /// The solution inside of this bladder
    /// </summary>
    [ViewVariables]
    public Entity<SolutionComponent>? Solution;

    /// <summary>
    /// The type of piss to fill with
    /// </summary>
    [DataField(customTypeSerializer:typeof(PrototypeIdDictionarySerializer<FixedPoint2, ReagentPrototype>))]
    public Dictionary<string, FixedPoint2> PissReagents = new();

    /// <summary>
    /// If this bladder should process, set to true when inserted, false when removed
    /// </summary>
    [DataField]
    public bool Enabled;

    [AutoNetworkedField, AutoPausedField]
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan NextTick;

    /// <summary>
    /// Amount of piss to piss per piss
    /// </summary>
    [DataField]
    public FixedPoint2 PissAmount = 10;
}
