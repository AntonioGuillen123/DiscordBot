using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Models.Amazon
{
	public class AmazonProduct
	{
		public string UrlProduct { get; set; }
		public string Image { get; set; }
		public string Name { get; set; }
		public double Price { get; set; }
		public int Reviews { get; set; }

	}
}
