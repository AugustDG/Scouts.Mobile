using System.Collections.Generic;
using MvvmHelpers;
using Scouts.Models;
using Stormlion.PhotoBrowser;
using Xamarin.Forms;

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

        public void Init()
        {
            var backColor = Color.FromHsla(CurrentModel.InfoBackColor.Hue, CurrentModel.InfoBackColor.Saturation,
                CurrentModel.InfoBackColor.Luminosity);
            InfoButtColor = backColor.WithLuminosity(0.9);

            ImageHeightRequest = CurrentModel.Image == null ? 0 : 175;
        }

        private void ShowImage()
        {
            if (CurrentModel.Image is null)
                return;

            new PhotoBrowser
            {
                Photos = new List<Photo>
                {
                    new Photo
                    {
                        Title = "",
                        URL = $"file://{CurrentModel.Image}"
                    }
                }
            }.Show();
        }
    }
}