using System.Collections.Generic;
using Windows.UI.Xaml.Controls;

namespace GUTSchedule.UWP.Controls
{
	public sealed partial class ClearCalendarControl : ContentDialog
	{
		public IEnumerable<object> SelectedCalendars => targetsList.SelectedItems;
		public bool ClearUpcomingOnly => clearUpcoming.IsChecked.Value;

		public ClearCalendarControl() =>
			InitializeComponent();

		private async void ContentDialog_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e) =>
			targetsList.ItemsSource = await Calendar.GetCalendars();
	}
}