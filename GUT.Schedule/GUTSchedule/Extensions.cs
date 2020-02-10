using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace GUTSchedule
{
	public static class Extensions
	{
		/// <summary>
		/// Returns <see cref="DateTime"/> instance based on study week number, weekday and semester start day number
		/// </summary>
		/// <param name="week">Number of the study week</param>
		/// <param name="weekday">Weekday</param>
		/// <returns><see cref="DateTime"/> instance based on study week number, weekday and semester start day number</returns>
		public static DateTime GetDateFromWeeks(int week, int weekday)
		{
			DateTime dt = new DateTime(DateTime.Today.Year, DateTime.Today.Month >= 8 ? 9 : 2, Data.FirstWeekDay);

			dt = dt.AddDays(--week * 7);
			dt = dt.AddDays(--weekday);

			return dt;
		}

		public static void SetContent(this HttpRequestMessage request, params (string key, string value)[] values)
		{
			if (request == null)
				throw new ArgumentNullException(nameof(request));

			Dictionary<string, string> body = new Dictionary<string, string>();
			foreach ((string key, string value) in values)
				body.Add(key, value);
			request.Content = new FormUrlEncodedContent(body);
		}
		public static async Task<string> GetString(this HttpResponseMessage response)
		{
			if (response == null)
				throw new ArgumentNullException(nameof(response));

			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
			return Encoding.GetEncoding("Windows-1251").GetString(await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false));
		}
	}
}