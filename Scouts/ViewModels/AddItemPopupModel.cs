using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using MvvmHelpers;
using MvvmHelpers.Commands;
using Plugin.FilePicker;
using Rg.Plugins.Popup.Services;
using Scouts.Dev;
using Scouts.Events;
using Scouts.Fetchers;
using Scouts.Interfaces;
using Scouts.Models;
using Scouts.Models.Enums;
using Scouts.Services;
using Scouts.Settings;
using Scouts.View.Popups;
using Xamarin.Essentials;
using Xamarin.Forms;
using XF.Material.Forms.UI.Dialogs;
using Command = Xamarin.Forms.Command;

namespace Scouts.ViewModels
{
    public class AddItemPopupModel : BaseViewModel
    {
        public string PostTitle
        {
            get => _postTitle;
            set => SetProperty(ref _postTitle, value);
        }

        private string _postTitle = string.Empty;

        public string PostContent
        {
            get => _postContent;
            set => SetProperty(ref _postContent, value);
        }

        private string _postContent = string.Empty;

        public int InfoAttachType
        {
            get => _infoAttachType;
            set { SetProperty(ref _infoAttachType, value); }
        }

        public int _infoAttachType;

        public int InfoEventType
        {
            get => _infoEventType;
            set => SetProperty(ref _infoEventType, value);
        }

        public int _infoEventType = 3;

        public int InfoPublicType
        {
            get => _infoPublicType;
            set => SetProperty(ref _infoPublicType, value);
        }

        private int _infoPublicType = 7;

        public bool IsUrgent
        {
            get => _isUrgent;
            set => SetProperty(ref _isUrgent, value);
        }

        public bool _isUrgent;

        public ImageSource ButtonImage
        {
            get => _buttonImage;
            set => SetProperty(ref _buttonImage, value);
        }

        private ImageSource _buttonImage = "@drawable/add_100.png";

        public Aspect ImageAspect
        {
            get => _imageAspect;
            set => SetProperty(ref _imageAspect, value);
        }

        private Aspect _imageAspect = Aspect.AspectFit;

        public int CarouselPosition
        {
            get => _carouselPosition;
            set => SetProperty(ref _carouselPosition, value);
        }

        private int _carouselPosition = 1;

        #region Error Section

        public bool TitleHasError
        {
            get => _titleHasError;
            set => SetProperty(ref _titleHasError, value);
        }

        private bool _titleHasError;

        public string TitleErrorMsg
        {
            get => _titleErrorMsg;
            set => SetProperty(ref _titleErrorMsg, value);
        }

        private string _titleErrorMsg;

        public bool ContentHasError
        {
            get => _contentHasError;
            set => SetProperty(ref _contentHasError, value);
        }

        private bool _contentHasError;

        public string ContentErrorMsg
        {
            get => _contentErrorMsg;
            set => SetProperty(ref _contentErrorMsg, value);
        }

        private string _contentErrorMsg;

        #endregion Error Section

        public Command PickCommand => new Command(PickFile);
        public Command SubmitNewsCommand => new Command(SubmitNews);
        public AsyncCommand CancelCommand => new AsyncCommand(ClosePopupAsync);
        public Command FinishedTitleCommand => new Command(_page.FocusContentLayout);

        private AddItemPopup _page;
        private byte[] _docBytes;
        private bool _hasAsked;

        public AddItemPopupModel(AddItemPopup pg)
        {
            _page = pg;
        }

        private Task ClosePopupAsync()
        {
            return PopupNavigation.Instance.PopAsync(false);
        }

        private async void PickFile()
        {
            try
            {
                if (InfoAttachType == 1)
                {
                    InfoAttachType = 0;
                    _docBytes = null;
                    ButtonImage = "@drawable/add_100.png";
                    ImageAspect = Aspect.AspectFit;
                }

                var fileData = await CrossFilePicker.Current.PickFile(new[] {"image/*"});

                if (fileData == null)
                {
                    InfoAttachType = 0;
                    return;
                }

                InfoAttachType = 1;
                ImageAspect = Aspect.AspectFill;
                _docBytes = await DependencyService.Get<IImageResizerService>()
                    .ResizeImage(fileData.DataArray, 900, 300);

                if (_docBytes.Length == 1)
                {
                    InfoAttachType = 0;
                    return;
                }

                ButtonImage = ImageSource.FromStream(() => new MemoryStream(_docBytes));
            }
            catch (Exception e)
            {
                Crashes.TrackError(e);
            }
        }

