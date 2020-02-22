using Microsoft.Services.Store.Engagement;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Resources;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;

namespace GUTSchedule.UWP
{
	public sealed partial class AboutPage : Page
	{
		private readonly ResourceLoader resources = ResourceLoader.GetForCurrentView();
		public AboutPage()
		{
			InitializeComponent();
			KeyDown += (s, e) =>
			{
				if (e.Key == VirtualKey.Back || e.Key == VirtualKey.GoBack)
					BackRequested(s, null);
			};
		}

		private async void Page_Loaded(object sender, RoutedEventArgs e)
		{
			PackageVersion ver = Package.Current.Id.Version;
			version.Text = $"v{ver.Major}{ver.Minor}.{ver.Revision} (ci-id #{ver.Build})";

			List<string> contributorsList = new List<string>();
			try
			{
				HttpClient client = new HttpClient();
				HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "https://api.github.com/repos/xfox111/gutschedule/contributors");
				request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:72.0) Gecko/20100101 Firefox/72.0");

				HttpResponseMessage response = await client.SendAsync(request);
				string resposeContent = await response.Content.ReadAsStringAsync();
				dynamic parsedResponse = JsonConvert.DeserializeObject(resposeContent);

				foreach (var i in parsedResponse)
					if (i.type == "User" && ((string)i.login).ToLower() != "xfox111")
						contributorsList.Add((string)i.login);

				request.Dispose();
				client.Dispose();
			}
			finally
			{
				if (contributorsList.Count > 0)
				{
					foreach(string i in contributorsList)
					{
						Hyperlink link = new Hyperlink
						{
							NavigateUri = new Uri("https://github.com.i")
						};
						link.Inlines.Add(new Run
						{
							Text = "@" + i
						});
						contributors.Inlines.Add(link);
						contributors.Inlines.Add(new Run
						{
							Text = ", "
						});
					}
					contributors.Inlines.RemoveAt(contributors.Inlines.Count - 1);

					contributorsTitle.Visibility = Visibility.Visible;
					contributors.Visibility = Visibility.Visible;
				}
			}
		}

		private async void Feedback_Click(object sender, RoutedEventArgs e)
		{
			if (StoreServicesFeedbackLauncher.IsSupported())
				await StoreServicesFeedbackLauncher.GetDefault().LaunchAsync();
			else
				await Launcher.LaunchUriAsync(new Uri("mailto:feedback@xfox111.net"));
		}

		private void BackRequested(object sender, RoutedEventArgs e) =>
			Frame.GoBack();
	}
}
