using Content.Shared.Examine;
using Content.Shared.Ghost.Components;

namespace Content.Shared.Warps;

public sealed partial class WarpPointSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<WarpPointComponent, ExaminedEvent>(OnWarpPointExamine);
    }

    private void OnWarpPointExamine(EntityUid uid, WarpPointComponent component, ExaminedEvent args)
    {
        if (!HasComp<GhostComponent>(args.Examiner))
            return;

        // Monkestation start - don't need to add to the examine for mobs
        if (component.Mob)
            return;
        // Monkestation end

        var loc = component.Location == null ? Name(uid) : Loc.GetString(component.Location);
        args.PushText(Loc.GetString("warp-point-component-on-examine-success", ("location", loc)));
    }
}
