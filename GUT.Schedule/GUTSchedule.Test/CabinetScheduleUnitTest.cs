using NUnit.Framework;
using System.Threading.Tasks;
using GUTSchedule.Models;
using System;

namespace GUTSchedule.Test
{
	public class CabinetScheduleUnitTest
	{
		[Test]
		public async Task ScheduleListTest()
		{
			var list = await Parser.GetSchedule(new CabinetExportParameters
			{
				Email = "%EMAIL%",
				Password = "%PASSWORD%",
				EndDate = DateTime.Today.AddDays(7),
				StartDate = DateTime.Today
			});

			Assert.IsNotNull(list);
			Assert.IsTrue(list.Count > 0);

			
			Console.WriteLine("Events list:");
			foreach (var i in list)
			{
				Console.WriteLine("--------------------------------------------------");
				Console.WriteLine($"[{i.Group}] {i.Order}. {i.Name} ({i.Type})");
				Console.WriteLine(i.Cabinet);
				Console.WriteLine(i.StartTime.ToShortDateString());
				Console.WriteLine($"{i.StartTime.ToShortTimeString()}-{i.EndTime.ToShortTimeString()}");
				Console.WriteLine(i.Opponent);
			}
		}
	}
}