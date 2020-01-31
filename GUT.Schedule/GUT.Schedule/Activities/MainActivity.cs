using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Preferences;
using Android.Support.V4.Text;
using Android.Support.V7.App;
using Android.Text.Method;
using Android.Views;
using Android.Widget;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using GUT.Schedule.Models;

namespace GUT.Schedule
{
    [Activity(Theme = "@style/AppTheme")]
    public class MainActivity : AppCompatActivity
    {
        Button start, end, export;
        Button forDay, forWeek, forMonth, forSemester;
        Spinner faculty, course, group, reminder, calendar;
        CheckBox groupTitle, authorize;
        TextView error;
        LinearLayout studentParams, profParams;
        EditText email, password;

        ISharedPreferences prefs;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            prefs = PreferenceManager.GetDefaultSharedPreferences(this);

            AssignVariables();

            faculty.SetList(this, Data.Faculties.Select(i => i.Name));
            int s = Data.Faculties.FindIndex(i => i.Id == prefs.GetString("Faculty", "-123"));
            faculty.SetSelection(s == -1 ? 0 : s);

            course.SetList(this, "1234".ToCharArray());
            course.SetSelection(prefs.GetInt("Course", 0)); // IDK why but this shit triggers events anyway (even if they are set in the next line. It seem to be that there's some asynchronous shit somewhere there)
            // P.S. Fuck Android

            AddEvents();

            // Settings spinners' dropdown lists content
            reminder.SetList(this, new[] 
            {
                "Нет",
                "Во время начала",
                "За 5 мин",
                "За 10 мин"
            });
            reminder.SetSelection(prefs.GetInt("Reminder", 0));

            calendar.SetList(this, Calendar.Calendars.Select(i => i.Name));
            s = Calendar.Calendars.FindIndex(i => i.Id == prefs.GetString("Calendar", "-123"));
            calendar.SetSelection(s == -1 ? 0 : s);

            end.Text = Data.EndDate.ToShortDateString();
            start.Text = Data.StartDate.ToShortDateString();

            groupTitle.Checked = prefs.GetBoolean("AddGroupToHeader", false);
            authorize.Checked = prefs.GetBoolean("Authorize", true);

            email.Text = prefs.GetString("email", "");
            password.Text = prefs.GetString("password", "");
        }

        private async void Export_Click(object sender, EventArgs e)
        {
            error.Visibility = ViewStates.Gone;

            if (Data.StartDate > Data.EndDate)
            {
                error.Text = "Ошибка: Неправильный диапазон дат";
                error.Visibility = ViewStates.Visible;
                return;
            }

            HttpClient client = null;
            bool? isProf = null;
            if(authorize.Checked)
            {
                Toast.MakeText(ApplicationContext, "Авторизация...", ToastLength.Short).Show();
                if (string.IsNullOrWhiteSpace(email.Text) || string.IsNullOrWhiteSpace(password.Text))
                {
                    error.Text = "Ошибка: Введите корректные учетные данные";
                    error.Visibility = ViewStates.Visible;
                    return;
                }

                export.Enabled = false;
                client = new HttpClient();

                await client.GetAsync("https://cabs.itut.ru/cabinet/");

                using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "https://cabs.itut.ru/cabinet/lib/autentificationok.php");
                request.SetContent(
                    ("users", email.Text),
                    ("parole", password.Text));

                HttpResponseMessage response = await client.SendAsync(request);
                string responseContent = await response.GetString();
                export.Enabled = true;

                if (!response.IsSuccessStatusCode)
                {
                    error.Text = $"Ошибка авторизации: {response.StatusCode}: {responseContent}";
                    error.Visibility = ViewStates.Visible;
                    return;
                }

                if (!responseContent.StartsWith("1", StringComparison.OrdinalIgnoreCase))
                {
                    error.Text = $"Ошибка авторизации: Неверный e-mail и/или пароль ({string.Join("; ", responseContent.Replace("error=", "", StringComparison.OrdinalIgnoreCase).Split('|'))})";
                    error.Visibility = ViewStates.Visible;
                    return;
                }

                export.Enabled = false;
                HttpResponseMessage verificationResponse = await client.GetAsync("https://cabs.itut.ru/cabinet/?login=yes");
                export.Enabled = true;
                IHtmlDocument doc = new HtmlParser().ParseDocument(await verificationResponse.GetString());
                if (doc.QuerySelectorAll("option").Any(i => i.TextContent.Contains("Сотрудник")))
                    isProf = true;
                else
                    isProf = false;

                Data.Groups = null;

                // Если ты это читаешь и у тебя возникли вопросы по типу "А какого хуя творится в коде ниже?!", то во-первых:
                // According to this SO thread: https://stackoverflow.com/questions/1925486/android-storing-username-and-password
                // I consider Preferences as safe enough method for storing credentials
                // А во-вторых, даже такой казалось бы небезопасный метод хранения учетных данных в сто раз надежнее того дерьма,
                // что творится на серверах Бонча (я не шучу, там все ОЧЕНЬ плохо)
                // Ну и в-третьих: Андроид - это пиздец и настоящий ад разработчика. И если бы была моя воля, я бы под него никогда не писал #FuckAndroid
                // З.Ы. Помнишь про второй пункт? Так вот, если ты используешь такой же пароль как в ЛК где-то еще, настоятельно рекомендую его поменять
                PreferenceManager.GetDefaultSharedPreferences(this).Edit().PutString("email", email.Text).Apply();
                PreferenceManager.GetDefaultSharedPreferences(this).Edit().PutString("password", password.Text).Apply();
            }
            else
            {
                if(Data.Groups.Count < 1)
                {
                    error.Text = "Ошибка: Не выбрана группа";
                    error.Visibility = ViewStates.Visible;
                    return;
                }
            }

