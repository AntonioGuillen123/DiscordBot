using Discord;
using Discord.Audio;
using Discord.WebSocket;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using YoutubeExplode;
using YoutubeExplode.Common;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace DiscordBot
{
	public class Player
	{
		private const int THUMBNAIL_RESOLUTION = 57600;
		private const string PATTERN_YT = "^(https?:\\/\\/)?(www\\.)?(youtube\\.com|youtu\\.be)\\/.+";
		private const string PATTERN_PLAYLIST = "^(?:https?:\\/\\/)?(?:www\\.)?(?:youtube\\.com|youtu\\.be)\\/(?:playlist|watch)\\?(?=.*list=)(?:\\S+)?$";

		private DiscordSocketClient _client = Program.client;
		private IAudioClient _audioClient;
		private IVoiceChannel _voiceChannel;
		private ITextChannel _textChannel;

		private Queue<IVideo> _queque = new();
		private YoutubeClient _youtube = new();
		private bool _playing = false;


		public async void StartPlayer(IVoiceChannel voiceChannel, ITextChannel textChannel, string query)
		{
			_voiceChannel = voiceChannel;
			_textChannel = textChannel;

			//Program.client.UserVoiceStateUpdated += ErrorDis;

			bool isPlaylist = Regex.IsMatch(query, PATTERN_PLAYLIST);
			List<IVideo> videos = new();
			string text = "";
				
			if (isPlaylist)
			{
				foreach (IVideo video in await _youtube.Playlists.GetVideosAsync(query))
					videos.Add(video);

				text = "PLAYLIST WAS SUCCESSFULLY ADDED TO QUEUE :)";
			}
			else
			{
				bool isYT = Regex.IsMatch(query, PATTERN_YT);

				videos.Add(isYT ? await _youtube.Videos.GetAsync(query) : await _youtube.Search.GetVideosAsync(query).FirstOrDefaultAsync());

				text = "VIDEO WAS SUCCESSFULLY ADDED TO QUEUE :)";
			}

			videos.ForEach(_queque.Enqueue);

			if (_playing)
				await _textChannel.SendMessageAsync(text);

			CheckQueue();
		}

		public async void CheckQueue()
		{
			do
			{
				if (!_playing)
				{
					_playing = true;
					Play(_queque.Dequeue());
				}
			} while (_queque.Count != 0);
		}

		public async void Play(IVideo video)
		{
			if (_audioClient == null)
				_audioClient = await _voiceChannel.ConnectAsync();

			Stream videoStream = await FindVideoURLAsync(video);

			using Process ffmpeg = CreateStream();

			using Stream output = ffmpeg.StandardOutput.BaseStream;
			using AudioOutStream discord = _audioClient.CreatePCMStream(AudioApplication.Music);

			try
			{
				Task.Run(async () =>
				{
					videoStream.Position = 0;
					await videoStream.CopyToAsync(ffmpeg.StandardInput.BaseStream);
					await ffmpeg.StandardInput.BaseStream.FlushAsync();
					ffmpeg.StandardInput.BaseStream.Close();
				});

				await output.CopyToAsync(discord);
			}
			finally
			{
				await discord.FlushAsync();
				_playing = false;
			}
		}

		//private async Task ErrorDis(SocketUser user, SocketVoiceState oldChannel, SocketVoiceState newChannel)
		//{
		//	await _textChannel.SendMessageAsync("movio");

		//	if (user.Id == _client.CurrentUser.Id && oldChannel)
		//	{
				
		//	}
		//}

		private Process CreateStream() => Process.Start(new ProcessStartInfo
		{//-loglevel panic
		 //-ac 2 -f s16le -ar 48000 pipe:1
			FileName = "ffmpeg",
			Arguments = $"-hide_banner -i - -ac 2 -f s16le -ar 48000 pipe:",
			UseShellExecute = false,
			RedirectStandardInput = true,
			RedirectStandardOutput = true,
		});

		private async Task<Stream> FindVideoURLAsync(IVideo video)
		{
			await SendMessagesToChannelAsync(video);

			StreamManifest streamManifest = await _youtube.Videos.Streams.GetManifestAsync(video.Id);

			IStreamInfo streamInfo = streamManifest.GetAudioOnlyStreams()
				.Where(v => v.Container == Container.Mp4)
				.GetWithHighestBitrate();

			Stream stream = await _youtube.Videos.Streams.GetAsync(streamInfo);

			return stream;
		}

		private async Task SendMessagesToChannelAsync(IVideo video)
		{
			await _textChannel.SendMessageAsync($"PLAYING {video.Title}");

			int position = GetThumbnail(video);

			if (position != -1)
				await _textChannel.SendMessageAsync(video.Thumbnails[position].Url);
		}

		private int GetThumbnail(IVideo video)
		{
			bool contains = false;
			int count = video.Thumbnails.Count;
			int position = -1;

			for (int i = 0; !contains && i < count; i++)
			{
				if (video.Thumbnails[i].Resolution.Area == THUMBNAIL_RESOLUTION)
				{
					contains = true;
					position = i;
				}
			}

			return position;
		}

		public async void Stop() => await _voiceChannel.DisconnectAsync();
	}
}