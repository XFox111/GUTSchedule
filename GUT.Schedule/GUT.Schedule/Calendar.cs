using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.Database;
using Android.Net;
using Android.Provider;
using Android.Support.V4.Content;
using Java.Util;

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

            using Android.Support.V4.Content.CursorLoader loader = new Android.Support.V4.Content.CursorLoader(Application.Context, calendarsUri, calendarsProjection, null, null, null);
            ICursor cursor = (ICursor)loader.LoadInBackground();

            cursor.MoveToNext();
            for (int i = 0; i < cursor.Count; i++)
            {
                if (cursor.GetString(4) == "com.google" && !cursor.GetString(3).Contains("google"))
                    Data.Calendars.Add((cursor.GetString(0), $"{cursor.GetString(1)} ({cursor.GetString(2)})"));
                cursor.MoveToNext();
            }
        }

        public static void Export(string calendarId, IEnumerable<Subject> schedule, int? remindBefore, bool addGroupToTitle)
        {
            foreach (Subject item in schedule)
                AddEvent(calendarId, item, remindBefore, addGroupToTitle);
        }

        static void AddEvent(string calendarId, Subject subject, int? reminderMinutes, bool addHeader)
        {
            ContentValues eventValues = new ContentValues();

            eventValues.Put(CalendarContract.Events.InterfaceConsts.CalendarId, calendarId);
            eventValues.Put(CalendarContract.Events.InterfaceConsts.Title, $"{subject.Order}.{(addHeader ? $" [{subject.Group}]" : "")} {subject.Name} ({subject.Type})");
            eventValues.Put(CalendarContract.Events.InterfaceConsts.Description, subject.Professor);
            eventValues.Put(CalendarContract.Events.InterfaceConsts.EventLocation, string.Join(';', subject.Cabinets));

            eventValues.Put(CalendarContract.Events.InterfaceConsts.Availability, 0);

            if(reminderMinutes.HasValue)
                eventValues.Put(CalendarContract.Events.InterfaceConsts.HasAlarm, true);

            eventValues.Put(CalendarContract.Events.InterfaceConsts.Dtstart, subject.StartTime.ToUnixTime());
            eventValues.Put(CalendarContract.Events.InterfaceConsts.Dtend, Extensions.ToUnixTime(subject.EndTime));

            eventValues.Put(CalendarContract.Events.InterfaceConsts.EventTimezone, TimeZone.Default.ID);
            eventValues.Put(CalendarContract.Events.InterfaceConsts.EventEndTimezone, TimeZone.Default.ID);

            Uri response = Application.Context.ContentResolver.Insert(CalendarContract.Events.ContentUri, eventValues);

            if (reminderMinutes.HasValue)
            {
                ContentValues reminderValues = new ContentValues();
                reminderValues.Put(CalendarContract.Reminders.InterfaceConsts.EventId, long.Parse(response.LastPathSegment));
                reminderValues.Put(CalendarContract.Reminders.InterfaceConsts.Method, 1);
                reminderValues.Put(CalendarContract.Reminders.InterfaceConsts.Minutes, reminderMinutes.Value);

                Application.Context.ContentResolver.Insert(CalendarContract.Reminders.ContentUri, reminderValues);
            }
        }
    }
}