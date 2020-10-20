using Rg.Plugins.Popup.Services;
using Scouts.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Scouts.View.Popups
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

        protected override async void OnAppearingAnimationBegin()
        {
            _pageModel.RefreshUsername();
            
            this.FadeTo(1, 200, Easing.SinInOut);
            await OptionsLayout.TranslateTo(0, 0, 200, Easing.SinInOut);
        }

        public async void ClosePopup()
        {
            this.FadeTo(0, 100, Easing.SinIn);
            await OptionsLayout.TranslateTo(0, -500, 100, Easing.SinInOut);
            await PopupNavigation.Instance.PopAsync(false);
        }
    }
}