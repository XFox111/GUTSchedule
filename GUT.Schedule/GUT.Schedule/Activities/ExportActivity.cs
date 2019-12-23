using Android.App;
using Android.OS;
using Android.Widget;
using GUT.Schedule.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GUT.Schedule
{
    /// <summary>
    /// Shows status of schedule export process
    /// </summary>
    [Activity]
    public class ExportActivity : Activity
    {
        TextView status;

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.export_progress);

            status = FindViewById<TextView>(Resource.Id.status);

            status.Text = "Загрузка расписания";
            List<Subject> schedule = await Parser.LoadSchedule();

            schedule = schedule.FindAll(i => i.StartTime.Date >= Data.StartDate && i.StartTime.Date <= Data.EndDate);   // Filtering schedule according to export range

            status.Text = "Экспортирование в календарь";
            Calendar.Export(schedule);


            status.Text = "Готово";
            await Task.Delay(3000);
            base.OnBackPressed();   // Navigates back to main activity (always because I don't allow backward navigation)
        }

        public override void OnBackPressed() { }    // Disables back button
    }
}