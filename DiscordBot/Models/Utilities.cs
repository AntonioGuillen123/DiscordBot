using Discord;
using Discord.WebSocket;
using DiscordBot.Models.DatabaseFolder;
using Microsoft.EntityFrameworkCore.Metadata;

namespace DiscordBot.Models
{
    public static class Utilities
    {
		private static readonly Color EMBED_COLOR = Color.Red;
		private static DiscordSocketClient _client = Program.client;

		public static EmbedBuilder Builder { get => ResetBuilder(); }

		private static EmbedBuilder ResetBuilder() => new EmbedBuilder()
			.WithAuthor(_client.CurrentUser)
			.WithColor(EMBED_COLOR)
			.WithCurrentTimestamp();
	}
}
