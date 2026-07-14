using System.Numerics;
using Content.Shared._Monkestation.Body.Components;
using Content.Shared.Body;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.FixedPoint;
using Content.Shared.Fluids;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.IdentityManagement;
using Content.Shared.Physics;
using Content.Shared.Popups;
using Content.Shared.Toilet.Components;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Player;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Shared._Monkestation.Body.Systems;

/// <summary>
/// Adding piss to bladders
/// </summary>
public sealed partial class BladderSystem : EntitySystem
{
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private SharedSolutionContainerSystem _solutionContainer = default!;
    [Dependency] private SharedPopupSystem _popupSystem = default!;
    [Dependency] private SharedPhysicsSystem _physics = default!;
    [Dependency] private SharedTransformSystem _transform = default!;
    [Dependency] private SolutionTransferSystem _solutionTransferSystem = default!;
    [Dependency] private SharedPuddleSystem _puddleSystem = default!;
    [Dependency] private BodySystem _body = default!;
    [Dependency] private SharedHandsSystem _hands = default!;

    [Dependency] private EntityQuery<TransformComponent> _transformQuery;
    [Dependency] private EntityQuery<ToiletComponent> _toiletQuery;
    [Dependency] private EntityQuery<HandsComponent> _handsQuery;

    private const string DefaultSolutionName = "bladder";

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MSBladderComponent, OrganGotInsertedEvent>(HandleInsertion);
        SubscribeLocalEvent<MSBladderComponent, OrganGotRemovedEvent>(HandleRemoval);
        SubscribeLocalEvent<BodyComponent, TryPissEvent>(_body.RelayEvent);
        SubscribeLocalEvent<MSBladderComponent, BodyRelayedEvent<TryPissEvent>>(OnPiss);
    }

    private void HandleRemoval(EntityUid uid, MSBladderComponent component, OrganGotRemovedEvent args)
    {
        component.Enabled = false;
        Dirty(uid, component);
    }

    private void HandleInsertion(EntityUid uid, MSBladderComponent component, OrganGotInsertedEvent args)
    {
        component.Enabled = true;
        Dirty(uid, component);
    }

    // Copied the solution regeneration code here because the bladder contents are not the solution in the solution component
    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        // TODO: SolutionRegenerationComponent on Solution Entities!
        var query = EntityQueryEnumerator<MSBladderComponent>();
        while (query.MoveNext(out var uid, out var bladder))
        {
            if (!bladder.Enabled || bladder.PissSolution == null || _timing.CurTime < bladder.NextTick)
                continue;

            // timer ignores if its full, it's just a fixed cycle
            bladder.NextTick += bladder.Frequency;
            // Needs to be networked and dirtied so that the client can reroll it during prediction
            Dirty(uid, bladder);
            if (!_solutionContainer.ResolveSolution(uid, bladder.SolutionName, ref bladder.Solution, out var solution))
                continue;

            var amount = FixedPoint2.Min(solution.AvailableVolume, bladder.PissSolution.Volume);
            if (amount <= FixedPoint2.Zero)
                continue;

            // Don't bother cloning and splitting if adding the whole thing
            var generated = amount == bladder.PissSolution.Volume
                ? bladder.PissSolution
                : bladder.PissSolution.Clone().SplitSolution(amount);

            _solutionContainer.TryAddSolution(bladder.Solution.Value, generated);
        }
    }

    private void OnPiss(Entity<MSBladderComponent> ent, ref BodyRelayedEvent<TryPissEvent> args)
    {
        if (
            args.Args.Handled
            || !_solutionContainer.ResolveSolution(ent.Owner,
                DefaultSolutionName,
                ref ent.Comp.Solution,
                out var bladderSolution)
            || bladderSolution.Volume < ent.Comp.PissAmount)
        {
            return;
        }

        // Setting to handled because we know we can piss, now we just need to find out where
        var pissEvent = args.Args;
        pissEvent.Handled = true;
        args.Args = pissEvent;

        var user = _transformQuery.Get(args.Body);
        var userPos = _transform.ToMapCoordinates(user.Comp.Coordinates);
        var userRotation = _transform.GetWorldRotation(user.Comp);
        // (0, -1) is looking directly up, then rotated from there
        var dir = userRotation.RotateVec(new Vector2(0, -1));
        var ray = new CollisionRay(userPos.Position, dir, (int)CollisionGroup.MobMask);
        var results = _physics.IntersectRay(user.Comp.MapID, ray, 1, returnOnFirstHit: true);
        var toilet = results.FirstOrNull(result => _toiletQuery.HasComp(result.HitEntity));
        var otherFilter = Filter.PvsExcept(args.Body, entityManager: EntityManager);
        if (toilet.HasValue)
        {
            _popupSystem.PopupEntity(Loc.GetString("ms-chat-emote-piss-target-self",
                    ("target", Identity.Entity(toilet.Value.HitEntity, EntityManager, args.Body))),
                args.Body,
                args.Body);
            _popupSystem.PopupEntity(Loc.GetString("ms-chat-emote-piss-target-other",
                [
                    ("pisser", Identity.Entity(args.Body, EntityManager)),
                    ("target", Identity.Entity(toilet.Value.HitEntity, EntityManager))
                ]),
                args.Body,
                otherFilter,
                true);
            // Return value is ignored to just delete the reagents (toilets don't currently support actually having fluids in them)
            _solutionContainer.SplitSolution(ent.Comp.Solution!.Value, ent.Comp.PissAmount);
            return;
        }

        if (_handsQuery.TryGetComponent(args.Body, out var handsComponent)
            && _hands.TryGetActiveItem((args.Body, handsComponent), out var heldItem)
            && _solutionContainer.TryGetRefillableSolution(heldItem.Value, out var targetSolution, out _)
            && _solutionTransferSystem.Transfer(new SolutionTransferData(args.Body,
                ent,
                ent.Comp.Solution!.Value,
                heldItem.Value,
                targetSolution.Value,
                ent.Comp.PissAmount)) > 0)
        {
            _popupSystem.PopupEntity(Loc.GetString("ms-chat-emote-piss-target-self",
                    ("target", Identity.Entity(heldItem.Value, EntityManager, ent))),
                args.Body,
                args.Body);
            _popupSystem.PopupEntity(Loc.GetString("ms-chat-emote-piss-target-other",
                [
                    ("pisser", Identity.Entity(args.Body, EntityManager)),
                    ("target", Identity.Entity(heldItem.Value, EntityManager))
                ]),
                args.Body,
                otherFilter,
                true);
            return;
        }

        var pissedSolution = _solutionContainer.SplitSolution(ent.Comp.Solution!.Value, ent.Comp.PissAmount);
        _puddleSystem.TrySpillAt(_transform.ToCoordinates(user!, userPos.Offset(dir)), pissedSolution, out _, false);
        _popupSystem.PopupEntity(Loc.GetString("ms-chat-emote-piss-floor-self"), args.Body);
        _popupSystem.PopupEntity(
            Loc.GetString("ms-chat-emote-piss-floor-other", ("pisser", Identity.Entity(args.Body, EntityManager))),
            args.Body,
            otherFilter,
            true);
    }
}

/// <summary>
/// Event triggered on the body when the piss emote is used
/// </summary>
[ByRefEvent]
public record struct TryPissEvent(bool Handled = true);
