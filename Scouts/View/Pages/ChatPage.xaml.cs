using Scouts.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Scouts.View.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ChatPage : ContentPage
    {
        private ChatPageModel _pageModel;
        
        public ChatPage()
        {
            InitializeComponent();
            
            _pageModel = new ChatPageModel();

            BindingContext = _pageModel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            
            _pageModel.ConnectToRoomCommand.Execute(null);
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            
            _pageModel.DisonnectFromRoomCommand.Execute(null);
        }
    }
}