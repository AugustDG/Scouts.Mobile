using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using Acr.UserDialogs;
using MvvmHelpers;
using Scouts.Fetchers;
using Scouts.Models.Enums;
using Scouts.Security;
using Scouts.Services;
using Scouts.Settings;
using Scouts.View.Pages;
using Scouts.View.Popups;
using Xamarin.Forms;

namespace Scouts.ViewModels
{
    public class SettingsPageModel : BaseViewModel
    {
        public bool ScriptChange;

        public double CheckRotationPers { get; set; }
        public double CheckRotationAcc { get; set; }

        public Color ChosenColor
        {
            get => _chosenColor;
            set
            {
                SetProperty(ref _chosenColor, value);
                HasChanges = true;
            }
        }

        private Color _chosenColor = AppSettings.CurrentUser.Color;

        public bool HasChanges
        {
            get => _hasChanges;
            set => SetProperty(ref _hasChanges, value);
        }

        private bool _hasChanges;

        public bool IsSaveUsernameCheck
        {
            get => AppSettings.IsSaveUsername;
            set
            {
                if (value != AppSettings.IsSaveUsername) HasChanges = true;
                AppSettings.IsSaveUsername = value;
                SetProperty(ref _isSaveUsername, value);
            }
        }

        private bool _isSaveUsername;

        public bool IsLoginAutomaticCheck
        {
            get => AppSettings.IsLoginAutomatic;
            set
            {
                if (value != AppSettings.IsLoginAutomatic)
                {
                    /*UserDialogs.Instance.Toast(value
                            ? "En activant la rentrée automatique, vous recevrez toutes les notifications sélectionnées!"
                            : "En désactivant la rentrée automatique, vous ne recevrez plus les notifications choisies!",
                        TimeSpan.FromSeconds(2.5));*/

                    HasChanges = true;
                }

                IsSaveUsernameCheck = value;
                AppSettings.IsLoginAutomatic = value;
                SetProperty(ref _isLoginAutomatic, value);
            }
        }

        private bool _isLoginAutomatic;

        public string NewUsername
        {
            get => _newUsername;
            set => SetProperty(ref _newUsername, value);
        }

        private string _newUsername = AppSettings.CurrentUser.Username;

        public bool AllNotificationsChecked
        {
            get => _allNotificationsChecked;
            set => SetProperty(ref _allNotificationsChecked, value);
        }

        private bool _allNotificationsChecked;

        public ObservableCollection<bool> NotificationSubscriptions { get; set; } =
            new ObservableCollection<bool>(AppSettings.SavedNotificationSubscriptions);

        public Command<bool> AllNotifCheckedChangedCommand => new Command<bool>(AllNotifCheckedChanged);
        public Command OnNotifCheckedChangedCommand => new Command(OnCheckedChanged);
        public Command ShowColorPickerCommand => new Command(ShowColorPicker);
        public Command CloseColorPickerCommand => new Command(CloseColorPicker);
        public Command ChangeUsernameCommand => new Command(ChangeUsername);
        public Command ChangePasswordCommand => new Command(ChangePassword);
        public Command SaveChangesCommand => new Command(SaveChanges);
        public Command ExitSettingsCommand => new Command(ExitSettings);

        private SettingsPage _page;
        private ColorPickerPopup _colorPickerPopup;

        private bool _colorChanged;
        private bool _passwordChanged;
        private bool _usernameChanged;
        private bool _notificationsChanged;
        private string _newPassword = "";

        public SettingsPageModel(SettingsPage pg)
        {
            _page = pg;

            var isAllChecked = true;

            foreach (var subscription in NotificationSubscriptions)
            {
                if (!subscription)
                {
                    isAllChecked = false;
                    break;
                }
            }

            AllNotificationsChecked = isAllChecked;
        }

        public void ExpanderTapped(Expander exp, Image img)
        {
            img.RotateTo(exp.State == ExpanderState.Expanding ? 180 : 0, easing: Easing.SinInOut);
        }

