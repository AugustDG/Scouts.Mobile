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
        }

        protected override async void OnAppearing()
        {
            this.FadeTo(1, 200, Easing.SinOut);
            await this.TranslateTo(0, 0, 200, Easing.SinOut);
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
            this.FadeTo(0, 100, Easing.SinIn);
            await this.TranslateTo(0, 500, 100, Easing.SinOut);
            
            Shell.Current.Navigation.PopModalAsync(false);
        }
    }
}