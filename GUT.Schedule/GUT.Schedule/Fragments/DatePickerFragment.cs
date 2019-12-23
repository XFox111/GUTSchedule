using System;
using Android.OS;
using Android.Widget;
using Android.App;
using System.Threading.Tasks;
using Android.Content;

namespace GUT.Schedule
{
    /// <summary>
    /// Date picker
    /// </summary>
    public class DatePickerFragment : Android.Support.V4.App.DialogFragment, DatePickerDialog.IOnDateSetListener
    {
        DateTime _date;
        bool dismissed = false;
        public override Dialog OnCreateDialog(Bundle savedInstanceState) =>
            new DatePickerDialog(Activity, this, _date.Year, _date.Month - 1, _date.Day);

        // Occures when user selected a date
        public void OnDateSet(DatePicker view, int year, int month, int dayOfMonth)
        {
            _date = view.DateTime;
            dismissed = true;
        }

        public override void OnCancel(IDialogInterface dialog)
        {
            base.OnCancel(dialog);
            dismissed = true;
        }

        /// <summary>
        /// Shows date picker and waits for user input
        /// </summary>
        /// <param name="manager">Fragment manager of the current activity (In most common cases it is <c>this.FragmentManager</c>)</param>
        /// <param name="date">Date which is to be selected by default</param>
        /// <returns><see cref="DateTime"/> picked by user</returns>
        public async Task<DateTime> GetDate(Android.Support.V4.App.FragmentManager manager, DateTime date)
        {
            _date = date;
            Show(manager, "datePicker");

            while (!dismissed) 
                await Task.Delay(100);

            return _date;
        }
    }
}