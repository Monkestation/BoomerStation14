using System.Numerics;
using Content.Server.Decals;
using Content.Shared._Monkestation.Nutrition;
using Content.Shared._Monkestation.Nutrition.Components;
using Content.Shared.Actions;
using Content.Shared.Body;
using Content.Shared.Body.Components;
using Content.Shared.Body.Systems;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Decals;
using Content.Shared.FixedPoint;
using Content.Shared.Fluids.Components;
using Content.Shared.Inventory;
using Content.Shared.Nutrition.EntitySystems;
using Content.Shared.Popups;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map.Components;
using Robust.Shared.Timing;

namespace Content.Server._Monkestation.Nutrition.EntitySystems;

/// <summary>
/// Makes a barefoot slimeperson passively slurp up puddles and clean cleanable dirt off the
/// tiles it walks over. Slurped reagents are funneled into the stomach so they metabolize
/// normally (food nourishes, toxins harm), while cleaned dirt grants a flat bit of hunger.
/// The behaviour can be toggled with an innate action.
/// </summary>
public sealed class SlimeFloorAbsorptionSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solution = default!;
    [Dependency] private readonly BodySystem _body = default!;
    [Dependency] private readonly HungerSystem _hunger = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly DecalSystem _decals = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SlimeFloorAbsorptionComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<SlimeFloorAbsorptionComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<SlimeFloorAbsorptionComponent, MoveEvent>(OnMove);
        SubscribeLocalEvent<SlimeFloorAbsorptionComponent, ToggleSlimeFloorAbsorptionEvent>(OnToggle);
    }

    private void OnMapInit(Entity<SlimeFloorAbsorptionComponent> ent, ref MapInitEvent args)
    {
        _actions.AddAction(ent, ref ent.Comp.ToggleActionEntity, ent.Comp.ToggleAction);
        _actions.SetToggled(ent.Comp.ToggleActionEntity, ent.Comp.Enabled);
    }

    private void OnShutdown(Entity<SlimeFloorAbsorptionComponent> ent, ref ComponentShutdown args)
    {
        _actions.RemoveAction(ent.Owner, ent.Comp.ToggleActionEntity);
    }

    private void OnToggle(Entity<SlimeFloorAbsorptionComponent> ent, ref ToggleSlimeFloorAbsorptionEvent args)
    {
        if (args.Handled)
            return;

        args.Handled = true;
        ent.Comp.Enabled = !ent.Comp.Enabled;
        Dirty(ent);

        _actions.SetToggled(ent.Comp.ToggleActionEntity, ent.Comp.Enabled);
        _popup.PopupEntity(
            Loc.GetString(ent.Comp.Enabled ? "slime-absorption-toggle-on" : "slime-absorption-toggle-off"),
            ent,
            ent);
    }

    private void OnMove(Entity<SlimeFloorAbsorptionComponent> ent, ref MoveEvent args)
    {
        var comp = ent.Comp;
        if (!comp.Enabled)
            return;

        var xform = args.Component;
        if (xform.GridUid is not { } gridUid || !TryComp<MapGridComponent>(gridUid, out var grid))
            return;

        var tile = _map.CoordinatesToTile(gridUid, grid, xform.Coordinates);

        // Only act when we step onto a fresh tile.
        if (comp.LastGrid == gridUid && comp.LastTile == tile)
            return;

        comp.LastGrid = gridUid;
        comp.LastTile = tile;

        if (_timing.CurTime < comp.NextAbsorb)
            return;

        // Has to be barefoot to clean with its feet.
        if (_inventory.TryGetSlotEntity(ent.Owner, "shoes", out _))
            return;

        if (TryAbsorbTile(ent, gridUid, grid, tile))
            comp.NextAbsorb = _timing.CurTime + comp.Cooldown;
    }

    private bool TryAbsorbTile(Entity<SlimeFloorAbsorptionComponent> ent, EntityUid gridUid, MapGridComponent grid, Vector2i tile)
    {
        var comp = ent.Comp;
        var didSomething = false;

        // --- Puddles: ingest into the stomach so reagents metabolize per their own effects. ---
        if (_body.TryGetOrgansWithComponent<StomachComponent>(ent.Owner, out var stomachs))
        {
            var intersecting = _lookup.GetLocalEntitiesIntersecting(gridUid, tile, gridComp: grid);
            foreach (var uid in intersecting)
            {
                if (!TryComp<PuddleComponent>(uid, out var puddle))
                    continue;

                if (!_solution.ResolveSolution(uid, puddle.SolutionName, ref puddle.Solution, out var puddleSolution)
                    || puddleSolution.Volume <= 0)
                    continue;

                foreach (var stomach in stomachs)
                {
                    if (!_solution.ResolveSolution(stomach.Owner, StomachSystem.DefaultSolutionName, ref stomach.Comp.Solution, out var stomachSolution)
                        || stomachSolution.AvailableVolume <= 0)
                        continue;

                    var amount = FixedPoint2.Min(comp.AbsorbVolume, stomachSolution.AvailableVolume);
                    amount = FixedPoint2.Min(amount, puddleSolution.Volume);
                    if (amount <= 0)
                        continue;

                    var slurped = _solution.SplitSolution(puddle.Solution.Value, amount);
                    _solution.TryAddSolution(stomach.Comp.Solution.Value, slurped);

                    // Slurping anything off the floor always nourishes a little.
                    if (comp.NutritionPerVolume > 0)
                        _hunger.ModifyHunger(ent.Owner, slurped.Volume.Float() * comp.NutritionPerVolume);

                    didSomething = true;
                    break;
                }
            }
        }

        // --- Cleanable dirt decals: remove them and gain a flat bit of hunger each. ---
        if (TryComp<DecalGridComponent>(gridUid, out var decalGrid))
        {
            var bounds = _lookup.GetLocalBounds(tile, grid.TileSize).Enlarged(0.5f).Translated(new Vector2(-0.5f, -0.5f));
            foreach (var (index, decal) in _decals.GetDecalsIntersecting(gridUid, bounds, decalGrid))
            {
                if (!decal.Cleanable)
                    continue;

                _decals.RemoveDecal(gridUid, index, decalGrid);
                _hunger.ModifyHunger(ent.Owner, comp.NutritionPerDecal);
                didSomething = true;
            }
        }

        if (didSomething)
            _audio.PlayPvs(comp.AbsorbSound, ent);

        return didSomething;
    }
}
