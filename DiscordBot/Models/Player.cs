using Discord;
using Discord.Audio;
using Discord.WebSocket;
using System.Diagnostics;
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

		private IAudioClient _audioClient;
		private IVoiceChannel _voiceChannel;
		private ITextChannel _textChannel;

		private Queue<IVideo> _queque = new();
		private YoutubeClient _youtube = new();

		private bool _doQueque;
		private bool _playing;
		private CancellationTokenSource _token;


		public async void StartPlayer(IVoiceChannel voiceChannel, ITextChannel textChannel, string query)
		{
			_voiceChannel = voiceChannel;
			_textChannel = textChannel;

			//Program.client.UserVoiceStateUpdated += ErrorDis;

			try
			{
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

				_doQueque = false;

				CheckQueue();
			}
			catch (Exception)
			{
				await _textChannel.SendMessageAsync("AN ERROR HAS OCCURRED WITH THE VIDEO :(");
			}
		}

		public async void CheckQueue()
		{
			_doQueque = true;

			do
			{
				if (!_playing)
				{
					_playing = true;
					Play(_queque.Dequeue());
				}
			} while (_doQueque && _queque.Count != 0);
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
				_token = new CancellationTokenSource();

				Task.Run(async () =>
				{
					videoStream.Position = 0;
					await videoStream.CopyToAsync(ffmpeg.StandardInput.BaseStream);
					await ffmpeg.StandardInput.BaseStream.FlushAsync();
					ffmpeg.StandardInput.BaseStream.Close();
				});

				await output.CopyToAsync(discord, _token.Token);
			}
			catch (Exception)
			{

			}
			finally
			{
				await discord.FlushAsync();
				_playing = false;
			}
		}

		//public void PausePlayback()
		//{
		//	if (_playing && !_paused)
		//	{
		//		_token.Cancel();
		//		_paused = true;
		//	}
		//}

		//public void ResumePlayback()
		//{
		//	if (_playing && _paused)
		//	{
		//		_token = new CancellationTokenSource();
		//		_paused = false;

		//		Play();
		//	}
		//}

		//private async void Cancellation()
		//{
		//	_token?.Cancel();
		//}

		public async void Skip()
		{
			if (_playing && _queque.Count != 0)
			{
				await _textChannel.SendMessageAsync("SONG SKIPED SUCCESSFULLY");

				_token?.Cancel();
			}
			else
			{
				await _textChannel.SendMessageAsync("NO SONGS IN THE QUEUE");
			}
		}

		private Process CreateStream() => Process.Start(new ProcessStartInfo
		{
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

		public async void SeeQueque()
		{
			string text = "";
			int count = _queque.Count;

			text = count == 0 ? "NO SONGS IN THE QUEUE" : $"THERE ARE {count} SONGS IN QUEUE";

			await _textChannel.SendMessageAsync(text);
		}
		public async void Stop()
		{
			await _voiceChannel.DisconnectAsync();

			ResetPlayer();
		}

		private void ResetPlayer()
		{
			_audioClient = null;
			_queque = new();
			_youtube = new();
			_doQueque = false;
			_playing = false;
			_token?.Cancel();
			_token = null;
		}
	}
}