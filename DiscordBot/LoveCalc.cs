using Discord;
using Discord.WebSocket;
using System.Drawing;
using System.IO;

namespace DiscordBot
{
	public static class LoveCalc
	{
		private const int RESOLUTION = 512;

		public static async void StartLoveCalc(IEnumerable<SocketUser> mentionedUsers, ITextChannel textChannel)
		{
			List<IGuildUser> users = new();

			foreach (var item in mentionedUsers)
			{
				users.Add(item as IGuildUser);
			}

			long sum = (long)(users[0].Id + users[1].Id);
			long sub = (long)(users[0].Id - users[1].Id);

			int result = (int)Math.Abs(sum / sub);

			await SendLoveCalc(textChannel, users, result);
		}

		private static async Task SendLoveCalc(ITextChannel textChannel, List<IGuildUser> users, int result)
		{
			using Stream photo = await MakePhoto(users);

			await textChannel.SendMessageAsync($"***{users[0].DisplayName}*** + ***{users[1].DisplayName}*** = {result}% UWU");

			await textChannel.SendFileAsync(photo, "photo.png");
		}

		private static async Task<Stream> MakePhoto(List<IGuildUser> users)
		{
			List<Stream> photos = await GetPhotos(new List<string>()
			{
				users[0].GetAvatarUrl(ImageFormat.Auto, RESOLUTION),
				users[1].GetAvatarUrl(ImageFormat.Auto, RESOLUTION)
			});

			using Bitmap bitmap = new Bitmap(RESOLUTION * 3, RESOLUTION);

			using System.Drawing.Image image1 = System.Drawing.Image.FromStream(photos[0]);
			using System.Drawing.Image image2 = System.Drawing.Image.FromStream(photos[1]);
			using System.Drawing.Image imagePlus = System.Drawing.Image.FromFile("spiderplus.png");

			using (Graphics graphics = Graphics.FromImage(bitmap))
			{
				graphics.DrawImage(image1, 0, 0, RESOLUTION, RESOLUTION);
				graphics.DrawImage(imagePlus, RESOLUTION, 0, RESOLUTION, RESOLUTION);
				graphics.DrawImage(image2, RESOLUTION * 2, 0, RESOLUTION, RESOLUTION);
			};

			Stream result = new MemoryStream();

			bitmap.Save(result, System.Drawing.Imaging.ImageFormat.Png);

			return result;
		}

		private static async Task<List<Stream>> GetPhotos(List<string> urls)
		{
			HttpClient client1 = new();
			HttpClient client2 = new();

			var response1 = await client1.GetAsync(urls[0]);
			var response2 = await client2.GetAsync(urls[1]);

			response1.EnsureSuccessStatusCode();
			response2.EnsureSuccessStatusCode();

			return new List<Stream>(){
				await response1.Content.ReadAsStreamAsync(),
				await response2.Content.ReadAsStreamAsync()
			};
		}
	}
}
