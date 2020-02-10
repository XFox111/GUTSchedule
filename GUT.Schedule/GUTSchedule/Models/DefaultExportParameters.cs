namespace GUTSchedule.Models
{
	public class DefaultExportParameters : ExportParameters
	{
		public string FacultyId { get; set; }
		public string GroupId { get; set; }
		public string Course { get; set; }
		public bool Session { get; set; }
	}
}