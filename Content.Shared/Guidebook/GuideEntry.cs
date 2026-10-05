using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared.Guidebook;

[Prototype]
public sealed partial class GuideEntryPrototype : GuideEntry, IPrototype
{
    public string ID => Id;
}

[Virtual, DataDefinition]
public partial class GuideEntry
{
    /// <summary>
    ///     The file containing the contents of this guide.
    /// </summary>
    [DataField(required: true)] public ResPath Text = default!;

    /// <summary>
    ///     The unique id for this guide.
    /// </summary>
    [IdDataField]
    public string Id = default!;

    /// <summary>
    ///     The name of this guide. This gets localized.
    /// </summary>
    [DataField(required: true)] public LocId Name = default!;

    /// <summary>
    ///     The "children" of this guide for when guides are shown in a tree / table of contents.
    /// </summary>
    [DataField]
    public List<ProtoId<GuideEntryPrototype>> Children = new();

    /// <summary>
    ///     Enable filtering of items.
    /// </summary>
    [DataField] public bool FilterEnabled = default!;

    [DataField] public bool RuleEntry;

    // Monkestation edit - Added ability to disable guidebook entries
    /// <summary>
    /// If set to false, guidebook and it's children don't render.
    /// </summary>
    [DataField] public bool Enabled = true;

    /// <summary>
    ///     Priority for sorting top-level guides when shown in a tree / table of contents.
    ///     If the guide is the child of some other guide, the order simply determined by the order of children in <see cref="Children"/>.
    /// </summary>
    [DataField] public int Priority = 0;
}
