using Xamarin.Forms;
using Xamarin.Forms.Xaml;
#pragma warning disable 4014

namespace Scouts.View.Popups
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ColorPickerPopup
    {
        public ColorPickerPopup()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            this.ScaleTo(1, 200, Easing.SinOut);
            await this.FadeTo(1, 200, Easing.SinOut);
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
        
        protected override bool OnBackgroundClicked()
        {
            ClosePopup();

            return false;
        }

        public async void ClosePopup()
        {
            this.ScaleTo(0.8, 100, Easing.SinIn);
            await this.FadeTo(0, 100, Easing.SinIn);
            Scale = 1.2;
            await Shell.Current.Navigation.PopModalAsync(false);
        }
    }
}