using AngleSharp.Dom;
using Discord;
using Discord.Audio;
using System.Diagnostics;
using System.Net.NetworkInformation;
using YoutubeExplode;
using YoutubeExplode.Search;
using YoutubeExplode.Videos.Streams;
using YoutubeExplode.Videos;

namespace DiscordBot
{
	public class Player
	{
		private const int THUMBNAIL_RESOLUTION = 57600;

		private IAudioClient _audioClient;
		private IVoiceChannel _voiceChannel;
		private ITextChannel _textChannel;

		public async void Play(IVoiceChannel voiceChannel, ITextChannel textChannel, string query)
		{
			_voiceChannel = voiceChannel;
			_textChannel = textChannel;
			_audioClient = await _voiceChannel.ConnectAsync();

			string videoUrl = await FindVideo(query);

			using Process ffmpeg = CreateStream(videoUrl);
			using Stream output = ffmpeg.StandardOutput.BaseStream;
			using AudioOutStream discord = _audioClient.CreatePCMStream(AudioApplication.Music);

			try
			{
				await output.CopyToAsync(discord);
			}
			finally
			{
				await discord.FlushAsync();
			}
		}
		private async Task<string> FindVideo(string query)
		{
			bool contains = query.Contains("www.youtube.com");
			YoutubeClient youtube = new();
			IVideo video;

			video = contains ? await youtube.Videos.GetAsync(query) : await youtube.Search.GetVideosAsync(query).FirstAsync();

			return await FindVideoURLAsync(youtube, video);
		}

		private Process CreateStream(string path) => Process.Start(new ProcessStartInfo
		{
			FileName = "ffmpeg",
			Arguments = $"-hide_banner -loglevel panic -i \"{path}\" -ac 2 -f s16le -ar 48000 pipe:1",
			UseShellExecute = false,
			RedirectStandardOutput = true,
		});

		private async Task<string> FindVideoURLAsync(YoutubeClient youtube, IVideo video)
		{
			await SendMessagesToChannelAsync(video);

			StreamManifest streamManifest = await youtube.Videos.Streams.GetManifestAsync(video.Id);

			IStreamInfo streamInfo = streamManifest.GetAudioOnlyStreams()
				.Where(v => v.Container == Container.Mp4)
				.GetWithHighestBitrate();

			return streamInfo.Url;
		}

		private async Task SendMessagesToChannelAsync(IVideo video)
		{
			await _textChannel.SendMessageAsync($"Reproduciendo {video.Title}");

			int position = ContainsThumbnail(video);

			if (position != -1)
				await _textChannel.SendMessageAsync(video.Thumbnails[position].Url);
		}

		private int ContainsThumbnail(IVideo video)
		{
			bool contains = false;
			int count = video.Thumbnails.Count;
			int position = -1;

			for (int i = 0; !contains && i < count; i++)
			{
				if (video.Thumbnails[i].Resolution.Area == THUMBNAIL_RESOLUTION)
					contains = true;
					position = i;
			}

			return position;
		}

		public async void Stop() => await _voiceChannel.DisconnectAsync();
	}
}