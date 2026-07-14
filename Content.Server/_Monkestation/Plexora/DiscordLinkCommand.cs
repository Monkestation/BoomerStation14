using Content.Shared.Administration;
using Robust.Shared.Console;

namespace Content.Server._Monkestation.Plexora;

[AnyCommand]
public sealed partial class DiscordLinkCommand : LocalizedCommands
{
    [Dependency] private PlexoraManager _plexora = default!;

    public override string Command { get; } = "discordlink";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        GetDiscordLink(shell, argStr, args);
    }

    private async void GetDiscordLink(IConsoleShell shell, string argStr, string[] args)
    {
        if (shell.Player == null)
        {
            shell.WriteError(Loc.GetString($"cmd-{Command}-error-no-player"));
            return;
        }

        var token = await _plexora.GetDiscordLinkCode(shell.Player);

        if (token == null)
        {
            shell.WriteError(Loc.GetString($"cmd-{Command}-error-no-token"));
            return;
        }

        shell.WriteLine(Loc.GetString($"cmd-{Command}-success", ("token", token)));
        shell.RemoteExecuteCommand($"setclipboard {token}");
    }
}
