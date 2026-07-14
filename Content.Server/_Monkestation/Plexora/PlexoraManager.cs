using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Content.Shared._Monkestation;
using Robust.Shared.Configuration;
using Robust.Shared.Player;

namespace Content.Server._Monkestation.Plexora;

public sealed partial class PlexoraManager
{
    [Dependency] private IConfigurationManager _configManager = default!;
    private readonly HttpClient _httpClient = new();

    public async Task<PlexoraDonorApiResponse?> GetDonorInfo(ICommonSession session)
    {
        if (!_configManager.GetCVar(CCVarsMonke.PlexoraEnabled))
        {
            return new PlexoraDonorApiResponse(["Staff", "Twitch1", "Nukie", "AAAAA"]);
            // return null;
        }

        var donorRequest = new HttpRequestMessage(HttpMethod.Get, _configManager.GetCVar(CCVarsMonke.PlexoraUrl) + "/donor/" + session.UserId);
        donorRequest.Headers.Authorization = new AuthenticationHeaderValue("Basic", _configManager.GetCVar(CCVarsMonke.PlexoraToken));
        var result = await _httpClient.SendAsync(donorRequest);
        var apiResponse = await result.Content.ReadFromJsonAsync<PlexoraDonorApiResponse>();
        return apiResponse;
    }

}
