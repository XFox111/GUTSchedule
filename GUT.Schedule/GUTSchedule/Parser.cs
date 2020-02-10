using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using GUTSchedule.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace GUTSchedule
{
	public static class Parser
	{
		public static async Task<List<Subject>> LoadSchedule()
		{
			List<Subject> schedule = new List<Subject>();
			HttpClient client = new HttpClient();
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
				weeks = item.QuerySelector(".weeks").TextContent.Replace("(", "").Replace("н)", "").Replace(" ", "").Split(',');
				weekday = int.Parse(item.GetAttribute("weekday"));

				schedule.AddRange(Subject.GetSubject(name, type, professor, place, order, weeks, weekday, groupName));
			}

			return schedule;
		}

		public static async Task LoadFaculties()
		{
			Data.Faculties = new List<(string, string)>();
			HttpClient client = new HttpClient();
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
			if (string.IsNullOrWhiteSpace(responseBody))
				throw new NullReferenceException("Расписание на текущий семестр еще не объявлено");

			foreach (string s in responseBody.Split(';'))
				try { Data.Faculties.Add((s.Split(',')[0], s.Split(',')[1])); }
				catch { }
		}

		public static async Task LoadGroups(string facultyId, int course)
		{
			HttpClient client = new HttpClient();
			Dictionary<string, string> requestBody = new Dictionary<string, string>
			{
				{ "choice", "1" },
				{ "kurs", course.ToString() },
				{ "type_z", "1" },
				{ "faculty", facultyId },
				{ "schet", GetCurrentSemester() }
			};
			HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "https://cabinet.sut.ru/raspisanie_all_new.php")
			{
				Content = new FormUrlEncodedContent(requestBody)
			};
			request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

			HttpResponseMessage response = await client.SendAsync(request);
			string responseBody = await response.Content.ReadAsStringAsync();
			Data.Groups = new List<(string, string)>();
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

		public static async Task<List<CabinetSubject>> GetCabinetSchedule(HttpClient client, DateTime date, bool checkProfSchedule)
		{
			HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"https://cabs.itut.ru/cabinet/project/cabinet/forms/{(checkProfSchedule ? "pr_" : "")}raspisanie_kalendar.php");
			request.SetContent(
				("month", date.Month.ToString()),
				("year", date.Year.ToString()),
				("type_z", "0"));

			HttpResponseMessage response = await client.SendAsync(request).ConfigureAwait(false);
			string responseContent = await response.GetString().ConfigureAwait(false);

			if (!response.IsSuccessStatusCode)
				throw new HttpRequestException(responseContent);

			IHtmlDocument doc = new HtmlParser().ParseDocument(responseContent);
			List<CabinetSubject> schedule = new List<CabinetSubject>();

			if (!checkProfSchedule)
				Data.DataSet.Group = doc.QuerySelector(".style_gr b").TextContent;

			foreach (var i in doc.QuerySelectorAll("td").Where(i => i.GetAttribute("style") == "text-align: center; vertical-align: top"))
				for (int k = 0; k < i.QuerySelectorAll("i").Length; k++)
				{
					CabinetSubject item = new CabinetSubject(
						name: i.QuerySelectorAll("b")[k * 2 + 1].TextContent,
						type: i.QuerySelectorAll("i")[k].TextContent,
						cabinet: i.QuerySelectorAll("small")[k].NextSibling.TextContent.Replace("; Б22", ""),
						opponent: i.QuerySelectorAll("i")[k].NextSibling.NextSibling.NodeType == NodeType.Text ?
							i.QuerySelectorAll("i")[k].NextSibling.NextSibling.TextContent : "",
						year: date.Year,
						month: date.Month,
						day: int.Parse(i.ChildNodes[0].TextContent),
						schedule: i.QuerySelectorAll("b")[k * 2 + 2].TextContent,
						checkProfSchedule);
					schedule.Add(item);
				}

			// Merge duplicating entries
			schedule.OrderByDescending(i => i.StartTime);
			for (int k = 1; k < schedule.Count; k++)
				if (schedule[k - 1].StartTime == schedule[k].StartTime &&
					schedule[k - 1].Name == schedule[k].Name &&
					schedule[k - 1].Type == schedule[k].Type)
				{
					schedule[k - 1].Opponent += "\n" + schedule[k].Opponent;
					schedule[k - 1].Cabinet += "; " + schedule[k].Cabinet;
					schedule.RemoveAt(k--);
				}

			return schedule;
		}
	}
}