using Discord;
using Discord.WebSocket;
using RAE;
using System.Text;

namespace DiscordBot.Models.Games
{
	public class GameWords : Game
	{
		private const string URL_RAE = "https://definicionde.es/wp-content/uploads/2017/11/definicion-de-rae-min.jpg";
		private const int MIN_XP_LIVES = 1670;

		private ITextChannel _textChannel;
		private DiscordSocketClient _client;
		private Dictionary<ulong, Models.Games.Game> _games;
		private DRAE _rae = new();
		private int _lives = 5;

		private string _word;
		private IDefinition[] _definitions;

		public async void StartGameWords(Dictionary<ulong, Game> games, ITextChannel textChannel, DiscordSocketClient client)
		{
			_textChannel = textChannel;

			_games = games;

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

			IWord word = await _rae.FetchWordByIdAsync(responseWord.Id);

			_definitions = word.Definitions;

			await _textChannel.SendMessageAsync(embed: EmbedBuild());
		}

		private Embed EmbedBuild()
		{
			EmbedBuilder embed = Utilities.Builder;

			for (int i = 0; i < _definitions.Length; i++)
			{
				embed.AddField($"Definition {i + 1}", _definitions[i].Content.ToUpper());
			}

			embed.WithFooter(new EmbedFooterBuilder()
			{
				IconUrl = URL_RAE,
				Text = "rae.es"
			});

			return embed.Build();
		}

		private void Suscribe() => _client.MessageReceived += OnCommandRecivedAsync;

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
				await GameAsync(message);
			}
		}

		private async Task GameAsync(SocketUserMessage message)
		{
			string text;
			bool finish = false;
			string word = message.Content.ToUpper();

			if (word == _word)
			{
				int xpWon = MIN_XP_LIVES * _lives;
				IGuildUser user = message.Author as IGuildUser;

				text = $"***CONGRATULATIONS, {user.Mention} HAS GOT THE WORD RIGHT, HE RECEIVES: {xpWon}!***";

				XpLives(user, xpWon);

				Desuscribe();
			}
			else if (_lives == 1)
			{
				text = $"THE WORD WAS: ***{_word}***";

				Desuscribe();

			}
			else if (word == "GG")
			{
				text = $"YOU HAVE GIVEN UP :(, THE WORD WAS: ***{_word}***";

				Desuscribe();
			}
			else
			{
				text = $"WRONG WORD, HAS **{--_lives}** LIVES LEFT";

				finish = true;
			}

			if (!finish)
				_games.Remove(_textChannel.Id);

			await _textChannel.SendMessageAsync(text);
		}

		private void XpLives(IGuildUser user, int xpWon) => DatabaseFolder.Database.AddXP(user, xpWon);
	}
}