using Content.Server.Administration;
using Content.Server.EUI;
using Content.Shared.Administration;
using Robust.Shared.Console;

namespace Content.Server._Monkestation.Administration.Commands;

[AdminCommand(AdminFlags.Admin)]
public sealed partial class RoleTimeExemptionPanelCommand : LocalizedCommands
{
    [Dependency] private IPlayerLocator _locator = default!;
    [Dependency] private EuiManager _euis = default!;

    public override string Command => "role-time-exemption-panel";

    public override async void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (shell.Player is not { } player)
        {
            shell.WriteError(Loc.GetString("shell-cannot-run-command-from-server"));
            return;
        }

        switch (args.Length)
        {
            case 1:
                var located = await _locator.LookupIdByNameOrIdAsync(args[0]);
                if (located is null)
                {
                    shell.WriteError(Loc.GetString($"cmd-{Command}-player-err"));
                    return;
                }
                var ui = new RoleTimeExemptionPanelEui();
                _euis.OpenEui(ui, player);
                ui.ChangePlayer(located.UserId, located.Username);
                break;
            default:
                shell.WriteLine(Loc.GetString($"cmd-{Command}-invalid-arguments"));
                shell.WriteLine(Help);
                return;
        }
    }

}
