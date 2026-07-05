using Content.Shared.FixedPoint;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._Monkestation.Nutrition.Components;

/// <summary>
/// Lets a barefoot entity (slimeperson) passively slurp up puddles and clean dirt
/// off the tiles it walks over. Whatever is slurped is ingested into the stomach so it
/// metabolizes normally (food nourishes, water hydrates, toxins harm). Toggleable
/// through an innate action.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, AutoGenerateComponentPause]
public sealed partial class SlimeFloorAbsorptionComponent : Component
{
    /// <summary>
    /// Whether the passive absorption is currently active.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool Enabled = true;

    /// <summary>
    /// Maximum reagent volume slurped from puddles per tile stepped on.
    /// </summary>
    [DataField]
    public FixedPoint2 AbsorbVolume = FixedPoint2.New(15);

    /// <summary>
    /// Hunger restored for each cleanable dirt decal removed.
    /// </summary>
    [DataField]
    public float NutritionPerDecal = 2f;

    /// <summary>
    /// Flat hunger restored per unit of reagent slurped from puddles, on top of whatever
    /// those reagents do when metabolized. Makes cleaning always nourishing.
    /// </summary>
    [DataField]
    public float NutritionPerVolume = 0.3f;

    /// <summary>
    /// Minimum time between absorptions.
    /// </summary>
    [DataField]
    public TimeSpan Cooldown = TimeSpan.FromSeconds(0.5);

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
    public TimeSpan NextAbsorb;

    /// <summary>
    /// Sound played when something is absorbed.
    /// </summary>
    [DataField]
    public SoundSpecifier? AbsorbSound =
        new SoundPathSpecifier("/Audio/Effects/Fluids/watersplash.ogg");

    /// <summary>
    /// The toggle action prototype granted on map init.
    /// </summary>
    [DataField]
    public EntProtoId ToggleAction = "ActionToggleSlimeFloorAbsorption";

    /// <summary>
    /// The granted toggle action entity, tracked so it can be toggled and removed.
    /// </summary>
    [DataField]
    public EntityUid? ToggleActionEntity;

    /// <summary>
    /// Grid we last absorbed from. Runtime only; used to act once per new tile.
    /// </summary>
    public EntityUid? LastGrid;

    /// <summary>
    /// Tile indices we last absorbed from. Runtime only.
    /// </summary>
    public Vector2i LastTile;
}
