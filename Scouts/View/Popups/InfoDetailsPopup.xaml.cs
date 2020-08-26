using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
#pragma warning disable 4014

namespace Scouts.View
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class InfoDetailsPopup
    {
        public InfoDetailsPopup()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            PopupCachedImage.BindingContext = BindingContext;
            
            AppearAnimation();
        }
        
        protected override bool OnBackButtonPressed()
        {
            if (Shell.Current.Navigation.ModalStack.Count > 0)
            {
                ClosePopup();
                return true;   
            }
            else
                return false;
        }

        public async void AppearAnimation()
        {
            this.ScaleTo(1, 200, Easing.SinOut);
            await this.FadeTo(1, 200, Easing.SinOut);
        }
        
        public async void ClosePopup()
        {
            this.ScaleTo(0.8, 100, Easing.SinIn);
            await this.FadeTo(0, 100, Easing.SinIn);
            Scale = 1.2;
            await Shell.Current.Navigation.PopModalAsync(false);
        }

        private void Button_OnClicked(object sender, EventArgs e)
        {
            ClosePopup();
        }
    }
}