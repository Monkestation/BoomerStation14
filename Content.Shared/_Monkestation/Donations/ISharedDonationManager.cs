using Robust.Shared.Player;

namespace Content.Shared._Monkestation.Donations;

public interface ISharedDonationManager
{
    /// <summary>
    /// Gets the donor status for a specific entity
    /// </summary>
    /// <param name="uid">The entity to check</param>
    /// <returns>True if they are a donator</returns>
    DonorData? GetDonorData(EntityUid uid);

    /// <summary>
    /// Gets the donor status for a specific session
    /// </summary>
    /// <param name="session">The session to check</param>
    /// <returns>True if they are a donator</returns>
    DonorData? GetDonorData(ICommonSession session);
}


