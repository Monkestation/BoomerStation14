using Content.Shared.Chemistry.Reagent;
using Content.Shared.Containers.ItemSlots;
using Content.Server.Chemistry.EntitySystems;
using Content.Shared.Chemistry;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Server.Chemistry.Components
{
    /// <summary>
    /// A machine that generates reagents from a fixed list into a solution container, SS13-style.
    /// Dispensing costs energy from an internal buffer that recharges over time while powered.
    /// </summary>
    [RegisterComponent]
    [Access(typeof(ReagentDispenserSystem))]
    public sealed partial class ReagentDispenserComponent : Component
    {
        [DataField]
        public ItemSlot BeakerSlot = new();

        [DataField("clickSound"), ViewVariables(VVAccess.ReadWrite)]
        public SoundSpecifier ClickSound = new SoundPathSpecifier("/Audio/Machines/machine_switch.ogg");

        [ViewVariables(VVAccess.ReadWrite)]
        public ReagentDispenserDispenseAmount DispenseAmount = ReagentDispenserDispenseAmount.U10;

        // Boomer edit start: SS13-style energy-gated dispenser with a fixed reagent list.
        // The energy buffer is a real, swappable power cell in the cell slot. Better cell = bigger
        // buffer and faster recharge, so the machine can be upgraded over the course of the round.
        /// <summary>
        /// The reagents this dispenser can generate. Order does not matter, the UI sorts by name.
        /// </summary>
        [DataField]
        public List<ProtoId<ReagentPrototype>> ReagentIds = new();

        /// <summary>
        /// Fraction of the installed cell's max charge recharged per second from mains while powered.
        /// Scaling off the cell means a bigger cell also recharges faster.
        /// </summary>
        [DataField]
        public float RechargeFractionPerSecond = 0.02f;

        /// <summary>
        /// Energy cost, in joules (cell charge), of dispensing a single unit of reagent.
        /// </summary>
        [DataField]
        public float EnergyPerUnit = 2f;

        /// <summary>
        /// Temperature reagents are dispensed at, in kelvin.
        /// </summary>
        [DataField]
        public float DispensedTemperature = 293.15f;

        /// <summary>
        /// Accumulator so the UI energy bar is refreshed periodically while recharging, without spamming state every tick.
        /// </summary>
        [ViewVariables]
        public float UiUpdateAccumulator;
        // Boomer edit end.
    }
}
