using Content.Server.Atmos.EntitySystems;
using Content.Shared._Monkestation.Body.Components;
using Content.Shared.Chemistry.Components.SolutionManager;
using Content.Shared.IdentityManagement;
using Content.Shared.Popups;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Player;

namespace Content.Server._Monkestation.Body.Systems;

/// <summary>
/// Adding piss to bladders
/// </summary>
public sealed class ButtSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly AtmosphereSystem _atmos = default!;

    private EntityQuery<TransformComponent> _transformQuery;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        _transformQuery = GetEntityQuery<TransformComponent>();
    }

    /// <summary>
    /// Attempts to cause a butt to fart
    /// </summary>
    /// <param name="farter">The entity farting</param>
    /// <param name="butt">The butt they are farting with</param>
    /// <returns></returns>
    public bool TryFart(EntityUid farter, Entity<MSButtComponent> butt)
    {
        _audio.PlayPvs(butt.Comp.FartSound, farter);
        var transformComponent = _transformQuery.GetComponent(farter);
        var environment = _atmos.GetContainingMixture((farter, transformComponent), false, true);

        if (environment is not null)
            _atmos.Merge(environment, butt.Comp.FartGas);


        var otherFilter = Filter.PvsExcept(farter, entityManager: EntityManager);
        _popupSystem.PopupEntity(Loc.GetString("ms-chat-emote-fart-self"), farter, farter);
        _popupSystem.PopupEntity(
            Loc.GetString("ms-chat-emote-fart-other", ("farter", Identity.Entity(farter, EntityManager))),
            farter,
            otherFilter,
            true);
        return true;
    }
}
