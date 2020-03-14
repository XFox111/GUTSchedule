using System;
using System.Linq;
using Windows.ApplicationModel.Activation;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using GUTSchedule.UWP.Pages;

namespace GUTSchedule.UWP
{
	/// <summary>
	/// Provides application-specific behavior to supplement the default Application class.
	/// </summary>
	sealed partial class App : Application
	{
		/// <summary>
		/// Initializes the singleton application object.  This is the first line of authored code
		/// executed, and as such is the logical equivalent of main() or WinMain().
		/// </summary>
		public App()
		{
			if ((new string[] { "ua", "ru", "by", "kz", "kg", "md", "lv", "ee" }).Contains(Windows.System.UserProfile.GlobalizationPreferences.Languages[0].Split('-')[0]))
				Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = "ru";
			else
				Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = "en";

			InitializeComponent();
			UnhandledException += OnError;
		}

		private async void OnError(object sender, UnhandledExceptionEventArgs e)
		{
			e.Handled = true;
			await new MessageDialog(e.Message, e.GetType().ToString()).ShowAsync();
		}

		/// <summary>
		/// Invoked when the application is launched normally by the end user.  Other entry points
		/// will be used such as when the application is launched to open a specific file.
		/// </summary>
		/// <param name="e">Details about the launch request and process.</param>
		protected override void OnLaunched(LaunchActivatedEventArgs e)
		{
			if (!(Window.Current.Content is Frame rootFrame))
				Window.Current.Content = rootFrame = new Frame();

			if (e.PrelaunchActivated == false)
			{
				if (rootFrame.Content == null)
					rootFrame.Navigate(typeof(MainPage), e.Arguments);

				Window.Current.Activate();
			}
		}
	}
}
