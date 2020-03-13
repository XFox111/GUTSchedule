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
using Microsoft.Services.Store.Engagement;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Core;

namespace GUTSchedule.UWP
{
	public sealed partial class MainPage : Page
	{
		private readonly PasswordVault vault = new PasswordVault();
		private readonly ResourceLoader resources = ResourceLoader.GetForCurrentView();
		static readonly ApplicationDataContainer settings = ApplicationData.Current.LocalSettings;

		public MainPage() =>
			InitializeComponent();

		private async void Page_Loaded(object sender, RoutedEventArgs args)
		{
			try
			{
				PackageVersion ver = Package.Current.Id.Version;
				version.Text = $"v{ver.Major}{(ver.Minor < 1000 ? "0" + ver.Minor : ver.Minor.ToString())}.{ver.Revision} (ci-id #{ver.Build})";

				authorize.IsChecked = (bool?)settings.Values["Authorize"] ?? true;
				if (vault.RetrieveAll() is IReadOnlyList<PasswordCredential> credentials && credentials.Count > 0)
				{
					email.Text = credentials.First().UserName;
					credentials.First().RetrievePassword();
					password.Password = credentials.First().Password;
				}
				rememberCredential.IsChecked = (bool?)settings.Values["RememberCredential"] ?? true;

				faculty.ItemsSource = (await Parser.GetFaculties()).Select(i => new ComboBoxItem
				{
					Content = i.name,
					Tag = i.id,
					IsSelected = (string)settings.Values["Faculty"] == i.id
				}).ToList();
				faculty.SelectedIndex = (faculty.ItemsSource as List<ComboBoxItem>).FindIndex(i => i.IsSelected);
				if (faculty.SelectedIndex < 0)
					faculty.SelectedIndex = 0;
				course.SelectedIndex = (int?)settings.Values["Course"] ?? 0;

				startDate.Date = DateTime.Today;
				endDate.Date = startDate.Date.Value.AddDays(6);

				reminder.SelectedIndex = (int?)settings.Values["Reminder"] ?? 2;
				addGroupToTitle.IsChecked = (bool?)settings.Values["AddGroupToTitle"] ?? false;
			}
			catch (HttpRequestException e)
			{
				PushInternetExceptionMessage(e, () => Page_Loaded(sender, args));
				return;
			}
			catch (Exception e)
			{
				MessageDialog dialog = new MessageDialog(e.Message, e.GetType().ToString());
				dialog.Commands.Add(new UICommand("OK", (command) => CoreApplication.Exit()));

				await dialog.ShowAsync();
			}
		}

		private async void AppBarButton_Click(object sender, RoutedEventArgs e)
		{
			switch (((FrameworkElement)sender).Tag)
			{
				case "clear":
					ClearCalendarControl clearControl = new ClearCalendarControl();
					ContentDialogResult result = await clearControl.ShowAsync();
					if(result == ContentDialogResult.Primary)
					{
						await Calendar.Clear(clearControl.SelectedCalendars, clearControl.ClearUpcomingOnly);
						await new MessageDialog(resources.GetString("clearScheduleDone"), resources.GetString("clearScheduleTitle/Title")).ShowAsync();
					}
					break;
				case "about":
					Frame.Navigate(typeof(AboutPage));
					break;
				case "report":
					if (StoreServicesFeedbackLauncher.IsSupported())
						await StoreServicesFeedbackLauncher.GetDefault().LaunchAsync();
					else
						await Launcher.LaunchUriAsync(new Uri("mailto:feedback@xfox111.net"));
					break;
			}
		}

		private void ChangeAuthorizationMethod(object sender, RoutedEventArgs e)
		{
			if (credentialMethod == null)
				return;

			if (authorize.IsChecked.Value)
			{
				credentialMethod.Visibility = Visibility.Visible;
				defaultMethod.Visibility = Visibility.Collapsed;
			}
			else
			{
				credentialMethod.Visibility = Visibility.Collapsed;
				defaultMethod.Visibility = Visibility.Visible;
			}

			settings.Values["Authorize"] = authorize.IsChecked;
		}

