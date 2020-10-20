using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.AppCenter.Analytics;
using MvvmHelpers;
using Scouts.Dev;
using Scouts.Events;
using Scouts.Fetchers;
using Scouts.Models;
using Scouts.Security;
using Scouts.Services;
using Scouts.Settings;
using Scouts.View.Pages;
using Scouts.View.Popups;
using Xamarin.Essentials;
using Xamarin.Forms;
using XF.Material.Forms.UI.Dialogs;
using Command = MvvmHelpers.Commands.Command;

namespace Scouts.ViewModels
{
    public class LoginPageModel : BaseViewModel
    {
        public INavigation Navigation;
        
        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }

        private string _username;

        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        private string _password;

        public Thickness CheckMargin
        {
            get => _checkMargin;
            set => SetProperty(ref _checkMargin, value);
        }

        private Thickness _checkMargin;

        public string AccessCode
        {
            get => _accessCode;
            set => SetProperty(ref _accessCode, value);
        }

        private string _accessCode;

        public Command CheckUserCommand => new Command(CheckUser);
        public Command SubmitPassCommandSignIn => new Command(SubmitPassSignIn);
        public Command SubmitPassCommandSignUp => new Command(SubmitPassSignUp);
        public Command GoBackCommand => new Command(layout => AnimateLayoutInOut((Layout) layout, _page.SignInFrame));

        private LoginPage _page;

        public LoginPageModel(LoginPage pg)
        {
            _page = pg;
            
            AppSettings.Init();

            Username = AppSettings.CurrentUser?.Username;
            
            _page.AnimateEntry();
        }

        private void AnimateLayoutOutIn(Layout layOut, Layout layIn)
        {
            if (Device.Idiom == TargetIdiom.Phone)
            {
                layOut.TranslateTo(-750, 0, 550, Easing.CubicInOut).ContinueWith(x => { layOut.IsEnabled = false; });

                layIn.IsEnabled = true;
                layIn.TranslateTo(0, 0, 550, Easing.CubicInOut);
            }
            else if (Device.Idiom == TargetIdiom.Tablet)
            {
                layOut.TranslateTo(-1250, 0, 550, Easing.CubicInOut).ContinueWith(x => { layOut.IsEnabled = false; });

                layIn.IsEnabled = true;
                layIn.TranslateTo(0, 0, 550, Easing.CubicInOut);
            }
        }

        public void AnimateLayoutInOut(Layout layOut, Layout layIn)
        {
            if (Device.Idiom == TargetIdiom.Phone)
            {
                layOut.TranslateTo(750, 0, 550, Easing.CubicInOut).ContinueWith(x => { layOut.IsEnabled = false; });

                layIn.IsEnabled = true;
                layIn.TranslateTo(0, 0, 550, Easing.CubicInOut);
            }
            else if (Device.Idiom == TargetIdiom.Tablet)
            {
                layOut.TranslateTo(1250, 0, 550, Easing.CubicInOut).ContinueWith(x => { layOut.IsEnabled = false; });

                layIn.IsEnabled = true;
                layIn.TranslateTo(0, 0, 550, Easing.CubicInOut);
            }
        }

        private async void CheckUser()
        {
            IsBusy = true;

            _page.SignInUsernameLayout.HasError = false;
            _page.SignInUsernameLayout.ErrorText = "";

            var rx = new Regex("^(?=.{2,20}$)(?![_.])(?!.*[_.]{2})[a-zA-Z0-9._]+(?<![_.])$");

            if (Username is null)
            {
                _page.SignInUsernameLayout.HasError = true;
                _page.SignInUsernameLayout.ErrorText = "Nom d'utilisateur vide!";
                _page.SignInUsername.Focus();

                IsBusy = false;

                return;
            }

            if (!rx.IsMatch(Username))
            {
                _page.SignInUsernameLayout.HasError = true;
                _page.SignInUsernameLayout.ErrorText = "Nom d'utilisateur invalide!";
                _page.SignInUsername.Focus();

                IsBusy = false;

                return;
            }

            AppSettings.CurrentUser = await MongoClient.Instance.GetOneUserModelTask(Username);

            if (AppSettings.CurrentUser is null)
            {
                IsBusy = false;

                //TODO:fix this literature
                var willSignUp = await MaterialDialog.Instance.ConfirmAsync("Nom d'utilisateur inconnu 😮",
                    "Voulez-vous créer un compte?", "ye", "nah", configuration: ColorSettings.DefaultMaterialAlertDialogConfiguration);

                if (willSignUp ?? false) AnimateLayoutOutIn(_page.SignInFrame, _page.SignUpFrame);
            }
            else
            {
                IsBusy = false;

                if (Password is null) _page.SignInPassword.Focus();
                else SubmitPassCommandSignIn.Execute(null);
            }
        }

