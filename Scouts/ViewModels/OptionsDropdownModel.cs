using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AppCenter.Analytics;
using MvvmHelpers;
using MvvmHelpers.Commands;
using Rg.Plugins.Popup.Services;
using Scouts.Events;
using Scouts.Interfaces;
using Scouts.Settings;
using Scouts.View.Pages;
using Scouts.View.Popups;
using Xamarin.Essentials;
using Xamarin.Forms;
using Command = Xamarin.Forms.Command;

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
        public AsyncCommand DisconnectCommand => new AsyncCommand(Disconnect);

        private readonly OptionsDropdown _page;
        private SettingsPage _settingsPage;

        public OptionsDropdownModel(OptionsDropdown optionsDropOut)
        {
            _page = optionsDropOut;
        }

        public void RefreshUsername()
        {
            CurrentUsername = AppSettings.CurrentUser.Username.ToUpper();
            CurrentUserType = AppSettings.CurrentUser.UserType.ToString().ToUpper();
            UserColor = (Color) AppSettings.CurrentUser?.Color;

            OnPropertyChanged(nameof(CurrentUsername));
            OnPropertyChanged(nameof(CurrentUserType));
        }

        private async void ShowSettingsPage()
        {
            _settingsPage = new SettingsPage();

            if (!App.Navigation.ModalStack.Contains(_settingsPage))
            {
                await App.Navigation.PushModalAsync(_settingsPage, true);
                await PopupNavigation.Instance.PopAllAsync();
            }
        }

        private async Task Disconnect()
        {
            DependencyService.Get<IMessagingService>().WipeToken();
            
            Analytics.TrackEvent("UserDisconnected",
                new Dictionary<string, string>
                {
                    {"User", AppSettings.CurrentUser.Username + "/" + AppSettings.CurrentUser.UserId},
                    {"UserType", AppSettings.CurrentUser.UserType.ToString()},
                    {"App Version", AppInfo.VersionString + "/" + AppInfo.BuildString}
                });

            //Clears personal user information
            AppSettings.CurrentUser = null;

            //Everything is cleared, except the installation id
            AppSettings.DeviceInstallation.Tags = null;
            AppSettings.DeviceInstallation.Platform = null;
            AppSettings.DeviceInstallation.PushChannel = null;
            
            AppSettings.QueueClearSavedObjects(new []{nameof(AppSettings.CurrentUser), nameof(AppSettings.SavedNotificationSubscriptions)});
            AppSettings.QueueSaveChangedObjects(new [] {nameof(AppSettings.DeviceInstallation)});

            App.Navigation.PopToRootAsync(false);
            PopupNavigation.Instance.PopAllAsync(false);
            await App.Navigation.PushModalAsync(new LoginPage(), true);
            
            AppEvents.WipeAllUserData.Invoke(this, EventArgs.Empty);
        }
    }
}