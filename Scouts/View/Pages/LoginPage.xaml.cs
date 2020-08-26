using Scouts.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Scouts.View
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoginPage
    {
        private LoginPageModel _pageModel;

        public LoginPage()
        {
            InitializeComponent();

            _pageModel = new LoginPageModel(this);
            BindingContext = _pageModel;
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            _pageModel.CheckMargin = width > height ? new Thickness(35,0, 0, 0) : new Thickness(20,0, 0, 0);
            base.OnSizeAllocated(width, height);
        }

        protected override bool OnBackButtonPressed()
        {
            if (Shell.Current.Navigation.ModalStack.Count > 0)
                return false;
            else
                return true;
        }

        public async void AnimateEntry()
        {
            await SignInFrame.FadeTo(1.0, 300, Easing.CubicInOut);
        }

        public async void CloseLogin()
        {
            await Shell.Current.GoToAsync("///main/infos");
            //Shell.Current.SendAppearing();
        }
    }
}