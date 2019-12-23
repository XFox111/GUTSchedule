using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using GUT.Schedule.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace GUT.Schedule
{
    public static class Parser
    {
        public static async Task<List<Subject>> LoadSchedule()
        {
            List<Subject> schedule = new List<Subject>();
            using HttpClient client = new HttpClient();
            Dictionary<string, string> requestBody = new Dictionary<string, string>
            {
                { "group_el", "0" },
                { "kurs", Data.DataSet.Course.ToString() },
                { "type_z", "1" },
                { "faculty", Data.DataSet.Faculty },
                { "group", Data.DataSet.Group },
                { "ok", "Показать" },
                { "schet", GetCurrentSemester() }
            };
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "https://cabinet.sut.ru/raspisanie_all_new")
            {
                Content = new FormUrlEncodedContent(requestBody)
            };
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

            HttpResponseMessage response = await client.SendAsync(request);

            IHtmlDocument doc = new HtmlParser().ParseDocument(await response.Content.ReadAsStringAsync());

            string groupName = Data.Groups.First(i => i.Id == Data.DataSet.Group).Name;

            IHtmlCollection<IElement> pairs = doc.QuerySelectorAll(".pair");
            foreach (IElement item in pairs)
            {
                string name, type, professor, place;
                int order, weekday;
                string[] weeks;

                name = item.QuerySelector(".subect strong")?.TextContent ?? "Неизвестный предмет (см. Расписание)";
                type = item.QuerySelector(".type").TextContent.Replace("(", "").Replace(")", "");
                professor = item.QuerySelector(".teacher")?.GetAttribute("title").Replace(";", "") ?? "";
                place = item.QuerySelector(".aud")?.TextContent ?? "СПбГУТ";
                order = int.Parse(item.GetAttribute("pair")) - 1;
                weeks = item.QuerySelector(".weeks").TextContent.Replace("(", "").Replace("н)", "").Split(", ");
                weekday = int.Parse(item.GetAttribute("weekday"));

                schedule.AddRange(Subject.GetSubject(name, type, professor, place, order, weeks, weekday, groupName));
            }

            return schedule;
        }

        public static async Task LoadFaculties()
        {
            Data.Faculties = new List<(string, string)>();
            using HttpClient client = new HttpClient();
            Dictionary<string, string> requestBody = new Dictionary<string, string>
            {
                { "choice", "1" },
                { "kurs", "0" },
                { "type_z", "1" },
                { "schet", GetCurrentSemester() }
            };
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "https://cabinet.sut.ru/raspisanie_all_new.php")
            {
                Content = new FormUrlEncodedContent(requestBody)
            };
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

            HttpResponseMessage response = await client.SendAsync(request);
            string responseBody = await response.Content.ReadAsStringAsync();
            foreach (string s in responseBody.Split(';'))
                try { Data.Faculties.Add((s.Split(',')[0], s.Split(',')[1])); }
                catch { }
        }

        public static async Task LoadGroups(string facultyId, int course)
        {
            Data.Groups = new List<(string, string)>();
            using HttpClient client = new HttpClient();
            Dictionary<string, string> requestBody = new Dictionary<string, string>
            {
                { "choice", "1" },
                { "kurs", course.ToString() },
                { "type_z", "1" },
                { "faculty", facultyId },
                { "schet", GetCurrentSemester() }
            };
            using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "https://cabinet.sut.ru/raspisanie_all_new.php")
            {
                Content = new FormUrlEncodedContent(requestBody)
            };
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

            HttpResponseMessage response = await client.SendAsync(request);
            string responseBody = await response.Content.ReadAsStringAsync();
            foreach (string s in responseBody.Split(';'))
                try { Data.Groups.Add((s.Split(',')[0], s.Split(',')[1])); }
                catch { }
        }

        static string GetCurrentSemester()
        {
            DateTime now = DateTime.Today;

            if (now.Month > 8)
                return $"205.{now.Year - 2000}{now.Year - 1999}/1";
            else
                return $"205.{now.Year - 2001}{now.Year - 2000}/2";
        }

    }
}