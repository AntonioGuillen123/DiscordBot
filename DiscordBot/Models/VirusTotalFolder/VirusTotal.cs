using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Mail;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace DiscordBot.Models.VirusTotalFolder
{
	public static class VirusTotal
	{
		private static readonly string BASE_URL = "https://www.virustotal.com/api/v3/";
		private static readonly string TOKEN_PATH = "virustotaltokenfile.txt";
		private static readonly TimeSpan DELAY = TimeSpan.FromSeconds(4);

		private static string _token;

		private static ITextChannel _textChannel;
		private static Discord.Attachment _file;
		private static HttpClient _httpClient;

		static VirusTotal() => GetToken();

		public static async void StartVirus(ITextChannel textChannel, Discord.Attachment file, string password)
		{
			_textChannel = textChannel;

			_file = file;

			var message = await _textChannel.SendMessageAsync("***ANALIZING...*** :mag_right: :bug:");

			FillHttpClient();

			Analysis analysis = await FetchFileAsync(password);

			string fileName = $"analisys{analysis.Element}.png";

			GraphicVirusCreator graphic = new GraphicVirusCreator(analysis, fileName);

			graphic.CreateChart();

			Embed embedd = await EmbedBuild(analysis, fileName);

			await _textChannel.SendFileAsync(fileName, embed: embedd);

			await message.DeleteAsync();

			File.Delete(fileName);
		}

		private static void FillHttpClient()
		{
			_httpClient = new HttpClient();
			_httpClient.BaseAddress = new Uri(BASE_URL);
			_httpClient.DefaultRequestHeaders.Add("x-apikey", _token);
		}

		private static async Task<Analysis> FetchFileAsync(string password)
		{
			HttpResponseMessage response = await _httpClient.GetAsync(_file.Url);
			
			MultipartFormDataContent content = new MultipartFormDataContent()
			{
				{new StreamContent(response.Content.ReadAsStream()), "file", _file.Filename }
			};

			if (!string.IsNullOrEmpty(password))
				content.Add(new StringContent(password), "password");

			JsonNode analysisNode = await ScanAsync("files", content);
			string idAnalysis = analysisNode["meta"]["file_info"]["sha256"].ToString();
			string guiUrl = $"https://www.virustotal.com/gui/file/{idAnalysis}?nocache=1";

			return GetAnalysis(analysisNode, _file.Filename, guiUrl);
		}

		private static async Task<JsonNode> ScanAsync(string url, HttpContent content)
		{
			HttpResponseMessage response = await _httpClient.PostAsync(url, content);
			string requestJson = await response.Content.ReadAsStringAsync();
			string analysisUrl = JsonNode.Parse(requestJson)["data"]["links"]["self"].ToString();

			JsonNode analysisNode;

			do
			{
				await Task.Delay(DELAY);
				string analysisJson = await _httpClient.GetStringAsync(analysisUrl);
				analysisNode = JsonNode.Parse(analysisJson);
			}
			while (analysisNode["data"]["attributes"]["status"].ToString() != "completed");

			return analysisNode;
		}

		private static Analysis GetAnalysis(JsonNode node, string element, string guiUrl)
		{
			JsonNode dataNode = node["data"];
			JsonNode attributesNode = dataNode["attributes"];

			Analysis result = new Analysis()
			{
				Element = element,
				DateTimeOffset = DateTimeOffset.FromUnixTimeSeconds((long)attributesNode["date"]),
				GuiUrl = guiUrl,
				ConfirmedTimeout = new List<string>(),
				Failure = new List<string>(),
				Harmless = new List<string>(),
				Undetected = new List<string>(),
				Suspicious = new List<string>(),
				Malicious = new List<string>(),
				TypeUnsupported = new List<string>()
			};

			JsonNode resultsNode = attributesNode["results"];

			foreach (KeyValuePair<string, JsonNode> properties in resultsNode.AsObject())
			{
				IList<string> list = (string)properties.Value["category"] switch
				{
					"confirmed-timeout" => result.ConfirmedTimeout,
					"failure" => result.Failure,
					"harmless" => result.Harmless,
					"undetected" => result.Undetected,
					"suspicious" => result.Suspicious,
					"malicious" => result.Malicious,
					_ => result.TypeUnsupported
				};

				list.Add(properties.Key);
			}

			return result;
		}

		private static async Task<Embed> EmbedBuild(Analysis analysis, string fileName)
		{
			EmbedBuilder builder = Utilities.Builder;

			StringBuilder sb = new StringBuilder();

			sb.AppendLine($":white_check_mark: INOFENSIVE: {analysis.Undetected.Count}");
			sb.AppendLine($":warning: SUSPICIOUS: {analysis.Suspicious.Count}");
			sb.AppendLine($":name_badge: MALICIOUS: {analysis.Malicious.Count}");

			builder.WithTitle("Analisys File");
			builder.AddField("ANALYZED ELEMENT", _file.Filename);
			builder.AddField("RESULTS", sb.ToString());
			builder.WithImageUrl($"attachment://{fileName}");

			builder.WithFooter(new EmbedFooterBuilder()
			{
				IconUrl = "https://pbs.twimg.com/card_img/1699811253646946304/BhgMcMbA?format=png&name=small",
				Text = "VirusTotal"
			});

			return builder.Build();
		}

		private static void GetToken() => _token = File.ReadAllText(TOKEN_PATH);
	}
}
