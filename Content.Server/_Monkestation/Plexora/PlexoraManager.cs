using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Content.Shared._Monkestation;
using Robust.Shared.Configuration;
using Robust.Shared.Player;

namespace Content.Server._Monkestation.Plexora;

public sealed partial class PlexoraManager : IPostInjectInit
{
    [Dependency] private IConfigurationManager _configManager = default!;
    [Dependency] private ILogManager _logManager = default!;
    private readonly HttpClient _httpClient = new();

    public void PostInject()
    {
        _sawmill = _logManager.GetSawmill("plexora");
    }
    private ISawmill _sawmill = default!;

    public async Task<PlexoraDonorApiResponse?> GetDonorInfo(ICommonSession session)
    {
        if (!IsPlexoraConfigured())
        {
            return null;
        }

        var donorRequest = new HttpRequestMessage(HttpMethod.Get, _configManager.GetCVar(CCVarsMonke.PlexoraUrl) + "/donor/" + session.UserId);
        donorRequest.Headers.Authorization = new AuthenticationHeaderValue("Basic", _configManager.GetCVar(CCVarsMonke.PlexoraToken));
        var result = await _httpClient.SendAsync(donorRequest);
        var apiResponse = await result.Content.ReadFromJsonAsync<PlexoraDonorApiResponse>();
        return apiResponse;
    }

    public async Task<string?> GetDiscordLinkCode(ICommonSession session)
    {
        if (!IsPlexoraConfigured())
        {
            return "PLX-VERIFY-this_is_a_token";
            // return null; TODO: Re-enable
        }

        var linkCodeRequest = new HttpRequestMessage(HttpMethod.Get, _configManager.GetCVar(CCVarsMonke.PlexoraToken) + "/link/" + session.UserId);
        linkCodeRequest.Headers.Authorization = new AuthenticationHeaderValue("Basic", _configManager.GetCVar(CCVarsMonke.PlexoraToken));
        var result = await _httpClient.SendAsync(linkCodeRequest);
        var apiResponse = await result.Content.ReadFromJsonAsync<PlexoraLinkCodeApiResponse>();
        return apiResponse?.Code;
    }

    private bool IsPlexoraConfigured()
    {
        if (!_configManager.GetCVar(CCVarsMonke.PlexoraEnabled))
        {
            return false;
        }

        if (_configManager.GetCVar(CCVarsMonke.PlexoraToken) == string.Empty
            || _configManager.GetCVar(CCVarsMonke.PlexoraUrl) == string.Empty)
        {
            _sawmill.Warning("Plexora is enabled but missing either token or url.");
            return false;
        }

        return true;
    }

    private record PlexoraLinkCodeApiResponse(string Code);
}
