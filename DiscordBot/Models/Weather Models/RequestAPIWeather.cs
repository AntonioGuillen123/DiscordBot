using System.Text.Json.Serialization;

namespace DiscordBot.Models
{
	public class RequestAPIWeather
	{
		[JsonPropertyName("list")]
		public List<IntervalWeather> Intervals { get; set; }
	}
}
