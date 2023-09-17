using Discord;
using Discord.Rest;
using DiscordBot.Models.DatabaseFolder;
using System.Text;

namespace DiscordBot.Models.Games.EmojiRouletteFolder
{
	public class EmojiRoulette : Game
	{
		private const string EMOJIS_PATH = "emojis.txt";

		private bool _doBet;
		private bool _playing;
		private string[,] _message;
		private ITextChannel _textChannel;
		private static List<string> _allEmojis;
		private Dictionary<IGuildUser, Bet> _players;
		private Dictionary<ulong, Models.Games.Game> _games;

		static EmojiRoulette()
		{
			string text = File.ReadAllText(EMOJIS_PATH);

			_allEmojis = text.Replace("\r", "").Split('\n').ToList();
		}

		public EmojiRoulette() => _players = new();

		public async void StartEmojiRoulette(ITextChannel textChannel, Dictionary<ulong, Models.Games.Game> games)
		{
			_textChannel = textChannel;

			_games = games;

			await GameAsync();
		}

		public async void StopEmojiRoulette()
		{
			_playing = false;

			_games.Remove(_textChannel.Id);
		}

		private async Task GameAsync()
		{
			_playing = true;

			while (_playing)
			{
				var message = await _textChannel.SendMessageAsync("***THE GAME WILL START IN 30 SECONDS, PLACE YOUR BETS...***");

				_doBet = true;

				Thread.Sleep(25000);

				await message.DeleteAsync();

				message = await _textChannel.SendMessageAsync("***ONLY 5 SECONDS LEFT...***");

				_doBet = false;

				string emojiMessage = CreateEmojis().ToString();

				Thread.Sleep(5000);

				await message.DeleteAsync();

				await _textChannel.SendMessageAsync(emojiMessage);

				await ManageResults();

				Thread.Sleep(3000);
			}
		}

		public async void Bet(IGuildUser player, Bet bet)
		{
			DataUser user = Database.SearchUser(player);

			if (_doBet && bet.InitialBetAmount <= user.CurrentLevel.EXP)
			{
				_players[player] = bet;

				await _textChannel.SendMessageAsync($"***{player.Mention} BET {string.Join("", bet.Emojis)}***");
			}
			else
			{
				await _textChannel.SendMessageAsync("YOU CAN´T BET");
			}
		}

		private async Task ManageResults()
		{
			RestXP();

			CheckEmojis();

			AddXP();

			if(_players.Count != 0)
				await _textChannel.SendMessageAsync(embed: EmbedBuild());

			_players = new();
		}

		private Embed EmbedBuild()
		{
			var builder = Utilities.Builder;

			StringBuilder b1 = new StringBuilder();
			StringBuilder b2 = new StringBuilder();
			StringBuilder b3 = new StringBuilder();
			StringBuilder b4 = new StringBuilder();

			foreach (var item in _players)
			{
				b1.AppendLine(item.Key.DisplayName);
				b2.AppendLine(string.Join("", item.Value.Emojis));
				b3.AppendLine(item.Value.InitialBetAmount.ToString());
				b4.AppendLine(item.Value.BetAmount.ToString());
			}

			builder.AddField("USER", $"***{b1}***", true);
			//builder.AddField("BET", b2.ToString(), true);
			builder.AddField("BET AMOUNT", b3.ToString(), true);
			builder.AddField("WIN", b4.ToString(), true);

			builder.WithFooter(new EmbedFooterBuilder()
			{
				Text = "Emoji Roulette",
				IconUrl = "https://cdn-icons-png.flaticon.com/512/2656/2656498.png"
			});

			return builder.Build();
		}

		private void RestXP()
		{
			foreach (var player in _players)
			{
				Database.RestXP(player.Key, player.Value.BetAmount);
			}
		}

		private void AddXP()
		{
			foreach (var player in _players)
			{
				if (player.Value.BetAmount != 0)
				{
					Database.AddXP(player.Key, player.Value.BetAmount);
				}
			}
		}

		private void CheckEmojis()
		{
			Dictionary<string, (int, int)> emojis = new();

			foreach (var player in _players)
			{
				bool lose = true;

				foreach (var emoji in player.Value.Emojis)
				{
					bool findEmoji = false;

					if (emojis.ContainsKey(emoji))
					{
						findEmoji = true;
					}
					else
					{
						for (int i = 0; !findEmoji && i < _message.GetLength(0); i++)
						{
							for (int j = 0; !findEmoji && j < _message.GetLength(1); j++)
							{
								if (_message[i, j] == emoji)
								{
									emojis[emoji] = (i, j);

									findEmoji = true;
								}
							}
						}
					}

					if (findEmoji)
					{
						player.Value.BetAmount += (int)(player.Value.BetAmount * 1.5);
						lose = false;
					}
				}

				if (lose)
					player.Value.BetAmount = 0;
			}
		}


		private StringBuilder CreateEmojis()
		{
			_message = new string[5, 5];
			StringBuilder sb = new StringBuilder();

			int lenght = _allEmojis.Count;

			string emoji;
			Random random = new Random();

			for (int i = 0; i < _message.GetLength(0); i++)
			{
				for (int j = 0; j < _message.GetLength(1); j++)
				{
					emoji = _allEmojis[random.Next(lenght)];

					sb.Append(emoji);
					_message[i, j] = emoji;
				}

				sb.AppendLine();
			}
			return sb;
		}

		private async Task CheckEmojisADMIN()
		{
			StringBuilder stringBuilder = new StringBuilder();

			int count = 0;

			for (int i = 0; i < _allEmojis.Count; i++)
			{
				var e = _allEmojis[i];

				stringBuilder.AppendLine(e.ToString());

				count++;

				if (count >= 30)
				{
					await _textChannel.SendMessageAsync(stringBuilder.ToString());

					stringBuilder = new StringBuilder();

					count = 0;
				}
			}
		}
	}
}