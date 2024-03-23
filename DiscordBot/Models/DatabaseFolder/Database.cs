using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace DiscordBot.Models.DatabaseFolder
{
	public static class Database
	{
		private const int XP_BASE = 300;
		public const string DATABASE_PATH = "../../../Databases/";

		public static DataUser SearchUser(IGuildUser user)
		{
			using MyDbContext context = new MyDbContext(user.Guild.Name);

			DataUser dataUser = context.Users.Include(u => u.CurrentLevel).Include(u => u.NextLevel).FirstOrDefault(u => user.Id == u.Id);

			return dataUser;
		}
		public static async Task<Embed> CheckLevel(IGuildUser user) => await EmbedBuildLevelAsync(SearchUser(user));

		public static async Task<Embed> CheckLaderBoardsAsync(IGuild guild)
		{
			using MyDbContext context = new MyDbContext(guild.Name);

			List<DataUser> dataUsers = context.Users.Include(u => u.CurrentLevel).Include(u => u.NextLevel).OrderByDescending(item => item.CurrentLevel.Level).ToList();

			return await EmbedBuildLeaderBoardsAsync(dataUsers);
		}

		public static void AddXP(IGuildUser user, int xp)
		{
			using MyDbContext context = new MyDbContext(user.Guild.Name);

			DataUser dataUser = context.Users.Include(u => u.CurrentLevel).FirstOrDefault(u => user.Id == u.Id);

			dataUser.CurrentLevel.AddXP(xp);

			context.SaveChanges();
		}
		//MIRAR BIEN COMO REUTILIZAR ESTA WEA, AHORA NO QUE ESTOY MENTALBREAKDOWN
		public static void RestXP(IGuildUser user, int xp)
		{
			using MyDbContext context = new MyDbContext(user.Guild.Name);

			DataUser dataUser = context.Users.Include(u => u.CurrentLevel).FirstOrDefault(u => user.Id == u.Id);

			dataUser.CurrentLevel.RestXP(xp);

			context.SaveChanges();
		}

		public static async Task OnInsertUsersAsync()
		{
			var guilds = Program.client.Guilds;

			foreach (var guild in guilds)
			{
				Directory.CreateDirectory($"{DATABASE_PATH}{guild.Name.Replace(" ", "")}");

				using MyDbContext context = new MyDbContext(guild.Name);

				if (context.Database.EnsureCreated())
				{
					int countXP = 1;
					var usersGuild = guild.Users.Where(user => user.IsBot == false).ToList();

					foreach (var user in usersGuild)
					{
						IGuildUser guildUser = user as IGuildUser;

						AddUser(context, guildUser, guild, countXP);

						countXP += 2;
					}

					context.SaveChanges();
				}
			}
		}

		private static void AddUser(MyDbContext context, IGuildUser user, IGuild guild, int countXP)
		{
			DataXP level = new() { Id = countXP, EXP = 0, Level = 1 };
			DataXP nextLevel = new() { Id = level.Id + 1, EXP = XP_BASE, Level = level.Level + 1 };

			context.XP.Add(level);
			context.XP.Add(nextLevel);
			context.Users.Add(new()
			{
				Id = user.Id,
				GuildId = guild.Id,
				Name = user.DisplayName,
				IsBot = user.IsBot,
				CurrentLevel = level,
				NextLevel = nextLevel
			});
		}

		public static async Task MessageXPAdd(SocketUserMessage message)
		{
			if (message.Author.IsBot == false)
			{
				var guild = message.Channel as SocketGuildChannel;

				using MyDbContext context = new(guild.Guild.Name);

				var user = context.Users.Include(user => user.CurrentLevel)
					.Include(user => user.NextLevel)
					.FirstOrDefault(user => user.Id == message.Author.Id);

				Random random = new Random();

				user.CurrentLevel.AddXP(random.Next(10, 51));

				await user.LevelUpAsync(message.Channel as ITextChannel);

				context.SaveChanges();
			}
		}

		public static async Task OnNewUserGuild(IGuildUser guildUser)
		{
			using MyDbContext context = new(guildUser.Guild.Name);

			int id = context.XP.Include(data => data.Id).Max(data => data.Id);

			AddUser(context, guildUser, guildUser.Guild, id);

			context.SaveChanges();
		}

		//public static async Task DeleteUserGuild(SocketGuild guild, IGuildUser guildUser)
		//{
		//	using MyDbContext context = new(guildUser.Guild.Name);

		//	int id = context.XP.Include(data => data.Id).Max(data => data.Id);

		//	AddUser(context, guildUser, guildUser.Guild, id);

		//	context.SaveChanges();
		//}

		private static async Task<Embed> EmbedBuildLevelAsync(DataUser user)
		{
			EmbedBuilder builder = Utilities.Builder;

			var disUser = await user.SearchUserAsync();

			builder.WithAuthor(disUser);
			builder.WithTitle($"Level {user.CurrentLevel.Level}");
			builder.AddField($"Current XP", user.CurrentLevel.EXP, true);
			builder.AddField($"Next Level XP", user.NextLevel.EXP, true);

			builder.WithFooter(new EmbedFooterBuilder()
			{
				Text = disUser.Guild.Name,
				IconUrl = disUser.Guild.IconUrl
			});

			return builder.Build();
		}

		private static async Task<Embed> EmbedBuildLeaderBoardsAsync(List<DataUser> dataUsers)
		{
			EmbedBuilder builder = Utilities.Builder;

			var disUser = await dataUsers[0].SearchUserAsync();

			builder.WithTitle(disUser.Guild.Name);
			builder.WithThumbnailUrl(disUser.Guild.IconUrl);

			StringBuilder b1 = new();
			StringBuilder b2 = new();
			StringBuilder b3 = new();

			foreach (var user in dataUsers)
			{
				b1.AppendLine(user.Name);
				b2.AppendLine(user.CurrentLevel.Level.ToString());
				b3.AppendLine(user.CurrentLevel.EXP.ToString());
			}

			builder.AddField("User", $"***{b1}***", true);
			builder.AddField("Level", b2.ToString(), true);
			builder.AddField("Current XP", b3.ToString(), true);

			builder.WithFooter(new EmbedFooterBuilder()
			{
				Text = disUser.Guild.Name,
				IconUrl = disUser.Guild.IconUrl
			});

			return builder.Build();
		}
	}
}