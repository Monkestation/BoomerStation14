using System.Threading.Tasks;
using Robust.Server.ServerStatus;
using System.Net.Http;
using System.Text.Json.Serialization;
using Content.Server.Chat.Managers;

// Namespace moved to be the same as the base class
// ReSharper disable once CheckNamespace
namespace Content.Server.Administration;

public sealed partial class ServerApi
{
    [Dependency] private IChatManager _chatManager = default!;

    private void Monke_RegisterEndpoints()
    {
        RegisterHandler(HttpMethod.Post, "/admin/announce", Monke_AdminAnnounce);
    }

    /// <summary>
    ///     Sends and announcement to the server
    /// </summary>
    private async Task Monke_AdminAnnounce(IStatusHandlerContext context)
    {
        var announcement = await ReadJson<Monke_ServerAnnounceBody>(context);
        if (announcement == null)
            return;
        await RunOnMainThread(() =>
        {
            if (announcement.Subtitle != null)
            {
                _chatManager.DispatchServerAnnouncement(announcement.Subtitle, Color.Red);
            }
            _chatManager.DispatchServerAnnouncement(announcement.Message);
        });
        await RespondOk(context);
    }

    private sealed class Monke_ServerAnnounceBody
    {
        [JsonPropertyName("subtitle")]
        public string? Subtitle { get; init; }
        [JsonPropertyName("message")]
        public required string Message { get; init; }
    }
}
