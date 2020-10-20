using Rg.Plugins.Popup.Services;
using Scouts.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

#pragma warning disable 4014

namespace Scouts.View.Popups
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class FilterPopup
    {
        public FilterPopup()
        {
            InitializeComponent();

            BindingContext = new FilterPopupModel(this);
        }

        protected override async void OnAppearingAnimationBegin()
        {
            this.FadeTo(1, 200, Easing.SinOut);
            await this.TranslateTo(0, 0, 200, Easing.SinOut);
        }

        public async void ClosePopup()
        {
            this.FadeTo(0, 100, Easing.SinIn);
            await this.TranslateTo(0, 500, 100, Easing.SinOut);
            await PopupNavigation.Instance.PopAsync(false);
        }
    }
}