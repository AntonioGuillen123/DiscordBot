using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordBot.Models.Amazon;
using DiscordBot.Models.DatabaseFolder;
using YoutubeExplode.Common;

namespace DiscordBot.Models
{
	public class Commands : ModuleBase<SocketCommandContext>
	{
		private Player _player;
		private Weather _weather;
		private DiscordSocketClient _client = Program.client;

		public Commands(Player player, Weather weather)
		{
			_player = player;
			_weather = weather;
		}

		[Command("amazon", RunMode = RunMode.Async)]
		public async Task AmazonCommandAsync([Remainder]string input)
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

			if (mentionedUsers.Count == 2)
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

			if (textChannel != null)
			{
				gameWords.StartGameWords(textChannel, _client);
			}
		}

		[Command("weather", RunMode = RunMode.Async)]
		public async Task TimeCommandAsync(string locationName)
		{
			ITextChannel textChannel = Context.Channel as ITextChannel;

			_weather.StartTime(textChannel, locationName);
		}

		[Command("level", RunMode = RunMode.Async)]
		public async Task CheckLevelCommandAsync(/*laderboards*/)
		{
			IGuildUser user = Context.User as IGuildUser;

			await ReplyAsync(embed: await Database.CheckLevel(user));
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
					ITextChannel channel = Context.Channel as ITextChannel;

					var messages = await channel.GetMessagesAsync(++value).FlattenAsync();

					foreach (IMessage message in messages)
						await message.DeleteAsync();

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

		[Command("play", RunMode = RunMode.Async)]
		public async Task PlayAsync([Remainder] string input)
		{
			IVoiceChannel voiceChannel = (Context.User as IGuildUser).VoiceChannel;
			ITextChannel textChannel = Context.Channel as ITextChannel;

			if (voiceChannel != null)
			{
				_player.StartPlayer(voiceChannel, textChannel, input);
			}
			else
			{
				await ReplyAsync($"YOU ARE NOT ON ANY VOICE CHANNEL :(");
			}
		}

		[Command("skip", RunMode = RunMode.Async)]
		public async Task SkipAsync() => _player.Skip();

		[Command("queque", RunMode = RunMode.Async)]
		public async Task QuequeAsync() => _player.SeeQueque();

		[Command("stop", RunMode = RunMode.Async)]
		public async Task StopAsync() => _player.Stop();

		[Command("troll", RunMode = RunMode.Async)]
		public async Task TrollAsync()
		{
			//var client = new DiscordSocketClient(new DiscordSocketConfig
			//{
			//	GatewayIntents = GatewayIntents.All
			//});

			//SocketUser user = client.GetUser(414395803973713940);

			//for (int i = 0; i < 999; i++)
			//{
			//	await Utilities.ConteSendMessageWithColor("ffffffffffff", Color.Red);
			//}
		}
	}
}
