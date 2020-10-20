using Android.App;
using Android.Content;
using Android.OS;

namespace Scouts.Droid
{
    [Activity(Theme = "@style/splashScreen", MainLauncher = true, NoHistory = true)]
    public class SplashActivity : Activity
    {
        public override void OnCreate(Bundle savedInstanceState, PersistableBundle persistableBundle)
        {
            base.OnCreate(savedInstanceState, persistableBundle);
        }

        public override void OnBackPressed() { }

        protected override void OnResume()
        {
            base.OnResume();
            StartActivity(new Intent(Application, typeof(MainActivity)));
        }
    }
}