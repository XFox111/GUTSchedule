using GUTSchedule.Models;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.ApplicationModel.Appointments;
using Windows.ApplicationModel.Resources;

namespace GUTSchedule.UWP
{
	public static class Calendar
	{
		public static async Task<IReadOnlyList<AppointmentCalendar>> GetCalendars()
		{
			AppointmentStore calendarStore = await AppointmentManager.RequestStoreAsync(AppointmentStoreAccessType.AppCalendarsReadWrite);
			return await calendarStore.FindAppointmentCalendarsAsync();
		}

		public static async Task Export(List<Occupation> schedule, bool addGroupToTitle, int reminder)
		{
			AppointmentStore calendarStore = await AppointmentManager.RequestStoreAsync(AppointmentStoreAccessType.AllCalendarsReadWrite);
			string calendarName = schedule.Any(i => string.IsNullOrWhiteSpace(i.Group)) ? ResourceLoader.GetForCurrentView().GetString("mySchedule") : schedule.FirstOrDefault().Group;
			AppointmentCalendar cal = 
				(await calendarStore.FindAppointmentCalendarsAsync()).FirstOrDefault(i => i.DisplayName == calendarName) ??
				await calendarStore.CreateAppointmentCalendarAsync(calendarName);

			foreach (Occupation item in schedule)
			{
				Appointment appointment = new Appointment
				{
					BusyStatus = AppointmentBusyStatus.Busy,
					Details = item.Opponent,
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

		public static async Task Clear(IEnumerable<object> targets, bool keepPrevious)
		{
			foreach (AppointmentCalendar calendar in targets)
				if(keepPrevious)
					foreach (Appointment appointment in await calendar.FindAppointmentsAsync(DateTime.Now, TimeSpan.FromDays(365)))
						await calendar.DeleteAppointmentAsync(appointment.LocalId);
				else
					await calendar.DeleteAsync();
		}
	}
}
