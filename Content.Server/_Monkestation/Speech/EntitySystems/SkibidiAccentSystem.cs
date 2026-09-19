using System.Linq;
using Content.Shared.Speech;
using Robust.Shared.Random;
using System.Text.RegularExpressions;
using Content.Server._Monkestation.Speech.Components;
using Content.Shared.Speech.EntitySystems;

namespace Content.Server._Monkestation.Speech.EntitySystems;

public sealed partial class SkibidiAccentSystem : EntitySystem
{
    [GeneratedRegex(@"^(\S+)")]
    private static partial Regex FirstWordAllCapsRegex { get; }

    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private ReplacementAccentSystem _replacement = default!;

    // converts left word when typed into the right word. For example typing you becomes ye.
    private string Accentuate(string message, SkibidiAccentComponent component)
    {
        Log.Error("Accentuating with brainrot");
        var msg = _replacement.ApplyReplacements(message, "skibidi");

        // First, see if the message is cringe. If not, just send it.
        if (!_random.Prob(component.CringeChance))
            return msg;
        //Checks if the first word of the sentence is all caps
        //So the prefix can be allcapped and to not resanitize the captial
        var firstWordAllCaps = !FirstWordAllCapsRegex.Match(msg).Value.Any(char.IsLower);

        // Add a prefix.
        if (_random.Prob(component.CringePrefixChance))
        {
            var pick1 = _random.Pick(component.SkibidiPrefixs);
            var skibidiPrefix = Loc.GetString(pick1);
            // Reverse sanitize capital
            if (!firstWordAllCaps)
                msg = msg[0].ToString().ToLower() + msg.Remove(0, 1);
            else
                skibidiPrefix = skibidiPrefix.ToUpper();
            msg = skibidiPrefix + " " + msg;
        }

        //Add a suffix.
        if (_random.Prob(component.CringeSuffixChance))
        {
            var pick2 = _random.Pick(component.SkibidiSuffixs);
            var skibidiSuffix = Loc.GetString(pick2);
            msg = msg + skibidiSuffix;
        }

        return msg;
    }

    [SubscribeLocalEvent]
    private void OnAccentGet(EntityUid uid, SkibidiAccentComponent component, ref AccentGetEvent args)
    {
        args.Message = Accentuate(args.Message, component);
    }
}
