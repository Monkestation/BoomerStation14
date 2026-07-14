using System.Linq;
using Content.Shared._Monkestation.Donations;
using Robust.Client.Player;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Client._Monkestation.Donations;

public sealed partial class ClientDonationManager : ISharedDonationManager
{
    [Dependency] private IClientNetManager _netMgr = default!;
    [Dependency] private IPlayerManager _player = default!;
    [Dependency] private IPrototypeManager _prototype = default!;

    [ViewVariables(VVAccess.ReadOnly)]
    private HashSet<ProtoId<MSDonorTierPrototype>> _donorTiers = [];

    public void Initialize()
    {
        _netMgr.RegisterNetMessage<MsgUpdateDonorStatus>(UpdateMessageRx);
    }

    private void UpdateMessageRx(MsgUpdateDonorStatus message)
    {
        _donorTiers = message.DonorData;
    }

    public DonorData? GetDonorData(EntityUid uid)
    {
        return uid == _player.LocalEntity ? GetHydratedDonorData() : null;
    }

    public DonorData? GetDonorData(ICommonSession session)
    {
        return session.UserId == _player.LocalUser ? GetHydratedDonorData() : null;
    }

    private DonorData? GetHydratedDonorData()
    {
        return _donorTiers.Count != 0
            ? new DonorData(_donorTiers.Select(tier => _prototype.Index(tier)).ToList())
            : null;
    }
}
