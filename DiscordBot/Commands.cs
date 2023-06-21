using Discord;
using Discord.Commands;
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

		[Command("help", RunMode = RunMode.Async)]
		public async Task HelpAsync()
		{
			await ReplyAsync($"Hola {Context.Message.Author}");
		}

		[Command("play", RunMode = RunMode.Async)]
		public async Task PlayAsync([Remainder]string videoName)
		{
			IVoiceChannel channel = (Context.User as IGuildUser).VoiceChannel;

			if (channel != null)
			{
				_player.Play(channel, videoName);
			}
			else
			{
				await ReplyAsync($"No estas en nigún canal de voz");
			}
		}


		[Command("stop", RunMode = RunMode.Async)]
		public async Task StopAsync()
		{
			_player.Stop();
		}
	}
}
