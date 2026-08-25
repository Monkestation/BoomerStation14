using NetCord;
using NetCord.Rest;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace Content.Server.Discord.DiscordLink;

public sealed partial class DiscordLink
{
    /// <summary>
    /// Sends an embed to a Discord channel with the specified ID. Without any mentions.
    /// </summary>
    public async Task SendEmbedAsync(ulong channelId, EmbedProperties embed)
    {
        if (_client == null)
        {
            return;
        }

        var channel = await _client.Rest.GetChannelAsync(channelId) as TextChannel;
        if (channel == null)
        {
            _sawmill.Error("Tried to send a message to Discord but the channel {Channel} was not found.", channel);
            return;
        }

        await channel.SendMessageAsync(new MessageProperties()
        {
            AllowedMentions = AllowedMentionsProperties.None,
            Embeds = [embed],
        });
    }
}
