
using Scouts.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Scouts.View.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoginPage
    {
        private LoginPageModel _page;
        
        public LoginPage()
        {
            InitializeComponent();
            
            _page = new LoginPageModel(this) {Navigation = Navigation};
            BindingContext = _page;
        }

        protected override bool OnBackButtonPressed()
        {
            return false;
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            _page.CheckMargin = width > height ? new Thickness(35, 0, 0, 0) : new Thickness(20, 0, 0, 0);

            base.OnSizeAllocated(width, height);
        }

        public async void AnimateEntry()
        {
            await SignInFrame.FadeTo(1.0, 300, Easing.CubicInOut);
        }
    }
}