        private void SubmitPassSignIn()
        {
            IsBusy = true;

            _page.SignInPasswordLayout.HasError = false;
            _page.SignInPasswordLayout.ErrorText = "";

            var rx = new Regex("^(?=.{2,50}$)");

            if (Password is null)
            {
                _page.SignInPasswordLayout.HasError = true;
                _page.SignInPasswordLayout.ErrorText = "Mot de passe vide!";
                _page.SignInPassword.Focus();

                IsBusy = false;

                return;
            }

            if (!rx.IsMatch(Password))
            {
                _page.SignInPasswordLayout.HasError = true;
                _page.SignInPasswordLayout.ErrorText = "Mot de passe invalide!";
                _page.SignInPassword.Focus();

                IsBusy = false;

                return;
            }

            if (PasswordStorage.VerifyPassword(Password, AppSettings.CurrentUser.Password))
            {
                AppSettings.CurrentUser.Username = Username;

                InstallationService.CreateOrUpdateServerInstallation(new List<string>()
                {
                    "default",
                    $"UserId:{AppSettings.CurrentUser.UserId}",
                    $"UserType:{AppSettings.CurrentUser.UserType.ToString()}"
                });

                AppSettings.QueueSaveChangedObjects(new[] {nameof(AppSettings.CurrentUser)});

                Analytics.TrackEvent("UserSignedIn",
                    new Dictionary<string, string>
                    {
                        {"User", Username + "/" + AppSettings.CurrentUser.UserId},
                        {"AccessCode", AppSettings.CurrentUser.AccessCodeUsed},
                        {"UserType", AppSettings.CurrentUser.UserType.ToString()},
                        {"App Version", AppInfo.VersionString + "/" + AppInfo.BuildString}
                    });

                Device.BeginInvokeOnMainThread(async () =>
                {
                    Application.Current.Resources["UserColor"] = AppSettings.CurrentUser.Color;
                    AppEvents.SwitchHomePage (this, 0);
                    await App.Navigation.PopModalAsync(true);
                });
            }
            else
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    _page.SignInPasswordLayout.HasError = true;
                    _page.SignInPasswordLayout.ErrorText = "Mot de passe incorrect!";
                    _page.SignInPassword.Focus();
                });

                IsBusy = false;
            }
        }

        private async void SubmitPassSignUp()
        {
            IsBusy = true;

            _page.SignUpPasswordLayout.HasError = false;
            _page.SignUpPasswordLayout.ErrorText = "";

            _page.SignUpAccessLayout.HasError = false;
            _page.SignUpAccessLayout.ErrorText = "";

            var access = await MongoClient.Instance.GetOneAccessCodeModelTask(AccessCode);

            var rx = new Regex("^(?=.{2,50}$)");
            if (Username is null)
            {
                _page.SignUpUsernameLayout.HasError = true;
                _page.SignUpUsernameLayout.ErrorText = "Nom d'utilisateur vide!";
                _page.SignUpUsername.Focus();

                IsBusy = false;

                return;
            }

            if (Password is null)
            {
                _page.SignUpPasswordLayout.HasError = true;
                _page.SignUpPasswordLayout.ErrorText = "Mot de passe vide!";
                _page.SignUpPasswordLayout.Focus();

                IsBusy = false;

                return;
            }

            if (!rx.IsMatch(Password))
            {
                _page.SignUpPasswordLayout.HasError = true;
                _page.SignUpPasswordLayout.ErrorText = "Mot de passe invalide!";
                _page.SignUpPasswordLayout.Focus();

                IsBusy = false;

                return;
            }

            if (access is null)
            {
                _page.SignUpAccessLayout.HasError = true;
                _page.SignUpAccessLayout.ErrorText = "Code d'accès incorrect!";
                _page.SignUpAccess.Focus();

                IsBusy = false;
            }

            else
            {
                var testUserId = Helpers.GenerateRandomString(2);

                while (MongoClient.Instance.DoesUserExist(testUserId))
                {
                    testUserId = Helpers.GenerateRandomString(2);
                }

                AppSettings.CurrentUser = new UserDataModel
                {
                    Username = Username,
                    Password = PasswordStorage.CreateHash(Password),
                    AccessCodeUsed = access.AccessCode,
                    UserType = access.UserType,
                    UserId = testUserId,
                    CreatedOn = DateTime.Now,
                    Color = Helpers.GenerateRandomColor()
                };

                Application.Current.Resources["UserColor"] = AppSettings.CurrentUser.Color;

                InstallationService.CreateOrUpdateServerInstallation(new List<string>()
                {
                    "default",
                    $"UserId:{AppSettings.CurrentUser.UserId}",
                    $"UserType:{AppSettings.CurrentUser.UserType.ToString()}"
                });

                Analytics.TrackEvent("UserSignedUp",
                    new Dictionary<string, string>
                    {
                        {"User", Username + "/" + AppSettings.CurrentUser.UserId},
                        {"AccessCode", access.AccessCode},
                        {"UserType", access.UserType.ToString()},
                        {"App Version", AppInfo.VersionString + "/" + AppInfo.BuildString}
                    });

                await MongoClient.Instance.SetOneUserModelTask(AppSettings.CurrentUser);

                Device.BeginInvokeOnMainThread(async () =>
                {
                    AppEvents.SwitchHomePage (this, 0);
                    await App.Navigation.PopModalAsync(true);
                });
            }
        }
    }
}