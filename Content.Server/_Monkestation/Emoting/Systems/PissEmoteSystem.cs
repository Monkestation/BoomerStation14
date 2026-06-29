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
public sealed partial class PissEmoteSystem : EntitySystem
{
    [Dependency] private SharedPopupSystem _popupSystem = default!;
    [Dependency] private BladderSystem _bladderSystem = default!;

    [Dependency] private EntityQuery<MSBladderComponent> _bladderQuery;
    [Dependency] private EntityQuery<ContainerManagerComponent> _containerQuery;

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

        if (!TryPiss(ent))
        {
            _popupSystem.PopupEntity(Loc.GetString("ms-chat-emote-piss-failed"), ent, ent);
        }
    }

    private bool TryPiss(Entity<MSPissEmoteComponent> ent)
    {
        ContainerManagerComponent? containerManagerComponent = null;
        if (!_containerQuery.Resolve(ent, ref containerManagerComponent))
        {
            return false;
        }

        var anyBladder = false;
        foreach(var entity in containerManagerComponent.Containers[BodyComponent.ContainerID].ContainedEntities)
        {
            if (!_bladderQuery.TryComp(entity, out var bladder))
            {
                continue;
            }

            anyBladder = true;
            if (_bladderSystem.TryPiss(ent, entity, bladder))
            {
                return true;
            }
        }

        return anyBladder;
    }
}
