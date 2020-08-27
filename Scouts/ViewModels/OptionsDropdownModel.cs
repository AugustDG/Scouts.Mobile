using System;
using System.Collections.Generic;
using Microsoft.AppCenter.Analytics;
using MvvmHelpers;
using Scouts.Events;
using Scouts.Interfaces;
using Scouts.Services;
using Scouts.Settings;
using Scouts.View;
using Scouts.View.Pages;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Scouts.ViewModels
{
    public class OptionsDropdownModel : BaseViewModel
    {
        public string CurrentUsername
        {
            get => _currentUsername;
            set
            {
                _currentUsername = value;
                SetProperty(ref _currentUsername, value);
            }
        }

        private string _currentUsername;

        public string CurrentUserType
        {
            get => _currentUserType;
            set
            {
                _currentUserType = value;
                SetProperty(ref _currentUserType, value);
            }
        }

        private string _currentUserType;

        public Color UserColor
        {
            get => _userColor;
            set => SetProperty(ref _userColor, value);
        }

        private Color _userColor;

        public Command ClosePopupCommand => new Command(_page.ClosePopup);
        public Command ShowSettingsPageCommand => new Command(ShowSettingsPage);
        public Command DisconnectCommand => new Command(Disconnect);

        private OptionsDropdown _page;

        public OptionsDropdownModel(OptionsDropdown optionsDropOut)
        {
            _page = optionsDropOut;
        }

        public void RefreshUsername()
        {
            CurrentUsername = AppSettings.CurrentUser.Username.ToUpper();
            CurrentUserType = AppSettings.CurrentUser.UserType.ToString().ToUpper();
            UserColor = AppSettings.CurrentUser?.Color ?? Color.Accent;

            OnPropertyChanged(nameof(CurrentUsername));
            OnPropertyChanged(nameof(CurrentUserType));
        }

        private async void ShowSettingsPage()
        {
            var settingsPage = new SettingsPage();

            await Shell.Current.Navigation.PushModalAsync(settingsPage);
        }

        private async void Disconnect()
        {
            if (!AppSettings.IsSaveUsername) AppSettings.CurrentUser.Username = null!;

            DependencyService.Get<IDroidMessagingService>().WipeToken();
            //InstallationService.DeleteServerInstallation(); 

            AppSettings.IsLoginAutomatic = false;
            AppSettings.CurrentUser.Password = null!;
            AppSettings.CurrentUser.UserId = null!;
            AppSettings.DeviceInstallation.Tags = null!;
            AppSettings.DeviceInstallation.Platform = null!;
            AppSettings.DeviceInstallation.PushChannel = null!;
            
            AppSettings.QueueClearSavedObjects(new []{nameof(AppSettings.CurrentUser), nameof(AppSettings.SavedNotificationSubscriptions)});
            AppSettings.QueueSaveChangedObjects(new [] {nameof(AppSettings.DeviceInstallation)});

            Analytics.TrackEvent("UserDisconnected",
                new Dictionary<string, string>
                {
                    {"User", AppSettings.CurrentUser.Username + "/" + AppSettings.CurrentUser.UserId},
                    {"UserType", AppSettings.CurrentUser.UserType.ToString()},
                    {"App Version", AppInfo.VersionString + "/" + AppInfo.BuildString}
                });

            await Shell.Current.Navigation.PopToRootAsync(false);
            await Shell.Current.Navigation.PushModalAsync(new LoginPage());

            AppEvents.WipeAllUserData.Invoke(this, EventArgs.Empty);
        }
    }
}