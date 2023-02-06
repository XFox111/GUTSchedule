using System;
using System.Linq;
using System.Threading.Tasks;
using GUTSchedule.Models;
using NUnit.Framework;

namespace GUTSchedule.Test
{
	public class AnonymousScheduleUnitTest
	{
		[Test]
		public async Task FacultiesListTest()
		{
			var list = await Parser.GetFaculties();
			Assert.IsNotNull(list);
			if (list.Count < 1)
				Assert.Warn("No faculties available");

			Console.WriteLine("Faculties list:");
			list.ForEach(i =>
				Console.WriteLine($"{i.name} ({i.id})"));
		}

		[Test]
		public async Task GroupListTest()
		{
			var faculties = await Parser.GetFaculties();
			if (faculties == null || faculties.Count < 1)
			{
				Assert.Warn("No faculties found");
				return;
			}

			var (id, name) = faculties[new Random().Next(0, faculties.Count)];
			Console.WriteLine($"Randomly selected faculty: {name} ({id})");

			var list = await Parser.GetGroups(id);
			Assert.IsNotNull(list);
			Assert.IsTrue(list.Count > 0);

			Console.WriteLine("Groups list:");
			list.ForEach(i =>
				Console.WriteLine($"{i.name} ({i.id})"));
		}

		[Test]
		public async Task ScheduleListTest()
		{
			var faculties = await Parser.GetFaculties();
			if (faculties == null || faculties.Count < 1)
			{
				Assert.Warn("No faculties found");
				return;
			}
			var faculty = faculties[new Random().Next(0, faculties.Count)];
			Console.WriteLine($"Randomly selected faculty: {faculty.name} ({faculty.id})");

			var groups = await Parser.GetGroups(faculty.id);
			if (groups == null || groups.Count < 1)
			{
				Assert.Warn("No groups found");
				return;
			}
			var group = groups[new Random().Next(0, groups.Count)];
			Console.WriteLine($"Randomly selected group: {group.name} ({group.id})");

			try
			{
				var list = await Parser.GetSchedule(new DefaultExportParameters
				{
					Course = "0",
					FacultyId = faculty.id,
					GroupId = group.id,
					EndDate = DateTime.Today.AddDays(7),
					StartDate = DateTime.Today
				});

				Assert.IsNotNull(list);
				Assert.IsTrue(list.Count > 0);

				Console.WriteLine("Events list:");
				foreach (var i in list.OrderBy(i => i.StartTime))
				{
					Console.WriteLine("--------------------------------------------------");
					Console.WriteLine($"[{i.Group}] {i.Order}. {i.Name} ({i.Type})");
					Console.WriteLine(i.Cabinet);
					Console.WriteLine(i.StartTime.ToShortDateString());
					Console.WriteLine($"{i.StartTime.ToShortTimeString()}-{i.EndTime.ToShortTimeString()}");
					Console.WriteLine(i.Opponent);
				}
			}
			catch (NullReferenceException e)
			{
				Assert.Warn(e.Message);
			}
		}
	}
}