        private void OnCheckedChanged()
        {
            var isAllChecked = true;

            foreach (var subscription in NotificationSubscriptions)
            {
                if (!subscription)
                {
                    isAllChecked = false;
                    break;
                }
            }

            ScriptChange = true;

            AllNotificationsChecked = isAllChecked;

            ScriptChange = false;

            _notificationsChanged = true;
            HasChanges = true;
        }

        private void AllNotifCheckedChanged(bool checkStatus)
        {
            for (var i = 0; i < NotificationSubscriptions.Count; i++)
            {
                NotificationSubscriptions[i] = checkStatus;
            }

            OnPropertyChanged(nameof(NotificationSubscriptions));
        }

        private async void ShowColorPicker()
        {
            ChosenColor = AppSettings.CurrentUser.Color;

            _colorPickerPopup ??= new ColorPickerPopup
            {
                BindingContext = this
            };

            await Shell.Current.Navigation.PushModalAsync(_colorPickerPopup, false);
        }

        private void CloseColorPicker()
        {
            _colorPickerPopup?.ClosePopup();

            ChangeColor();
        }

        private async void ChangeUsername()
        {
            var dialogPopup = new UserDialogPopup(false);

            var popupModel = (UserDialogPopupModel) dialogPopup.BindingContext;

            var isUsernameValid = false;

            popupModel.OnTextChangedCommand = new Command<string>(text =>
            {
                var rx = new Regex("^(?=.{2,20}$)(?![_.])(?!.*[_.]{2})[a-zA-Z0-9._]+(?<![_.])$");

                if (text is null) return;

                if (text != "" && rx.IsMatch(text))
                {
                    popupModel.TextColor = (Color) Application.Current.Resources["SyncPrimaryForegroundColor"];

                    isUsernameValid = true;
                }
                else
                {
                    popupModel.TextColor = (Color) Application.Current.Resources["SyncErrorColor"];

                    isUsernameValid = false;
                }
            });

            popupModel.SubmitTextEvent += (sender, args) =>
            {
                if (isUsernameValid)
                {
                    NewUsername = popupModel.EntryText;
                    HasChanges = true;
                    _usernameChanged = true;
                    popupModel.ExitPopupCommand.Execute(null);
                }
                else
                {
                    UserDialogs.Instance.Toast("Nom d'utilisateur invalide!", TimeSpan.FromSeconds(1.5));
                }
            };

            await Shell.Current.Navigation.PushModalAsync(dialogPopup, false);
        }

        private async void ChangePassword()
        {
            var dialogPopup = new UserDialogPopup(true);

            var popupModel = (UserDialogPopupModel) dialogPopup.BindingContext;

            var isPasswordValid = false;

            popupModel.OnTextChangedCommand = new Command<string>(text =>
            {
                var rx = new Regex("^(?=.{2,50}$)");

                if (text is null) return;

                if (text != "" && rx.IsMatch(text))
                {
                    popupModel.TextColor = (Color) Application.Current.Resources["SyncPrimaryForegroundColor"];

                    isPasswordValid = true;
                }
                else
                {
                    popupModel.TextColor = (Color) Application.Current.Resources["SyncErrorColor"];

                    isPasswordValid = false;
                }
            });

            popupModel.SubmitTextEvent += (sender, args) =>
            {
                if (isPasswordValid)
                {
                    _newPassword = popupModel.EntryText;
                    HasChanges = true;
                    _passwordChanged = true;
                    popupModel.ExitPopupCommand.Execute(null);
                }
                else
                {
                    UserDialogs.Instance.Toast("Mot de passe invalide!", TimeSpan.FromSeconds(1.5));
                }
            };

            await Shell.Current.Navigation.PushModalAsync(dialogPopup, false);
        }

        private void ChangeColor()
        {
            if (!ChosenColor.Equals(AppSettings.CurrentUser.Color))
            {
                AppSettings.CurrentUser.Color = ChosenColor;
                Application.Current.Resources["UserColor"] =  AppSettings.CurrentUser.Color;

                _colorChanged = true;
            }
        }

