using System;

namespace GUTSchedule.Models
{
	public class CabinetSubject
	{
		public string Name { get; set; }
		public string Type { get; set; }
		public string Cabinet { get; set; }
		public string Order { get; set; }
		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }
		public string Opponent { get; set; }
		public bool ProfessorSchedule { get; set; }

		public CabinetSubject(string name, string type, string cabinet, string opponent, int year, int month, int day, string schedule, bool profSchedule)
		{
			Name = name;
			Type = type;
			Cabinet = cabinet;
			Opponent = opponent;
			ProfessorSchedule = profSchedule;

			string[] time = schedule.Split('-');

			StartTime = new DateTime(year, month, day, int.Parse(time[0].Split('.')[0]), int.Parse(time[0].Split('.')[1]), 0);
			EndTime = new DateTime(year, month, day, int.Parse(time[1].Split('.')[0]), int.Parse(time[1].Split('.')[1]), 0);
			switch (time[0])
			{
				case "09.00": Order = "1"; break;
				case "10.45": Order = "2"; break;
				case "13.00": Order = "3"; break;
				case "14.45": Order = "4"; break;
				case "16.30": Order = "5"; break;
				case "18.15": Order = "6"; break;
				case "20.00": Order = "7"; break;
				case "10.30": Order = "2"; break; //Расписание для пар по физ-ре
				case "12.00": Order = "3"; break;
				case "13.30": Order = "4"; break;
				case "15.00": Order = "5"; break;
				case "18.00": Order = "7"; break;
				default: Order = ""; break;
			}
		}
	}
}