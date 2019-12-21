using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Android.Support.V7.App;
using Android.Widget;
using System.Linq;
using System.Net.Http;

namespace GUT.Schedule
{
    [Activity(MainLauncher = true, Theme = "@style/AppTheme.NoActionBar")]
    public class StartActivity : AppCompatActivity
    {
        TextView status;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            SetContentView(Resource.Layout.splash_screen);
            base.OnCreate(savedInstanceState);

            status = FindViewById<TextView>(Resource.Id.status);

            status.Text = "Проверка наличия разрешений";

            if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.WriteCalendar) != Permission.Granted)
            {
                if (ActivityCompat.ShouldShowRequestPermissionRationale(this, Manifest.Permission.WriteCalendar))
                    ShowDialog();
                else
                    RequestPermissions();
            }
            else
                Proceed();
        }

        private async void Proceed()
        {
            status.Text = "Загрузка списка доступных для записи календарей";
            Calendar.LoadCalendars();

            status.Text = "Загрузка списка факультетов";
            await Parser.LoadFaculties();

            status.Text = "Загрузка дат смещения";
            using (HttpClient client = new HttpClient())
                Data.FirstWeekDay = int.Parse(await client.GetStringAsync("https://xfox111.net/schedule_offset.txt"));

            StartActivity(new Intent(this, typeof(MainActivity)));
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            if (grantResults.All(i => i == Permission.Granted))
                Proceed();
            else
                ShowDialog();
        }

        private void RequestPermissions() =>
            ActivityCompat.RequestPermissions(this, new string[]
            {
                Manifest.Permission.ReadCalendar, Manifest.Permission.WriteCalendar
            }, 76);

        private void ShowDialog()
        {
            Android.Support.V7.App.AlertDialog.Builder builder = new Android.Support.V7.App.AlertDialog.Builder(this);
            builder.SetMessage("Разрешите приложению получать доступ к календарю. Без этого разрешения приложение не сможет добавлять расписание в ваш календарь")
                .SetTitle("Доступ к календарю")
                .SetPositiveButton("ОК", (s, e) => RequestPermissions());

            Android.Support.V7.App.AlertDialog dialog = builder.Create();
            dialog.Show();
        }

        public override void OnBackPressed() { }
    }
}