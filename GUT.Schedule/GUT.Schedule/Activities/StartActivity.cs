﻿using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Android.Support.V7.App;
using Android.Widget;
using System;
using System.Linq;
using System.Net.Http;

namespace GUT.Schedule
{
    /// <summary>
    /// Splash screen activity. Loads init data
    /// </summary>
    [Activity(MainLauncher = true, Theme = "@style/AppTheme.Light.SplashScreen")]
    public class StartActivity : AppCompatActivity
    {
        TextView status;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            SetContentView(Resource.Layout.splash_screen);
            base.OnCreate(savedInstanceState);

            status = FindViewById<TextView>(Resource.Id.status);
            PackageInfo version = PackageManager.GetPackageInfo(PackageName, PackageInfoFlags.MatchAll);
            FindViewById<TextView>(Resource.Id.version).Text = $"v{version.VersionName} (ci-id #{version.VersionCode})";

            status.Text = "Проверка наличия разрешений";

            if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.WriteCalendar) != Permission.Granted)
            {
                if (ActivityCompat.ShouldShowRequestPermissionRationale(this, Manifest.Permission.WriteCalendar))
                    ShowDialog("Доступ к календарю", "Разрешите приложению получать доступ к календарю. Без этого разрешения приложение не сможет добавлять расписание в ваш календарь", RequestPermissions);
                else
                    RequestPermissions();
            }
            else
                Proceed();
        }

        private async void Proceed()
        {
            try
            {
                status.Text = "Загрузка списка календарей";
                Calendar.LoadCalendars();
                if (Calendar.Calendars.Count == 0)
                {
                    ShowDialog("Создайте новый календарь", "На вашем устройстве нет календарей пригодных для записи расписания");
                    return;
                }

                status.Text = "Загрузка списка факультетов";
                await Parser.LoadFaculties();

                status.Text = "Загрузка дат смещения";
                using HttpClient client = new HttpClient();
                Data.FirstWeekDay = int.Parse(await client.GetStringAsync("https://xfox111.net/schedule_offset.txt"));
            }
            catch(HttpRequestException e)
            {
                ShowDialog(e.Message, "Невозможно загрузить расписание. Проверьте интернет-соединение или попробуйте позже", Proceed, FinishAndRemoveTask, "Повторить", "Выйти");
                return;
            } 
            catch (Exception e)
            {
                ShowDialog(e.GetType().ToString(), e.Message, FinishAndRemoveTask);
                return;
            }
            StartActivity(new Intent(this, typeof(MainActivity)));
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            if (grantResults.All(i => i == Permission.Granted))
                Proceed();
            else
                ShowDialog("Доступ к календарю", "Разрешите приложению получать доступ к календарю. Без этого разрешения приложение не сможет добавлять расписание в ваш календарь", RequestPermissions);
        }

        private void RequestPermissions() =>
            ActivityCompat.RequestPermissions(this, new[]
            {
                Manifest.Permission.ReadCalendar, 
                Manifest.Permission.WriteCalendar,
                Manifest.Permission.Internet
            }, 0);

        private void ShowDialog(string title, string content, Action posAction = null, Action negAction = null, string posActionLabel = null, string negActionLabel = null)
        {
            Android.Support.V7.App.AlertDialog.Builder builder = new Android.Support.V7.App.AlertDialog.Builder(this);
            builder.SetMessage(content)
                .SetTitle(title).SetPositiveButton(posActionLabel ?? "OK", (s, e) => posAction?.Invoke());

            if (negAction != null)
                builder.SetNegativeButton(negActionLabel ?? "Close", (s, e) => negAction.Invoke());

            Android.Support.V7.App.AlertDialog dialog = builder.Create();
            dialog.Show();
        }

        public override void OnBackPressed() { }    // Disables back button
    }
}