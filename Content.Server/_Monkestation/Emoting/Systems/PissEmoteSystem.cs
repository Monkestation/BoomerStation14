using Content.Shared._Monkestation.Body.Systems;
using Content.Shared._Monkestation.Emoting.Components;
using Content.Shared.Chat;
using Content.Shared.Popups;

namespace Content.Server._Monkestation.Emoting.Systems;

/// <summary>
/// This handles the piss emote
/// </summary>
public sealed partial class PissEmoteSystem : EntitySystem
{
    [Dependency] private SharedPopupSystem _popupSystem = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MSPissEmoteComponent, EmoteEvent>(OnEmote);
    }

    private void OnEmote(Entity<MSPissEmoteComponent> ent, ref EmoteEvent args)
    {
        // Probably bad practice, but I'm not sure what else we would do with emotes that would result in pissing
        if (args.Emote.ID != "MSPiss")
        {
            return;
        }

        var ev = new TryPissEvent();
        RaiseLocalEvent(ent, ref ev);

        if (!ev.Handled)
        {
            _popupSystem.PopupEntity(Loc.GetString("ms-chat-emote-piss-failed"), ent, ent);
        }
    }
}
