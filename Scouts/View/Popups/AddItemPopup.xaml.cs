using System;
using Scouts.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
#pragma warning disable 4014

namespace Scouts.View.Popups
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AddItemPopup
    {
        public InfoPageModel InfoPageModel { get; set; }
        
        private AddItemPopupModel _pageModel;

        public AddItemPopup(InfoPageModel infoPageModel)
        {
            InitializeComponent();
            
            _pageModel = new AddItemPopupModel(this);
            BindingContext = _pageModel;

            InfoPageModel = infoPageModel;
        }
        
        protected override void OnAppearing()
        {
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
        
        private void DeetsExpander_OnTapped(object sender, EventArgs e)
        {
            _pageModel.ExpanderTapped((Expander) sender, DeetsExpanderImg);
        }
        
        private void ContentExpander_OnTapped(object sender, EventArgs e)
        {
            _pageModel.ExpanderTapped((Expander) sender, ContentExpanderImg);
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

        public void FocusContentLayout()
        {
            ContentEntry.Focus();
        }
    }
}