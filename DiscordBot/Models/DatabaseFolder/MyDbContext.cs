using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Models.DatabaseFolder
{
	public class MyDbContext : DbContext
	{
		private const string PATH = "spiderbot.db";

		public string DbPath { get; private set; }

		public DbSet<DataUser> Users { get; private set; }
		public DbSet<DataXP> XP { get; private set; }

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			string baseDir = AppDomain.CurrentDomain.BaseDirectory;

			optionsBuilder.UseSqlite($"DataSource={baseDir}{PATH}");
		}
	}
}
