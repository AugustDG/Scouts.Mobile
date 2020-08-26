using Scouts.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
#pragma warning disable 4014

namespace Scouts.View.Popups
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class UserDialogPopup
    {
        private UserDialogPopupModel _model;
        
        public UserDialogPopup(bool isPassword)
        {
            InitializeComponent();
            
            _model = new UserDialogPopupModel(this);
            BindingContext = _model;

            if (isPassword)
            {
                PopupLabel.Text = "MOT DE PASSE";
                PopupEntry.Placeholder = "Mot de passe";
            }
            else
            {
                PopupLabel.Text = "NOM D'UTILISATEUR";
                PopupEntry.Placeholder = "Nom";
            }
            
            PopupEntry.IsPassword = isPassword;
        }
        
        protected override void OnAppearing()
        {
            OpenPopupAnimation();
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

        public async void OpenPopupAnimation()
        {
            this.ScaleTo(1, 200, Easing.SinOut);
            await this.FadeTo(1, 200, Easing.SinOut);
        }
        
        public async void ClosePopup()
        {
            this.ScaleTo(0.8, 100, Easing.SinIn);
            await this.FadeTo(0, 100, Easing.SinIn);
            Scale = 1;
            await Shell.Current.Navigation.PopModalAsync(false);
        }

        private void InputView_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            _model.OnTextChangedCommand?.Execute(e.NewTextValue);
        }
    }
}