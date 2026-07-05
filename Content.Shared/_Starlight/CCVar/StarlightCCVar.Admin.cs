using Robust.Shared.Configuration;

namespace Content.Shared._Starlight.CCVar;

[CVarDefs] // Monke - make this one annotated, if the main one is ported remove this
public sealed partial class StarlightCCVars
{
    public static readonly CVarDef<string> AdminGhostScriptPath =
        CVarDef.Create("admin.admin_ghost_script_path", string.Empty, CVar.CLIENTONLY | CVar.ARCHIVE);
}
