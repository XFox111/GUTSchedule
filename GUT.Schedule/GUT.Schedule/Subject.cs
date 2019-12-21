using System;
using System.Collections.Generic;

namespace GUT.Schedule
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
            StartTime = StartTime.Add(TimeSpan.Parse(order switch
            {
                "1" => "9:00",
                "2" => "10:45",
                "3" => "13:00",
                "4" => "14:45",
                "5" => "16:30",
                "6" => "18:15",
                "7" => "20:00",
                "Ф1" => "9:00",   //Расписание для пар по физ-ре
                "Ф2" => "10:30",
                "Ф3" => "12:00",
                "Ф4" => "13:30",
                "Ф5" => "15:00",
                "Ф6" => "16:30",
                "Ф7" => "18:00",
                _ => "9:00"
            }));
            EndTime = StartTime + TimeSpan.FromMinutes(order.Contains("Ф") ? 90 : 95);
        }
    }
}