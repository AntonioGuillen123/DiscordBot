using AngleSharp.Text;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using RAE;
using System.Text;
using System.Text.RegularExpressions;

namespace DiscordBot.Models
{
	public class GameWords
	{
		private const string URL_RAE = "https://definicionde.es/wp-content/uploads/2017/11/definicion-de-rae-min.jpg";

		private ITextChannel _textChannel;
		private DiscordSocketClient _client;
		private DRAE _rae = new();
		private int _lives = 5;

		private string _word;
		private string[] _definitions;

		public async void StartGameWords(ITextChannel textChannel, DiscordSocketClient client)
		{
			_textChannel = textChannel;

			if (_client == null)
			{
				_client = client;

				GenerateDefinition();

				Suscribe();
			}
		}

		private async void GenerateDefinition()
		{
			var responseWord = await _rae.GetRandomWordAsync();

			_word = responseWord.Content.Normalize(NormalizationForm.FormD).Split(',')[0].ToUpper();

			_definitions = await _rae.FetchWordByIdAsync(responseWord.Id);

			await _textChannel.SendMessageAsync(embed: EmbedBuild());
		}

		private Embed EmbedBuild()
		{
			EmbedBuilder embed = Utilities.Builder;

			for (int i = 0; i < _definitions.Length; i++)
			{
				embed.AddField($"Definition {i + 1}", _definitions[i].Substring(2).ToUpper());
			}

			embed.WithFooter(new EmbedFooterBuilder()
			{
				IconUrl = URL_RAE,
				Text = "rae.es"
			});

			return embed.Build();
		}

		private void Suscribe()
		{
			_client.MessageReceived += OnCommandRecivedAsync;
		}

		private void Desuscribe()
		{
			_client.MessageReceived -= OnCommandRecivedAsync;
			_lives = 5;
		}

		private async Task OnCommandRecivedAsync(SocketMessage arg)
		{
			SocketUserMessage message = arg as SocketUserMessage;
			ITextChannel textChannel = message.Channel as ITextChannel;

			if (message != null && textChannel == _textChannel && message.Author.IsBot == false)
			{
				await GameAsync(message.Content);
			}
		}

		private async Task GameAsync(string message)
		{
			string text;

			if (message.ToUpper() == _word)
			{
				text = "CONGRATULATIONS YOU HAVE GOT THE WORD RIGHT!";

				Desuscribe();
			}
			else if (_lives == 1)
			{
				text = $"THE WORD WAS: ***{_word}***";

				Desuscribe();

			}else if (message.ToUpper() == "GG")
			{
				text = $"YOU HAVE GIVEN UP :(, THE WORD WAS: ***{_word}***";

				Desuscribe();
			}
			else
			{
				text = $"WRONG WORD, HAS **{--_lives}** LIVES LEFT";
			}

			await _textChannel.SendMessageAsync(text);
		}
	}
}
