using Robust.Shared.Configuration;

namespace Content.Shared._Monkestation.CCVars;

[CVarDefs]
public sealed class MonkeCCVars
{
    /// <summary>
    ///     The prototype to use for secret weights.
    /// </summary>
    public static readonly CVarDef<string> AnnouncerWeightPrototype =
        CVarDef.Create("monke.announcer_weight_prototype", "Announcers", CVar.SERVERONLY);
}
