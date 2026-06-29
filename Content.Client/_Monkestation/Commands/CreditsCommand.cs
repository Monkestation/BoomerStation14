using Content.Client.Credits;
using Content.Shared.Administration;
using JetBrains.Annotations;
using Robust.Client.UserInterface;
using Robust.Shared.Configuration;
using Robust.Shared.Console;

namespace Content.Client._Monkestation.Commands;

[UsedImplicitly, AnyCommand]
public sealed partial class OpenUrlCommand : LocalizedCommands
{
    [Dependency] private IUriOpener _uriOpener = default!;
    public override string Command => "openurl";

    public override string Help => LocalizationManager.GetString($"cmd-{Command}-help", ("command", Command));

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length != 1)
        {
            shell.WriteLine(Help);
            return;
        }

        _uriOpener.OpenUri(args[0]);
    }
}
