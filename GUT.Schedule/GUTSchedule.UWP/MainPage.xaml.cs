using GUTSchedule.Models;
using System;
using Windows.Security.Credentials;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Windows.UI.Popups;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Storage;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace GUTSchedule.UWP
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class MainPage : Page
	{
		private readonly PasswordVault vault = new PasswordVault();
		private readonly ResourceLoader resources = ResourceLoader.GetForCurrentView();
		static readonly ApplicationDataContainer settings = ApplicationData.Current.LocalSettings;

		public static List<(string id, string name)> Faculties { get; set; }

		public MainPage()
		{
			InitializeComponent();
			startDate.Date = DateTime.Today;
			endDate.Date = startDate.Date.Value.AddDays(6);

			if (vault.FindAllByResource("xfox111.gutschedule") is IReadOnlyList<PasswordCredential> credentials && credentials.Count > 0)
			{
				email.Text = credentials.First().UserName;
				credentials.First().RetrievePassword();
				password.Password = credentials.First().Password;
			}

			authorize.IsChecked = (bool?)settings.Values["Authorize"] ?? true;
			rememberCredential.IsChecked = (bool?)settings.Values["RememberCredential"] ?? true;
			reminder.SelectedIndex = (int?)settings.Values["Reminder"] ?? 1;
			addGroupToTitle.IsChecked = (bool?)settings.Values["AddGroupToTitle"] ?? false;

			faculty.ItemsSource = Faculties.Select(i => new ComboBoxItem
			{
				Content = i.name,
				Tag = i.id
			});
			faculty.SelectedIndex = (int?)settings.Values["Faculty"] ?? 0;
			course.SelectedIndex = (int?)settings.Values["Course"] ?? 0;
		}

		private async void AppBarButton_Click(object sender, RoutedEventArgs e)
		{
			switch (((FrameworkElement)sender).Tag)
			{
				case "clear":
					break;
				case "about":
					Frame.Navigate(typeof(AboutPage));
					break;
				case "report":
					await Launcher.LaunchUriAsync(new Uri("mailto:feedback@xfox111.net"));
					break;
			}
		}

		private void ChangeAuthorizationMethod(object sender, RoutedEventArgs e)
		{
			if (authorize.IsChecked.Value)
			{
				credentialMethod.Visibility = Visibility.Visible;
				credentialMethod.Visibility = Visibility.Collapsed;
			}
			else
			{
				credentialMethod.Visibility = Visibility.Collapsed;
				credentialMethod.Visibility = Visibility.Visible;
			}
		}

		private void SetTodayDate(object sender, RoutedEventArgs e) =>
			startDate.Date = DateTime.Today;

		private void SetEndDate(object sender, RoutedEventArgs e) =>
			endDate.Date = startDate.Date.Value.AddDays((int)((FrameworkElement)sender).Tag);

		private void SetForSemester(object sender, RoutedEventArgs e)
		{
			if (DateTime.Today.Month == 1)
				endDate.Date = new DateTime(DateTime.Today.Year, 1, 31);
			else if (DateTime.Today.Month > 8)
				endDate.Date = new DateTime(DateTime.Today.Year + 1, 1, 31);
			else
				endDate.Date = new DateTime(DateTime.Today.Year, 8, 31);
		}

		private async void Export(object sender, RoutedEventArgs args)
		{
			errorPlaceholder.Visibility = Visibility.Collapsed;

			if (startDate.Date > endDate.Date)
			{
				errorPlaceholder.Text = resources.GetString("invalidDateRangeError");
				errorPlaceholder.Visibility = Visibility.Visible;
				return;
			}

			ExportParameters exportParameters;

			if (authorize.IsChecked.Value)
			{
				if (string.IsNullOrWhiteSpace(email.Text) || string.IsNullOrWhiteSpace(password.Password))
				{
					errorPlaceholder.Text = resources.GetString("invalidAuthorizationError");
					errorPlaceholder.Visibility = Visibility.Visible;
					return;
				}

				exportParameters = new CabinetExportParameters
				{
					Email = email.Text,
					Password = password.Password,
					EndDate = endDate.Date.Value.DateTime,
					StartDate = startDate.Date.Value.DateTime
				};

				if (rememberCredential.IsChecked.Value)
					vault.Add(new PasswordCredential("xfox111.gutschedule", email.Text, password.Password));
				else
					foreach (PasswordCredential credential in vault.FindAllByResource("xfox111.gutschedule"))
						vault.Remove(credential);
			}
			else
			{
				if (group.Items.Count < 1)
				{
					errorPlaceholder.Text = resources.GetString("groupSelectionError");
					errorPlaceholder.Visibility = Visibility.Visible;
					return;
				}

				exportParameters = new DefaultExportParameters
				{
					EndDate = endDate.Date.Value.DateTime,
					StartDate = startDate.Date.Value.DateTime,
					Course = (course.SelectedIndex + 1).ToString(),
					FacultyId = ((ComboBoxItem)faculty.SelectedItem).Tag as string,
					GroupId = ((ComboBoxItem)group.SelectedItem).Tag as string
				};
			}

			AddGroupToTitle = addGroupToTitle.IsChecked;
			SelectedCalendarIndex = calendar.SelectedItemPosition;
			Reminder = (reminder.SelectedIndex - 1) * 5;

			loading.Visibility = Visibility.Visible;
			try
			{
				status.Text = resources.GetString("loadingStatus");

				List<Occupation> schedule = await Parser.GetSchedule(exportParameters);

				status.Text = resources.GetString("calendarExportStatus");
				Calendar.Export(schedule);

				status.Text = resources.GetString("doneStatus");
				await Task.Delay(1000);
			}
			catch (HttpRequestException e)
			{
				MessageDialog dialog = new MessageDialog(resources.GetString("connectionFailMessage"), e.Message);
				dialog.Commands.Add(new UICommand(resources.GetString("repeat"), (command) => Export(sender, args)));
				dialog.Commands.Add(new UICommand("OK", (command) => loading.Visibility = Visibility.Collapsed));

				dialog.CancelCommandIndex = 1;
				dialog.DefaultCommandIndex = 0;

				await dialog.ShowAsync();
				return;
			}
			catch (Exception e)
			{
				MessageDialog dialog = new MessageDialog(e.Message, e.GetType().ToString());
				dialog.Commands.Add(new UICommand("OK", (command) => loading.Visibility = Visibility.Collapsed));

				await dialog.ShowAsync();
			}

			loading.Visibility = Visibility.Collapsed;
		}

		private void Faculty_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{

		}

		private void Course_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{

		}

		private async void UpdateGroupsList()
		{
			List<(string id, string name)> groups = await Parser.GetGroups(Faculties[faculty.SelectedIndex].id, (course.SelectedIndex + 1).ToString());
			group.ItemsSource = groups.Select(i => new ComboBoxItem
			{
				Content = i.name,
				Tag = i.id
			});

			group.SelectedIndex = (int?)settings.Values["Group"] ?? 0;
		}
	}
}