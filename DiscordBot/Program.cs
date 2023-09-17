using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordBot.Models;
using DiscordBot.Models.DatabaseFolder;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace DiscordBot
{
	public class Program
	{
		//public const string PREFIX = "/";
		public const string PREFIX = "s!";

		public static string token;
		public static CommandService commandService;
		public static IServiceProvider serviceProvider;
		public static DiscordSocketClient client;

		public static async Task Main()
		{
			GetToken();

			commandService = new CommandService(new CommandServiceConfig
			{
				CaseSensitiveCommands = false
			});

			client = new DiscordSocketClient(new DiscordSocketConfig
			{
				GatewayIntents = GatewayIntents.All
			});

			client.Log += OnLogAsync;
			client.MessageReceived += OnMessageReceivedAsync;
			client.Ready += Database.OnInsertUsersAsync;
			client.UserJoined += Database.OnNewUserGuild;
			//client.UserLeft += Database.DeleteUserGuild;

			IServiceCollection servicies = new ServiceCollection()
				.AddSingleton(client)
				.AddSingleton<Dictionary<ulong, Player>>()
				.AddSingleton(new Dictionary<ulong, Models.Games.Game>())
				.AddSingleton<Weather>();

			serviceProvider = servicies.BuildServiceProvider();

			commandService.AddModulesAsync(Assembly.GetExecutingAssembly(), serviceProvider);

			await client.LoginAsync(TokenType.Bot, token);
			await client.StartAsync();

			await Task.Delay(-1);
		}
		private static async Task OnMessageReceivedAsync(SocketMessage arg)
		{
			SocketUserMessage message = arg as SocketUserMessage;

			if (message != null)
			{
				await Database.MessageXPAdd(message);

				int commandPosition = 0;

				if (message.HasStringPrefix(PREFIX, ref commandPosition))
				{
					SocketCommandContext commandContext = new SocketCommandContext(client, message);
					IResult result = await commandService.ExecuteAsync(commandContext, commandPosition, serviceProvider);

					if (!result.IsSuccess)
					{
						string error = result.Error switch
						{
							CommandError.UnknownCommand => "I DOESN´T KNOW THIS COMMAND",
							_ => "OTHER ERROR"
						};

						await commandContext.Channel.SendMessageAsync(error);
					}
				}
				else if (message.MentionedUsers.Select(user => user.Id).Contains(client.CurrentUser.Id))
				{
					await message.Channel.SendMessageAsync("KLK");
				}
			}
		}

		private static Task OnLogAsync(LogMessage msg)
		{
			Console.WriteLine(msg.ToString());
			return Task.CompletedTask;
		}
		private static void GetToken() => token = File.ReadAllText("tokenfile.txt");
	}
}