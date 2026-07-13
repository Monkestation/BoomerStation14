using Robust.Shared.Prototypes;

namespace Content.Server._Monkestation.StationEvents.Components;

[RegisterComponent]
public sealed partial class MSFalseAlarmRuleComponent : Component
{
    public EntProtoId? RuleId;
}
