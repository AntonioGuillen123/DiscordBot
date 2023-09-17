using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordBot.Models.Amazon;
using DiscordBot.Models.DatabaseFolder;
using DiscordBot.Models.Games;
using DiscordBot.Models.Games.EmojiRouletteFolder;
using DiscordBot.Models.VirusTotalFolder;
using YoutubeExplode.Common;

namespace DiscordBot.Models
{
	public class Commands : ModuleBase<SocketCommandContext>
	{
		private Weather _weather;
		private Dictionary<ulong, Player> _players;
		private Dictionary<ulong, Games.Game> _games;
		private DiscordSocketClient _client = Program.client;

		public Commands(Dictionary<ulong, Player> players, Dictionary<ulong, Models.Games.Game> games, Weather weather)
		{
			_players = players;
			_games = games;
			_weather = weather;
		}

		[Command("emoji", RunMode = RunMode.Async)]
		public async Task EmojisAsync()
		{
			ITextChannel textChannel = Context.Channel as ITextChannel;

			EmojiRoulette roulette = new();

			if (textChannel != null && !await CheckPLaying(textChannel, roulette))
			{
				roulette.StartEmojiRoulette(textChannel, _games);
			}
		}

		[Command("bet", RunMode = RunMode.Async)]
		public async Task BetEmojis(int betAmount, params string[] emojis)
		{
			ITextChannel textChannel = Context.Channel as ITextChannel;
			IGuildUser guildUser = Context.Message.Author as IGuildUser;

			if (textChannel != null)
			{
				if (emojis.Length == 5)
				{
					try
					{
						EmojiRoulette roulette = (EmojiRoulette)_games[textChannel.Id];

						Bet bet = new()
						{
							Emojis = emojis.ToList(),
							InitialBetAmount = betAmount,
							BetAmount = betAmount
						};

						roulette.Bet(guildUser, bet);
					}
					catch (Exception)
					{
						await ReplyAsync("***THERE IS NO ROULETTE AT THIS MOMENT***");
					}
				}
				else
				{
					await ReplyAsync("***YOU MUST ENTER ONLY 5 EMOJIS***");
				}
			}
		}

		[Command("stopemojis", RunMode = RunMode.Async)]
		public async Task StopEmojisAsync()
		{
			ITextChannel textChannel = Context.Channel as ITextChannel;

			if (textChannel != null)
			{
				try
				{
					EmojiRoulette roulette = (EmojiRoulette)_games[textChannel.Id];

					roulette.StopEmojiRoulette();
				}
				catch (Exception)
				{
					await ReplyAsync("***THERE IS NO ROULETTE AT THIS MOMENT***");
				}
			}
		}

		[Command("scanfile", RunMode = RunMode.Async)]
		public async Task VirusTotalAsync(string password = null)
		{
			List<Attachment> files = Context.Message.Attachments.ToList();
			ITextChannel textChannel = Context.Channel as ITextChannel;

			if (files.Count == 1)
			{
				VirusTotal.StartVirus(textChannel, files.First(), password);
			}
			else
			{
				await ReplyAsync("***YOU MUST ENTER 1 FILE***");
			}
		}

		[Command("amazon", RunMode = RunMode.Async)]
		public async Task AmazonCommandAsync([Remainder] string input)
		{
			ITextChannel textChannel = Context.Channel as ITextChannel;

			AmazonScrapper amazonScrapper = new();

			IUserMessage message = await ReplyAsync("***SEARCHING... :mag_right: :detective:***");

			amazonScrapper.StartAmazon(textChannel, _client, input, message);
		}

		[Command("love", RunMode = RunMode.Async)]
		public async Task LoveCommandAsync([Remainder] string input)
		{
			ITextChannel textChannel = Context.Channel as ITextChannel;
			var mentionedUsers = Context.Message.MentionedUsers;

			if (textChannel != null && mentionedUsers.Count == 2)
			{
				LoveCalc.StartLoveCalc(mentionedUsers, textChannel);
			}
			else
			{
				await ReplyAsync("YOU MUST ENTER 2 USERS");
			}
		}

		[Command("game", RunMode = RunMode.Async)]
		public async Task GameWordsAsync()
		{
			ITextChannel textChannel = Context.Channel as ITextChannel;

			GameWords gameWords = new();

			if (textChannel != null && !await CheckPLaying(textChannel, gameWords))
			{
				gameWords.StartGameWords(_games, textChannel, _client);
			}
		}

		private async Task<bool> CheckPLaying(ITextChannel textChannel, Games.Game game)
		{
			bool result = false;
			ulong id = textChannel.Id;

			if (!_games.ContainsKey(id))
			{
				_games[id] = game;
			}
			else
			{
				result = true;

				await textChannel.SendMessageAsync("***A GAME IS ALREADY PLAYING ON THIS CHANNEL***");
			}

			return result;
		}

		[Command("weather", RunMode = RunMode.Async)]
		public async Task TimeCommandAsync(string locationName)
		{
			ITextChannel textChannel = Context.Channel as ITextChannel;

			if (textChannel != null)
			{
				_weather.StartTime(textChannel, locationName);
			}
		}

		[Command("level", RunMode = RunMode.Async)]
		public async Task CheckLevelCommandAsync([Remainder] string all = null)
		{
			if (all == null)
			{
				IGuildUser user = Context.User as IGuildUser;

				await ReplyAsync(embed: await Database.CheckLevel(user));
			}
			else
			{
				IGuild guild = Context.Guild as IGuild;

				await ReplyAsync(embed: await Database.CheckLaderBoardsAsync(guild));
			}
		}

		[Command("clear", RunMode = RunMode.Async)]
		public async Task ClearAsync(string input)
		{
			IGuildUser user = Context.User as IGuildUser;

			if (user.GuildPermissions.ManageMessages)
			{
				int value;

				if (int.TryParse(input, out value))
				{
					ITextChannel textChannel = Context.Channel as ITextChannel;

					var messages = await textChannel.GetMessagesAsync(++value).FlattenAsync();

					await textChannel.DeleteMessagesAsync(messages);

					await ReplyAsync($" MESSAGES DELETED: {value} :)");
				}
				else
				{
					await ReplyAsync("YOU MUST ENTER A NUMBER GREATER THAN 0");
				}
			}
			else
			{
				await ReplyAsync("YOU DON'T HAVE PERMISSION TO DO THAT :(");
			}
		}

		private Player GetGuildPlayer()
		{
			IGuild guild = Context.Guild as IGuild;
			Player player = new();

			if (_players.ContainsKey(guild.Id))
			{
				player = _players[guild.Id];
			}
			else
			{
				_players.Add(guild.Id, player);
			}

			return player;
		}

		[Command("play", RunMode = RunMode.Async)]
		public async Task PlayAsync([Remainder] string input)
		{
			IVoiceChannel voiceChannel = (Context.User as IGuildUser).VoiceChannel;
			ITextChannel textChannel = Context.Channel as ITextChannel;

			if (voiceChannel != null)
			{
				Player player = GetGuildPlayer();

				player.StartPlayer(voiceChannel, textChannel, input);
			}
			else
			{
				await ReplyAsync($"YOU ARE NOT ON ANY VOICE CHANNEL :(");
			}
		}

		[Command("skip", RunMode = RunMode.Async)]
		public async Task SkipAsync() => GetGuildPlayer().Skip();

		[Command("queque", RunMode = RunMode.Async)]
		public async Task QuequeAsync() => GetGuildPlayer().SeeQueque();

		[Command("stop", RunMode = RunMode.Async)]
		public async Task StopAsync() => GetGuildPlayer().Stop();
	}
}
