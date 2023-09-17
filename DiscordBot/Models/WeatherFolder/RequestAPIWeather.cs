using System.Text.Json.Serialization;

namespace DiscordBot.Models
{
	public class RequestAPIWeather
	{
		[JsonPropertyName("list")]
		public IList<IntervalWeather> Intervals { get; set; }
	}
}
