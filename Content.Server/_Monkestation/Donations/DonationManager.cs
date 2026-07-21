using System.Linq;
using Content.Server._Monkestation.Plexora;
using Content.Shared._Monkestation.Donations;
using Robust.Server.Player;
using Robust.Shared.Enums;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Server._Monkestation.Donations;

public sealed partial class DonationManager : ISharedDonationManager, IPostInjectInit
{
    [Dependency] private IPlayerManager _playerManager = default!;
    [Dependency] private PlexoraManager _plexora = default!;
    [Dependency] private IServerNetManager _netMgr = default!;
    [Dependency] private IPrototypeManager _prototype = default!;


    [ViewVariables(VVAccess.ReadOnly)]
    private readonly Dictionary<ICommonSession, HashSet<ProtoId<MSDonorTierPrototype>>> _donors = new();
    [ViewVariables(VVAccess.ReadOnly)]
    private ProtoId<MSDonorTierPrototype>? _fallbackDonor;

    public void Initialize()
    {
        _netMgr.RegisterNetMessage<MsgUpdateDonorStatus>();

        _fallbackDonor = _prototype.EnumeratePrototypes<MSDonorTierPrototype>()
            .FirstOrDefault(proto => proto is
                {
                    FallbackTier: true,
                },
                null)
            ?.ID;
    }

    public void PostInject()
    {
        _playerManager.PlayerStatusChanged += PlayerStatusChanged;
    }

    private void PlayerStatusChanged(object? sender, SessionStatusEventArgs e)
    {
        if (e.NewStatus == SessionStatus.Connected)
        {
            UpdateDonatorStatus(e.Session);
        }
    }

    private async void UpdateDonatorStatus(ICommonSession session)
    {
        var status = await _plexora.GetDonorInfo(session);
        if (status?.DonorTiers == null)
        {
            return;
        }

        var tiers = status.DonorTiers
            .Select<string, ProtoId<MSDonorTierPrototype>?>(tier =>
                _prototype.HasIndex<MSDonorTierPrototype>(tier) ? tier : _fallbackDonor)
            .Where(tier => tier != null)
            .Select(tier => tier!.Value)
            .ToHashSet();
        _donors.Add(session, tiers);

        var msg = new MsgUpdateDonorStatus
        {
            DonorData = tiers,
        };
        _netMgr.ServerSendMessage(msg, session.Channel);
    }

    public DonorData? GetDonorData(EntityUid uid)
    {
        return _playerManager.TryGetSessionByEntity(uid, out var session) ? GetDonorData(session) : null;
    }

    public DonorData? GetDonorData(ICommonSession session)
    {
        return _donors.TryGetValue(session, out var tiers)
            ? new DonorData(tiers.Select(protoId => _prototype.Index(protoId)).ToList())
            : null;
    }
}
