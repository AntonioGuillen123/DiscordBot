using Discord;
using Discord.Rest;

namespace DiscordBot.Models.DatabaseFolder
{
	public class DataUser
	{
		private DataXP _level;
		private DataXP _nextLevel;

		public ulong Id { get; set; }
		public string Name { get; set; }
		public bool IsBot { get; set; }
		public DataXP CurrentLevel { get => _level; set => _level = value; }
		public DataXP NextLevel { get => _nextLevel; set => _nextLevel = value; }

		public async Task LevelUpAsync(ITextChannel textChannel) 
		{
			if (CurrentLevel >= NextLevel)
			{
				int newXP = CurrentLevel.EXP - NextLevel.EXP;

				NextLevel.Level++;
				NextLevel.EXP += (int)(CurrentLevel.EXP * 0.25);

				CurrentLevel.EXP = newXP;
				CurrentLevel.Level++;

				await SendLevelUpMessageAsync(textChannel);
			}
		}

		public async Task SendLevelUpMessageAsync(ITextChannel textChannel)
		{
			IUser user = await SearchUserAsync();

			await textChannel.SendMessageAsync($"Congratulations {user.Mention} YOU ARE LEVEL: {CurrentLevel.Level}");
		}

		public async Task<IUser> SearchUserAsync() => await Program.client.GetUserAsync(Id);
	}
}
