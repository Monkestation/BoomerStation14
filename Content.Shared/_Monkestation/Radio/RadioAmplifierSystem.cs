using Content.Shared._Monkestation.Verbs;
using Content.Shared.Interaction;
using Content.Shared.Medical.SuitSensors;
using Content.Shared.Popups;
using Content.Shared.Verbs;

namespace Content.Shared._Monkestation.Radio;

/// <summary>
/// This handles the radio amplifier component (mostly just setting it)
/// </summary>
public sealed partial class RadioAmplifierSystem : EntitySystem
{
    [Dependency] private SharedInteractionSystem _interactionSystem = default!;
    [Dependency] private SharedPopupSystem _popupSystem = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MSRadioAmplifierComponent, GetVerbsEvent<Verb>>(OnVerb);
    }

    private void OnVerb(Entity<MSRadioAmplifierComponent> ent, ref GetVerbsEvent<Verb> args)
    {
        // standard interaction checks
        if (!args.CanInteract || args.Hands == null)
            return;

        if (!_interactionSystem.InRangeUnobstructed(args.User, args.Target))
            return;

        args.Verbs.UnionWith(new[]
        {
            CreateVerb(ent, args.User, true),
            CreateVerb(ent, args.User, false),
        });
    }

    /// <summary>
    /// Create a verb for viewing and setting the state of the amplifier.
    /// </summary>
    /// <param name="ent">Entity with a <see cref="SuitSensorComponent"/> to be verbed.</param>
    /// <param name="userUid">Actor requesting the verb, used to identify if a foreign actor is requesting a verb.</param>
    /// <param name="enabled">Mode to change the state to</param>
    /// <returns>A created <see cref="Verb"/> that will attempt to change to a specific mode.</returns>
    private Verb CreateVerb(Entity<MSRadioAmplifierComponent> ent, EntityUid userUid, bool enabled)
    {
        return new Verb()
        {
            Text = Loc.GetString(enabled ? "ui-button-on" : "ui-button-off"),
            Message = Loc.GetString(enabled ? "ms-radio-amplifier-on" : "ms-radio-amplifier-off"),
            Disabled = ent.Comp.Enabled == enabled,
            Priority = enabled ? 1 : 0, // sort them in descending order
            Category = MonkeVerbCategory.SetSensor,
            Act = () => SetAmplifier(ent.AsNullable(), enabled, userUid)
        };
    }

    /// <summary>
    /// Sets mode of the <see cref="SuitSensorComponent"/> of the chosen entity.
    /// Makes popup when <param name="userUid"> not null
    /// </summary>
    /// <param name="sensors">Entity and it's component that should be changed</param>
    /// <param name="enabled">If loud mode should be enabled</param>
    /// <param name="userUid">uid, required for the popup</param>
    private void SetAmplifier(Entity<MSRadioAmplifierComponent?> sensors, bool enabled, EntityUid? userUid = null)
    {
        if (!Resolve(sensors, ref sensors.Comp, false))
            return;

        sensors.Comp.Enabled = enabled;
        Dirty(sensors);

        if (userUid != null)
        {
            var msg = Loc.GetString(enabled ? "ms-loud-mode-state-set-on" : "ms-loud-mode-state-set-off");
            _popupSystem.PopupClient(msg, sensors, userUid.Value);
        }
    }
}
