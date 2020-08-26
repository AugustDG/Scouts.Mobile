using Scouts.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
#pragma warning disable 4014

namespace Scouts.View
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class OptionsDropdown
    {
        public static OptionsDropdown DropdownInstance;
        
        private OptionsDropdownModel _pageModel;
        
        public OptionsDropdown()
        {
            InitializeComponent();

            _pageModel = new OptionsDropdownModel(this);
            
            BindingContext = _pageModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            
            _pageModel.RefreshUsername();
            
            this.FadeTo(1, 200, Easing.SinInOut);
            await OptionsLayout.TranslateTo(0, 0, 200,Easing.SinInOut);
        }

        protected override bool OnBackButtonPressed()
        {
            if (Shell.Current.Navigation.ModalStack.Count >= 1)
            {
                ClosePopup();
                return true;
            }
            else
            {
                return false;
            }
        }
        
        public async void ClosePopup()
        {
            await OptionsLayout.TranslateTo(0, -500, 125, Easing.SinInOut);
            await Shell.Current.Navigation.PopModalAsync(false);
        }
    }
}