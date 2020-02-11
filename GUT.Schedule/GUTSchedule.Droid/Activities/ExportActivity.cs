using Android.App;
using Android.OS;
using Android.Widget;
using GUTSchedule.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace GUTSchedule.Droid.Activities
{
	/// <summary>
	/// Shows status of schedule export process
	/// </summary>
	[Activity(Theme = "@style/AppTheme.Light.SplashScreen")]
	public class ExportActivity : Activity
	{
		TextView status;

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			SetContentView(Resource.Layout.Export);

			status = FindViewById<TextView>(Resource.Id.status);

			Export();
		}

		private async void Export()
		{
			try
			{
				status.Text = Resources.GetText(Resource.String.loadingStatus);

				List<Occupation> schedule = await Parser.GetSchedule(MainActivity.ExportParameters);

				status.Text = Resources.GetText(Resource.String.calendarExportStatus);
				Calendar.Export(schedule);

				status.Text = Resources.GetText(Resource.String.doneStatus);
				await Task.Delay(1000);
			}
			catch (HttpRequestException e)
			{
				Android.Support.V7.App.AlertDialog.Builder builder = new Android.Support.V7.App.AlertDialog.Builder(this);
				builder.SetMessage(Resources.GetText(Resource.String.connectionFailMessage))
					.SetTitle(e.Message)
					.SetPositiveButton("ОК", (s, e) => base.OnBackPressed())
					.SetNegativeButton(Resources.GetText(Resource.String.repeat), (s, e) => Export());

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