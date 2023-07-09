using Discord;

namespace DiscordBot
{
	public static class Utilities
	{
		public static async Task SendMessageWithColor(this ITextChannel textChannel, string message, Color color)
		{
			string messageContent = $"```ini\n{message}\n```";

			var embed = new EmbedBuilder()
				.WithDescription(messageContent)
				.WithColor(color);

			await textChannel.SendMessageAsync(embed: embed.Build());
		}
	}
}
