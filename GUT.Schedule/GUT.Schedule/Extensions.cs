using System;
using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Android.Widget;

namespace GUT.Schedule
{
    public static class Extensions
    {
        public static void SetList<T>(this Spinner spinner, Context context, IEnumerable<T> array)
        {
            ArrayAdapter adapter = new ArrayAdapter(context, Resource.Layout.support_simple_spinner_dropdown_item, array.ToList());
            spinner.Adapter = adapter;
        }
        public static DateTime GetDateFromWeeks(int week, int weekday)
        {
            DateTime dt = new DateTime(DateTime.Today.Year, 9, Data.FirstWeekDay);

            dt = dt.AddDays(--week * 7);
            dt = dt.AddDays(--weekday);

            return dt;
        }
        public static long ToUnixTime(this DateTime dt) =>
            (long)dt.ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
    }
}