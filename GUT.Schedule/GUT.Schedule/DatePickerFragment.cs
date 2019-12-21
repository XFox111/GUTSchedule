using System;
using Android.OS;
using Android.Widget;
using Android.Support.V4.App;
using System.Threading.Tasks;

namespace GUT.Schedule
{
    public class DatePickerFragment : DialogFragment, Android.App.DatePickerDialog.IOnDateSetListener
    {
        DateTime date;
        bool dismissed = false;
        public override Android.App.Dialog OnCreateDialog(Bundle savedInstanceState)
        {
            DateTime now = DateTime.Today;
            return new Android.App.DatePickerDialog(Activity, this, now.Year, now.Month - 1, now.Day);
        }

        public void OnDateSet(DatePicker view, int year, int month, int dayOfMonth)
        {
            SetDate(view.DateTime);
        }

        public async Task<DateTime> GetDate(FragmentManager manager, string tag)
        {
            Show(manager, tag);

            while (!dismissed) 
                await Task.Delay(500);

            return date;
        }

        private void SetDate(DateTime date)
        {
            this.date = date;
            dismissed = true;
        }
    }
}