using System.Linq;
using System.Numerics;
using Content.Shared._Monkestation.Body.Components;
using Content.Shared._Monkestation.Emoting.Components;
using Content.Shared.Body;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Components.SolutionManager;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Fluids;
using Content.Shared.Hands.Components;
using Content.Shared.IdentityManagement;
using Content.Shared.Physics;
using Content.Shared.Popups;
using Content.Shared.Toilet.Components;
using Robust.Shared.Containers;
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
    [Dependency] private SharedSolutionContainerSystem _solutionContainerSystem = default!;
    [Dependency] private SharedPopupSystem _popupSystem = default!;
    [Dependency] private SharedPhysicsSystem _physics = default!;
    [Dependency] private SharedTransformSystem _transform = default!;
    [Dependency] private SolutionTransferSystem _solutionTransferSystem = default!;
    [Dependency] private SharedPuddleSystem _puddleSystem = default!;

    [Dependency] private EntityQuery<SolutionManagerComponent> _solutionManagerQuery = default!;
    [Dependency] private EntityQuery<TransformComponent> _transformQuery = default!;
    [Dependency] private EntityQuery<ToiletComponent> _toiletQuery = default!;
    [Dependency] private EntityQuery<HandsComponent> _handsQuery = default!;
    [Dependency] private EntityQuery<ContainerManagerComponent> _containerQuery = default!;
    [Dependency] private EntityQuery<RefillableSolutionComponent> _refillableSolutionQuery = default!;

    private const string DefaultSolutionName = "bladder";
    private static readonly TimeSpan PissCooldown = TimeSpan.FromSeconds(75);

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MSBladderComponent, OrganGotInsertedEvent>(HandleInsertion);
        SubscribeLocalEvent<MSBladderComponent, OrganGotRemovedEvent>(HandleRemoval);
        SubscribeLocalEvent<MSBladderComponent, MapInitEvent>(OnBladderMapInit);
    }

    private void OnBladderMapInit(Entity<MSBladderComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.NextTick = _timing.CurTime + PissCooldown;
        Dirty(ent);
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

    public override void Update(float frameTime)
    {
        base.Update(frameTime);


        var bladderQuery = EntityQueryEnumerator<MSBladderComponent>();
        while (bladderQuery.MoveNext(out var entity, out var bladder))
        {
            if (!bladder.Enabled)
            {
                continue;
            }

            if (bladder.NextTick > _timing.CurTime)
            {
                continue;
            }

            bladder.NextTick = _timing.CurTime + PissCooldown;
            Dirty(entity, bladder);
            SolutionManagerComponent? solutionContainer = null;
            if (!_solutionManagerQuery.Resolve(entity, ref solutionContainer))
            {
                continue;
            }

            var solution = new Solution();
            foreach (var product in bladder.PissReagents)
            {
                solution.AddReagent(product.Key, product.Value);
            }

            TryTransferSolution(entity, solution, bladder, solutionContainer);
        }
    }

    private bool CanTransferSolution(
        EntityUid uid,
        Solution solution,
        MSBladderComponent? bladder = null,
        SolutionManagerComponent? solutions = null)
    {
        return Resolve(uid, ref bladder, ref solutions, logMissing: false)
               && _solutionContainerSystem.ResolveSolution((uid, solutions),
                   DefaultSolutionName,
                   ref bladder.Solution,
                   out var bladderSolution)
               // TODO: For now no partial transfers. Potentially change by design
               && bladderSolution.CanAddSolution(solution);
    }

    private bool TryTransferSolution(
        EntityUid uid,
        Solution solution,
        MSBladderComponent? bladder = null,
        SolutionManagerComponent? solutions = null)
    {
        if (!Resolve(uid, ref bladder, ref solutions, logMissing: false)
            || !_solutionContainerSystem.ResolveSolution((uid, solutions), DefaultSolutionName, ref bladder.Solution)
            || !CanTransferSolution(uid, solution, bladder, solutions))
        {
            return false;
        }

        _solutionContainerSystem.TryAddSolution(bladder.Solution.Value, solution);
        _solutionContainerSystem.UpdateChemicals(bladder.Solution.Value);

        return true;
    }

    /// <summary>
    /// Attempts to cause a bladder to piss
    /// </summary>
    /// <param name="ent">The mob containing the bladder</param>
    /// <param name="entity">The bladder entity</param>
    /// <param name="bladder">The bladder component</param>
    /// <returns>If the piss attempt was handled</returns>
    public bool TryPiss(Entity<MSPissEmoteComponent> ent, EntityUid entity, MSBladderComponent bladder)
    {
        SolutionManagerComponent? solutions = null;
        if (!_solutionManagerQuery.Resolve(entity, ref solutions))
        {
            return false;
        }

        if (!Resolve(entity, ref solutions, logMissing: false)
            || !_solutionContainerSystem.ResolveSolution((entity, solutions),
                DefaultSolutionName,
                ref bladder.Solution))
        {
            return false;
        }

        if (bladder.Solution?.Comp.Solution.Volume < bladder.PissAmount)
        {
            _popupSystem.PopupEntity(Loc.GetString("ms-chat-emote-piss-empty"), ent, ent);
            return true;
        }

        var user = _transformQuery.Get(ent);
        var userPos = _transform.ToMapCoordinates(user.Comp.Coordinates);
        var userRotation = _transform.GetWorldRotation(user.Comp);
        var dir = userRotation.RotateVec(new Vector2(0, -1));
        var ray = new CollisionRay(userPos.Position, dir, (int)CollisionGroup.MobMask);
        var results = _physics.IntersectRay(user.Comp.MapID, ray, 1, returnOnFirstHit: true);
        var toilet = results.FirstOrNull(result => _toiletQuery.HasComp(result.HitEntity));
        var otherFilter = Filter.PvsExcept(ent, entityManager: EntityManager);
        if (toilet.HasValue)
        {
            _popupSystem.PopupClient(Loc.GetString("ms-chat-emote-piss-target-self",
                    ("target", Identity.Entity(toilet.Value.HitEntity, EntityManager, ent))),
                ent,
                ent);
            _popupSystem.PopupEntity(Loc.GetString("ms-chat-emote-piss-target-other",
                [
                    ("pisser", Identity.Entity(ent, EntityManager)),
                    ("target", Identity.Entity(toilet.Value.HitEntity, EntityManager))
                ]),
                ent,
                otherFilter,
                true);
            _solutionContainerSystem.SplitSolution(bladder.Solution!.Value, bladder.PissAmount);
            return true;
        }

        Entity<SolutionComponent>? targetSolution = default!;
        if (_handsQuery.TryGetComponent(ent, out var handsComponent)
            && handsComponent.ActiveHandId != null
            && _containerQuery.TryGetComponent(ent, out var containers)
            && containers.Containers.TryGetValue(handsComponent.ActiveHandId, out var heldItemContainer)
            && heldItemContainer is ContainerSlot { ContainedEntity: not null } containerSlot
            && _refillableSolutionQuery.TryGetComponent(containerSlot.ContainedEntity,
                out var refillableSolutionComponent)
            && _solutionManagerQuery.TryGetComponent(containerSlot.ContainedEntity, out var solutionManagerComponent)
            && _solutionContainerSystem.ResolveSolution((containerSlot.ContainedEntity.Value, solutionManagerComponent),
                refillableSolutionComponent.Solution,
                ref targetSolution)
            && _solutionTransferSystem.Transfer(new SolutionTransferData(ent,
                ent,
                bladder.Solution!.Value,
                containerSlot.ContainedEntity.Value,
                targetSolution.Value,
                bladder.PissAmount)) > 0)
        {
            _popupSystem.PopupClient(Loc.GetString("ms-chat-emote-piss-target-self",
                    ("target", Identity.Entity(containerSlot.ContainedEntity.Value, EntityManager, ent))),
                ent,
                ent);
            _popupSystem.PopupEntity(Loc.GetString("ms-chat-emote-piss-target-other",
                [
                    ("pisser", Identity.Entity(ent, EntityManager)),
                    ("target", Identity.Entity(containerSlot.ContainedEntity.Value, EntityManager))
                ]),
                ent,
                otherFilter,
                true);
            return true;
        }

        var pissedSolution = _solutionContainerSystem.SplitSolution(bladder.Solution!.Value, bladder.PissAmount);
        _puddleSystem.TrySpillAt(_transform.ToCoordinates(user!, userPos.Offset(dir)), pissedSolution, out _, false);
        _popupSystem.PopupClient(Loc.GetString("ms-chat-emote-piss-floor-self"), ent);
        _popupSystem.PopupEntity(
            Loc.GetString("ms-chat-emote-piss-floor-other", ("pisser", Identity.Entity(ent, EntityManager))),
            ent,
            otherFilter,
            true);

        return true;
    }
}
