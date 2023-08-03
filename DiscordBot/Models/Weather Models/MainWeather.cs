using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DiscordBot.Models.Weather_Models
{
	public class MainWeather
	{
		[JsonPropertyName("temp")]
		public double Temp { get; set; }
	}
}
