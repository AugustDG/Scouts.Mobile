using System.Collections.Generic;
using Microsoft.AppCenter.Analytics;
using Scouts.Fetchers;
using Scouts.Settings;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Scouts.View.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainLoadingPage : ContentPage
    {
        public MainLoadingPage()
        {
            InitializeComponent();
            
            LoadingInvoked();
        }

        private async void LoadingInvoked()
        {
            if (ShouldLoginAutomatically())
            {
                Analytics.TrackEvent("UserAutoSignedIn",
                    new Dictionary<string, string>
                    {
                        {"User", AppSettings.CurrentUser.Username + "/" + AppSettings.CurrentUser.UserId},
                        {"AccessCode", AppSettings.CurrentUser.AccessCodeUsed},
                        {"UserType", AppSettings.CurrentUser.UserType.ToString()},
                        {"App Version", AppInfo.VersionString + "/" + AppInfo.BuildString}
                    });

                Application.Current.Resources["UserColor"] = AppSettings.CurrentUser.Color;

                await Shell.Current.GoToAsync("///main");
            }
            else await Shell.Current.Navigation.PushModalAsync(new LoginPage());
        }

        private bool ShouldLoginAutomatically()
        {
            //TODO: Implement search by id
            AppSettings.CurrentUser = MongoClient.Instance.GetOneUserModel(AppSettings.CurrentUser?.Username);

            if (AppSettings.CurrentUser is null) return false;

            if (AppSettings.IsLoginAutomatic) return true;
            else return false;
        }
    }
}