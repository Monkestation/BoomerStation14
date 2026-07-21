namespace Content.Shared._Monkestation.Donations;

/// <summary>
/// The data for a donor, and their tiers
/// </summary>
/// <param name="Tiers">The prototype data for each tier the user has</param>
public sealed record DonorData(List<MSDonorTierPrototype> Tiers);
