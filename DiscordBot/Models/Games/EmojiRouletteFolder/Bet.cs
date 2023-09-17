using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Models.Games.EmojiRouletteFolder
{
	public class Bet
	{
		public List<string> Emojis { get; set; }
		public int BetAmount { get; set; }
		public int InitialBetAmount { get; set; }
	}
}
