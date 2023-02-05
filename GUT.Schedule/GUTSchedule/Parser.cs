using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using GUTSchedule.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GUTSchedule
{
	public static class Parser
	{
		private static readonly HttpClient client = new HttpClient(new HttpClientHandler
		{
			ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
		});

		public static async Task<HttpClient> VaildateAuthorization(string email, string password)
		{
			if (string.IsNullOrWhiteSpace(email))
				throw new ArgumentNullException(nameof(email));
			if (string.IsNullOrWhiteSpace(password))
				throw new ArgumentNullException(nameof(password));

			await client.GetAsync("https://lk.sut.ru/cabinet/");

			HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "https://lk.sut.ru/cabinet/lib/autentificationok.php");
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
				if (responseContent.Length > 1)
					foreach (string i in responseContent.Split('&'))
						responseQuery.Add(i.Split('=')[0], i.Split('=')[1]);

				throw new System.Security.VerificationException(responseQuery["error"].Replace("|", "; "));
			}

			await client.GetAsync("https://lk.sut.ru/cabinet/?login=yes");

			return client;
		}

		public static async Task<List<Occupation>> GetSchedule(ExportParameters exportParameters)
		{
			List<Occupation> schedule = new List<Occupation>();

			if (exportParameters is CabinetExportParameters cabinetArgs)
			{
				HttpClient client = await VaildateAuthorization(cabinetArgs.Email, cabinetArgs.Password);
				for (DateTime d = exportParameters.StartDate.AddDays(1 - exportParameters.StartDate.Day); d <= exportParameters.EndDate.AddDays(1 - exportParameters.EndDate.Day); d = d.AddMonths(1))
				{
					schedule.AddRange(await GetCabinetSchedule(client, d, false));
					schedule.AddRange(await GetCabinetSchedule(client, d, true));
				}
			}
			else if (exportParameters is DefaultExportParameters args)
			{
				DateTime startDate = new DateTime(2023, 2, 6);
				IHtmlDocument[] rawSchedule = await GetRawSchedule(args.FacultyId, args.Course, args.GroupId);
				if(rawSchedule[0] != null)
					schedule.AddRange(ParseRegularSchedule(startDate, rawSchedule[0]));
				if(rawSchedule[1] != null)
					schedule.AddRange(ParseSessionSchedule(rawSchedule[1]));
			}
			else
				throw new ArgumentException("Invaild argument instance", nameof(exportParameters));

			schedule = schedule.FindAll(i => i.StartTime.Date >= exportParameters.StartDate.Date && i.EndTime.Date <= exportParameters.EndDate.Date);

			if (schedule.Count < 1)
				throw new NullReferenceException("Не удалось найти расписание соответствующее критериям. Ничего не экспортировано");

			// Merge duplicating entries
			schedule.OrderBy(i => i.StartTime);
			for (int k = 1; k < schedule.Count; k++)
				if (schedule[k - 1].StartTime == schedule[k].StartTime &&
					schedule[k - 1].Name == schedule[k].Name &&
					schedule[k - 1].Type == schedule[k].Type)
				{
					schedule[k - 1].Opponent += $" ({schedule[k-1].Cabinet}) \n{schedule[k].Opponent} ({schedule[k].Cabinet})";
					schedule[k - 1].Cabinet += "; " + schedule[k].Cabinet;
					schedule.RemoveAt(k--);
				}

			return schedule;
		}

		public static async Task<List<(string id, string name)>> GetFaculties()
		{
			List<(string, string)> list = await GetList(
				("choice", "1"),
				("kurs", "0"),
				("type_z", "1"),
				("schet", GetCurrentSemester()));

			if (list.Count < 1)
				list = await GetList(
				("choice", "1"),
				("kurs", "0"),
				("type_z", "2"),
				("schet", GetCurrentSemester()));
			else
				return list;

			if (list.Count < 1)
				list = new List<(string, string)>();

			return list;
		}

		public static async Task<List<(string id, string name)>> GetGroups(string facultyId, string course = "0") =>
			await GetList(
				("choice", "1"),
				("kurs", course),
				("type_z", "1"),
				("schet", GetCurrentSemester()),
				("faculty", facultyId));

		private static async Task<List<(string id, string name)>> GetList(params (string key, string value)[] parameters)
		{
			List<(string id, string name)> list = new List<(string, string)>();
			HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "https://cabinet.sut.ru/raspisanie_all_new.php");
			request.SetContent(parameters);
			request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

			HttpResponseMessage response = await client.SendAsync(request);
			string responseBody = await response.Content.ReadAsStringAsync();
			if (string.IsNullOrWhiteSpace(responseBody))
				return list;

			foreach (string s in responseBody.Remove(responseBody.Length - 1).Split(';'))
				list.Add((s.Split(',')[0], s.Split(',')[1]));

			return list;
		}

		private static string GetCurrentSemester()
		{
			DateTime now = DateTime.Today;

			string semester = now.Month >= 8 || now.Month < 2 ? "1" : "2";

			if (now.Month >= 8)
				return $"205.{now.Year - 2000}{now.Year - 1999}/{semester}";
			else
				return $"205.{now.Year - 2001}{now.Year - 2000}/{semester}";
		}

		private static DateTime[] GetDatesFromWeeks(DateTime date, int weekday, string[] weeks)
		{
			List<DateTime> dates = new List<DateTime>();
			foreach(string rawWeek in weeks)
			{
				int week = int.Parse(rawWeek.Replace("*", "").Replace("д", ""));

				DateTime newDate = date.AddDays(--week * 7);
				newDate = newDate.AddDays(weekday - 1);

				dates.Add(newDate);
			}

			return dates.ToArray();
		}

		private static async Task<IHtmlDocument[]> GetRawSchedule(string facultyId, string course, string groupId)
		{
			if (string.IsNullOrWhiteSpace(facultyId))
				throw new ArgumentNullException(nameof(facultyId));
			if (string.IsNullOrWhiteSpace(course))
				throw new ArgumentNullException(nameof(course));
			if (string.IsNullOrWhiteSpace(groupId))
				throw new ArgumentNullException(nameof(groupId));

			IHtmlDocument[] docs = new IHtmlDocument[2];
			for (int k = 1; k < 3; k++)
			{
				HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "https://cabinet.sut.ru/raspisanie_all_new");
				request.SetContent(
					("group_el", "0"),
					("kurs", course),
					("type_z", k.ToString()),
					("faculty", facultyId),
					("group", groupId),
					("ok", "Показать"),
					("schet", GetCurrentSemester()));

				request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

				HttpResponseMessage response = await client.SendAsync(request);
				string responseContent = await response.Content.ReadAsStringAsync();
				if (string.IsNullOrWhiteSpace(responseContent))
					docs[k - 1] = null;

				try { docs[k - 1] = new HtmlParser().ParseDocument(responseContent); }
				catch (ArgumentException) { }
			}

			return docs;
		}

		private static List<Occupation> ParseRegularSchedule(DateTime startDate, IHtmlDocument raw)
		{
			if (raw == null)
				throw new ArgumentNullException(nameof(raw));

			List<Occupation> schedule = new List<Occupation>();
			string groupName = raw.QuerySelector("#group").Children.FirstOrDefault(i => i.HasAttribute("selected")).TextContent;

			IHtmlCollection<IElement> pairs = raw.QuerySelectorAll(".pair");
			foreach (IElement item in pairs)
			{
				DateTime[] dates = GetDatesFromWeeks(
					startDate,
					int.Parse(item.GetAttribute("weekday")),
					item.QuerySelector(".weeks").TextContent.Replace("(", "").Replace("н)", "").Replace(" ", "").Split(','));

				foreach (DateTime date in dates)
				{
					int order = int.Parse(item.GetAttribute("pair")) - 1;
					Occupation occupation = new Occupation
					{
						Name = item.QuerySelector(".subect").TextContent.Replace(" (1)", "").Replace(" (2)", ""),
						Type = item.QuerySelector(".type").TextContent.Replace("(", "").Replace(")", ""),
						Group = groupName,
						Opponent = item.QuerySelector(".teacher")?.GetAttribute("title").Replace("; ", "\n") ?? "",
						Cabinet = item.QuerySelector(".aud")?.TextContent.Replace("ауд.: ", "").Replace("; Б22", "").Replace(" ", "") ?? "СПбГУТ",
						Order = order > 50 ? $"Ф{order - 81}" : order.ToString()
					};

					string startTime;
					switch (order)
					{
						case 1:
							startTime = "9:00"; break;
						case 2:
							startTime = "10:45"; break;
						case 3:
							startTime = "13:00"; break;
						case 4:
							startTime = "14:45"; break;
						case 5:
							startTime = "16:30"; break;
						case 6:
							startTime = "18:15"; break;
						case 7:
							startTime = "20:00"; break;
						case 29:
							startTime = "20:00";
							occupation.Order = "7";
							break;
						case 82:
							startTime = "9:00"; break;
						case 83:
							startTime = "10:30"; break;
						case 84:
							startTime = "12:00"; break;
						case 85:
							startTime = "13:30"; break;
						case 86:
							startTime = "15:00"; break;
						case 87:
							startTime = "16:30"; break;
						case 88:
							startTime = "18:00"; break;
						default:
							startTime = "9:00"; break;
					}

					occupation.StartTime = date.Add(TimeSpan.Parse(startTime));
					occupation.EndTime = occupation.StartTime.AddMinutes(order > 10 ? 90 : 95);

					schedule.Add(occupation);
				}
			}

			return schedule;
		}

		private static List<Occupation> ParseSessionSchedule(IHtmlDocument raw)
		{
			if (raw == null)
				throw new ArgumentNullException(nameof(raw));

			List<Occupation> schedule = new List<Occupation>();
			string groupName = raw.QuerySelector("#group").Children.FirstOrDefault(i => i.HasAttribute("selected"))?.TextContent;

			IHtmlCollection<IElement> pairs = raw.QuerySelectorAll(".pair");
			foreach (IElement item in pairs)
			{
				Occupation occupation = new Occupation
				{
					Name = item.QuerySelector(".subect").TextContent,
					Type = item.QuerySelector(".type").TextContent,
					Group = groupName,
					Cabinet = item.QuerySelector(".aud").TextContent,
					Opponent = item.QuerySelector(".teacher")?.GetAttribute("title"),
					Order = "Сессия"
				};

				DateTime date = DateTime.Parse(item.FirstChild.FirstChild.TextContent, new CultureInfo("ru-RU"));
				string rawTime = item.ChildNodes[2].TextContent;
				try
				{
					rawTime = rawTime.Substring(rawTime.IndexOf('(')).Replace(")", "").Replace('.', ':');
					occupation.StartTime = date.Add(TimeSpan.Parse(rawTime.Split('-')[0]));
					occupation.EndTime = date.Add(TimeSpan.Parse(rawTime.Split('-')[1]));
				}
				catch
				{
					occupation.StartTime = date;
					occupation.EndTime = date;
				}

				schedule.Add(occupation);
			}

			return schedule;
		}

		private static async Task<List<Occupation>> GetCabinetSchedule(HttpClient client, DateTime date, bool checkProfSchedule)
		{
			HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"https://lk.sut.ru/cabinet/project/cabinet/forms/{(checkProfSchedule ? "pr_" : "")}raspisanie_kalendar.php");
			request.SetContent(
				("month", date.Month.ToString()),
				("year", date.Year.ToString()),
				("type_z", "0"));
			request.Headers.UserAgent.Clear();
			request.Headers.UserAgent.Add(new ProductInfoHeaderValue("Mozilla", "5.0"));
			request.Headers.UserAgent.Add(new ProductInfoHeaderValue("AppleWebKit", "537.36"));
			request.Headers.UserAgent.Add(new ProductInfoHeaderValue("Chrome", "88.0.4324.150"));
			request.Headers.UserAgent.Add(new ProductInfoHeaderValue("Safari", "537.36"));
			request.Headers.UserAgent.Add(new ProductInfoHeaderValue("Edg", "88.0.705.68"));

			HttpResponseMessage response = await client.SendAsync(request).ConfigureAwait(false);
			string responseContent = await response.GetString().ConfigureAwait(false);

			if (!response.IsSuccessStatusCode)
				throw new HttpRequestException(responseContent);

			IHtmlDocument doc = new HtmlParser().ParseDocument(responseContent);
			List<Occupation> schedule = new List<Occupation>();

			string groupName = checkProfSchedule ? null : doc.QuerySelector(".style_gr b").TextContent;

			foreach (var i in doc.QuerySelectorAll("td").Where(i => i.GetAttribute("style") == "text-align: center; vertical-align: top"))
				for (int k = 0; k < i.QuerySelectorAll("i").Length; k++)
				{
					Occupation item = new Occupation
					{
						Name = i.QuerySelectorAll("b")[k * 2 + 1].TextContent,
						Type = i.QuerySelectorAll("i")[k].TextContent,
						Group = groupName,
						Opponent = i.QuerySelectorAll("i")[k].NextSibling.NextSibling?.NodeType == NodeType.Text ?
							i.QuerySelectorAll("i")[k].NextSibling.NextSibling.TextContent : "",
					};

					if (string.IsNullOrWhiteSpace(item.Opponent))
						item.Opponent = i.QuerySelectorAll("i")[k].NextSibling.NextSibling.NextSibling?.NodeType == NodeType.Text ?
							i.QuerySelectorAll("i")[k].NextSibling.NextSibling.NextSibling.TextContent : "";

					try { item.Cabinet = i.QuerySelectorAll("small")[k].NextSibling.TextContent.Replace(" ", "").Replace(";Б22", ""); }
					catch
					{
						INode cab = i?.QuerySelectorAll("b")[k * 2 + 2].NextSibling?.NextSibling;
						if (cab?.NodeType == NodeType.Text)
							item.Cabinet = cab.TextContent;
						else
							item.Cabinet = "СПбГУТ";
					}

					string rawTime = i.QuerySelectorAll("b")[k * 2 + 2].TextContent;
					item.StartTime = new DateTime(
						year: date.Year,
						month: date.Month,
						day: int.Parse(i.ChildNodes[0].TextContent));
					item.StartTime = item.StartTime.Add(TimeSpan.Parse(rawTime.Replace('.', ':').Split('-')[0]));

					item.EndTime = new DateTime(
						year: date.Year,
						month: date.Month,
						day: int.Parse(i.ChildNodes[0].TextContent));
					item.EndTime = item.EndTime.Add(TimeSpan.Parse(rawTime.Replace('.', ':').Split('-')[1]));

					switch(rawTime.Split('-')[1])
					{
						case "10.35":
							item.Order = "1";
							break;
						case "12.20":
							item.Order = "2";
							break;
						case "14.35":
							item.Order = "3";
							break;
						case "16.20":
							item.Order = "4";
							break;
						case "18.05":
							item.Order = "5";
							break;
						case "19.50":
							item.Order = "6";
							break;
						case "21.35":
							item.Order = "7";
							break;
						case "10.30":
							item.Order = "Ф1";
							break;
						case "12.00":
							item.Order = "Ф2";
							break;
						case "13.30":
							item.Order = "Ф3";
							break;
						case "15.00":
							item.Order = "Ф4";
							break;
						case "16.30":
							item.Order = "Ф5";
							break;
						case "18.00":
							item.Order = "Ф6";
							break;
						case "19.30":
							item.Order = "Ф7";
							break;

						default:
							if (item.Name.Contains("Факультатив"))
								item.Order = "Ф";
							else
								item.Order = "0";
							break;
					}

					if (checkProfSchedule)
						item.Order = item.Order.Insert(0, "📚 ");

					schedule.Add(item);
				}

			return schedule;
		}

		public static async Task<List<(string, string)>> CheckAvailableOccupations(string email, string password)
		{
			HttpClient client = await VaildateAuthorization(email, password);
			HttpResponseMessage response = await client.GetAsync("https://lk.sut.ru/cabinet//project/cabinet/forms/raspisanie_bak.php");
			string responseContent = await response.GetString();

			if (!response.IsSuccessStatusCode)
				throw new HttpRequestException(responseContent);

			IHtmlDocument doc = new HtmlParser().ParseDocument(responseContent);
			List<(string, string)> occupations = doc.QuerySelectorAll(".simple-little-table td[align=left] > span[id] > a").Select(i =>
			{
				string[] parameters = new Regex(@"(?<=\()[0-9,]*(?=\))").Match(i.Attributes["onclick"].Value).Value.Split(',');
				return (parameters[0], parameters[1]);
			}).ToList();
			return occupations;
		}

		public static async Task ApplyForOccupations(string email, string password, List<(string, string)> occupations)
		{
			HttpClient client = await VaildateAuthorization(email, password);
			foreach (var i in occupations)
				await client.GetAsync($"https://lk.sut.ru/cabinet/project/cabinet/forms/raspisanie_bak.php?open=1&rasp={i.Item1}&week={i.Item2}");
		}
	}
}