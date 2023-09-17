using DiscordBot.Models.WeatherFolder;
using System.Text.Json.Serialization;

namespace DiscordBot.Models
{
	public class IntervalWeather
	{
		[JsonPropertyName("weather")]
		public List<IconWeather> IconWeather { get; set; }

		[JsonPropertyName("main")]
		public MainWeather MainWeather { get; set; }

		[JsonPropertyName("pop")]
		public double Pop { get; set; }

		[JsonPropertyName("wind")]
		public WindWeather WindWeather { get; set; }

		[JsonPropertyName("dt_txt")]
		public string StringDateTime { get; set; }

		public DateTime FullDateTime { get => DateTime.Parse(StringDateTime); }
	}
}