        private async void SaveChanges()
        {
            if (!HasChanges) return;

            //Resets HasChanges
            if (_isLoginAutomatic == AppSettings.IsLoginAutomatic) HasChanges = false;
            if (_isSaveUsername == AppSettings.IsSaveUsername) HasChanges = false;

            //Notifications saving
            if (_notificationsChanged)
            {
                var tagsAdd = new List<string>();
                var tagsRemove = new List<string>();

                for (var i = 0; i < NotificationSubscriptions.Count; i++)
                {
                    if (NotificationSubscriptions[i])
                    {
                        tagsAdd.Add(i == 7
                            ? $"NotificationType:{((TargetPublicType) i).ToString()}-{AppSettings.CurrentUser.UserId}"
                            : $"NotificationType:{((TargetPublicType) i).ToString()}");
                    }
                    else
                    {
                        tagsRemove.Add(i == 7
                            ? $"NotificationType:{((TargetPublicType) i).ToString()}-{AppSettings.CurrentUser.UserId}"
                            : $"NotificationType:{((TargetPublicType) i).ToString()}");
                    }
                }
                
                if (NotificationSubscriptions.Contains(true)) tagsAdd.Add($"NotificationType:{TargetPublicType.AllMessages.ToString()}");
                else tagsRemove.Add($"NotificationType:{TargetPublicType.AllMessages.ToString()}");

                InstallationService.CreateOrUpdateServerInstallation(tagsAdd, tagsRemove);

                AppSettings.SavedNotificationSubscriptions = NotificationSubscriptions.ToList();
                AppSettings.QueueSaveChangedObjects(new[] {nameof(AppSettings.SavedNotificationSubscriptions)});

                _notificationsChanged = false;
                HasChanges = false;
            }

            //Username saving
            if (_usernameChanged)
            {
                var nbUsersChanged = await MongoClient.Instance.UpdateUsersAsync(NewUsername, "Username",
                    $"UserId+{AppSettings.CurrentUser.UserId}", UpdateType.MatchSpecificField);

                AppSettings.CurrentUser.Username = NewUsername;

                if (nbUsersChanged != 1)
                    UserDialogs.Instance.Toast("Erreur dans le changement de nom", TimeSpan.FromSeconds(1.5));
                else
                {
                    _usernameChanged = false;
                    HasChanges = false;
                }
            }

            //Password saving
            if (_passwordChanged)
            {
                var newHashedPass = PasswordStorage.CreateHash(_newPassword);

                if (AppSettings.IsLoginAutomatic) AppSettings.CurrentUser.Password = newHashedPass;

                var nbUsersChanged = await MongoClient.Instance.UpdateUsersAsync(newHashedPass, "Password",
                    $"UserId+{AppSettings.CurrentUser.UserId}", UpdateType.MatchSpecificField);

                if (nbUsersChanged != 1)
                    UserDialogs.Instance.Toast("Erreur dans le changement du mot de passe");
                else
                {
                    _passwordChanged = false;
                    HasChanges = false;
                }
            }

            //Color saving
            if (_colorChanged)
            {
                var nbUsersChanged = await MongoClient.Instance.UpdateUsersAsync(AppSettings.CurrentUser.RGB, "RGB",
                    $"UserId+{AppSettings.CurrentUser.UserId}", UpdateType.MatchSpecificField);

                if (nbUsersChanged != 1)
                    UserDialogs.Instance.Toast("Erreur dans le changement de la couleur", TimeSpan.FromSeconds(1.5));
                else
                {
                    _colorChanged = false;
                    HasChanges = false;
                }
            }
        }

        private async void ExitSettings()
        {
            if (HasChanges)
            {
                var willQuit = await UserDialogs.Instance.ConfirmAsync(
                    "Êtes-vous sûr de vouloir quitter sans sauvegarder?",
                    "Attention", "Oui!", "Non...");

                if (willQuit) await Shell.Current.Navigation.PopModalAsync();
            }
            else
                await Shell.Current.Navigation.PopModalAsync();
        }
    }
}