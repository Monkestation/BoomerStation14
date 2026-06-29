using System.Threading.Tasks;
using Content.Server._Monkestation.Administration.Managers;
using Content.Server.Administration;
using Content.Server.Administration.Managers;
using Content.Server.Chat.Managers;
using Content.Server.EUI;
using Content.Shared._Monkestation.Administration;
using Content.Shared._Monkestation.Players;
using Content.Shared.Administration;
using Content.Shared.Eui;
using Robust.Shared.Network;

namespace Content.Server._Monkestation.Administration;

public sealed partial class RoleTimeExemptionPanelEui : BaseEui
{
    [Dependency] private ILogManager _log = default!;
    [Dependency] private IAdminManager _admins = default!;
    [Dependency] private IChatManager _chat = default!;
    [Dependency] private IPlayerLocator _playerLocator = default!;
    [Dependency] private RoleTimeExemptionManager _exemptions = default!;

    private readonly ISawmill _sawmill;

    private string PlayerName { get; set; } = string.Empty;
    private NetUserId? PlayerId { get; set; }
    private RoleTimeExemptionsData? _roleTimeExemptionsData = null;

    public RoleTimeExemptionPanelEui()
    {
        IoCManager.InjectDependencies(this);

        _sawmill = _log.GetSawmill("admin.role_time_exemptions_eui");
    }

    public override EuiStateBase GetNewState()
    {
        var isAdmin = _admins.HasAdminFlag(Player, AdminFlags.Admin);
        return new RoleTimeExemptionPanelEuiState(PlayerName, _roleTimeExemptionsData, isAdmin); // TODO: Populate
    }

    public override void HandleMessage(EuiMessageBase msg)
    {
        base.HandleMessage(msg);

        switch (msg)
        {
            case RoleTimeExemptionPanelEuiStateMsg.SetExemptionsRequest r:
                SetExemptions(r.Exemptions);
                break;
        }
    }

    public async void ChangePlayer(string playerNameOrId)
    {
        var located = await _playerLocator.LookupIdByNameOrIdAsync(playerNameOrId);
        ChangePlayer(located?.UserId, located?.Username ?? string.Empty);
    }

    public void ChangePlayer(NetUserId? playerId, string playerName)
    {
        if (playerId == null)
        {
            PlayerId = playerId;
            PlayerName = playerName;
            StateDirty();
            return;
        }

        var playerExemptionsDataTask = _exemptions.GetRoleExemptions(playerId.Value);
        playerExemptionsDataTask.ContinueWith(task =>
        {
            // Result can be problematic, but isn't here because we are in a ContinueWith
#pragma warning disable RA0004
            var data = task.Result;
#pragma warning restore RA0004
            PlayerId = playerId;
            PlayerName = playerName;
            _roleTimeExemptionsData = data;
            StateDirty();
        },
        TaskContinuationOptions.OnlyOnRanToCompletion);
    }

    private void SetExemptions(RoleTimeExemptions exemptions)
    {
        if (!_admins.HasAdminFlag(Player, AdminFlags.Admin))
        {
            _sawmill.Warning(
                $"{Player.Name} ({Player.UserId}) tried to create a role time exemption, without the appropriate permissions");
            return;
        }

        if (PlayerId == null)
        {
            _chat.DispatchServerMessage(Player, Loc.GetString("role-exemption-panel-no-data"));
            return;
        }

        RoleTimeExemptionsData data = new (exemptions.ExemptJobs, exemptions.ExemptAntags);
        _exemptions.SetExemptions(PlayerId.Value, data);
    }

    public override async void Opened()
    {
        base.Opened();
        _admins.OnPermsChanged += OnPermsChanged;
    }

    public override void Closed()
    {
        base.Closed();
        _admins.OnPermsChanged -= OnPermsChanged;
    }

    private void OnPermsChanged(AdminPermsChangedEventArgs args)
    {
        if (args.Player != Player)
        {
            return;
        }

        StateDirty();
    }
}
