using System.ComponentModel.DataAnnotations;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.Array;

namespace Content.Shared._Monkestation.Donations;

/// <summary>
/// This is a prototype for donor tiers
/// </summary>
[Prototype("msDonorTier")]
public sealed partial class MSDonorTierPrototype : IPrototype, IInheritingPrototype
{
    /// <inheritdoc/>
    [IdDataField]
    public string ID { get; private set; } = default!;

    /// <inheritdoc/>
    [ParentDataField(typeof(AbstractPrototypeIdArraySerializer<MSDonorTierPrototype>))]
    public string[]? Parents { get; private set; }

    /// <inheritdoc/>
    [NeverPushInheritance]
    [AbstractDataField]
    public bool Abstract { get; private set; }

    /// <summary>
    /// Localization key for this donor tier
    /// </summary>
    [DataField]
    public string Name { get; private set; } = "ms-donor-tier-default";

    /// <summary>
    /// If this tier should be used as a fallback in the event an unknown tier is encountered.
    /// </summary>
    /// <remarks>
    /// Only one prototype should be set as the fallback. If multiple are set, one will be selected as the fallback.
    /// The purpose is to handle a case where a tier is modified or renamed, but the server hasn't been updated to match,
    /// not as a regular thing.
    /// </remarks>
    [NeverPushInheritance, DataField]
    public bool FallbackTier { get; private set; }

    /// <summary>
    /// The order by which this tier is sorted in the credits. Lower numbers appear first.
    /// </summary>
    /// <remarks>
    /// For values that cannot be combined (like ooc color) this also determines the priority of effect.
    /// </remarks>
    [DataField]
    public int SortOrder { get; private set; }

    /// <summary>
    /// The color to use for the username in OOC, empty doesn't affect OOC color
    /// </summary>
    [DataField]
    public string OocColor { get; private set; } = "";
}
