using Lidgren.Network;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._Monkestation.Donations;

public sealed class MsgUpdateDonorStatus : NetMessage
{
    public override MsgGroups MsgGroup => MsgGroups.Command;

    public HashSet<ProtoId<MSDonorTierPrototype>> DonorData = [];

    public override void ReadFromBuffer(NetIncomingMessage buffer, IRobustSerializer serializer)
    {
        var tierCount = buffer.ReadVariableInt32();
        DonorData.EnsureCapacity(tierCount);

        for (var i = 0; i < tierCount; i++)
        {
            DonorData.Add(buffer.ReadString());
        }
    }

    public override void WriteToBuffer(NetOutgoingMessage buffer, IRobustSerializer serializer)
    {
        buffer.WriteVariableInt32(DonorData.Count);
        foreach (var donor in DonorData)
        {
            buffer.Write(donor);
        }
    }

    public override NetDeliveryMethod DeliveryMethod => NetDeliveryMethod.ReliableOrdered;
}