        private async void SubmitNews()
        {
            IsBusy = true;

            TitleHasError = false;
            ContentHasError = false;

            var titleRegex = new Regex("^(?=.{2,500}$)");
            var contentRegex = new Regex("^(?=.{2,5000}$)");

            if (string.IsNullOrWhiteSpace(PostTitle))
            {
                TitleErrorMsg = "Titre Vide";
                TitleHasError = true;
                IsBusy = false;
                return;
            }

            if (string.IsNullOrWhiteSpace(PostContent))
            {
                ContentErrorMsg = "Contenu Vide";
                ContentHasError = true;
                IsBusy = false;
                return;
            }

            if (!titleRegex.IsMatch(PostTitle))
            {
                Helpers.DisplayMessage("Titre Invalide");
                TitleErrorMsg = "Titre Invalide!";
                TitleHasError = true;
                IsBusy = false;
                return;
            }

            if (!contentRegex.IsMatch(PostContent))
            {
                ContentErrorMsg = "Contenu Invalide";
                ContentHasError = true;
                IsBusy = false;
                return;
            }

            if (InfoEventType == 3 && InfoPublicType == 7 && !_hasAsked)
            {
                if (!await MaterialDialog.Instance.ConfirmAsync("Confirmation",
                    "Vous n'avez pas changé le type d'article, ni le public concerné: êtes-vous sûrs de procéder?",
                    "Oui!", "Peut-être pas...")?? false)
                {
                    CarouselPosition = 0;
                    IsBusy = false;
                    _hasAsked = true;
                    return;
                }
            }
            else if (InfoEventType == 3 && !_hasAsked)
            {
                if (!await MaterialDialog.Instance.ConfirmAsync("Confirmation",
                    "Vous n'avez pas changé le type d'article: êtes-vous sûrs de procéder?",
                    "Oui!", "Peut-être pas...") ?? false)
                {
                    CarouselPosition = 0;
                    IsBusy = false;
                    _hasAsked = true;
                    return;
                }
            }
            else if (InfoPublicType == 7 && !_hasAsked)
            {
                if (!await MaterialDialog.Instance.ConfirmAsync("Confirmation",
                    "Vous n'avez pas changé le public concerné: êtes-vous sûrs de procéder?",
                    "Oui!", "Peut-être pas...") ?? false)
                {
                    CarouselPosition = 0;
                    IsBusy = false;
                    _hasAsked = true;
                    return;
                }
            }

            if (InfoAttachType != 1 || _docBytes?.Length == 1)
            {
                InfoAttachType = 0;
                _docBytes = null;
            }

            var model = new InfoModel
            {
                Title = PostTitle,
                Summary = PostContent,
                PostedTime = DateTime.Now.ToLocalTime(),
                InfoPublicType = (TargetPublicType) InfoPublicType,
                InfoAttachType = (FileType) InfoAttachType,
                InfoEventType = (EventType) InfoEventType,
            };

            try
            {
                var folder = model.id.ToString();

                await DropboxClient.Instance.UploadImage(_docBytes, folder);

                await NotificationHubConnectionService.ConnectAsync();

                await NotificationHubConnectionService.NotifyAsync(
                    $"INF-NOPIC^^^{AppSettings.CurrentUser.Username} vient de poster: {PostTitle}!^^^{PostContent}",
                    $"NotificationType:{((TargetPublicType) InfoPublicType).ToString()}");

                MongoClient.Instance.SetOneNewsModel(model);

                Analytics.TrackEvent("AddedInfoItem",
                    new Dictionary<string, string>
                    {
                        {"User", AppSettings.CurrentUser.Username + "/" + AppSettings.CurrentUser.UserId},
                        {"UserType", AppSettings.CurrentUser.UserType.ToString()},
                        {"Title", PostTitle},
                        {"Time", DateTime.Now.ToShortTimeString()},
                        {"App Version", AppInfo.VersionString + "/" + AppInfo.BuildString}
                    });

                PostTitle = string.Empty;
                PostContent = string.Empty;

                InfoAttachType = 0;
                InfoEventType = 3;
                InfoPublicType = 7;

                _docBytes = null;
                ButtonImage = "@drawable/add_100.png";
                ImageAspect = Aspect.AspectFit;
                
                AppEvents.RefreshInfoFeed.Invoke(this, EventArgs.Empty);

                Device.BeginInvokeOnMainThread(async () =>
                {
                    var isQuit = await MaterialDialog.Instance.ConfirmAsync("SUCCÈS", "Info envoyée avec succès :D", "OK!",
                        "Une autre!");

                    if (isQuit ?? false)
                    {
                        await ClosePopupAsync();
                    }
                });
            }
            catch (Exception e)
            {
                await Helpers.DisplayMessageAsync(e.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}