using System;

namespace GUTSchedule.Models
{
	public class Occupation
	{
		public string Name { get; set; }
		public string Type { get; set; }
		public string Cabinet { get; set; }
		public string Order { get; set; }
		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }
		public string Opponent { get; set; }

		public string Group { get; set; }
	}
}