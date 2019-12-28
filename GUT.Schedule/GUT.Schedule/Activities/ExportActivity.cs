using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using GUT.Schedule.Models;
using System;
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

            try
            {
                status.Text = "Загрузка расписания";
                List<Subject> schedule = await Parser.LoadSchedule();

                schedule = schedule.FindAll(i => i.StartTime.Date >= Data.StartDate && i.StartTime.Date <= Data.EndDate);   // Filtering schedule according to export range

                status.Text = "Экспортирование в календарь";
                Calendar.Export(schedule);


                status.Text = "Готово";
                await Task.Delay(1000);
            }
            catch (Exception e)
            {
                Android.Support.V7.App.AlertDialog.Builder builder = new Android.Support.V7.App.AlertDialog.Builder(this);
                builder.SetMessage(e.Message)
                    .SetTitle(e.GetType().ToString())
                    .SetPositiveButton("ОК", (EventHandler<DialogClickEventArgs>)null);

                Android.Support.V7.App.AlertDialog dialog = builder.Create();
                dialog.Show();
            }
            base.OnBackPressed();   // Navigates back to main activity (always because I don't allow backward navigation)
        }

        public override void OnBackPressed() { }    // Disables back button
    }
}