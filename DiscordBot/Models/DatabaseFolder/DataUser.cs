using Discord;
using Discord.WebSocket;

namespace DiscordBot.Models.DatabaseFolder
{
	public class DataUser
	{
		private DataXP _level;
		private DataXP _nextLevel;

		public ulong Id { get; set; }
		public ulong GuildId { get; set; }
		public string Name { get; set; }
		public bool IsBot { get; set; }
		public DataXP CurrentLevel { get => _level; set => _level = value; }
		public DataXP NextLevel { get => _nextLevel; set => _nextLevel = value; }

		public async Task LevelUpAsync(ITextChannel textChannel)
		{
			bool levelUP = false;

			while (CurrentLevel >= NextLevel)
			{
				levelUP = true;

				int newXP = CurrentLevel.EXP - NextLevel.EXP;

				NextLevel.Level++;
				NextLevel.EXP += (int)(NextLevel.EXP * 1.5);

				CurrentLevel.EXP = newXP;
				CurrentLevel.Level++;
			}

			if (levelUP)
				await SendLevelUpMessageAsync(textChannel);
		}

		public async Task SendLevelUpMessageAsync(ITextChannel textChannel)
		{
			IGuildUser user = await SearchUserAsync();

			await textChannel.SendMessageAsync($"Congratulations {user.Mention} YOU ARE LEVEL: {CurrentLevel.Level}");
		}

		public IGuild SearchGuild() => Program.client.GetGuild(GuildId);
		public async Task<IGuildUser> SearchUserAsync() => await SearchGuild().GetUserAsync(Id);
	}
}
