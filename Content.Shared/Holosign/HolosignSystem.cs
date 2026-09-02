using Content.Shared.Charges.Components;
using Content.Shared.Charges.Systems;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Events;
using Content.Shared.Storage;
using Robust.Shared.Network;

namespace Content.Shared.Holosign;

public sealed partial class HolosignSystem : EntitySystem
{
    [Dependency] private SharedChargesSystem _charges = default!;
    [Dependency] private INetManager _net = default!;

    // Boomer edit - wipe every hologram this projector has out (charges come back via OnSignTerminating).
    [SubscribeLocalEvent]
    private void OnUseInHand(Entity<HolosignProjectorComponent> ent, ref UseInHandEvent args)
    {
        if (args.Handled)
            return;

        if (_net.IsServer)
        {
            var query = EntityQueryEnumerator<HolosignSignComponent>();
            while (query.MoveNext(out var uid, out var sign))
            {
                if (sign.Projector == ent.Owner)
                    QueueDel(uid);
            }
        }

        args.Handled = true;
    }

    [SubscribeLocalEvent]
    private void OnBeforeInteract(Entity<HolosignProjectorComponent> ent, ref BeforeRangedInteractEvent args)
    {
        if (args.Handled
            || !args.CanReach // prevent placing out of range
            || HasComp<StorageComponent>(args.Target) // if it's a storage component like a bag, we ignore usage so it can be stored
            )
            return;

        // Boomer edit - clicking one of our own holograms picks it back up and refunds the charge.
        if (TryComp<HolosignSignComponent>(args.Target, out var existingSign) && existingSign.Projector == ent.Owner)
        {
            if (_net.IsServer)
                QueueDel(args.Target.Value); // refund happens in OnSignTerminating
            args.Handled = true;
            return;
        }

        // Boomer edit - need a free hologram slot instead of a power cell charge.
        if (!_charges.TryUseCharge(ent.Owner))
            return;

        if (ent.Comp.PredictedSpawn || _net.IsServer)
        {
            var holosign = PredictedSpawnAtPosition(ent.Comp.SignProto, args.ClickLocation);

            var sign = EnsureComp<HolosignSignComponent>(holosign);
            sign.Projector = ent.Owner;
            Dirty(holosign, sign);
        }

        args.Handled = true;
    }

    [SubscribeLocalEvent]
    private void OnSignTerminating(Entity<HolosignSignComponent> ent, ref EntityTerminatingEvent args)
    {
        // Only the server owns the charge bookkeeping.
        if (!_net.IsServer)
            return;

        var projector = ent.Comp.Projector;
        if (!TryComp<LimitedChargesComponent>(projector, out var charges))
            return;

        _charges.AddCharges((projector.Value, charges, null), 1); // clamped to MaxCharges, so double-refunds are harmless
    }
}
