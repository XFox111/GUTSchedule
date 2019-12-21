using System.Collections.Generic;
using Android.App;
using Android.Database;
using Android.Provider;
using Android.Support.V4.Content;

namespace GUT.Schedule
{
    public static class Calendar
    {
        public static void LoadCalendars()
        {
            Data.Calendars = new List<(string, string)>();

            var calendarsUri = CalendarContract.Calendars.ContentUri;
            string[] calendarsProjection = {
                CalendarContract.Calendars.InterfaceConsts.Id,
                CalendarContract.Calendars.InterfaceConsts.CalendarDisplayName,
                CalendarContract.Calendars.InterfaceConsts.AccountName,
                CalendarContract.Calendars.InterfaceConsts.OwnerAccount,
                CalendarContract.Calendars.InterfaceConsts.AccountType,
            };

            using CursorLoader loader = new CursorLoader(Application.Context, calendarsUri, calendarsProjection, null, null, null);
            ICursor cursor = (ICursor)loader.LoadInBackground();

            cursor.MoveToNext();
            for (int i = 0; i < cursor.Count; i++)
            {
                if (cursor.GetString(4) == "com.google" && !cursor.GetString(3).Contains("google"))
                    Data.Calendars.Add((cursor.GetString(0), $"{cursor.GetString(1)} ({cursor.GetString(2)})"));
                cursor.MoveToNext();
            }
        }
    }
}