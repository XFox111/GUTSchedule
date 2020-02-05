using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Support.V4.Text;
using Android.Support.V7.App;
using Android.Text.Method;
using Android.Widget;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace GUT.Schedule.Activities
{
    [Activity(Label = "@string/aboutTitle")]
    public class AboutActivity : AppCompatActivity
    {
        protected override async void OnCreate(Bundle savedInstanceState)
        {
            (string name, string handle, string link)[] contacts = new (string, string, string)[]
            {
                (Resources.GetText(Resource.String.websiteContact), "https://xfox111.net", "https://xfox111.net"),
                (Resources.GetText(Resource.String.twitterContact), "@xfox111", "https://twitter.com/xfox111"),
                (Resources.GetText(Resource.String.vkontakteContact), "@xfox.mike", "https://vk.com/xfox.mike"),
                ("LinkedIn", "@xfox", "https://linkedin.com/in/xfox"),
                ("GitHub", "@xfox111", "https://github.com/xfox111"),
            };
            (string name, string link)[] links = new (string, string)[]
            {
                (Resources.GetText(Resource.String.privacyPolicyLink), "https://xfox111.net/Projects/GUTSchedule/PrivacyPolicy.txt"),
                ("General Public License v3", "https://www.gnu.org/licenses/gpl-3.0"),
                (Resources.GetText(Resource.String.repositoryLink), "https://github.com/xfox111/gutschedule"),
                (Resources.GetText(Resource.String.notsLink), "http://tios.spbgut.ru/index.php"),
                (Resources.GetText(Resource.String.sutLink), "https://sut.ru"),
            };

            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.About);
            PackageInfo version = PackageManager.GetPackageInfo(PackageName, PackageInfoFlags.MatchAll);
            FindViewById<TextView>(Resource.Id.version).Text = $"v{version.VersionName} (ci-id #{version.VersionCode})";

            FindViewById<Button>(Resource.Id.feedback).Click += (s, e) => 
                StartActivity(new Intent(Intent.ActionView, Android.Net.Uri.Parse("mailto:feedback@xfox111.net")));

            FindViewById<TextView>(Resource.Id.contacts).SetText(
                HtmlCompat.FromHtml(string.Join("<br />", contacts.Select(i => $"<span>{i.name}:</span> <a href=\"{i.link}\">{i.handle}</a>")), HtmlCompat.FromHtmlModeLegacy),
                TextView.BufferType.Normal);
            FindViewById<TextView>(Resource.Id.contacts).MovementMethod = LinkMovementMethod.Instance;

            FindViewById<TextView>(Resource.Id.links).SetText(
                HtmlCompat.FromHtml(string.Join("<br />", links.Select(i => $"<a href=\"{i.link}\">{i.name}</a>")), HtmlCompat.FromHtmlModeLegacy),
                TextView.BufferType.Normal);
            FindViewById<TextView>(Resource.Id.links).MovementMethod = LinkMovementMethod.Instance;

            List<string> contributors = new List<string>();
            try
            {
                using HttpClient client = new HttpClient();
                using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, "https://api.github.com/repos/xfox111/gutschedule/contributors");
                request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:72.0) Gecko/20100101 Firefox/72.0");

                HttpResponseMessage response = await client.SendAsync(request);
                string resposeContent = await response.Content.ReadAsStringAsync();
                dynamic parsedResponse = JsonConvert.DeserializeObject(resposeContent);

                foreach (var i in parsedResponse)
                    if (i.type == "User" && ((string)i.login).ToLower() != "xfox111")
                        contributors.Add((string)i.login);
            }
            finally
            {
                if (contributors.Count > 0)
                {
                    FindViewById<TextView>(Resource.Id.contributors).SetText(
                        HtmlCompat.FromHtml(string.Join(", ", contributors.Select(i => $"<a href=\"https://github.com/{i}\">@{i}</a>")), HtmlCompat.FromHtmlModeLegacy),
                        TextView.BufferType.Normal);
                    FindViewById<TextView>(Resource.Id.contributors).MovementMethod = LinkMovementMethod.Instance;

                    FindViewById<TextView>(Resource.Id.contributorsTitle).Visibility = Android.Views.ViewStates.Visible;
                    FindViewById<TextView>(Resource.Id.contributors).Visibility = Android.Views.ViewStates.Visible;
                }
            }
        }
    }
}