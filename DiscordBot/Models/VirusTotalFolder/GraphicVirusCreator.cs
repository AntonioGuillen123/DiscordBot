using ImageChartsLib;

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
			ImageCharts pie = new ImageCharts()
				.chco("3DA224,FAC100,FB0107")
				.chd($"t:{_analysis.Undetected.Count},{_analysis.Suspicious.Count},{_analysis.Malicious.Count}")
				.chdl("Inofensive|Suspicious|Malicious")
				.chdlp("l")
				.chdls("ffffff")
				.chf("a,s,00000000")
				.chma("10")
				.chs("250x200")
				.cht("pd")
				.chxt("x,y");

			pie.toFile(_fileName);
		}
	}
}
