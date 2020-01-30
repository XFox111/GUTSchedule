using System.Collections.Generic;
using Android.App;
using Android.Content;
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
                CalendarContract.Calendars.InterfaceConsts.AccountName
            };

            // Retrieving calendars data
            ICursor cursor = Application.Context.ContentResolver.Query(calendarsUri, calendarsProjection, null, null, null);

            while (cursor.MoveToNext())
                Calendars.Add((cursor.GetString(0), $"{cursor.GetString(1)} ({cursor.GetString(2)})"));

            cursor.Close();
        }
        
        public static void Export(IEnumerable<Subject> schedule)
        {
            DataSet data = Data.DataSet;

            foreach (Subject item in schedule)
            {
                ContentValues eventValues = new ContentValues();

                eventValues.Put(CalendarContract.Events.InterfaceConsts.CalendarId, data.Calendar);

                eventValues.Put(CalendarContract.Events.InterfaceConsts.Title, string.Format("{0}.{1} {2} ({3})", 
                    item.Order, 
                    data.AddGroupToTitle ? $" [{item.Group}]" : "", 
                    item.Name, 
                    item.Type));

                eventValues.Put(CalendarContract.Events.InterfaceConsts.Description, item.Professor);
                eventValues.Put(CalendarContract.Events.InterfaceConsts.EventLocation, string.Join(';', item.Cabinets));

                eventValues.Put(CalendarContract.Events.InterfaceConsts.Availability, 0);

                eventValues.Put(CalendarContract.Events.InterfaceConsts.HasAlarm, data.Reminder != -5);
                // For some reason Google calendars ignore HasAlarm = false and set reminder for 30 minutes. Local calendars don't seem to have this issue

                eventValues.Put(CalendarContract.Events.InterfaceConsts.Dtstart, item.StartTime.ToUnixTime());
                eventValues.Put(CalendarContract.Events.InterfaceConsts.Dtend, item.EndTime.ToUnixTime());

                eventValues.Put(CalendarContract.Events.InterfaceConsts.EventTimezone, TimeZone.Default.ID);
                eventValues.Put(CalendarContract.Events.InterfaceConsts.EventEndTimezone, TimeZone.Default.ID);
                eventValues.Put(CalendarContract.Events.InterfaceConsts.CustomAppPackage, Application.Context.PackageName);

                Uri response = Application.Context.ContentResolver.Insert(CalendarContract.Events.ContentUri, eventValues);

                // Settings reminder
                if(data.Reminder != -5)
                {
                    ContentValues reminderValues = new ContentValues();
                    reminderValues.Put(CalendarContract.Reminders.InterfaceConsts.EventId, long.Parse(response.LastPathSegment));
                    reminderValues.Put(CalendarContract.Reminders.InterfaceConsts.Minutes, data.Reminder);

                    Application.Context.ContentResolver.Insert(CalendarContract.Reminders.ContentUri, reminderValues);
                }
            }
        }
        
        public static void Export(IEnumerable<CabinetSubject> schedule)
        {
            DataSet data = Data.DataSet;

            foreach (CabinetSubject item in schedule)
            {
                ContentValues eventValues = new ContentValues();

                eventValues.Put(CalendarContract.Events.InterfaceConsts.CalendarId, data.Calendar);

                eventValues.Put(CalendarContract.Events.InterfaceConsts.Title, string.Format("{0}. {1} ({2})", 
                    item.Order, 
                    item.Name, 
                    item.Type));

                eventValues.Put(CalendarContract.Events.InterfaceConsts.Description, item.Opponent);
                eventValues.Put(CalendarContract.Events.InterfaceConsts.EventLocation, item.Cabinet);

                eventValues.Put(CalendarContract.Events.InterfaceConsts.Availability, 0);

                eventValues.Put(CalendarContract.Events.InterfaceConsts.HasAlarm, data.Reminder != -5);
                // For some reason Google calendars ignore HasAlarm = false and set reminder for 30 minutes. Local calendars don't seem to have this issue

                eventValues.Put(CalendarContract.Events.InterfaceConsts.Dtstart, item.StartTime.ToUnixTime());
                eventValues.Put(CalendarContract.Events.InterfaceConsts.Dtend, item.EndTime.ToUnixTime());

                eventValues.Put(CalendarContract.Events.InterfaceConsts.EventTimezone, TimeZone.Default.ID);
                eventValues.Put(CalendarContract.Events.InterfaceConsts.EventEndTimezone, TimeZone.Default.ID);
                eventValues.Put(CalendarContract.Events.InterfaceConsts.CustomAppPackage, Application.Context.PackageName);

                Uri response = Application.Context.ContentResolver.Insert(CalendarContract.Events.ContentUri, eventValues);

                // Settings reminder
                if(data.Reminder != -5)
                {
                    ContentValues reminderValues = new ContentValues();
                    reminderValues.Put(CalendarContract.Reminders.InterfaceConsts.EventId, long.Parse(response.LastPathSegment));
                    reminderValues.Put(CalendarContract.Reminders.InterfaceConsts.Minutes, data.Reminder);

                    Application.Context.ContentResolver.Insert(CalendarContract.Reminders.ContentUri, reminderValues);
                }
            }
        }

        public static void Clear(bool keepPrevious = true)
        {
            Uri contentUri = CalendarContract.Events.ContentUri;
            string selector = $"({CalendarContract.Events.InterfaceConsts.CustomAppPackage} == \"{Application.Context.PackageName}\") AND (deleted != 1)";
            if (keepPrevious)
                selector += $" AND (dtstart > {System.DateTime.Now.ToUnixTime()})";

            string[] calendarsProjection = {
                CalendarContract.Events.InterfaceConsts.Id,
                CalendarContract.Events.InterfaceConsts.Dtstart,
                CalendarContract.Events.InterfaceConsts.CustomAppPackage,
            };

            // Retrieving calendars data
            ICursor cursor = Application.Context.ContentResolver.Query(contentUri, calendarsProjection, selector, null, null);
            while (cursor.MoveToNext())
                Application.Context.ContentResolver.Delete(ContentUris.WithAppendedId(CalendarContract.Events.ContentUri, cursor.GetLong(0)), null, null);

            cursor.Close();
        }
    }
}