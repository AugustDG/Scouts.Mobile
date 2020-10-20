using MvvmHelpers;
using Scouts.View.Pages;
using Scouts.View.Popups;
using Scouts.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Scouts
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPage
    {
        public MainPage()
        {
            InitializeComponent();

            BindingContext = new MainPageModel();

            Navigation.PushModalAsync(new LoginPage(), true);
            Navigation.PushModalAsync(new MainLoadingPage(), true);
        }
    }
}