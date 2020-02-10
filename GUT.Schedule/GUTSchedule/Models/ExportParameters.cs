using System;

namespace GUTSchedule.Models
{
	public abstract class ExportParameters
	{
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
	}
}