using ImageChartsLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Models.VirusTotalFolder
{
	public class GraphicVirusCreator
	{
		private Analysis _analysis;
		private string _fileName;

		public GraphicVirusCreator(Analysis analysis, string fileName)
		{
			_analysis = analysis;
			_fileName = fileName;
		}

		public void CreateChart()
		{
			ImageCharts pie = new ImageCharts();

			pie.chco("3DA224,FAC100,FB0107");
			pie.chd($"t:{_analysis.Undetected.Count},{_analysis.Suspicious.Count},{_analysis.Malicious.Count}");
			pie.chdl("Inofensive|Suspicious|Malicious");
			pie.chdlp("l");
			pie.chdls("ffffff");
			pie.chf("a,s,00000000");
			pie.chma("10");
			pie.chs("250x200");
			pie.cht("pd");
			pie.chxt("x,y");

			pie.toFile(_fileName);
		}
	}
}
