using Octokit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot
{
	public class Github
	{
		public async void StartGithub()
		{
			var client = new GitHubClient(new ProductHeaderValue("SpiderBottt"));

			//var user = await client.User.Get();
		}
	}
}
