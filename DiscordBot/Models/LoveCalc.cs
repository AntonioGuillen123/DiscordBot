using Discord;
using Discord.WebSocket;
using System.Drawing;
using System.IO;

namespace DiscordBot.Models
{
    public static class LoveCalc
    {
        //private const string CLIENT_ID = "04c3ba3f7f3eabf";

        private const int RESOLUTION = 512;
        private static List<IGuildUser> _users = new();

        public static async void StartLoveCalc(IEnumerable<SocketUser> mentionedUsers, ITextChannel textChannel)
        {
            foreach (var item in mentionedUsers)
            {
                _users.Add(item as IGuildUser);
            }

            long sum = (long)(_users[0].Id + _users[1].Id);
            long sub = (long)(_users[0].Id - _users[1].Id);

            int result = (int)Math.Abs(sum / sub);

            await SendLoveCalcAsync(textChannel, result);
        }

        private static async Task SendLoveCalcAsync(ITextChannel textChannel, int result)
        {
            using Stream photo = await MakePhotoAsync(_users);

            await textChannel.SendMessageAsync($"***{_users[0].DisplayName}*** + ***{_users[1].DisplayName}*** = {result}% UWU");

            await textChannel.SendFileAsync(photo, "image.png");

            //await FetchAsync(photo);

            //await textChannel.SendMessageAsync(embed: EmbedBuild(result));
        }

        private static async Task<Stream> MakePhotoAsync(List<IGuildUser> users)
        {
            List<Stream> photos = await GetPhotosAsync(new()
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

  //      private static async Task FetchAsync(Stream photo)
  //      {
		//	var client = new HttpClient();
		//	var request = new HttpRequestMessage(HttpMethod.Post, "https://api.imgur.com/3/image");
		//	request.Headers.Add("Authorization", $"Client-ID {CLIENT_ID}");
		//	var content = new MultipartFormDataContent();
		//	content.Add(new StreamContent(photo), "image", "image.png");
		//	request.Content = content;
		//	var response = await client.SendAsync(request);
		//	response.EnsureSuccessStatusCode();
		//	Console.WriteLine(await response.Content.ReadAsStringAsync());

		//}


		//private static Embed EmbedBuild(int result, Stream photo)
  //      {
  //          EmbedBuilder embed = Utilities.Builder;

  //          embed.WithTitle($"{result}% UWU");
  //          embed.AddField("User 1", _users[0].DisplayName, true);
		//	embed.AddField("User 2", _users[1].DisplayName, true);



		//	return embed.Build();
  //      }

        private static async Task<List<Stream>> GetPhotosAsync(List<string> urls)
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
