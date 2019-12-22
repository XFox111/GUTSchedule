using GUT.Schedule.Models;
using System;
using System.Collections.Generic;

namespace GUT.Schedule
{
    public static class Data
    {
        public static List<(string Id, string Name)> Faculties { get; set; }
        public static List<(string Id, string Name)> Groups { get; set; }
        public static int FirstWeekDay { get; set; }
        public static DateTime StartDate { get; set; } = DateTime.Today;
        public static DateTime EndDate { get; set; } = DateTime.Today.AddDays(7);

        /// <summary>
        /// Export parameters
        /// </summary>
        public static DataSet DataSet { get; set; }
    }
}