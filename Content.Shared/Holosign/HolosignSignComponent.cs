using Robust.Shared.GameStates;

namespace Content.Shared.Holosign;

// Boomer edit - links a placed holosign back to the projector that made it, so removing/destroying it refunds a charge.
/// <summary>
/// Added to a hologram spawned by a <see cref="HolosignProjectorComponent"/>.
/// When this entity is removed the owning projector gets its charge back.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class HolosignSignComponent : Component
{
    /// <summary>
    /// The projector that placed this hologram.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? Projector;
}
