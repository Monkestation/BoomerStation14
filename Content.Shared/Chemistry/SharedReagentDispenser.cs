using Content.Shared.Chemistry.Reagent;
using Content.Shared.FixedPoint;
using Content.Shared.Storage;
using Robust.Shared.Serialization;

namespace Content.Shared.Chemistry
{
    /// <summary>
    /// This class holds constants that are shared between client and server.
    /// </summary>
    public sealed class SharedReagentDispenser
    {
        public const string OutputSlotName = "beakerSlot";
    }

    [Serializable, NetSerializable]
    public sealed class ReagentDispenserSetDispenseAmountMessage : BoundUserInterfaceMessage
    {
        public readonly ReagentDispenserDispenseAmount ReagentDispenserDispenseAmount;

        public ReagentDispenserSetDispenseAmountMessage(ReagentDispenserDispenseAmount amount)
        {
            ReagentDispenserDispenseAmount = amount;
        }

        /// <summary>
        ///     Create a new instance from interpreting a String as an integer,
        ///     throwing an exception if it is unable to parse.
        /// </summary>
        public ReagentDispenserSetDispenseAmountMessage(String s)
        {
            switch (s)
            {
                case "1":
                    ReagentDispenserDispenseAmount = ReagentDispenserDispenseAmount.U1;
                    break;
                case "5":
                    ReagentDispenserDispenseAmount = ReagentDispenserDispenseAmount.U5;
                    break;
                case "10":
                    ReagentDispenserDispenseAmount = ReagentDispenserDispenseAmount.U10;
                    break;
                case "15":
                    ReagentDispenserDispenseAmount = ReagentDispenserDispenseAmount.U15;
                    break;
                case "20":
                    ReagentDispenserDispenseAmount = ReagentDispenserDispenseAmount.U20;
                    break;
                case "30":
                    ReagentDispenserDispenseAmount = ReagentDispenserDispenseAmount.U30;
                    break;
                case "40":
                    ReagentDispenserDispenseAmount = ReagentDispenserDispenseAmount.U40;
                    break;
                case "60":
                    ReagentDispenserDispenseAmount = ReagentDispenserDispenseAmount.U60;
                    break;
                case "120":
                    ReagentDispenserDispenseAmount = ReagentDispenserDispenseAmount.U120;
                    break;
                default:
                    throw new Exception($"Cannot convert the string `{s}` into a valid ReagentDispenser DispenseAmount");
            }
        }
    }

    [Serializable, NetSerializable]
    public sealed class ReagentDispenserDispenseReagentMessage : BoundUserInterfaceMessage
    {
        // Boomer edit: SS13-style dispensers generate reagents from a fixed list, identified by id, instead of draining physical jugs.
        public readonly string ReagentId;

        public ReagentDispenserDispenseReagentMessage(string reagentId)
        {
            ReagentId = reagentId;
        }
    }

    [Serializable, NetSerializable]
    public sealed class ReagentDispenserClearContainerSolutionMessage : BoundUserInterfaceMessage
    {

    }

    public enum ReagentDispenserDispenseAmount
    {
        U1 = 1,
        U5 = 5,
        U10 = 10,
        U15 = 15,
        U20 = 20,
        U30 = 30,
        U40 = 40,
        U60 = 60,
        U120 = 120,
    }

    [Serializable, NetSerializable]
    public sealed class ReagentInventoryItem(string reagentId, string reagentLabel, Color reagentColor)
    {
        // Boomer edit: reagents are a fixed list generated on demand, so an item is just an id/name/color, no physical stock.
        public string ReagentId = reagentId;
        public string ReagentLabel = reagentLabel;
        public Color ReagentColor = reagentColor;
    }

    [Serializable, NetSerializable]
    public sealed class ReagentDispenserBoundUserInterfaceState : BoundUserInterfaceState
    {
        public readonly ContainerInfo? OutputContainer;

        public readonly NetEntity? OutputContainerEntity;

        /// <summary>
        /// A list of the reagents which this dispenser can dispense.
        /// </summary>
        public readonly List<ReagentInventoryItem> Inventory;

        public readonly ReagentDispenserDispenseAmount SelectedDispenseAmount;

        // Boomer edit: SS13-style internal energy buffer that recharges from power.
        public readonly float Energy;
        public readonly float MaxEnergy;
        public readonly float EnergyPerUnit;

        public ReagentDispenserBoundUserInterfaceState(ContainerInfo? outputContainer, NetEntity? outputContainerEntity, List<ReagentInventoryItem> inventory, ReagentDispenserDispenseAmount selectedDispenseAmount, float energy, float maxEnergy, float energyPerUnit)
        {
            OutputContainer = outputContainer;
            OutputContainerEntity = outputContainerEntity;
            Inventory = inventory;
            SelectedDispenseAmount = selectedDispenseAmount;
            Energy = energy;
            MaxEnergy = maxEnergy;
            EnergyPerUnit = energyPerUnit;
        }
    }

    [Serializable, NetSerializable]
    public enum ReagentDispenserUiKey
    {
        Key
    }
}
