using System;
using System.Linq;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;

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

            faculty.SetList(this, Data.Faculties.Select(i => i.Name));
            course.SetList(this, "12345".ToCharArray());

            reminder.SetList(this, new string[] 
            {
                "Нет",
                "Во время начала",
                "За 5 мин",
                "За 10 мин"
            });
            calendar.SetList(this, Data.Calendars.Select(i => i.Name));

            end.Text = Data.EndDate.ToShortDateString();
            start.Text = Data.StartDate.ToShortDateString();
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu_main, menu);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            int id = item.ItemId;
            if (id == Resource.Id.github)
            {
                StartActivity(new Intent(Intent.ActionView, Android.Net.Uri.Parse("https://github.com/xfox111/GUTSchedule")));
                return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        private void Export_Click(object sender, EventArgs e)
        {
            error.Visibility = ViewStates.Gone;

            if (Data.StartDate > Data.EndDate)
            {
                error.Text = "Ошибка: Неправильный диапазон дат";
                error.Visibility = ViewStates.Visible;
            }


            StartActivity(new Intent(this, typeof(ExportActivity)));
        }

        private async void End_Click(object sender, EventArgs e)
        {
            DatePickerFragment picker = new DatePickerFragment();
            Data.EndDate = await picker.GetDate(SupportFragmentManager, "datePicker");
            end.Text = Data.EndDate.ToShortDateString();
        }

        private async void Start_Click(object sender, EventArgs e)
        {
            DatePickerFragment picker = new DatePickerFragment();
            Data.StartDate = await picker.GetDate(SupportFragmentManager, "datePicker");
            start.Text = Data.StartDate.ToShortDateString();
        }

        private async void UpdateGroupsList(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            if (course.SelectedItem == null)
                return;

            await Parser.LoadGroups(Data.Faculties[faculty.SelectedItemPosition].Id, course.SelectedItemPosition + 1);
            group.SetList(this, Data.Groups.Select(i => i.Name));
        }

        public override void OnBackPressed() { }

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
    }
}