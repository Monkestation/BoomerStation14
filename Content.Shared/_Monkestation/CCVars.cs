using Robust.Shared.Configuration;

namespace Content.Shared._Monkestation;

[CVarDefs]
public sealed class CCVarsMonke
{
    /// <summary>
    /// If the plexora system should be enabled at all.
    /// </summary>
    public static readonly CVarDef<bool> PlexoraEnabled =
        CVarDef.Create("plexora.enabled", false, CVar.SERVERONLY);

    /// <summary>
    /// The base url to contact plexora at
    /// </summary>
    public static readonly CVarDef<string> PlexoraUrl =
        CVarDef.Create("plexora.url", String.Empty, CVar.SERVERONLY);

    /// <summary>
    /// The auth token for plexora
    /// </summary>
    public static readonly CVarDef<string> PlexoraToken =
        CVarDef.Create("plexora.token", String.Empty, CVar.SERVERONLY | CVar.CONFIDENTIAL);

    public static readonly CVarDef<bool> InheritPatrons =
        CVarDef.Create("monke.inherit_patrons", false, CVar.SERVER | CVar.REPLICATED);
}
