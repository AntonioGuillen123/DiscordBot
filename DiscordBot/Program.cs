using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Transactions;

namespace DiscordBot
{
	public class Program
	{
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

			client.Log += Log;
			client.MessageReceived += OnMessageReceivedAsync;

			IServiceCollection servicies = new ServiceCollection()
				.AddSingleton(client)
				.AddSingleton<Player>();

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
				int commandPosition = 0;

				if (message.HasStringPrefix(PREFIX, ref commandPosition))
				{
					SocketCommandContext commandContext = new SocketCommandContext(client, message);
					IResult result = await commandService.ExecuteAsync(commandContext, commandPosition, serviceProvider);

					if (!result.IsSuccess)
					{
						string error = result.Error switch
						{
							CommandError.UnknownCommand => "No conozco ese comando",
							_ => "Otro error"
						};

						await commandContext.Channel.SendMessageAsync(error);
					}
				}
			}
		}

		private static Task Log(LogMessage msg)
		{
			Console.WriteLine(msg.ToString());
			return Task.CompletedTask;
		}
		private static void GetToken() => token = File.ReadAllText("tokenfile.txt");
	}
}