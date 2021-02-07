using System;
using System.Linq;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Preferences;
using Android.Support.V7.App;
using Android.Text.Method;
using Android.Views;
using Android.Widget;
using GUTSchedule.Models;
using GUTSchedule.Droid.Fragments;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GUTSchedule.Droid.Activities
{
	[Activity]
	public class MainActivity : AppCompatActivity
	{
		public static ExportParameters ExportParameters { get; set; }
		public static List<(string id, string name)> Faculties { get; set; }
		public static List<(string id, string name)> Groups { get; set; }
		public static bool AddGroupToTitle { get; set; }
		public static int SelectedCalendarIndex { get; set; }
		public static int Reminder { get; set; }

		private List<(string, string)> _availableOccupations;
		private List<(string, string)> AvailableOccupations
		{
			get => _availableOccupations;
			set
			{
				_availableOccupations = value;
				applyForOccupation.Visibility = value.Count > 0 ? ViewStates.Visible : ViewStates.Gone;
			}
		}

		DateTime startDate = DateTime.Today;
		DateTime endDate = DateTime.Today.AddDays(7);

		Button start, end, export, applyForOccupation, validateCredential;
		Button forDay, forWeek, forMonth, forSemester;
		Spinner faculty, course, group, reminder, calendar;
		CheckBox groupTitle, authorize;
		TextView error;
		LinearLayout studentParams, profParams;
		EditText email, password;

		ISharedPreferences prefs;

		protected override async void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			SetContentView(Resource.Layout.Main);
			PackageInfo version = PackageManager.GetPackageInfo(PackageName, PackageInfoFlags.MatchAll);
			FindViewById<TextView>(Resource.Id.version).Text = $"v{version.VersionName} (ci-id #{version.VersionCode})";

			prefs = PreferenceManager.GetDefaultSharedPreferences(this);

			AssignVariables();

			/*faculty.SetList(this, Faculties.Select(i => i.name));
			int s = Faculties.FindIndex(i => i.id == prefs.GetString("Faculty", "-123"));
			faculty.SetSelection(s == -1 ? 0 : s);

			course.SetList(this, "1234".ToCharArray());
			course.SetSelection(prefs.GetInt("Course", 0)); // IDK why but this shit triggers events anyway (even if they are set in the next line. It seem to be that there's some asynchronous shit somewhere there)
															// P.S. Fuck Android

			await Task.Delay(100);
			UpdateGroupsList(); */	// TODO: Temp

			AddEvents();

			// Settings spinners' dropdown lists content
			reminder.SetList(this, new[]
			{
				Resources.GetText(Resource.String.noReminderOption),
				Resources.GetText(Resource.String.inTimeReminderOption),
				Resources.GetText(Resource.String.fiveMinuteReminderOption),
				Resources.GetText(Resource.String.tenMinuteReminderOption)
			});
			reminder.SetSelection(prefs.GetInt("Reminder", 0));

			calendar.SetList(this, Calendar.Calendars.Select(i => i.Name));
			int s = Calendar.Calendars.FindIndex(i => i.Id == prefs.GetString("Calendar", "-123"));
			calendar.SetSelection(s == -1 ? 0 : s);

			end.Text = endDate.ToShortDateString();
			start.Text = startDate.ToShortDateString();

			groupTitle.Checked = prefs.GetBoolean("AddGroupToHeader", false);
			authorize.Checked = true;// prefs.GetBoolean("Authorize", true);	// TODO: Temp

			email.Text = prefs.GetString("email", "");
			password.Text = prefs.GetString("password", "");

			try
			{
				AvailableOccupations = await Parser.CheckAvailableOccupations(email.Text, password.Text);
			}
			catch
			{
				AvailableOccupations = new List<(string, string)>();
			}
		}

		private void Export_Click(object sender, EventArgs e)
		{
			error.Visibility = ViewStates.Gone;

			if (startDate > endDate)
			{
				error.Text = Resources.GetText(Resource.String.invalidDateRangeError);
				error.Visibility = ViewStates.Visible;
				return;
			}

			if (authorize.Checked)
			{
				if (string.IsNullOrWhiteSpace(email.Text) || string.IsNullOrWhiteSpace(password.Text))
				{
					error.Text = Resources.GetText(Resource.String.invalidAuthorizationError);
					error.Visibility = ViewStates.Visible;
					return;
				}

				ExportParameters = new CabinetExportParameters
				{
					Email = email.Text,
					Password = password.Text,
					EndDate = endDate,
					StartDate = startDate
				};

				// Если ты это читаешь и у тебя возникли вопросы по типу "А какого хуя творится в коде ниже?!", то во-первых:
				// According to this SO thread: https://stackoverflow.com/questions/1925486/android-storing-username-and-password
				// I consider Preferences as safe enough method for storing credentials
				// А во-вторых, даже такой казалось бы небезопасный метод хранения учетных данных в сто раз надежнее того дерьма,
				// что творится на серверах Бонча (я не шучу, там пиздец)
				// Ну и в-третьих: Андроид - это пиздец и настоящий ад разработчика. И если бы была моя воля, я бы под него никогда не писал #FuckAndroid
				// З.Ы. Помнишь про второй пункт? Так вот, если ты используешь такой же пароль как в ЛК где-то еще, настоятельно рекомендую его поменять
				PreferenceManager.GetDefaultSharedPreferences(this).Edit().PutString("email", email.Text).Apply();
				PreferenceManager.GetDefaultSharedPreferences(this).Edit().PutString("password", password.Text).Apply();
			}
			else
			{
				if (Groups.Count < 1)
				{
					error.Text = Resources.GetText(Resource.String.groupSelectionError);
					error.Visibility = ViewStates.Visible;
					return;
				}

				ExportParameters = new DefaultExportParameters
				{
					EndDate = endDate,
					StartDate = startDate,
					Course = (course.SelectedItemPosition + 1).ToString(),
					FacultyId = Faculties[faculty.SelectedItemPosition].id,
					GroupId = Groups[group.SelectedItemPosition].id,
				};
			}

			AddGroupToTitle = groupTitle.Checked;
			SelectedCalendarIndex = calendar.SelectedItemPosition;
			Reminder = (reminder.SelectedItemPosition - 1) * 5;

			StartActivity(new Intent(this, typeof(ExportActivity)));
		}

		private async void End_Click(object sender, EventArgs e)
		{
			endDate = await new DatePickerFragment().GetDate(SupportFragmentManager, endDate);
			end.Text = endDate.ToShortDateString();
		}

		private async void Start_Click(object sender, EventArgs e)
		{
			startDate = await new DatePickerFragment().GetDate(SupportFragmentManager, startDate);
			start.Text = startDate.ToShortDateString();
		}

		private async void UpdateGroupsList()
		{
			if (course.SelectedItem == null || Faculties.Count < 1)
				return;

			Groups = await Parser.GetGroups(Faculties[faculty.SelectedItemPosition].id, (course.SelectedItemPosition + 1).ToString());
			group.SetList(this, Groups.Select(i => i.name));

			int s = Groups?.FindIndex(i => i.id == prefs.GetString("Group", "-123")) ?? 0;
			group.SetSelection(s == -1 ? 0 : s);
		}

		private void SetDate(int days)
		{
			endDate = startDate.AddDays(days);
			end.Text = endDate.ToShortDateString();
		}

		#region Init stuff
		private void AssignVariables()
		{
			start = FindViewById<Button>(Resource.Id.start);
			end = FindViewById<Button>(Resource.Id.end);
			export = FindViewById<Button>(Resource.Id.export);

			forDay = FindViewById<Button>(Resource.Id.forDay);
			forWeek = FindViewById<Button>(Resource.Id.forWeek);
			forMonth = FindViewById<Button>(Resource.Id.forMonth);
			forSemester = FindViewById<Button>(Resource.Id.forSemester);
			applyForOccupation = FindViewById<Button>(Resource.Id.applyForOccupation);
			validateCredential = FindViewById<Button>(Resource.Id.validateCredential);

			faculty = FindViewById<Spinner>(Resource.Id.faculty);
			course = FindViewById<Spinner>(Resource.Id.course);
			group = FindViewById<Spinner>(Resource.Id.group);
			reminder = FindViewById<Spinner>(Resource.Id.reminder);
			calendar = FindViewById<Spinner>(Resource.Id.calendar);

			error = FindViewById<TextView>(Resource.Id.error);

			groupTitle = FindViewById<CheckBox>(Resource.Id.groupTitle);
			authorize = FindViewById<CheckBox>(Resource.Id.authorization);

			studentParams = FindViewById<LinearLayout>(Resource.Id.studentParams);
			profParams = FindViewById<LinearLayout>(Resource.Id.professorParams);

			email = FindViewById<EditText>(Resource.Id.email);
			password = FindViewById<EditText>(Resource.Id.password);
		}

		private void AddEvents()
		{
			faculty.ItemSelected += (s, e) =>
			{
				prefs.Edit().PutString("Faculty", Faculties[e.Position].id).Apply();
				UpdateGroupsList();
			};
			course.ItemSelected += (s, e) =>
			{
				prefs.Edit().PutInt("Course", e.Position).Apply();
				UpdateGroupsList();
			};
			authorize.CheckedChange += (s, e) =>
			{
				prefs.Edit().PutBoolean("Authorize", e.IsChecked).Apply();
				if (e.IsChecked)
				{
					studentParams.Visibility = ViewStates.Gone;
					profParams.Visibility = ViewStates.Visible;
				}
				else
				{
					studentParams.Visibility = ViewStates.Visible;
					profParams.Visibility = ViewStates.Gone;
				}
			};
			calendar.ItemSelected += (s, e) =>
				prefs.Edit().PutString("Calendar", Calendar.Calendars[e.Position].Id).Apply();
			reminder.ItemSelected += (s, e) =>
				prefs.Edit().PutInt("Reminder", e.Position).Apply();
			group.ItemSelected += (s, e) =>
				prefs.Edit().PutString("Group", Groups[e.Position].id).Apply();

			groupTitle.Click += (s, e) =>
				prefs.Edit().PutBoolean("AddGroupToHeader", groupTitle.Checked).Apply();


			forDay.Click += (s, e) => SetDate(0);
			forWeek.Click += (s, e) => SetDate(6);
			forMonth.Click += (s, e) => SetDate(30);
			forSemester.Click += (s, e) =>
			{
				if (DateTime.Today.Month == 1)
					endDate = new DateTime(DateTime.Today.Year, 1, 31);
				else if (DateTime.Today.Month > 8)
					endDate = new DateTime(DateTime.Today.Year + 1, 1, 31);
				else
					endDate = new DateTime(DateTime.Today.Year, 8, 31);
				end.Text = endDate.ToShortDateString();
			};
			applyForOccupation.Click += async (s, e) =>
			{
				try
				{
					applyForOccupation.Visibility = ViewStates.Gone;
					var occupations = await Parser.CheckAvailableOccupations(email.Text, password.Text);
					await Parser.ApplyForOccupations(email.Text, password.Text, occupations);
					Toast.MakeText(ApplicationContext, Resources.GetText(Resource.String.attendSuccess), ToastLength.Short).Show();
				}
				catch (Exception ex)
				{
					Toast.MakeText(ApplicationContext, $"{Resources.GetText(Resource.String.attendFailed)}\n{ex.Message}", ToastLength.Short).Show();
				}
				AvailableOccupations = await Parser.CheckAvailableOccupations(email.Text, password.Text);
			};
			validateCredential.Click += async (s, e) =>
			{
				try
				{
					validateCredential.Enabled = false;
					await Parser.VaildateAuthorization(email.Text, password.Text);

					PreferenceManager.GetDefaultSharedPreferences(this).Edit().PutString("email", email.Text).Apply();
					PreferenceManager.GetDefaultSharedPreferences(this).Edit().PutString("password", password.Text).Apply();

					AvailableOccupations = await Parser.CheckAvailableOccupations(email.Text, password.Text);
					Toast.MakeText(ApplicationContext, Resources.GetText(Resource.String.validationSuccess), ToastLength.Short).Show();
				}
				catch (Exception ex)
				{
					Toast.MakeText(ApplicationContext, $"{Resources.GetText(Resource.String.validationFailed)}\n{ex.Message}", ToastLength.Short).Show();
				}
				validateCredential.Enabled = true;
			};

			start.Click += Start_Click;
			end.Click += End_Click;

			export.Click += Export_Click;
		}
		#endregion

		#region Menu stuff
		public override bool OnCreateOptionsMenu(IMenu menu)
		{
			MenuInflater.Inflate(Resource.Menu.MainContextMenu, menu);
			return true;
		}

		public void Clear(bool keepPrevious = true)
		{
			try
			{
				Toast.MakeText(ApplicationContext, Resources.GetText(Resource.String.clearingStatus), ToastLength.Short).Show();
				Calendar.Clear(keepPrevious);
				Toast.MakeText(ApplicationContext, Resources.GetText(Resource.String.doneStatus), ToastLength.Short).Show();
			}
			catch (Exception e)
			{
				Android.Support.V7.App.AlertDialog.Builder builder = new Android.Support.V7.App.AlertDialog.Builder(this);
				builder.SetMessage(e.Message)
					.SetTitle(e.GetType().ToString())
					.SetPositiveButton("ОК", (IDialogInterfaceOnClickListener)null);

				Android.Support.V7.App.AlertDialog dialog = builder.Create();
				dialog.Show();
			}
		}

		public override bool OnOptionsItemSelected(IMenuItem item)
		{
			Android.Support.V7.App.AlertDialog.Builder builder;
			Android.Support.V7.App.AlertDialog dialog;
			switch (item.ItemId)
			{
				case Resource.Id.about:
					StartActivity(new Intent(this, typeof(AboutActivity)));
					return true;
				case Resource.Id.email:
					StartActivity(new Intent(Intent.ActionView, Android.Net.Uri.Parse("mailto:feedback@xfox111.net")));
					return true;

				case Resource.Id.clear:
					builder = new Android.Support.V7.App.AlertDialog.Builder(this);
					builder.SetMessage(Resources.GetText(Resource.String.clearScheduleMessage))
						.SetTitle(Resources.GetText(Resource.String.clearScheduleTitle))
						.SetPositiveButton(Resources.GetText(Resource.String.clearUpcomingOption), (s, e) => Clear())
						.SetNegativeButton(Resources.GetText(Resource.String.clearAllOption), (s, e) => Clear(false))
						.SetNeutralButton(Resources.GetText(Resource.String.cancelOption), (IDialogInterfaceOnClickListener)null);

					dialog = builder.Create();
					dialog.Show();

					// Making links clickable
					dialog.FindViewById<TextView>(Android.Resource.Id.Message).MovementMethod = LinkMovementMethod.Instance;
					return true;
			}

			return base.OnOptionsItemSelected(item);
		}
		#endregion

		public override void OnBackPressed() =>
			FinishAffinity();   // Close application
	}
}
