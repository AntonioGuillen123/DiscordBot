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
		
		public string PATH = "database.db";

		private string _guild;

		public string DbPath { get; private set; }

		public DbSet<DataUser> Users { get; private set; }
		public DbSet<DataXP> XP { get; private set; }

		public MyDbContext(string guild)
		{
			_guild = guild;

			PATH = $"{guild}{PATH}".Replace(" ", "");
		}

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			var dataPath = $"DataSource={DatabaseFolder.Database.DATABASE_PATH}{_guild}/{PATH}".Replace(" ", "");

			optionsBuilder.UseSqlite(dataPath);
		}
	}
}
