using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DiscordBot.Models.Weather_Models
{
	public class LocationWeather
	{
		[JsonPropertyName("name")]
		public string Name { get; set; }

		[JsonPropertyName("lat")]
		public double Latitude { get; set; }

		[JsonPropertyName("lon")]
		public double Longitude { get; set; }

		[JsonPropertyName("country")]
		public string Country { get; set; }

		[JsonPropertyName("state")]
		public string State { get; set; }
	}
}
