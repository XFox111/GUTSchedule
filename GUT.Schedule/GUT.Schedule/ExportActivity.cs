using Android.App;
using Android.OS;
using Android.Widget;
using System.Threading.Tasks;

namespace GUT.Schedule
{
    [Activity(Theme = "@style/AppTheme.NoActionBar")]
    public class ExportActivity : Activity
    {
        TextView status;
        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.export_progress);

            status = FindViewById<TextView>(Resource.Id.status);

            status.Text = "Загрузка расписания";
            await Parser.LoadSchedule();

            status.Text = "Экспортирование в календарь";
            int minutes = Data.Reminder switch
            {
                1 => 0,
                2 => 5,
                3 => 10,
                _ => -1
            };
            Calendar.Export(Data.Calendars[Data.Calendar].Id, Data.Schedule, minutes < 0 ? (int?)null : minutes, Data.AddTitle);

            status.Text = "Готово";

            await Task.Delay(3000);
            base.OnBackPressed();
        }

        public override void OnBackPressed() { }
    }
}