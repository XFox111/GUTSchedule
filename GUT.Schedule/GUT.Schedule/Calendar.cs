using System.Collections.Generic;
using Android.App;
using Android.Database;
using Android.Net;
using Android.Provider;
using GUT.Schedule.Models;
using Java.Util;

namespace GUT.Schedule
{
    public static class Calendar
    {
        /// <summary>
        /// List of all existing Google calendars on the device
        /// </summary>
        public static List<(string Id, string Name)> Calendars { get; private set; } = new List<(string Id, string Name)>();

        /// <summary>
        /// Retrieves all Google Accounts' calendars existing on the device and puts them to <see cref="Calendars"/>
        /// </summary>
        public static void LoadCalendars()
        {
            Calendars = new List<(string, string)>();   // Resetting current calendars list

            // Building calendar data retrieval projections
            Uri calendarsUri = CalendarContract.Calendars.ContentUri;
            string[] calendarsProjection = {
                CalendarContract.Calendars.InterfaceConsts.Id,
                CalendarContract.Calendars.InterfaceConsts.CalendarDisplayName,
                CalendarContract.Calendars.InterfaceConsts.AccountName,
                CalendarContract.Calendars.InterfaceConsts.OwnerAccount,
                CalendarContract.Calendars.InterfaceConsts.AccountType,
            };

            // Retrieving calendars data
            ICursor cursor = Application.Context.ContentResolver.Query(calendarsUri, calendarsProjection, null, null);

            cursor.MoveToNext();
            for (int i = 0; i < cursor.Count; i++)
            {
                if (cursor.GetString(4) == "com.google" && !cursor.GetString(3).Contains("google"))         // Loading only users' main calendars
                    Calendars.Add((cursor.GetString(0), $"{cursor.GetString(1)} ({cursor.GetString(2)})"));
                cursor.MoveToNext();
            }
        }

        
        public static void Export(IEnumerable<Subject> schedule)
        {
            DataSet data = Data.DataSet;

            foreach (Subject item in schedule)
            {
                Android.Content.ContentValues eventValues = new Android.Content.ContentValues();

                eventValues.Put(CalendarContract.Events.InterfaceConsts.CalendarId, data.Calendar);

                eventValues.Put(CalendarContract.Events.InterfaceConsts.Title, string.Format("{0}.{1} {2} ({3})", 
                    item.Order, 
                    data.AddGroupToTitle ? $" [{item.Group}]" : "", 
                    item.Name, 
                    item.Type));

                eventValues.Put(CalendarContract.Events.InterfaceConsts.Description, item.Professor);
                eventValues.Put(CalendarContract.Events.InterfaceConsts.EventLocation, string.Join(';', item.Cabinets));

                eventValues.Put(CalendarContract.Events.InterfaceConsts.Availability, 0);

                eventValues.Put(CalendarContract.Events.InterfaceConsts.HasAlarm, data.Reminder.HasValue);

                eventValues.Put(CalendarContract.Events.InterfaceConsts.Dtstart, item.StartTime.ToUnixTime());
                eventValues.Put(CalendarContract.Events.InterfaceConsts.Dtend, Extensions.ToUnixTime(item.EndTime));

                eventValues.Put(CalendarContract.Events.InterfaceConsts.EventTimezone, TimeZone.Default.ID);
                eventValues.Put(CalendarContract.Events.InterfaceConsts.EventEndTimezone, TimeZone.Default.ID);
                eventValues.Put(CalendarContract.Reminders.InterfaceConsts.CustomAppPackage, "bonch.schedule");

                Uri response = Application.Context.ContentResolver.Insert(CalendarContract.Events.ContentUri, eventValues);

                Android.Content.ContentValues reminderValues = new Android.Content.ContentValues();
                reminderValues.Put(CalendarContract.Reminders.InterfaceConsts.EventId, long.Parse(response.LastPathSegment));
                reminderValues.Put(CalendarContract.Reminders.InterfaceConsts.CustomAppPackage, "bonch.schedule");

                // Fuck Android!
                reminderValues.Put(CalendarContract.Reminders.InterfaceConsts.Minutes, data.Reminder?.ToString());
                // P.S. I mean fuck Android!

                Application.Context.ContentResolver.Insert(CalendarContract.Reminders.ContentUri, reminderValues);
            }
        }
    }
}