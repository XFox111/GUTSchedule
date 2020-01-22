using System;
using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Android.Widget;

namespace GUT.Schedule
{
    public static class Extensions
    {
        /// <summary>
        /// Sets array as Spinner dropdown list content
        /// </summary>
        /// <typeparam name="T">Array items type</typeparam>
        /// <param name="spinner">Spinner on which array will be assigned to</param>
        /// <param name="context">Current activity context. In most common cases <c>this</c> will do</param>
        /// <param name="array">Array of items to be displayed</param>
        public static void SetList<T>(this Spinner spinner, Context context, IEnumerable<T> array)
        {
            ArrayAdapter adapter = new ArrayAdapter(context, Resource.Layout.support_simple_spinner_dropdown_item, array.ToList());
            spinner.Adapter = adapter;
        }

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

        /// <summary>
        /// Converts <see cref="DateTime"/> to milliseconds count
        /// </summary>
        /// <remarks>In the nearest future we will be fucked because of that shit</remarks>
        /// <param name="dt"><see cref="DateTime"/> which is to be converted to UNIX time</param>
        /// <returns><see cref="long"/> which is represented by total milliseconds count passed since 1970</returns>
        public static long ToUnixTime(this DateTime dt) =>
            (long)dt.ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
    }
}