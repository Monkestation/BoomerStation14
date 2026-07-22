using Content.Server.Chemistry.Components;
using Content.Server.Power.Components;
using Content.Shared.Chemistry;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.FixedPoint;
using Content.Shared.Popups;
using Content.Shared.Power.Components;
using Content.Shared.Power.EntitySystems;
using Content.Shared.PowerCell;
using JetBrains.Annotations;
using Robust.Server.Audio;
using Robust.Server.GameObjects;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Server.Chemistry.EntitySystems
{
    /// <summary>
    /// Contains all the server-side logic for reagent dispensers.
    /// <seealso cref="ReagentDispenserComponent"/>
    /// </summary>
    [UsedImplicitly]
    public sealed partial class ReagentDispenserSystem : EntitySystem
    {
        [Dependency] private readonly AudioSystem _audioSystem = default!;
        [Dependency] private readonly SharedSolutionContainerSystem _solutionContainerSystem = default!;
        [Dependency] private readonly ItemSlotsSystem _itemSlotsSystem = default!;
        [Dependency] private readonly UserInterfaceSystem _userInterfaceSystem = default!;
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
        [Dependency] private readonly SharedPopupSystem _popup = default!;
        [Dependency] private readonly PowerCellSystem _powerCell = default!;
        [Dependency] private readonly SharedBatterySystem _battery = default!;

        /// <summary>How often (seconds) the UI energy bar is refreshed while recharging.</summary>
        private const float UiUpdateInterval = 1f;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<ReagentDispenserComponent, ComponentStartup>(SubscribeUpdateUiState);
            SubscribeLocalEvent<ReagentDispenserComponent, SolutionChangedEvent>(SubscribeUpdateUiState);
            SubscribeLocalEvent<ReagentDispenserComponent, BoundUIOpenedEvent>(SubscribeUpdateUiState);

            SubscribeLocalEvent<ReagentDispenserComponent, ReagentDispenserSetDispenseAmountMessage>(OnSetDispenseAmountMessage);
            SubscribeLocalEvent<ReagentDispenserComponent, ReagentDispenserDispenseReagentMessage>(OnDispenseReagentMessage);
            SubscribeLocalEvent<ReagentDispenserComponent, ReagentDispenserClearContainerSolutionMessage>(OnClearContainerSolutionMessage);

            SubscribeLocalEvent<ReagentDispenserComponent, MapInitEvent>(OnMapInit, before: new[] { typeof(ItemSlotsSystem) });
        }

        // Boomer edit: recharge the installed power cell from mains over time while powered, SS13-style.
        public override void Update(float frameTime)
        {
            base.Update(frameTime);

            var query = EntityQueryEnumerator<ReagentDispenserComponent, ApcPowerReceiverComponent>();
            while (query.MoveNext(out var uid, out var comp, out var power))
            {
                // No mains power, or no cell installed: nothing to recharge.
                if (!power.Powered || !_powerCell.TryGetBatteryFromSlot(uid, out var battery))
                    continue;

                var bat = battery.Value;
                if (_battery.IsFull(bat.Owner))
                    continue;

                // Recharge rate scales with cell size, so a better cell also fills faster.
                var newCharge = _battery.GetCharge(bat.Owner)
                    + bat.Comp.MaxCharge * comp.RechargeFractionPerSecond * frameTime;
                _battery.SetCharge(bat.Owner, newCharge);

                comp.UiUpdateAccumulator += frameTime;
                if (comp.UiUpdateAccumulator < UiUpdateInterval)
                    continue;

                comp.UiUpdateAccumulator = 0f;
                if (_userInterfaceSystem.IsUiOpen(uid, ReagentDispenserUiKey.Key))
                    UpdateUiState((uid, comp));
            }
        }

        private void SubscribeUpdateUiState<T>(Entity<ReagentDispenserComponent> ent, ref T ev)
        {
            UpdateUiState(ent);
        }

        private void UpdateUiState(Entity<ReagentDispenserComponent> reagentDispenser)
        {
            var outputContainer = _itemSlotsSystem.GetItemOrNull(reagentDispenser, SharedReagentDispenser.OutputSlotName);
            var outputContainerInfo = BuildOutputContainerInfo(outputContainer);

            var inventory = GetInventory(reagentDispenser);

            var comp = reagentDispenser.Comp;

            // Energy readout comes from the installed cell; no cell means an empty gauge.
            var energy = 0f;
            var maxEnergy = 0f;
            if (_powerCell.TryGetBatteryFromSlot(reagentDispenser.Owner, out var battery))
            {
                energy = _battery.GetCharge(battery.Value.Owner);
                maxEnergy = battery.Value.Comp.MaxCharge;
            }

            var state = new ReagentDispenserBoundUserInterfaceState(outputContainerInfo, GetNetEntity(outputContainer), inventory,
                comp.DispenseAmount, energy, maxEnergy, comp.EnergyPerUnit);
            _userInterfaceSystem.SetUiState(reagentDispenser.Owner, ReagentDispenserUiKey.Key, state);
        }

        private ContainerInfo? BuildOutputContainerInfo(EntityUid? container)
        {
            if (container is not { Valid: true })
                return null;

            if (_solutionContainerSystem.TryGetFitsInDispenser(container.Value, out _, out var solution))
            {
                return new ContainerInfo(Name(container.Value), solution.Volume, solution.MaxVolume)
                {
                    Reagents = solution.Contents
                };
            }

            return null;
        }

        // Boomer edit: inventory is the fixed reagent list, not physical jugs.
        private List<ReagentInventoryItem> GetInventory(Entity<ReagentDispenserComponent> reagentDispenser)
        {
            var inventory = new List<ReagentInventoryItem>();

            foreach (var reagentId in reagentDispenser.Comp.ReagentIds)
            {
                if (!_prototypeManager.TryIndex(reagentId, out ReagentPrototype? proto))
                    continue;

                inventory.Add(new ReagentInventoryItem(reagentId, proto.LocalizedName, proto.SubstanceColor));
            }

            return inventory;
        }

        private void OnSetDispenseAmountMessage(Entity<ReagentDispenserComponent> reagentDispenser, ref ReagentDispenserSetDispenseAmountMessage message)
        {
            reagentDispenser.Comp.DispenseAmount = message.ReagentDispenserDispenseAmount;
            UpdateUiState(reagentDispenser);
            ClickSound(reagentDispenser);
        }

        // Boomer edit: generate the reagent from nothing, paying energy from the internal buffer.
        private void OnDispenseReagentMessage(Entity<ReagentDispenserComponent> reagentDispenser, ref ReagentDispenserDispenseReagentMessage message)
        {
            // Ensure the machine is powered before it can dispense.
            if (!TryComp<ApcPowerReceiverComponent>(reagentDispenser, out var power) || !power.Powered)
                return;

            // Ensure that the reagent is something this reagent dispenser can dispense.
            if (!reagentDispenser.Comp.ReagentIds.Contains(message.ReagentId))
                return;

            var outputContainer = _itemSlotsSystem.GetItemOrNull(reagentDispenser, SharedReagentDispenser.OutputSlotName);
            if (outputContainer is not { Valid: true } || !_solutionContainerSystem.TryGetFitsInDispenser(outputContainer.Value, out var soln, out var solution))
                return;

            var toDispense = FixedPoint2.Min((int)reagentDispenser.Comp.DispenseAmount, solution.AvailableVolume);
            if (toDispense <= 0)
            {
                _popup.PopupEntity(Loc.GetString("reagent-dispenser-window-container-full-text"), reagentDispenser, message.Actor);
                return;
            }

            // Pay the energy cost from the installed cell. TryUseCharge handles the "no cell" and
            // "not enough charge" popups for us.
            var energyCost = toDispense.Float() * reagentDispenser.Comp.EnergyPerUnit;
            if (!_powerCell.TryUseCharge(reagentDispenser.Owner, energyCost, message.Actor))
                return;

            _solutionContainerSystem.TryAddReagent(soln.Value, message.ReagentId, toDispense, out _, reagentDispenser.Comp.DispensedTemperature);

            UpdateUiState(reagentDispenser);
            ClickSound(reagentDispenser);
        }

        private void OnClearContainerSolutionMessage(Entity<ReagentDispenserComponent> reagentDispenser, ref ReagentDispenserClearContainerSolutionMessage message)
        {
            var outputContainer = _itemSlotsSystem.GetItemOrNull(reagentDispenser, SharedReagentDispenser.OutputSlotName);
            if (outputContainer is not { Valid: true } || !_solutionContainerSystem.TryGetFitsInDispenser(outputContainer.Value, out var solution, out _))
                return;

            _solutionContainerSystem.RemoveAllSolution(solution.Value);
            UpdateUiState(reagentDispenser);
            ClickSound(reagentDispenser);
        }

        private void ClickSound(Entity<ReagentDispenserComponent> reagentDispenser)
        {
            _audioSystem.PlayPvs(reagentDispenser.Comp.ClickSound, reagentDispenser, AudioParams.Default.WithVolume(-2f));
        }

        /// <summary>
        /// Initializes the beaker slot
        /// </summary>
        private void OnMapInit(Entity<ReagentDispenserComponent> ent, ref MapInitEvent args)
        {
            _itemSlotsSystem.AddItemSlot(ent.Owner, SharedReagentDispenser.OutputSlotName, ent.Comp.BeakerSlot);
        }
    }
}
