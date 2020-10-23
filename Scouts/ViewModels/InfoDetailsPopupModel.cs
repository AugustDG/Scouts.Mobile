using System;
using System.Collections.Generic;
using Microsoft.AppCenter.Crashes;
using MvvmHelpers;
using Rg.Plugins.Popup.Services;
using Scouts.Events;
using Scouts.Models;
using Scouts.Settings;
using Scouts.View.Popups;
using Stormlion.PhotoBrowser;
using Xamarin.Forms;
using Command = MvvmHelpers.Commands.Command;

namespace Scouts.ViewModels
{
    public class InfoDetailsPopupModel : BaseViewModel
    {
        public InfoModel CurrentModel
        {
            get => _currentModel;
            set => SetProperty(ref _currentModel, value);
        }

        private InfoModel _currentModel;

        public int ImageHeightRequest
        {
            get => _imageHeightRequest;
            set => SetProperty(ref _imageHeightRequest, value);
        }

        private int _imageHeightRequest;

        public Color InfoButtColor
        {
            get => _infoButtColor;
            set => SetProperty(ref _infoButtColor, value);
        }

        private Color _infoButtColor;

        public Command ShowImageCommand => new Command(ShowImage);
        public Command ClosePopupCommand => new Command(ClosePopup);

        private InfoDetailsPopup _popup;

        public InfoDetailsPopupModel(InfoDetailsPopup pop)
        {
            _popup = pop;
            
            AppEvents.OpenInfoDetails += LoadDetails;
        }
        
        
        public async void ClosePopup()
        {
            await PopupNavigation.Instance.PopAsync();
        }

        private void LoadDetails(object sender, InfoModel infoModel)
        {
            CurrentModel = infoModel;
            
            var backColor = Color.FromHsla(CurrentModel.InfoBackColor.Hue, CurrentModel.InfoBackColor.Saturation,
                CurrentModel.InfoBackColor.Luminosity);
            InfoButtColor = backColor.WithLuminosity(0.9);

            ImageHeightRequest = CurrentModel.Image == null ? 0 : 175;
        }

        private void ShowImage()
        {
            try
            {
                if (CurrentModel.Image?.AutomationId == null)
                    return;

                var uriSource = (UriImageSource) CurrentModel.Image;

                new PhotoBrowser
                {
                    Photos = new List<Photo>
                    {
                        new Photo
                        {
                            Title = "",
                            URL = uriSource.Uri.AbsoluteUri
                        }
                    }
                }.Show();
            }
            catch (Exception e)
            {
                Crashes.TrackError(e, new Dictionary<string, string>() {{CurrentModel.Title, CurrentModel.Image.ToString()}});
            }
        }
    }
}