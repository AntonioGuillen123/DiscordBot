using DiscordBot.Models.VirusTotalFolder;
using System.Text.Json.Nodes;

namespace DiscordBot.Logic.Miscelaneous.Scanning;

public class VirusTotalAPI
{
    private const string BASE_URL = "https://www.virustotal.com/api/v3/";
    private static readonly TimeSpan DELAY = TimeSpan.FromSeconds(4);

    private HttpClient _httpClient;

    public VirusTotalAPI(string apiKey)
    {
        _httpClient = new HttpClient();
        _httpClient.BaseAddress = new Uri(BASE_URL);
        _httpClient.DefaultRequestHeaders.Add("x-apikey", apiKey);
    }

    public async Task<Analysis> ScanFileAsync(string fileUrl, string fileName, string password = null)
    {
        HttpResponseMessage response = await _httpClient.GetAsync(fileUrl);
        MultipartFormDataContent content = new MultipartFormDataContent()
        {
            { new StreamContent(response.Content.ReadAsStream()), "file", fileName }
        };

        if (!string.IsNullOrEmpty(password))
            content.Add(new StringContent(password), "password");

        JsonNode analysisNode = await ScanAsync("files", content);
        string id = (string)analysisNode["meta"]["file_info"]["sha256"];
        string guiUrl = $"https://www.virustotal.com/gui/file/{id}?nocache=1";

        return GetAnalysis(analysisNode, fileName, guiUrl);
    }

    public async Task<Analysis> ScanUrlAsync(string url)
    {
        FormUrlEncodedContent content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("url", url)
        });

        JsonNode analysisNode = await ScanAsync("urls", content);
        string id = (string)analysisNode["meta"]["url_info"]["id"];
        string guiUrl = $"https://www.virustotal.com/gui/url/{id}?nocache=1";

        return GetAnalysis(analysisNode, url, guiUrl);
    }

    private async Task<JsonNode> ScanAsync(string url, HttpContent content)
    {
        HttpResponseMessage response = await _httpClient.PostAsync(url, content);
        string requestJson = await response.Content.ReadAsStringAsync();
        string analysisUrl = (string)JsonNode.Parse(requestJson)["data"]["links"]["self"];

        JsonNode analysisNode;

        do
        {
            await Task.Delay(DELAY);
            string analysisJson = await _httpClient.GetStringAsync(analysisUrl);
            analysisNode = JsonNode.Parse(analysisJson);
        }
        while ((string)analysisNode["data"]["attributes"]["status"] != "completed");

        return analysisNode;
    }

    private Analysis GetAnalysis(JsonNode node, string element, string guiUrl)
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
}
