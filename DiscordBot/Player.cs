using Discord;
using Discord.Audio;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YoutubeExplode;
using YoutubeExplode.Search;
using YoutubeExplode.Videos.Streams;

namespace DiscordBot
{
	public class Player
	{
		private IAudioClient audioClient;
		private IVoiceChannel _channel;
		private DiscordSocketClient _client;

		public Player(DiscordSocketClient client)
		{
			_client = client;
		}

		public async void Play(IVoiceChannel channel, string query)
		{
			_channel = channel;
			audioClient = await _channel.ConnectAsync();

			string videoUrl = await FindVideoUrl(query);
			using Process ffmpeg = CreateStream(videoUrl);
			using Stream output = ffmpeg.StandardOutput.BaseStream;
			using AudioOutStream discord = audioClient.CreatePCMStream(AudioApplication.Music);

			try
			{
				await output.CopyToAsync(discord);
			}
			finally
			{
				await discord.FlushAsync();
			}
		}

		private Process CreateStream(string path)
		{
			return Process.Start(new ProcessStartInfo
			{
				FileName = "ffmpeg",
				Arguments = $"-hide_banner -loglevel panic -i \"{path}\" -ac 2 -f s16le -ar 48000 pipe:1",
				UseShellExecute = false,
				RedirectStandardOutput = true,
			});
		}

		private async Task<string> FindVideoUrl(string query)
		{
			var youtube = new YoutubeClient();

			VideoSearchResult video = await youtube.Search.GetVideosAsync(query)
				.FirstAsync();

			StreamManifest streamManifest = await youtube.Videos.Streams.GetManifestAsync(video.Id);
			IStreamInfo streamInfo = streamManifest.GetAudioOnlyStreams()
				.Where(v => v.Container == Container.Mp4)
				.GetWithHighestBitrate();

			return streamInfo.Url;
		}

		public async void Stop()
		{
			await _channel.DisconnectAsync();
		}
	}
}
