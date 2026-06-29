using Content.Server._Monkestation.Body.Systems;
using Content.Shared._Monkestation.Body.Components;
using Content.Shared._Monkestation.Body.Systems;
using Content.Shared._Monkestation.Emoting.Components;
using Content.Shared.Body;
using Content.Shared.Chat;
using Content.Shared.Popups;
using Robust.Shared.Containers;

namespace Content.Server._Monkestation.Emoting.Systems;

/// <summary>
/// This handles the piss emote
/// </summary>
public sealed partial class FartEmoteSystem : EntitySystem
{
    [Dependency] private SharedPopupSystem _popupSystem = default!;
    [Dependency] private ButtSystem _buttSystem = default!;

    [Dependency] private EntityQuery<MSButtComponent> _buttQuery;
    [Dependency] private EntityQuery<ContainerManagerComponent> _containerQuery;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MSFartEmoteComponent, EmoteEvent>(OnEmote);
    }

    private void OnEmote(Entity<MSFartEmoteComponent> ent, ref EmoteEvent args)
    {
        // Probably bad practice, but I'm not sure what else we would do with emotes that would result in pissing
        if (args.Emote.ID != "MSFart")
        {
            return;
        }

        if (!TryFart(ent))
        {
            _popupSystem.PopupEntity(Loc.GetString("ms-chat-emote-fart-failed"), ent, ent);
        }
    }

    private bool TryFart(Entity<MSFartEmoteComponent> ent)
    {
        ContainerManagerComponent? containerManagerComponent = null;
        if (!_containerQuery.Resolve(ent, ref containerManagerComponent))
        {
            return false;
        }

        var anyBladder = false;
        foreach(var entity in containerManagerComponent.Containers[BodyComponent.ContainerID].ContainedEntities)
        {
            if (!_buttQuery.TryComp(entity, out var bladder))
            {
                continue;
            }

            anyBladder = true;
            if (_buttSystem.TryFart(ent, (entity, bladder)))
            {
                return true;
            }
        }

        return anyBladder;
    }
}
