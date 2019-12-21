using System;
using System.Collections.Generic;

namespace GUT.Schedule
{
    public static class Data
    {
        public static List<(string Id, string Name)> Faculties { get; set; }
        public static List<(string Id, string Name)> Groups { get; set; }
        public static List<(string Id, string Name)> Calendars { get; set; }
        public static DateTime StartDate { get; set; } = DateTime.Today;
        public static DateTime EndDate { get; set; } = DateTime.Today.AddDays(7);
        
        public static (int faculty, int group, int calendar, int reminder) ExportData { get; set; }
    }
}