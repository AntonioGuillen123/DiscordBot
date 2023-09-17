using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Models.VirusTotalFolder
{
	public class Analysis
	{
		public string Element { get; set; }
		public DateTimeOffset DateTimeOffset { get; set; }
		public string GuiUrl { get; set; }
		public IList<string> ConfirmedTimeout { get; set; }
		public IList<string> Failure { get; set; }
		public IList<string> Harmless { get; set; }
		public IList<string> Undetected { get; set; }
		public IList<string> Suspicious { get; set; }
		public IList<string> Malicious { get; set; }
		public IList<string> TypeUnsupported { get; set; }
	}
}
