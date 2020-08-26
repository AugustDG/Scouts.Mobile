using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Scouts.Fetchers;
using Scouts.Interfaces;
using Scouts.Services;
using Scouts.Settings;
using Xamarin.Forms;
using Device = Xamarin.Forms.Device;

namespace Scouts
{
    public partial class App
    {
        public App()
        {
            InitializeComponent();

            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(
                "Mjg5NjUzQDMxMzgyZTMyMmUzMEJXVHg2SjdNMUxHelhORnFYZG5CUmN5WHN3em1BMXVEVWxqUEhtNFM1RTQ9");
            
            MongoClient.Instance ??= new MongoClient();
            AppSettings.Init();
            
            NotificationHubConnectionService.Init();
            DependencyService.Get<IDroidMessagingService>().WipeToken();

            Device.SetFlags(new[] {"Expander_Experimental", "Shapes_Experimental", "SwipeView_Experimental", "Brush_Experimental" });

            MainPage = new AppShell();
        }

        protected override void OnStart()
        {
            AppCenter.Start("android=26aa05e1-3027-45cd-9291-50bbceda8306;",
                typeof(Analytics), typeof(Crashes));
        }

        protected override void OnSleep()
        {
            
        }

        protected override void OnResume()
        {
        }
    }
}