		private void SetTodayDate(object sender, RoutedEventArgs e) =>
			startDate.Date = DateTime.Today;

		private void SetEndDate(object sender, RoutedEventArgs e) =>
			endDate.Date = startDate.Date.Value.AddDays(int.Parse(((FrameworkElement)sender).Tag as string));

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
					vault.Add(new PasswordCredential
					{
						UserName = email.Text,
						Password = password.Password,
						Resource = "xfox111.gutschedule"
					});
				else
					foreach (PasswordCredential credential in vault.RetrieveAll())
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

			loading.Visibility = Visibility.Visible;
			TopAppBar.Visibility = Visibility.Collapsed;
			try
			{
				status.Text = resources.GetString("loadingStatus");
				List<Occupation> schedule = await Parser.GetSchedule(exportParameters);

				status.Text = resources.GetString("calendarExportStatus");
				await Calendar.Export(schedule, addGroupToTitle.IsChecked.Value, (reminder.SelectedIndex - 1) * 5);

				status.Text = resources.GetString("doneStatus");
				await Task.Delay(1000);
			}
			catch (HttpRequestException e)
			{
				PushInternetExceptionMessage(e, () => Export(sender, args));
				return;
			}
			catch (Exception e)
			{
				MessageDialog dialog = new MessageDialog(e.Message, e.GetType().ToString());
				dialog.Commands.Add(new UICommand("OK", (command) => loading.Visibility = Visibility.Collapsed));

				await dialog.ShowAsync();
			}

			loading.Visibility = Visibility.Collapsed;
			TopAppBar.Visibility = Visibility.Visible;
		}

		private void Faculty_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			UpdateGroupsList();
			settings.Values["Faculty"] = ((ComboBoxItem)faculty.SelectedItem).Tag;
		}

		private void Course_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			UpdateGroupsList();
			settings.Values["Course"] = course.SelectedIndex;
		}

		private void Reminder_SelectionChanged(object sender, SelectionChangedEventArgs e) =>
			settings.Values["Reminder"] = reminder.SelectedIndex;

		private void Group_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if(group.SelectedItem != null)
				settings.Values["Group"] = ((ComboBoxItem)group.SelectedItem).Tag;
		}

		private void RememberCredential_Checked(object sender, RoutedEventArgs e) =>
			settings.Values["RememberCredential"] = rememberCredential.IsChecked;

		private void AddGroupToTitle_Checked(object sender, RoutedEventArgs e) =>
			settings.Values["AddGroupToTitle"] = rememberCredential.IsChecked;

		private async void UpdateGroupsList()
		{
			List<(string id, string name)> groups = await Parser.GetGroups(((ComboBoxItem)faculty.SelectedItem).Tag as string, (course.SelectedIndex + 1).ToString());
			group.ItemsSource = groups.Select(i => new ComboBoxItem
			{
				Content = i.name,
				Tag = i.id,
				IsSelected = (string)settings.Values["Group"] == i.id
			}).ToList();
			group.SelectedIndex = (group.ItemsSource as List<ComboBoxItem>).FindIndex(i => i.IsSelected);
			if (group.SelectedIndex < 0)
				group.SelectedIndex = 0;
		}

		public async void PushInternetExceptionMessage(HttpRequestException e, Action retryAction)
		{
			MessageDialog dialog = new MessageDialog(resources.GetString("connectionFailMessage"), e.Message);
			dialog.Commands.Add(new UICommand(resources.GetString("repeat"), (command) => retryAction()));
			dialog.Commands.Add(new UICommand("OK", (command) => loading.Visibility = Visibility.Collapsed));

			dialog.CancelCommandIndex = 1;
			dialog.DefaultCommandIndex = 0;

			await dialog.ShowAsync();
		}
	}
}

// TODO: Reminder prefs broken
// TODO: Calendar prefs broken
// TODO: Faculty prefs broken