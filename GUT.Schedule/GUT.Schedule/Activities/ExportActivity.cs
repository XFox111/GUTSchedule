using Android.App;
using Android.OS;
using Android.Widget;
using GUT.Schedule.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
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

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.export_progress);

            status = FindViewById<TextView>(Resource.Id.status);

            Export();
        }

        private async void Export()
        {
            try
            {
                if(Data.DataSet.HttpClient != null)
                {
                    status.Text = "Загрузка расписания с картофельных серверов Бонча";
                    List<ProfessorSubject> schedule = new List<ProfessorSubject>();

                    for (DateTime d = Data.StartDate; int.Parse($"{d.Year}{d.Month:00}") <= int.Parse($"{Data.EndDate.Year}{Data.EndDate.Month:00}"); d = d.AddMonths(1))
                        schedule.AddRange(await Parser.GetProfessorSchedule(Data.DataSet.HttpClient, d));

                    schedule = schedule.FindAll(i => i.StartTime.Date >= Data.StartDate && i.StartTime.Date <= Data.EndDate);   // Filtering schedule according to export range

                    status.Text = "Экспортирование в календарь";
                    Calendar.Export(schedule);
                }
                else
                {
                    status.Text = "Загрузка расписания";
                    List<Subject> schedule = await Parser.LoadSchedule();

                    schedule = schedule.FindAll(i => i.StartTime.Date >= Data.StartDate && i.StartTime.Date <= Data.EndDate);   // Filtering schedule according to export range

                    status.Text = "Экспортирование в календарь";
                    Calendar.Export(schedule);
                }

                status.Text = "Готово";
                await Task.Delay(1000);
            }
            catch (HttpRequestException e)
            {
                Android.Support.V7.App.AlertDialog.Builder builder = new Android.Support.V7.App.AlertDialog.Builder(this);
                builder.SetMessage("Невозможно загрузить расписание. Проверьте интернет-соединение или попробуйте позже")
                    .SetTitle(e.Message)
                    .SetPositiveButton("ОК", (s, e) => base.OnBackPressed())
                    .SetNegativeButton("Повторить", (s, e) => Export());

                Android.Support.V7.App.AlertDialog dialog = builder.Create();
                dialog.Show();
                return;
            }
            catch (Exception e)
            {
                Android.Support.V7.App.AlertDialog.Builder builder = new Android.Support.V7.App.AlertDialog.Builder(this);
                builder.SetMessage(e.Message)
                    .SetTitle(e.GetType().ToString())
                    .SetPositiveButton("ОК", (s, e) => base.OnBackPressed());

                Android.Support.V7.App.AlertDialog dialog = builder.Create();
                dialog.Show();
                return;
            }
            base.OnBackPressed();   // Navigates back to main activity (always because I don't allow backward navigation)
        }

        public override void OnBackPressed() { }    // Disables back button
    }
}