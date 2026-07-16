using System.Collections.Immutable;
using System.Linq;
using Content.Server._Monkestation.StationEvents.Components;
using Content.Server.GameTicking;
using Content.Server.StationEvents;
using Content.Server.StationEvents.Components;
using Content.Server.StationEvents.Events;
using Content.Shared._Monkestation.GameTicking.Components;
using Content.Shared.GameTicking.Components;
using Content.Shared.Prototypes;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._Monkestation.StationEvents.Events;

public sealed partial class FalseAlarmRule : StationEventSystem<MSFalseAlarmRuleComponent>
{
    [Dependency] private IPrototypeManager _prototypeManager = default!;
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private MetaDataSystem _metaData = default!;

    private ImmutableList<EntityPrototype>? FakeableEventCache;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MSFalseAlarmRuleComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<PrototypesReloadedEventArgs>(OnPrototypesReloaded);
    }

    private void OnMapInit(Entity<MSFalseAlarmRuleComponent> ent, ref MapInitEvent args)
    {
        if (ent.Comp.RuleId == null)
        {
            var allStationEvents = FakeableEvents();
            ent.Comp.RuleId = _random.Pick(allStationEvents);
        }

        _metaData.SetEntityName(ent, $"False Alarm ({ent.Comp.RuleId})");
    }

    private void OnPrototypesReloaded(PrototypesReloadedEventArgs args)
    {
        if (args.WasModified<EntityPrototype>())
            FakeableEventCache = GetFakeableEvents().ToImmutableList();
    }

    protected override void Added(EntityUid uid, MSFalseAlarmRuleComponent component, GameRuleComponent gameRule, GameRuleAddedEvent args)
    {
        base.Added(uid, component, gameRule, args);
        if (component.RuleId != null)
        {
            AnnounceFakeEvent(component.RuleId.Value);
        }
        else
        {
            Log.Error("Attempted to add fake game rule with null rule id. This shouldn't happen.");
        }
    }

    public void AnnounceFakeEvent(EntProtoId ruleId)
    {
        var ruleEntity = Spawn(ruleId, MapCoordinates.Nullspace);
        var ev = new GameRuleAddedEvent(ruleEntity, ruleId, true);
        RaiseLocalEvent(ruleEntity, ref ev, true);
        Del(ruleEntity);
    }

    public ImmutableList<EntityPrototype> FakeableEvents()
    {
        return FakeableEventCache ?? GetFakeableEvents().ToImmutableList();
    }

    /// <summary>
    /// Gets all valid event prototypes
    /// </summary>
    private IEnumerable<EntityPrototype> GetFakeableEvents()
    {
        foreach (var proto in _prototypeManager.EnumeratePrototypes<EntityPrototype>())
        {
            if (proto.Abstract || proto.HasComponent<MSUnFakeableComponent>())
                continue;

            if (proto.HasComponent<StationEventComponent>())
                yield return proto;
        }
    }
}
