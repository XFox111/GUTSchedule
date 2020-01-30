using System;

namespace GUT.Schedule.Models
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

        public CabinetSubject(string name, string type, string cabinet, string opponent, int year, int month, int day, string schedule)
        {
            Name = name;
            Type = type;
            Cabinet = cabinet;
            Opponent = opponent;

            string[] time = schedule.Split('-');

            StartTime = new DateTime(year, month, day, int.Parse(time[0].Split('.')[0]), int.Parse(time[0].Split('.')[1]), 0);
            EndTime = new DateTime(year, month, day, int.Parse(time[1].Split('.')[0]), int.Parse(time[1].Split('.')[1]), 0);
            Order = time[0] switch 
            {
                "09.00" => "1",
                "10.45" => "2",
                "13.00" => "3",
                "14.45" => "4",
                "16.30" => "5",
                "18.15" => "6",
                "20.00" => "7",
                "10.30" => "2", //Расписание для пар по физ-ре
                "12.00" => "3",
                "13.30" => "4",
                "15.00" => "5",
                "18.00" => "7",
                _ => ""
            };
        }
    }
}