using Android.App;
using Android.OS;
using System.Threading.Tasks;

namespace GUT.Schedule
{
    [Activity(Theme = "@style/AppTheme.NoActionBar")]
    public class ExportActivity : Activity
    {
        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.export_progress);

            await Task.Delay(5000);

            base.OnBackPressed();
        }

        public override void OnBackPressed() { }
    }
}