using System;
using System.Collections.Generic;

namespace GUT.Schedule
{
    public static class Data
    {
        public static List<(string Id, string Name)> Faculties { get; set; }
        public static List<(string Id, string Name)> Groups { get; set; }
        public static List<(string Id, string Name)> Calendars { get; set; }
        public static List<Subject> Schedule { get; set; }
        public static int FirstWeekDay { get; set; }
        public static DateTime StartDate { get; set; } = DateTime.Today;
        public static DateTime EndDate { get; set; } = DateTime.Today.AddDays(7);
        
        public static int Faculty { get; set; }
        public static int Group { get; set; }
        public static int Course { get; set; }
        public static int Calendar { get; set; }
        public static int Reminder { get; set; }
        public static bool AddTitle { get; set; }
    }
}