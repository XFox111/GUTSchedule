using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace GUT.Schedule
{
    public static class Extensions
    {
        public static void SetList<T>(this Spinner spinner, Context context, IEnumerable<T> array)
        {
            ArrayAdapter adapter = new ArrayAdapter(context, Resource.Layout.support_simple_spinner_dropdown_item, array.ToList());
            spinner.Adapter = adapter;
        }
    }
}