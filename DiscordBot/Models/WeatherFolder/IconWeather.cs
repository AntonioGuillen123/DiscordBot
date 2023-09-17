using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DiscordBot.Models.WeatherFolder
{
	public class IconWeather
	{
		private string _icon;

		[JsonPropertyName("icon")]
		public string Icon { get => _icon; set => _icon = $"http://openweathermap.org/img/w/{value}.png"; }
	}
}
