using Discord;
using Octokit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot
{
	public static class Github
	{
		public static async Task StartGithub(ITextChannel textChannel, string input)
		{
			var client = new GitHubClient(new ProductHeaderValue("SpiderBotxD"));

			var response = new SearchUsersRequest(input);

			var result = await client.Search.SearchUsers(response);

			User user = result.Items[0];

			string message = CreateMessage(user).ToString();

			await textChannel.SendMessageWithColor(message, Color.Red);
		}

		private static StringBuilder CreateMessage(User user)
		{
			StringBuilder builder = new();

			builder.AppendLine($"**NAME:**\t{user.Login}");
			builder.AppendLine($"**Nº PUBLIC REPOSITORIES:**\t{user.PublicRepos}");
			builder.AppendLine($"**USER BIO:**\t{user.Bio}");
			builder.AppendLine($"**AVATAR:**");
			builder.AppendLine(user.AvatarUrl);

			return builder;
		}
	}
}
