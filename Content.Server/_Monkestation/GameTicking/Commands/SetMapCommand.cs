using System.Linq;
using Content.Server.Administration;
using Content.Server.Maps;
using Content.Shared.Administration;
using Content.Shared.Maps;
using Robust.Shared.Console;
using Robust.Shared.Prototypes;

namespace Content.Server._Monkestation.GameTicking.Commands;

[AdminCommand(AdminFlags.Round)]
public sealed partial class SetMapCommand : LocalizedCommands
{
    [Dependency] private IPrototypeManager _prototypeManager = default!;
    [Dependency] private IGameMapManager _gameMapManager = default!;

    public override string Command => "setmap";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length is < 1 or > 2)
        {
            shell.WriteError(Loc.GetString("shell-need-between-arguments", ("lower", 1), ("upper", 2), ("currentAmount", args.Length)));
            return;
        }

        var name = args[0];

        if (!string.IsNullOrEmpty(name) && !_gameMapManager.CheckMapExists(name))
        {
            shell.WriteLine(Loc.GetString("cmd-setmap-map-not-found", ("map", name)));
            return;
        }

        var rounds = 1;

        if (args.Length >= 2 && !int.TryParse(args[1], out rounds))
        {
            shell.WriteError(Loc.GetString("cmd-setmap-optional-argument-not-integer"));
            return;
        }

        var immediate = _gameMapManager.SetMap(name, rounds);
        var clearing = string.IsNullOrEmpty(name) || rounds <= 0;

        var locKey = $"cmd-setmap-{(clearing ? "reset" : "set")}-{(immediate ? "immediate" : "delayed")}";

        shell.WriteLine(Loc.GetString(locKey, ("map", name), ("rounds", rounds)));
    }

    public override CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        switch (args.Length)
        {
            case 1:
            {
                var options = _prototypeManager
                    .EnumeratePrototypes<GameMapPrototype>()
                    .Select(p => new CompletionOption(p.ID, p.MapName))
                    .OrderBy(p => p.Value);

                return CompletionResult.FromHintOptions(options, Loc.GetString($"cmd-setmap-hint"));
            }
            case 2:
                return CompletionResult.FromHint(Loc.GetString("cmd-setmap-hint-2"));
            default:
                return CompletionResult.Empty;
        }
    }
}
