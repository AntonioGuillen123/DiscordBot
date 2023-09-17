using Discord;
using Discord.WebSocket;
using Microsoft.Playwright;

namespace DiscordBot.Models.Amazon
{
	public class AmazonScrapper
	{
		private const string COOKIES = "#sp-cc-accept";
		private const string SEARCH = "[name=\"field-keywords\"]";
		private const string ICON_URL = "https://cdn.icon-icons.com/icons2/91/PNG/512/amazon_16438.png";

		private DiscordSocketClient _client;
		private ITextChannel _textChannel;
		private IPlaywright _playWright;
		private IBrowser _browser;

		public async void StartAmazon(ITextChannel textChannel, DiscordSocketClient client, string input, IUserMessage message)
		{
			_client = client;
			_textChannel = textChannel;

			List<AmazonProduct> amazonProducts = await GetProducts(await Scrapper(input));

			AmazonProduct bestProduct = amazonProducts.OrderByDescending(item => item.Reviews).FirstOrDefault();

			await _textChannel.SendMessageAsync(embed: EmbedBuild(bestProduct));

			await message.DeleteAsync();
		}

		private async Task<IReadOnlyList<IElementHandle>> Scrapper(string input)
		{
			Microsoft.Playwright.Program.Main(new[] { "install" });

			_playWright = await Playwright.CreateAsync();

			_browser = await _playWright.Chromium.LaunchAsync(new() { Headless = true });

			var page = await _browser.NewPageAsync();

			await page.GotoAsync("https://www.amazon.es/");

			var accept = await page.QuerySelectorAsync(COOKIES);

			if (accept != null)
				await AcceptCookies(page);

			var search = await page.QuerySelectorAsync(SEARCH);

			await search.TypeAsync($"{input}\n");

			Thread.Sleep(1500);

			return await page.QuerySelectorAllAsync("[data-component-type=\"s-search-result\"]");
		}

		private Embed EmbedBuild(AmazonProduct product)
		{
			var builder = Utilities.Builder;

			builder.WithTitle(product.Name);
			builder.AddField("Price", $"{product.Price} €", true);
			builder.AddField("Nº Reviews", product.Reviews, true);
			builder.WithUrl(product.UrlProduct);
			builder.WithImageUrl(product.Image);
			builder.WithFooter(new EmbedFooterBuilder()
			{
				IconUrl = ICON_URL,
				Text = "amazon.es"
			});

			return builder.Build();
		}

		private async Task<List<AmazonProduct>> GetProducts(IReadOnlyList<IElementHandle> containers)
		{
			List<AmazonProduct> newList = new();

			foreach (var container in containers)
			{
				newList.Add(await FillProductData(container));
			}

			await _browser.DisposeAsync();
			_playWright.Dispose();

			return newList;
		}

		private async Task<AmazonProduct> FillProductData(IElementHandle container)
		{
			int review = 0;
			double price = 0;

			var urlContainer = await container.QuerySelectorAsync("span[data-component-type=\"s-product-image\"] > a");
			string urlProduct = $"https://www.amazon.es{await urlContainer.GetAttributeAsync("href")}";

			var imageContainer = await container.QuerySelectorAsync("img");
			string image = await imageContainer.GetAttributeAsync("src");
			string name = (await imageContainer.GetAttributeAsync("alt")).Replace("Anuncio patrocinado: ", "");

			var priceContainer = await container.QuerySelectorAsync(".a-price-whole");

			if (priceContainer != null)
				price = double.Parse(await priceContainer.InnerHTMLAsync());

			var reviewContainer = await container.QuerySelectorAsync("a.s-link-style > span.s-underline-text");

			if (reviewContainer != null)
			{
				var numberHtml = await reviewContainer.InnerTextAsync();

				review = int.Parse(numberHtml.Replace(".", ""));
			}

			return new AmazonProduct()
			{
				UrlProduct = urlProduct,
				Name = name,
				Image = image,
				Price = price,
				Reviews = review
			};
		}

		private async Task AcceptCookies(IPage page)
		{
			ILocator accepCookies = page.Locator(COOKIES);

			await accepCookies.WaitForAsync(new() { Timeout = 3000 });

			await accepCookies.ClickAsync();
		}
	}
}