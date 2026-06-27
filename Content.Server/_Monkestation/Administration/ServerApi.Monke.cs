using System.Threading.Tasks;
using Robust.Server.ServerStatus;
using System.Net.Http;
using System.Text.Json.Serialization;
using Content.Server.Administration.Logs;
using Content.Server.Chat.Managers;
using Content.Shared.Chat;
using Content.Shared.Database;
using Robust.Shared.Utility;

// Namespace moved to be the same as the base class
// ReSharper disable once CheckNamespace
namespace Content.Server.Administration;

public sealed partial class ServerApi
{
    [Dependency] private IChatManager _chatManager = default!;
    [Dependency] private IAdminLogManager _adminLogger = default!;

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
                var formattedSource = "";
                if (announcement.Source != null)
                {
                    var escapedSource = FormattedMessage.EscapeText(announcement.Source);
                    formattedSource = announcement.SourceUrl == null ? escapedSource : $"[cmdlink=\"{escapedSource}\" command=\"openurl {announcement.SourceUrl}\" /]";
                }
                var formattedSubtitle = FormattedMessage.EscapeText(announcement.Subtitle) + formattedSource;
                var wrappedSubtitle = Loc.GetString("chat-manager-server-wrap-message", ("message", formattedSubtitle));
                _chatManager.ChatMessageToAll(ChatChannel.Server, announcement.Subtitle, wrappedSubtitle, EntityUid.Invalid, hideChat: false, recordReplay: true, colorOverride: Color.Red);

                _adminLogger.Add(LogType.Chat, LogImpact.Low, $"Relayed announcement: {announcement.Subtitle}: {announcement.Message}");
            }
            else
            {
                _adminLogger.Add(LogType.Chat, LogImpact.Low, $"Relayed announcement: {announcement.Message}");
            }
            var wrappedMessage = Loc.GetString("chat-manager-server-wrap-message", ("message", FormattedMessage.EscapeText(announcement.Message)));
            _chatManager.ChatMessageToAll(ChatChannel.Server, announcement.Message, wrappedMessage, EntityUid.Invalid, hideChat: false, recordReplay: true);
        });
        await RespondOk(context);
    }

    private sealed class Monke_ServerAnnounceBody
    {
        [JsonPropertyName("subtitle")]
        public string? Subtitle { get; init; }
        [JsonPropertyName("message")]
        public required string Message { get; init; }
        [JsonPropertyName("source_url")]
        public string? SourceUrl { get; init; }
        [JsonPropertyName("source")]
        public string? Source { get; init; }
    }
}
