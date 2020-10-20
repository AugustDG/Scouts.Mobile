using System;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Scouts.Fetchers;
using Scouts.Interfaces;
using Scouts.Services;
using Scouts.Settings;
using Scouts.View.Popups;
using Xamarin.Forms;
using Device = Xamarin.Forms.Device;

namespace Scouts
{
    public partial class App
    {
        public static INavigation Navigation { get; set; }

        public App()
        {
            try
            {
                MongoClient.Instance ??= new MongoClient();
                DropboxClient.Instance ??= new DropboxClient();
                AppSettings.Init();

                Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(
                    "MzM2ODc4QDMxMzgyZTMzMmUzMElHaTluVDJVMjdPYVdCVXMrR2tVWUJJMzN6Y1V6V1JaeTAzNG1mT1Evb0E9");
                
                InitializeComponent();
                
                OptionsDropdown.DropdownInstance ??= new OptionsDropdown();
            }
            catch (Exception e)
            {
                Crashes.TrackError(e);
            }
            finally
            {
                XF.Material.Forms.Material.Init(this);
                NotificationHubConnectionService.Init();
                DependencyService.Get<IMessagingService>().WipeToken();

                Device.SetFlags(new[]
                    {"Expander_Experimental", "Shapes_Experimental", "SwipeView_Experimental", "Brush_Experimental"});
                
                var rootPage = new NavigationPage(new MainPage());
                Navigation = rootPage.Navigation;
                MainPage = rootPage;
            }
        }

        protected override void OnStart()
        {
#if !DEBUG
            AppCenter.Start("android=26aa05e1-3027-45cd-9291-50bbceda8306;",
                typeof(Analytics), typeof(Crashes));
#endif
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}