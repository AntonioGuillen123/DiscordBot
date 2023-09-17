using Discord;
using DiscordBot.Models.WeatherFolder;
using System.Text;
using System.Text.Json;

namespace DiscordBot.Models
{
	public class Weather
	{
		private const string TOKEN_PATH = "weathertokenfile.txt";
		private const string URL_LOCATION_MAIN = "https://api.openweathermap.org/geo/1.0/direct?appid=";
		private const string URL_WEATHER_MAIN = "https://api.openweathermap.org/data/2.5/forecast?units=metric&appid=";
		private const string URL_FOOTER = "https://openweathermap.org/themes/openweathermap/assets/img/mobile_app/android-app-top-banner.png";

		private static string _token;
		private static string _urlLocation;
		private static string _urlWeather;
		private static RequestAPIWeather _httpWeather;
		private static LocationWeather _locationWeather;

		private ITextChannel _textChannel;

		public Weather() => GetToken();

		private void GetToken()
		{
			_token = System.IO.File.ReadAllText(TOKEN_PATH);

			_urlLocation = $"{URL_LOCATION_MAIN}{_token}&q=";
			_urlWeather = $"{URL_WEATHER_MAIN}{_token}";
		}

		public async void StartTime(ITextChannel textChannel, string locationName)
		{
			_textChannel = textChannel;

			await APIFetchManagerAsync(locationName);

			await SendMessagesAsync();
		}
		private async Task APIFetchManagerAsync(string locationName)
		{
			string location = await APIFetchAsync($"{_urlLocation}{locationName}");
			_locationWeather = JsonSerializer.Deserialize<List<LocationWeather>>(location)[0];

			string weather = await APIFetchAsync($"{_urlWeather}&lat={_locationWeather.Latitude}&lon={_locationWeather.Longitude}");
			_httpWeather = JsonSerializer.Deserialize<RequestAPIWeather>(weather);
		}

		private async Task<string> APIFetchAsync(string url)
		{
			using HttpClient client = new();

			var response = await client.GetAsync(url);

			response.EnsureSuccessStatusCode();

			return await response.Content.ReadAsStringAsync();
		}

		private async Task SendMessagesAsync()
		{
			var intervals = _httpWeather.Intervals.OrderBy(item => item.FullDateTime).ToList();

			foreach (var interval in intervals)
			{
				await _textChannel.SendMessageAsync(embed: EmbedBuild(interval));
			}
		}

		private Embed EmbedBuild(IntervalWeather interval)
		{
			EmbedBuilder embed = Utilities.Builder;

			embed.AddField($"DateTime", interval.FullDateTime);
			embed.ThumbnailUrl = interval.IconWeather[0].Icon;
			embed.AddField($"Temp", $"{interval.MainWeather.Temp} ºC");
			embed.AddField($"Pop", $"{interval.Pop * 100}%");
			embed.AddField($"Wind Speed", $"{Math.Round(interval.WindWeather.Speed * 3.6, 2)} km/h");
			embed.WithFooter(new EmbedFooterBuilder()
			{
				IconUrl = URL_FOOTER,
				Text = "openweathermap.org"
			});

			return embed.Build();
		}
	}
}