using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Content.Server.Database;
using Content.Shared._Monkestation.Players;
using Content.Shared.Roles;
using Robust.Server.Player;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Server._Monkestation.Administration.Managers;

public sealed partial class RoleTimeExemptionManager : IPostInjectInit
{
    [Dependency] private ILogManager _logManager = default!;
    [Dependency] private INetManager _netManager = default!;
    [Dependency] private UserDbDataManager _userDbData = default!;
    [Dependency] private IServerDbManager _db = default!;
    [Dependency] private IPlayerManager _playerManager = default!;

    private ISawmill _sawmill = default!;

    private readonly Dictionary<ICommonSession, RoleTimeExemptionsData> _cachedRoleTimeExemptions = new();

    private const string SawmillId = "admin.roletimeexemptions";
    private const string DbTypeAntag = "Antag";
    private const string DbTypeJob = "Job";

    public void Initialize()
    {
        _netManager.RegisterNetMessage<MsgRoleTimeExemptions>();
        // TODO: Support db messages for cross server role time exemption updates
        // currently out of scope because that's a lot of effort and we only have the one server, they can relog

        _userDbData.AddOnLoadPlayer(CachePlayerData);
        _userDbData.AddOnPlayerDisconnect(ClearPlayerData);
    }

    private async Task CachePlayerData(ICommonSession player, CancellationToken cancel)
    {
        var roleTimeExemptions = await _db.GetRoleTimeExemptions(player.UserId, cancel);
        _cachedRoleTimeExemptions[player] = CacheRoleTimeExemptionsData(roleTimeExemptions);
        SendRoleExemptions(player);
    }

    private static RoleTimeExemptionsData CacheRoleTimeExemptionsData(
        List<MonkestationRoleTimeExemption> roleTimeExemptions)
    {
        return new RoleTimeExemptionsData(GetRoleExemptions<JobPrototype>(roleTimeExemptions).ToList(),
            GetRoleExemptions<AntagPrototype>(roleTimeExemptions).ToList());
    }

    private void ClearPlayerData(ICommonSession player)
    {
        _cachedRoleTimeExemptions.Remove(player);
    }

    public void PostInject()
    {
        _sawmill = _logManager.GetSawmill(SawmillId);
    }

    private void SendRoleExemptions(ICommonSession player)
    {
        MsgRoleTimeExemptions exemptions;
        if (!_cachedRoleTimeExemptions.TryGetValue(player, out var roleTimeExemptions))
        {
            exemptions = new MsgRoleTimeExemptions()
            {
                JobExemptions = [],
                AntagExemptions = [],
            };
        }
        else
        {
            exemptions = new MsgRoleTimeExemptions()
            {
                JobExemptions = roleTimeExemptions.JobExemptions,
                AntagExemptions = roleTimeExemptions.AntagExemptions,
            };
        }

        _sawmill.Debug($"Sent role exemptions to {player.Name}");
        _netManager.ServerSendMessage(exemptions, player.Channel);
    }

    private static HashSet<ProtoId<T>> GetRoleExemptions<T>(List<MonkestationRoleTimeExemption> exemptions)
        where T : class, IPrototype
    {
        var dbType = PrototypeKindToDbType<T>();

        return exemptions
            .Where(role => role.RoleType == dbType)
            .Select(role => new ProtoId<T>(role.RoleId))
            .ToHashSet();
    }

    /// <summary>
    /// Gets the time exemptions for a player by their uid. uses the cached value if available, or fetches from the db.
    /// </summary>
    /// <param name="player">The player id to get the data for</param>
    /// <returns>The data for that player.</returns>
    public async Task<RoleTimeExemptionsData> GetRoleExemptions(NetUserId player)
    {
        if (_playerManager.SessionsDict.TryGetValue(player, out var session)
            && _cachedRoleTimeExemptions.TryGetValue(session, out var roleTimeExemptionData))
        {
            return roleTimeExemptionData;
        }

        var roleTimeExemptions = await _db.GetRoleTimeExemptions(player.UserId);
        return CacheRoleTimeExemptionsData(roleTimeExemptions);
    }

    private static string PrototypeKindToDbType<T>() where T : class, IPrototype
    {
        if (typeof(T) == typeof(JobPrototype))
            return DbTypeJob;

        if (typeof(T) == typeof(AntagPrototype))
            return DbTypeAntag;

        throw new ArgumentException($"Unknown prototype kind for role bans: {typeof(T)}");
    }

    private static List<MonkestationRoleTimeExemption> RoleExemptionsToDbExemption<T>(NetUserId userId,
        List<ProtoId<T>> exemptions) where T : class, IPrototype
    {
        var dbType = PrototypeKindToDbType<T>();
        return exemptions.Select(exemption => new MonkestationRoleTimeExemption()
            {
                UserId = userId,
                RoleType = dbType,
                RoleId = exemption.Id,
            })
            .ToList();
    }

    public void SetExemptions(NetUserId playerId, RoleTimeExemptionsData data)
    {
        List<MonkestationRoleTimeExemption> roleTimeExemptions = [];
        roleTimeExemptions.AddRange(RoleExemptionsToDbExemption(playerId, data.JobExemptions));
        roleTimeExemptions.AddRange(RoleExemptionsToDbExemption(playerId, data.AntagExemptions));
        _db.SetRoleTimeExemptions(playerId, roleTimeExemptions);

        if (!_playerManager.SessionsDict.TryGetValue(playerId, out var session))
        {
            return;
        }

        _cachedRoleTimeExemptions[session] = data;
        SendRoleExemptions(session);
    }

    public bool IsExemptFor<T>(ICommonSession player, ProtoId<T> role) where T : class, IPrototype
    {
        if (!_cachedRoleTimeExemptions.TryGetValue(player, out var roleTimeExemptions))
        {
            return false;
        }

        if (role is ProtoId<JobPrototype> job)
            return roleTimeExemptions.JobExemptions.Contains(job);

        if (role is ProtoId<AntagPrototype> antag)
            return roleTimeExemptions.AntagExemptions.Contains(antag);

        throw new ArgumentException($"Unknown prototype kind for role bans: {typeof(T)}");
    }
}