            // Forming export parameters
            Data.DataSet = new DataSet
            {
                Faculty = Data.Faculties[faculty.SelectedItemPosition].Id,
                Group = Data.Groups?[group.SelectedItemPosition].Id,
                Course = course.SelectedItemPosition + 1,
                AddGroupToTitle = groupTitle.Checked,
                Calendar = Calendar.Calendars[calendar.SelectedItemPosition].Id,
                Reminder = (reminder.SelectedItemPosition - 1) * 5,
                HttpClient = client,
                IsProfessor = isProf
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

        private async void UpdateGroupsList()
        {
            if (course.SelectedItem == null)
                return;

            await Parser.LoadGroups(Data.Faculties[faculty.SelectedItemPosition].Id, course.SelectedItemPosition + 1);
            group.SetList(this, Data.Groups.Select(i => i.Name));

            int s = Data.Groups?.FindIndex(i => i.Id == prefs.GetString("Group", "-123")) ?? 0;
            group.SetSelection(s == -1 ? 0 : s);
        }

        private void SetDate(int days)
        {
            Data.EndDate = Data.StartDate.AddDays(days);
            end.Text = Data.EndDate.ToShortDateString();
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
                prefs.Edit().PutString("Faculty", Data.Faculties[e.Position].Id).Apply();
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
                prefs.Edit().PutString("Group", Data.Groups[e.Position].Id).Apply();

            groupTitle.Click += (s, e) =>
                prefs.Edit().PutBoolean("AddGroupToHeader", groupTitle.Checked).Apply();


            forDay.Click += (s, e) => SetDate(0);
            forWeek.Click += (s, e) => SetDate(6);
            forMonth.Click += (s, e) => SetDate(30);
            forSemester.Click += (s, e) =>
            {
                Data.EndDate = DateTime.Today.Month > 8 ? new DateTime(DateTime.Today.Year + 1, 1, 1) : new DateTime(DateTime.Today.Year, 8, 31);
                end.Text = Data.EndDate.ToShortDateString();
            };

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

        public void Clear(bool keepPrevious = true)
        {
            try
            {
                Toast.MakeText(ApplicationContext, "Очистка...", ToastLength.Short).Show();
                Calendar.Clear(keepPrevious);
                Toast.MakeText(ApplicationContext, "Готово!", ToastLength.Short).Show();
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
                    builder = new Android.Support.V7.App.AlertDialog.Builder(this);
                    builder.SetMessage(HtmlCompat.FromHtml(new StreamReader(Assets.Open("About.html")).ReadToEnd(), HtmlCompat.FromHtmlModeLegacy))
                        .SetTitle("ГУТ.Расписание")
                        .SetPositiveButton("ОК", (IDialogInterfaceOnClickListener)null);

                    dialog = builder.Create();
                    dialog.Show();

                    // Making links clickable
                    dialog.FindViewById<TextView>(Android.Resource.Id.Message).MovementMethod = LinkMovementMethod.Instance;
                    return true;
                case Resource.Id.email:
                    StartActivity(new Intent(Intent.ActionView, Android.Net.Uri.Parse("mailto:feedback@xfox111.net")));
                    return true;

                case Resource.Id.clear:
                    builder = new Android.Support.V7.App.AlertDialog.Builder(this);
                    builder.SetMessage("Это действие удалит экспортированное расписание из всех доступных календарей.\n" +
                        "Данное действие затронет только расписание, экспортированное этим приложением\n" +
                        "'Все' - удалит все события расписания, включая прошедшие\n" +
                        "'Только новые' - удалит будущие события расписания")
                        .SetTitle("Очистка календарей")
                        .SetPositiveButton("Только новые", (s, e) => Clear())
                        .SetNegativeButton("Все", (s, e) => Clear(false))
                        .SetNeutralButton("Отмена", (IDialogInterfaceOnClickListener)null);

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
