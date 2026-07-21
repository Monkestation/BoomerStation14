using System.Linq;
using System.Threading.Tasks;
using Content.Server.Administration.Managers;
using Content.Server.Discord.DiscordLink;
using Content.Server.GameTicking;
using Content.Server.Maps;
using Content.Shared.CCVar;
using NetCord;
using NetCord.Rest;
using Robust.Server;
using Robust.Server.Player;
using Robust.Shared.Asynchronous;
using Robust.Shared.Configuration;

namespace Content.Server._Monkestation.Discord.Commands;

public sealed partial class StatusDiscordCommand : IPostInjectInit
{
    [Dependency] private DiscordLink _discordLink = default!;
    [Dependency] private IConfigurationManager _cfg = default!;
    [Dependency] private IPlayerManager _playerManager = default!;
    [Dependency] private IAdminManager _adminManager = default!;
    [Dependency] private IBaseServer _baseServer = default!;
    [Dependency] private IGameMapManager _gameMapManager = default!;
    [Dependency] private IEntityManager _entityManager = default!;
    [Dependency] private ITaskManager _taskManager = default!;

    public void PostInject()
    {
        _discordLink.RegisterCommandCallback(OnStatusCommand, "status");
    }

    private async void OnStatusCommand(CommandReceivedEventArgs args)
    {
        var channelId = args.Message.Channel?.Id;
        if (channelId == null)
            return;
        var embed = await RunOnMainThread(() =>
        {
            var gameTicker = _entityManager.System<GameTicker>();

            List<string> rows = [];

            var playerCount = _cfg.GetCVar(CCVars.AdminsCountInReportedPlayerCount)
                ? _playerManager.PlayerCount
                : _playerManager.PlayerCount - _adminManager.ActiveAdmins.Count();
            var playerCap = _cfg.GetCVar(CCVars.SoftMaxPlayers);
            rows.Add(Loc.GetString("ms-discord-cmd-status-playercount", ("players", playerCount), ("maxPlayers", playerCap)));

            var mapName = _gameMapManager.GetSelectedMap()?.MapName;
            if (mapName != null)
                rows.Add(Loc.GetString("ms-discord-cmd-status-mapname", ("map", mapName)));

            var roundId = gameTicker.RoundId;
            rows.Add(Loc.GetString("ms-discord-cmd-status-roundid", ("round", roundId)));

            switch (gameTicker.RunLevel)
            {
                case GameRunLevel.PreRoundLobby:
                    rows.Add(Loc.GetString("ms-discord-cmd-status-runlevel-preround"));
                    break;
                case GameRunLevel.InRound:
                    rows.Add(Loc.GetString("ms-discord-cmd-status-runlevel-inround", ("time", TimeSpan.FromSeconds(Math.Round(gameTicker.RoundDuration().TotalSeconds)))));
                    break;
                case GameRunLevel.PostRound:
                    rows.Add(Loc.GetString("ms-discord-cmd-status-runlevel-postround"));
                    break;
            }

            return new EmbedProperties()
                .WithTitle(_baseServer.ServerName)
                .WithDescription(string.Join("\n", rows))
                .WithColor(new NetCord.Color(0, 255, 0));

        });

        await _discordLink.SendEmbedAsync(channelId.Value, embed);
    }

    private async Task<T> RunOnMainThread<T>(Func<T> func)
    {
        var taskCompletionSource = new TaskCompletionSource<T>();
        _taskManager.RunOnMainThread(() =>
        {
            try
            {
                taskCompletionSource.TrySetResult(func());
            }
            catch (Exception e)
            {
                taskCompletionSource.TrySetException(e);
            }
        });

        var result = await taskCompletionSource.Task;
        return result;
    }
}
