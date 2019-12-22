using System;
using System.Linq;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using GUT.Schedule.Models;

namespace GUT.Schedule
{
    [Activity(Theme = "@style/AppTheme")]
    public class MainActivity : AppCompatActivity
    {
        Button start, end, export;
        Spinner faculty, course, group, reminder, calendar;
        CheckBox groupTitle;
        TextView error;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            AssignVariables();
            AddEvents();

            // Settings spinners' dropdown lists content
            faculty.SetList(this, Data.Faculties.Select(i => i.Name));
            course.SetList(this, "1234".ToCharArray());
            reminder.SetList(this, new string[] 
            {
                "Нет",
                "Во время начала",
                "За 5 мин",
                "За 10 мин"
            });
            calendar.SetList(this, Calendar.Calendars.Select(i => i.Name));

            end.Text = Data.EndDate.ToShortDateString();
            start.Text = Data.StartDate.ToShortDateString();
        }

        private void Export_Click(object sender, EventArgs e)
        {
            error.Visibility = ViewStates.Gone;

            if (Data.StartDate > Data.EndDate)
            {
                error.Text = "Ошибка: Неправильный диапазон дат";
                error.Visibility = ViewStates.Visible;
            }

            // Forming export parameters
            Data.DataSet = new DataSet
            {
                Faculty = Data.Faculties[faculty.SelectedItemPosition].Id,
                Group = Data.Groups[group.SelectedItemPosition].Id,
                Course = course.SelectedItemPosition + 1,
                AddGroupToTitle = groupTitle.Checked,
                Calendar = Calendar.Calendars[calendar.SelectedItemPosition].Id,
                Reminder = reminder.SelectedItemPosition switch
                {
                    1 => 0,
                    2 => 5,
                    3 => 10,
                    _ => null
                }
            };

            StartActivity(new Intent(this, typeof(ExportActivity)));
        }

        private async void End_Click(object sender, EventArgs e)
        {
            Data.EndDate = await new DatePickerFragment().GetDate(SupportFragmentManager, Data.EndDate);
            end.Text = Data.EndDate.ToShortDateString();
        }

        private async void Start_Click(object sender, EventArgs e)
        {
            Data.StartDate = await new DatePickerFragment().GetDate(SupportFragmentManager, Data.StartDate);
            start.Text = Data.StartDate.ToShortDateString();
        }

        private async void UpdateGroupsList(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            if (course.SelectedItem == null)
                return;

            await Parser.LoadGroups(Data.Faculties[faculty.SelectedItemPosition].Id, course.SelectedItemPosition + 1);
            group.SetList(this, Data.Groups.Select(i => i.Name));
        }

        #region Init stuff
        private void AssignVariables()
        {
            start = FindViewById<Button>(Resource.Id.start);
            end = FindViewById<Button>(Resource.Id.end);
            export = FindViewById<Button>(Resource.Id.export);

            faculty = FindViewById<Spinner>(Resource.Id.faculty);
            course = FindViewById<Spinner>(Resource.Id.course);
            group = FindViewById<Spinner>(Resource.Id.group);
            reminder = FindViewById<Spinner>(Resource.Id.reminder);
            calendar = FindViewById<Spinner>(Resource.Id.calendar);

            error = FindViewById<TextView>(Resource.Id.error);
            groupTitle = FindViewById<CheckBox>(Resource.Id.groupTitle);
        }

        private void AddEvents()
        {
            faculty.ItemSelected += UpdateGroupsList;
            course.ItemSelected += UpdateGroupsList;

            start.Click += Start_Click;
            end.Click += End_Click;

            export.Click += Export_Click;
        }
        #endregion

        #region Menu stuff
        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu_main, menu);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.ItemId == Resource.Id.github)
            {
                StartActivity(new Intent(Intent.ActionView, Android.Net.Uri.Parse("https://github.com/xfox111/GUTSchedule")));
                return true;
            }

            return base.OnOptionsItemSelected(item);
        }
        #endregion

        public override void OnBackPressed() { }    // Disables back button
    }
}