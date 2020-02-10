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
	public enum ScheduleType
	{
		Default = 1,
		Session = 2
	}

	public static class Parser
	{
		public static async Task VaildateAuthorization(string email, string password)
		{
			if (string.IsNullOrWhiteSpace(email))
				throw new ArgumentNullException(nameof(email));
			if (string.IsNullOrWhiteSpace(password))
				throw new ArgumentNullException(nameof(password));

			HttpClient client = new HttpClient();

			await client.GetAsync("https://cabs.itut.ru/cabinet/");

			HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "https://cabs.itut.ru/cabinet/lib/autentificationok.php");
			request.SetContent(
				("users", email),
				("parole", password));

			HttpResponseMessage response = await client.SendAsync(request);
			string responseContent = await response.GetString();

			if (!response.IsSuccessStatusCode)
				throw new HttpRequestException($"{response.StatusCode} ({response.ReasonPhrase}): {responseContent}");

			if (!responseContent.StartsWith("1", StringComparison.OrdinalIgnoreCase))
			{
				Dictionary<string, string> responseQuery = new Dictionary<string, string>();
				foreach (string i in responseContent.Split('&'))
					responseQuery.Add(i.Split('=')[0], i.Split('=')[1]);

				throw new System.Security.VerificationException(responseQuery["error"].Replace("|", "; "));
			}
		}

		public static async Task<List<Occupation>> GetSchedule(ExportParameters exportParameters)
		{
			List<Occupation> schedule = new List<Occupation>();

			if (exportParameters is CabinetExportParameters)
			{

			}
			else if (exportParameters is DefaultExportParameters arg)
			{
				if (arg.Session)
					schedule.AddRange(await GetSessionSchedule());
				else
				{
					int offsetDay = int.Parse(await new HttpClient().GetStringAsync("https://xfox111.net/schedule_offset.txt"));
					schedule.AddRange(await GetRegularSchedule(offsetDay, arg.FacultyId, arg.Course, arg.GroupId));
				}
			}
			else
				throw new ArgumentException("Invaild argument instance", nameof(exportParameters));

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

			return schedule.FindAll(i => i.StartTime.Date >= exportParameters.StartDate && i.StartTime.Date <= exportParameters.EndDate);
		}

		public static async Task<List<(string id, string name)>> GetFaculties(ScheduleType scheduleType) =>
			await GetList(
				("choice", "1"),
				("kurs", "0"),
				("type_z", ((int)scheduleType).ToString()),
				("schet", GetCurrentSemester()));

		public static async Task<List<(string id, string name)>> GetGroups(ScheduleType scheduleType, string facultyId, string course = "0") =>
			await GetList(
				("choice", "1"),
				("kurs", course),
				("type_z", ((int)scheduleType).ToString()),
				("schet", GetCurrentSemester()),
				("faculty", facultyId));

		private static async Task<List<(string id, string name)>> GetList(params (string key, string value)[] parameters)
		{
			List<(string id, string name)> list = new List<(string, string)>();
			using (HttpClient client = new HttpClient())
			{
				HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "https://cabinet.sut.ru/raspisanie_all_new.php");
				request.SetContent(parameters);
				request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

				HttpResponseMessage response = await client.SendAsync(request);
				string responseBody = await response.Content.ReadAsStringAsync();
				if (string.IsNullOrWhiteSpace(responseBody))
					return list;

				foreach (string s in responseBody.Remove(responseBody.Length - 1).Split(';'))
					list.Add((s.Split(',')[0], s.Split(',')[1]));
			}

			return list;
		}

		private static string GetCurrentSemester()
		{
			DateTime now = DateTime.Today;

			if (now.Month > 8)
				return $"205.{now.Year - 2000}{now.Year - 1999}/1";
			else
				return $"205.{now.Year - 2001}{now.Year - 2000}/2";
		}

		private static DateTime[] GetDatesFromWeeks(int offsetDay, int weekday, string[] weeks)
		{
			List<DateTime> dates = new List<DateTime>();
			foreach(string rawWeek in weeks)
			{
				int week = int.Parse(rawWeek);
				DateTime date = new DateTime(DateTime.Today.Year, DateTime.Today.Month >= 8 ? 9 : 2, offsetDay);

				date = date.AddDays(--week * 7);
				date = date.AddDays(--weekday);

				dates.Add(date);
			}

			return dates.ToArray();
		}

		private static async Task<List<Occupation>> GetRegularSchedule(int offsetDay, string facultyId, string course, string groupId)
		{
			if (string.IsNullOrWhiteSpace(facultyId))
				throw new ArgumentNullException(nameof(facultyId));
			if (string.IsNullOrWhiteSpace(course))
				throw new ArgumentNullException(nameof(course));
			if (string.IsNullOrWhiteSpace(groupId))
				throw new ArgumentNullException(nameof(groupId));

			List<Occupation> schedule = new List<Occupation>();
			using (HttpClient client = new HttpClient())
			{
				HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "https://cabinet.sut.ru/raspisanie_all_new");
				request.SetContent(
					("group_el", "0"),
					("kurs", course),
					("type_z", "1"),
					("faculty", facultyId),
					("group", groupId),
					("ok", "Показать"),
					("schet", GetCurrentSemester()));

				request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

				HttpResponseMessage response = await client.SendAsync(request);
				string responseContent = await response.Content.ReadAsStringAsync();
				if (string.IsNullOrWhiteSpace(responseContent))
					return schedule;

				IHtmlDocument doc = new HtmlParser().ParseDocument(responseContent);

				string groupName = doc.QuerySelector("#group").Children.FirstOrDefault(i => i.HasAttribute("selected")).TextContent;

				IHtmlCollection<IElement> pairs = doc.QuerySelectorAll(".pair");
				foreach (IElement item in pairs)
				{
					DateTime[] dates = GetDatesFromWeeks(
						offsetDay,
						int.Parse(item.GetAttribute("weekday")),
						item.QuerySelector(".weeks").TextContent.Replace("(", "").Replace("н)", "").Replace(" ", "").Split(','));

					foreach(DateTime date in dates)
					{
						schedule.Add(new Occupation
						{
							Name = item.QuerySelector(".subect").TextContent,
							Type = item.QuerySelector(".type").TextContent,
							Group = groupName
						});
					}
					string name, type, professor, place;
					int order, weekday;
					string[] weeks;

					name = item.QuerySelector(".subect")?.TextContent ?? "Неизвестный предмет (см. Расписание)";
					type = item.QuerySelector(".type").TextContent.Replace("(", "").Replace(")", "");
					professor = item.QuerySelector(".teacher")?.GetAttribute("title").Replace(";", "") ?? "";
					place = item.QuerySelector(".aud")?.TextContent ?? "СПбГУТ";
					order = int.Parse(item.GetAttribute("pair")) - 1;
					weeks = item.QuerySelector(".weeks").TextContent.Replace("(", "").Replace("н)", "").Replace(" ", "").Split(',');
					weekday = int.Parse(item.GetAttribute("weekday"));

					schedule.AddRange(Occupation.GetSubject(name, type, professor, place, order, weeks, weekday, groupName));
				}
			}
			
			return schedule;
		}

		private static async Task<List<Occupation>> GetSessionSchedule()
		{

		}

		private static async Task<List<CabinetSubject>> GetCabinetSchedule(HttpClient client, DateTime date, bool checkProfSchedule)
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