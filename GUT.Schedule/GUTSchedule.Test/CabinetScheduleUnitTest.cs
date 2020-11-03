using NUnit.Framework;
using System.Threading.Tasks;
using GUTSchedule.Models;
using System;
using Newtonsoft.Json;
using System.IO;
using Newtonsoft.Json.Linq;

namespace GUTSchedule.Test
{
	public class CabinetScheduleUnitTest
	{
		[Test]
		public async Task ScheduleListTest()
		{
			JObject secrets = JsonConvert.DeserializeObject(File.ReadAllText(Directory.GetCurrentDirectory() + "\\TestCredential.json")) as JObject;
			var list = await Parser.GetSchedule(new CabinetExportParameters
			{
				Email = secrets["testEmail"].ToObject<string>(),
				Password = secrets["testPassword"].ToObject<string>(),
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

		[Test]
		public async Task OccupationsCheckTest()
		{
			JObject secrets = JsonConvert.DeserializeObject(File.ReadAllText(Directory.GetCurrentDirectory() + "\\TestCredential.json")) as JObject;
			var list = await Parser.CheckAvailableOccupations(secrets["testEmail"].ToObject<string>(), secrets["testPassword"].ToObject<string>());

			Assert.IsNotNull(list);
			if (list.Count < 1)
			{
				Assert.Warn("No available occupations");
				return;
			}

			Console.WriteLine("Available occupations:");
			list.ForEach(i => Console.WriteLine($"{i.Item1} / {i.Item2}"));
		}

		[Test]
		public async Task ApplyForOccupationsTest()
		{
			JObject secrets = JsonConvert.DeserializeObject(File.ReadAllText(Directory.GetCurrentDirectory() + "\\TestCredential.json")) as JObject;
			var list = await Parser.CheckAvailableOccupations(secrets["testEmail"].ToObject<string>(), secrets["testPassword"].ToObject<string>());

			Assert.IsNotNull(list);
			if (list.Count < 1)
			{
				Assert.Warn("No available occupations to test");
				return;
			}

			Console.WriteLine("Available occupations:");
			list.ForEach(i => Console.WriteLine($"{i.Item1} / {i.Item2}"));

			await Parser.ApplyForOccupations(secrets["testEmail"].ToObject<string>(), secrets["testPassword"].ToObject<string>(), list);
		}
	}
}