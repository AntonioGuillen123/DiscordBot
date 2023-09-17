using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DiscordBot.Models.WeatherFolder
{
	public class WindWeather
	{
		[JsonPropertyName("speed")]
		public double Speed { get; set; }
	}
}
