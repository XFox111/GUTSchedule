using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Android.Support.V7.App;
using Android.Widget;
using GUTSchedule;
using GUTSchedule.Droid;
using GUTSchedule.Droid.Activities;
using System;
using System.Linq;
using System.Net.Http;

namespace GUT.Schedule.Droid.Activities
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
			SetContentView(Resource.Layout.SplashScreen);
			base.OnCreate(savedInstanceState);

			status = FindViewById<TextView>(Resource.Id.status);
			PackageInfo version = PackageManager.GetPackageInfo(PackageName, PackageInfoFlags.MatchAll);
			FindViewById<TextView>(Resource.Id.version).Text = $"v{version.VersionName} (ci-id #{version.VersionCode})";

			status.Text = Resources.GetText(Resource.String.permissionsCheckStatus);

			if (ContextCompat.CheckSelfPermission(this, Android.Manifest.Permission.WriteCalendar) != Permission.Granted)
			{
				if (ActivityCompat.ShouldShowRequestPermissionRationale(this, Android.Manifest.Permission.WriteCalendar))
					ShowDialog(Resources.GetText(Resource.String.calendarAccessTitle), Resources.GetText(Resource.String.calendarAccessRationale), RequestPermissions);
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
				status.Text = Resources.GetText(Resource.String.calendarLoadingStatus);
				Calendar.LoadCalendars();
				if (Calendar.Calendars.Count == 0)
				{
					ShowDialog(Resources.GetText(Resource.String.createCalendarTitle), Resources.GetText(Resource.String.createCalendarMessage));
					return;
				}

				status.Text = Resources.GetText(Resource.String.facultiesLoadingStatus);
				//MainActivity.Faculties = await Parser.GetFaculties();	// TODO: Temp
			}
			catch (HttpRequestException e)
			{
				ShowDialog(e.Message, Resources.GetText(Resource.String.connectionFailMessage), Proceed, FinishAndRemoveTask, Resources.GetText(Resource.String.repeat), Resources.GetText(Resource.String.quit));
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
				ShowDialog(Resources.GetText(Resource.String.calendarAccessTitle), Resources.GetText(Resource.String.calendarAccessRationale), RequestPermissions);
		}

		private void RequestPermissions() =>
			ActivityCompat.RequestPermissions(this, new[]
			{
				Android.Manifest.Permission.ReadCalendar,
				Android.Manifest.Permission.WriteCalendar,
				Android.Manifest.Permission.Internet
			}, 0);

		private void ShowDialog(string title, string content, Action posAction = null, Action negAction = null, string posActionLabel = null, string negActionLabel = null)
		{
			Android.Support.V7.App.AlertDialog.Builder builder = new Android.Support.V7.App.AlertDialog.Builder(this);
			builder.SetMessage(content)
				.SetTitle(title).SetPositiveButton(posActionLabel ?? "OK", (s, e) => posAction?.Invoke());

			if (negAction != null)
				builder.SetNegativeButton(negActionLabel ?? Resources.GetText(Resource.String.close), (s, e) => negAction.Invoke());

			Android.Support.V7.App.AlertDialog dialog = builder.Create();
			dialog.Show();
		}

		public override void OnBackPressed() { }    // Disables back button
	}
}