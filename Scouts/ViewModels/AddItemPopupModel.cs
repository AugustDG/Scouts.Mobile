using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AppCenter.Analytics;
using MvvmHelpers;
using Plugin.Media;
using Plugin.Media.Abstractions;
using Scouts.Dev;
using Scouts.Fetchers;
using Scouts.Models;
using Scouts.Models.Enums;
using Scouts.Services;
using Scouts.Settings;
using Scouts.View.Popups;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Scouts.ViewModels
{
    public class AddItemPopupModel : BaseViewModel
    {
        public string PostTitle { get; set; }

        public string PostContent { get; set; }

        public int InfoAttachType
        {
            get => _infoAttachType;
            set
            {
                SetProperty(ref _infoAttachType, value);
                FileTypeChanged();
            }
        }

        public int _infoAttachType;

        public int InfoEventType
        {
            get => _infoEventType;
            set => SetProperty(ref _infoEventType, value);
        }

        public int _infoEventType = -1;

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

        private ImageSource _buttonImage = "@drawable/add.png";

        public ImageSource ButtonImageDoc
        {
            get => _buttonImageDoc;
            set => SetProperty(ref _buttonImageDoc, value);
        }

        private ImageSource _buttonImageDoc;

        public Thickness ButtonMargin
        {
            get => _buttonMargin;
            set => SetProperty(ref _buttonMargin, value);
        }

        private Thickness _buttonMargin = new Thickness(0, 0, 15, 0);

        public bool HasFile
        {
            get => _hasFile;
            set => SetProperty(ref _hasFile, value);
        }

        private bool _hasFile;

        public bool HasImgDoc
        {
            get => _hasImgDoc;
            set => SetProperty(ref _hasImgDoc, value);
        }

        private bool _hasImgDoc;

        public Command PickCommand => new Command(Pick);

        public Command PickDocumentCommand => new Command(PickDocument);

        public Command SubmitNewsCommand => new Command(SubmitNews);

        public Command CancelCommand => new Command(_page.ClosePopup);

        public Command FinishedTitleCommand => new Command(_page.FocusContentLayout);

        private AddItemPopup _page;
        private byte[] _imgBytes;

        public AddItemPopupModel(AddItemPopup pg)
        {
            _page = pg;

            ButtonImage = "@drawable/image.png";
            ButtonImageDoc = "@drawable/document.png";
        }

        private void FileTypeChanged()
        {
            var type = (FileType) InfoAttachType;

            switch (type)
            {
                case FileType.None:
                    HasFile = false;
                    HasImgDoc = false;
                    break;
                case FileType.Image:
                    ButtonImage = "@drawable/image.png";

                    HasFile = true;
                    HasImgDoc = false;

                    ButtonMargin = new Thickness(0, 0, 25, 0);

                    break;
                case FileType.Document:
                    ButtonImage = "@drawable/document.png";

                    HasFile = true;
                    HasImgDoc = false;

                    ButtonMargin = new Thickness(0, 0, 25, 0);

                    break;
                case FileType.ImageAndDocument:
                    ButtonImage = "@drawable/image.png";
                    ButtonImageDoc = "@drawable/document.png";

                    HasFile = true;
                    HasImgDoc = true;

                    ButtonMargin = new Thickness(0, 0, 75, 0);

                    break;
            }
        }

        private void Pick()
        {
            var type = (FileType) InfoAttachType;

            switch (type)
            {
                case FileType.Image:
                    PickImage();
                    break;
                case FileType.Document:
                    PickDocument();
                    break;
                case FileType.ImageAndDocument:
                    PickImage();
                    break;
            }
        }

        private void PickDocument()
        {
            var type = (FileType) InfoAttachType;

            switch (type)
            {
                case FileType.Document:
                    ButtonImage = "@drawable/checkmark.png";
                    break;
                case FileType.ImageAndDocument:
                    ButtonImageDoc = "@drawable/checkmark.png";
                    break;
            }
        }

        private async void PickImage()
        {
            try
            {
                var file = await CrossMedia.Current.PickPhotoAsync(new PickMediaOptions
                {
                    CompressionQuality = 100,
                    PhotoSize = PhotoSize.MaxWidthHeight,
                    MaxWidthHeight = 900
                });

                await Task.Run(() =>
                {
                    if (file == null)
                        return;

                    using (var ms = new MemoryStream())
                    {
                        using (var str = file.GetStream())
                        {
                            str.CopyTo(ms);
                            _imgBytes = ms.ToArray();
                        }
                    }

                    ButtonImage = ImageSource.FromFile(file.Path);
                });
            }
            catch (Exception e)
            {
                await Shell.Current.DisplayAlert("Erreur", e.Message, "Ok");
            }
        }

        private async void SubmitNews()
        {
            IsBusy = true;
            
            Debug.WriteLine("Sending...");

            var titleRegex = new Regex("^(?=.{2,50}$)");
            var contentRegex = new Regex("^(?=.{2,500}$)");

            if (PostTitle is null || PostTitle.Length < 2)
            {
                Helpers.DisplayMessage("Titre Vide");
                return;
            }

            if (PostContent is null || PostContent.Length < 2)
            {
                Helpers.DisplayMessage("Contenu Vide");
                return;
            }

            if (!titleRegex.IsMatch(PostTitle))
            {
                Helpers.DisplayMessage("Titre Invalide");
                return;
            }

            if (!contentRegex.IsMatch(PostContent))
            {
                Helpers.DisplayMessage("Contenu Invalide");
                return;
            }

            if (InfoEventType == -1)
            {
                Helpers.DisplayMessage("SVP choisir un type d'évènement");
                return;
            }

            if (InfoPublicType == -1)
            {
                Helpers.DisplayMessage("SVP choisir un public concerné");
                return;
            }

            if ((FileType) InfoAttachType == FileType.Image ^
                (FileType) InfoAttachType == FileType.ImageAndDocument && _imgBytes is null)
            {
                Helpers.DisplayMessage("Aucun document choisi!");
                return;
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

            var folder = model.id.ToString();

            if (_imgBytes != null) await DropboxClient.Instance.UploadImage(_imgBytes, folder);

            Analytics.TrackEvent("AddedInfoItem",
                new Dictionary<string, string>
                {
                    {"User", AppSettings.CurrentUser.Username + "/" + AppSettings.CurrentUser.UserId},
                    {"UserType", AppSettings.CurrentUser.UserType.ToString()},
                    {"Title", PostTitle},
                    {"Time", DateTime.Now.ToShortTimeString()},
                    {"App Version", AppInfo.VersionString + "/" + AppInfo.BuildString}
                });

            try
            {
                await NotificationHubConnectionService.NotifyAsync(
                    $"INF-NOPIC^^^{AppSettings.CurrentUser.Username} vient de poster: {PostTitle}!^^^{PostContent}",
                    $"NotificationType:{((TargetPublicType) InfoPublicType).ToString()}");

                MongoClient.Instance.SetOneNewsModel(model);

                _imgBytes = null;
                PostTitle = string.Empty;
                PostContent = string.Empty;

                InfoAttachType = 0;
                InfoEventType = -1;
                InfoPublicType = 7;

                _page.InfoPageModel.RefreshCommand.Execute(null);

                Device.BeginInvokeOnMainThread(async () =>
                {
                    var isQuit = await Shell.Current.DisplayAlert("SUCCÈS", "Info envoyée avec succès :D", "OK!",
                        "Une autre!");

                    FileTypeChanged();

                    if (isQuit)
                    {
                        _page.ClosePopup();
                    }
                });
            }
            catch (Exception e)
            {
                Helpers.DisplayMessage(e.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }

        public void ExpanderTapped(Expander exp, Image img)
        {
            img.RotateTo(exp.State == ExpanderState.Expanding ? 180 : 0, easing: Easing.SinInOut);
        }
    }
}