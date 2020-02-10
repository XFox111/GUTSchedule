using System;
using System.Collections.Generic;

namespace GUTSchedule.Models
{
	public class Subject
	{
		public string Name { get; set; }
		public string Type { get; set; }
		public string Professor { get; set; }
		public string[] Cabinets { get; set; }
		public string Order { get; set; }
		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }
		public string Group { get; set; }

		public static List<Subject> GetSubject(string name, string type, string professor, string place, int order, string[] weeks, int weekday, string group)
		{
			List<Subject> subjects = new List<Subject>();
			string[] cabinets = place.Replace("ауд.: ", "").Replace("; Б22", "").Split(';');
			string pair = order < 10 ? order.ToString() : $"Ф{order - 81}";

			foreach (string week in weeks)
				subjects.Add(new Subject(name, type, professor, cabinets, pair, int.Parse(week), weekday, group));

			return subjects;
		}

		public Subject(string name, string type, string prof, string[] cabs, string order, int week, int weekday, string group)
		{
			Name = name;
			Type = type;
			Professor = prof;
			Cabinets = cabs;
			Order = order;
			Group = group;

			StartTime = Extensions.GetDateFromWeeks(week, weekday);
			string rawTime;
			switch (order)
			{
				case "1": rawTime = "9:00"; break;
				case "2": rawTime = "10:45"; break;
				case "3": rawTime = "13:00"; break;
				case "4": rawTime = "14:45"; break;
				case "5": rawTime = "16:30"; break;
				case "6": rawTime = "18:15"; break;
				case "7": rawTime = "20:00"; break;
				case "Ф1": rawTime = "9:00"; break;   //Расписание для пар по физ-ре
				case "Ф2": rawTime = "10:30"; break;
				case "Ф3": rawTime = "12:00"; break;
				case "Ф4": rawTime = "13:30"; break;
				case "Ф5": rawTime = "15:00"; break;
				case "Ф6": rawTime = "16:30"; break;
				case "Ф7": rawTime = "18:00"; break;
				default: rawTime = "9:00"; break;
			}
			StartTime = StartTime.Add(TimeSpan.Parse(rawTime));
			EndTime = StartTime + TimeSpan.FromMinutes(order.Contains("Ф") ? 90 : 95);
		}
	}
}