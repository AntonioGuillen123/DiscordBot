using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata;

namespace DiscordBot.Models.DatabaseFolder
{
	public class DataXP
	{
		public int Id { get; set; }
		public int EXP { get; set; }
		public int Level { get; set; }
		public int LastXPAdd { get; set; }


		public void AddXP(int xp) 
		{
			LastXPAdd = xp;
			EXP += xp;
		}

		public void RestXP(int xp)
		{ 
			int newXP = EXP - xp;

			EXP = newXP >= 0 ? newXP : 0;
		}

		public static bool operator >=(DataXP xp1, DataXP xp2) => xp1.EXP >= xp2.EXP;
		public static bool operator <=(DataXP xp1, DataXP xp2) => xp1.EXP <= xp2.EXP;
	}
}
