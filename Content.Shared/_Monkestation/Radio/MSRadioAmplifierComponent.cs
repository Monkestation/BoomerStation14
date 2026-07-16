using Robust.Shared.GameStates;

namespace Content.Shared._Monkestation.Radio;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class MSRadioAmplifierComponent : Component
{
    [DataField, AutoNetworkedField]
    public bool Enabled = false;
}
