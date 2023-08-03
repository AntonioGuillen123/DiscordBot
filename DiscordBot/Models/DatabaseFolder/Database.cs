using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;

namespace DiscordBot.Models.DatabaseFolder
{
	public static class Database
	{
		private const int XP_ADD_MESSAGE = 7;

		public static async Task<Embed> CheckLevel(IGuildUser user)
		{
			using MyDbContext context = new MyDbContext();

			DataUser dataUser = context.Users.Include(u => u.CurrentLevel).Include(u => u.NextLevel).FirstOrDefault(u => user.Id == u.Id);

			return await EmbedBuild(dataUser);
		}
		public static async Task InsertUsersAsync()
		{
			using MyDbContext context = new MyDbContext();

			if (context.Database.EnsureCreated())
			{
				int countXP = 1;
				var usersGuild = Program.client.Guilds.FirstOrDefault().Users.Where(user => user.IsBot == false).ToList();

				foreach (var user in usersGuild)
				{
					DataXP level = new() { Id = countXP, EXP = 0, Level = 1 };
					DataXP nextLevel = new() { Id = level.Id + 1, EXP = 50, Level = level.Level + 1 };

					countXP += 2;

					context.XP.Add(level);
					context.XP.Add(nextLevel);
					context.Users.Add(new() 
					{ 
						Id = user.Id, 
						Name = user.DisplayName, 
						IsBot = user.IsBot, 
						CurrentLevel = level, 
						NextLevel = nextLevel });
				}

				context.SaveChanges();
			}
		}
		public static async Task MessageXPAdd(SocketUserMessage message)
		{
			if (message.Author.IsBot == false)
			{
				using MyDbContext context = new();

				var user = context.Users.Include(user => user.CurrentLevel).Include(user => user.NextLevel).FirstOrDefault(user => user.Id == message.Author.Id);

				user.CurrentLevel.AddXP(XP_ADD_MESSAGE);

				await user.LevelUpAsync(message.Channel as ITextChannel);

				context.SaveChanges();
			}
		}

		private static async Task<Embed> EmbedBuild(DataUser user)
		{
			EmbedBuilder builder = Utilities.Builder;
			var disUser = await user.SearchUserAsync();

			builder.WithAuthor(disUser);
			builder.WithTitle($"Level {user.CurrentLevel.Level}");
			builder.AddField($"Current XP", user.CurrentLevel.EXP, true);
			builder.AddField($"Next Level XP", user.NextLevel.EXP, true);

			return builder.Build();
		}
	}
}
