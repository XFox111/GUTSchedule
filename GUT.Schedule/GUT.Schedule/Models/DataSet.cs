using System.Net.Http;

namespace GUT.Schedule.Models
{
    public class DataSet
    {
        public string Calendar { get; set; }
        public string Faculty { get; set; }
        public int Course { get; set; }
        public string Group { get; set; }
        public int Reminder { get; set; }
        public bool AddGroupToTitle { get; set; }
        public HttpClient HttpClient { get; set; }
    }
}