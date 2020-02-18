using GUTSchedule.Models;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.ApplicationModel.Appointments;

namespace GUTSchedule.UWP
{
	public static class Calendar
	{
		public static async Task<IReadOnlyList<AppointmentCalendar>> GetCalendars()
		{
			AppointmentStore calendarStore = await AppointmentManager.RequestStoreAsync(AppointmentStoreAccessType.AllCalendarsReadWrite);
			return await calendarStore.FindAppointmentCalendarsAsync();
		}

		public static async Task Export(List<Occupation> schedule, bool addGroupToTitle, int reminder, string calendar)
		{
			AppointmentStore calendarStore = await AppointmentManager.RequestStoreAsync(AppointmentStoreAccessType.AllCalendarsReadWrite);
			AppointmentCalendar cal = await calendarStore.GetAppointmentCalendarAsync(calendar);

			foreach (Occupation item in schedule)
			{
				Appointment appointment = new Appointment
				{
					BusyStatus = AppointmentBusyStatus.Busy,
					Details = item.Opponent + "\xFEFF",
					DetailsKind = AppointmentDetailsKind.PlainText,
					Location = item.Cabinet,
					Reminder = reminder < 0 ? (TimeSpan?)null : TimeSpan.FromMinutes(reminder),
					Subject = string.Format("{0}.{1} {2} ({3})",
											item.Order,
											addGroupToTitle && !string.IsNullOrWhiteSpace(item.Group) ? $" [{item.Group}]" : "",
											item.Name,
											item.Type),
					StartTime = item.StartTime,
					Duration = item.EndTime.Subtract(item.StartTime)
				};

				await cal.SaveAppointmentAsync(appointment);
			}
		}

		public static async Task Clear(bool keepPrevious = true)
		{
			AppointmentStore appointmentStore = await AppointmentManager.RequestStoreAsync(AppointmentStoreAccessType.AppCalendarsReadWrite);

			List<Appointment> appointments = (await appointmentStore.FindAppointmentsAsync(keepPrevious ? DateTime.Today : DateTime.Today.Subtract(TimeSpan.FromDays(3000)), TimeSpan.FromDays(10000))).ToList();

			foreach(Appointment i in appointments)
				if (i.Details.Contains('\xFEFF'))
				{
					AppointmentCalendar cal = await appointmentStore.GetAppointmentCalendarAsync(i.CalendarId);
					await cal.DeleteAppointmentAsync(i.LocalId);
				}
		}
	}
}
