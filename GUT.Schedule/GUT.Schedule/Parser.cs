using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace GUT.Schedule
{
    public static class Parser
    {
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