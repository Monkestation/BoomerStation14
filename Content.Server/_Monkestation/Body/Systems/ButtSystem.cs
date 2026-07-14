using Content.Server.Atmos.EntitySystems;
using Content.Shared._Monkestation.Body.Components;
using Content.Shared.Body;
using Content.Shared.Chemistry.Components.SolutionManager;
using Content.Shared.IdentityManagement;
using Content.Shared.Popups;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Player;

namespace Content.Server._Monkestation.Body.Systems;

/// <summary>
/// Adding piss to bladders
/// </summary>
public sealed partial class ButtSystem : EntitySystem
{
    [Dependency] private SharedPopupSystem _popupSystem = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private AtmosphereSystem _atmos = default!;
    [Dependency] private BodySystem _body = default!;

    [Dependency] private EntityQuery<TransformComponent> _transformQuery;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<BodyComponent, TryFartEvent>(_body.RelayEvent);
        SubscribeLocalEvent<MSButtComponent, BodyRelayedEvent<TryFartEvent>>(OnFart);
    }

    private void OnFart(Entity<MSButtComponent> ent, ref BodyRelayedEvent<TryFartEvent> args)
    {
        if (args.Args.Handled)
            return;

        var fartEvent = args.Args;
        fartEvent.Handled = true;
        args.Args = fartEvent;

        _audio.PlayPvs(ent.Comp.FartSound, args.Body);
        var transformComponent = _transformQuery.GetComponent(args.Body);
        var environment = _atmos.GetContainingMixture((args.Body, transformComponent), false, true);

        if (environment is not null)
            _atmos.Merge(environment, ent.Comp.FartGas);


        var otherFilter = Filter.PvsExcept(args.Body, entityManager: EntityManager);
        _popupSystem.PopupEntity(Loc.GetString("ms-chat-emote-fart-self"), args.Body, args.Body);
        _popupSystem.PopupEntity(
            Loc.GetString("ms-chat-emote-fart-other", ("farter", Identity.Entity(args.Body, EntityManager))),
            args.Body,
            otherFilter,
            true);
    }
}

/// <summary>
/// Event triggered on the body when the fart emote is used
/// </summary>
[ByRefEvent]
public record struct TryFartEvent(bool Handled = true);
