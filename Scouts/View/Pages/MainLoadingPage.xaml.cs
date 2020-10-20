using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AppCenter.Analytics;
using Scouts.Dev;
using Scouts.Fetchers;
using Scouts.Settings;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using XF.Material.Forms.UI.Dialogs;

namespace Scouts.View.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainLoadingPage : ContentPage
    {
        public MainLoadingPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            AskMissingPermissions();
        }

        private async void AskMissingPermissions()
        {
            var canWriteToStorage =
                await MainThread.InvokeOnMainThreadAsync(Permissions.RequestAsync<Permissions.StorageWrite>);

            while (canWriteToStorage != PermissionStatus.Granted)
            {
                await Helpers.DisplayMessageAsync("Les permissions suivantes sont requises :)");
                try
                {
                    canWriteToStorage =
                        await MainThread.InvokeOnMainThreadAsync(Permissions
                            .RequestAsync<Permissions.StorageWrite>);
                }
                catch (Exception e)
                {
                    Helpers.DisplayMessage(e.Message);
                }
            }

            LoadingInvoked();
        }

        private async void LoadingInvoked()
        {
            if (await ShouldLoginAutomatically())
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

                ShowPrivacyAlert(false);
            }
            else ShowPrivacyAlert(true);
        }

        private async void ShowPrivacyAlert(bool showLogin)
        {
            var currentPrivacyAlert = await MaterialDialog.Instance.ConfirmAsync(
                "En continuant, vous acceptez que des données d'utilisation soient collectées afin d'améliorer l'application!",
                "Confidentialité 🔐",
                "Yep!", "Nope!", ColorSettings.DefaultMaterialAlertDialogConfiguration);

            if (!currentPrivacyAlert ?? false)
            {
                Analytics.TrackEvent("UserRejectedPrivacyAlert",
                    new Dictionary<string, string>
                    {
                        {"Device Model", DeviceInfo.Model},
                        {"Device Name", DeviceInfo.Name},
                        {"Time", DateTime.Now.ToShortTimeString()},
                        {"App Version", AppInfo.VersionString + "/" + AppInfo.BuildString}
                    });

                System.Diagnostics.Process.GetCurrentProcess().CloseMainWindow();
            }
            else
            {
                if (!showLogin)
                    while (Navigation.ModalStack.Count > 0)
                        Navigation.PopModalAsync(true);
                else
                    await Navigation.PopModalAsync(true);
            }
        }

        private Task<bool> ShouldLoginAutomatically()
        {
            return Task.Run(() =>
            {
                //TODO: Implement search by id
                AppSettings.CurrentUser = MongoClient.Instance.GetOneUserModelById(AppSettings.SavedUserId);

                if (AppSettings.CurrentUser == null) return false;
                else
                {
                    if (AppSettings.CurrentUser.Password != AppSettings.SavedPassword) return false;
                    else return true;
                }
            });
        }
    }
}