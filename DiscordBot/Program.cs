using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;
using System.Transactions;

namespace DiscordBot
{
	internal class Program
	{
		static CommandService commandService;
		static IServiceProvider serviceProvider;
		static DiscordSocketClient client;

		public static async Task Main()
		{
			commandService = new CommandService(new CommandServiceConfig
			{
				CaseSensitiveCommands = false
			});

			client = new DiscordSocketClient(new DiscordSocketConfig
			{
				GatewayIntents = GatewayIntents.All
			});

			client.Log += Log;
			client.MessageReceived += OnMessageReceived;

			IServiceCollection servicies = new ServiceCollection()
				.AddSingleton(client)
				.AddSingleton<Player>();

			serviceProvider = servicies.BuildServiceProvider();

			commandService.AddModulesAsync(Assembly.GetExecutingAssembly(), serviceProvider);

			var token = "MTEyMTA3OTYxNjIxNjQ5ODIzNw.GCGK8O.D1Dr-BXascJxwUzTTG3-jtCZj9U1aHmg0eL-aY";

			await client.LoginAsync(TokenType.Bot, token);
			await client.StartAsync();

			await Task.Delay(-1);
		}

		private static async Task OnMessageReceived(SocketMessage arg)
		{
			SocketUserMessage message = arg as SocketUserMessage;

			if (message != null)
			{
				int commandPosition = 0;

				if (message.HasStringPrefix("gg", ref commandPosition))
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
	}
}