using Xamarin.Forms.Xaml;

namespace Scouts.View.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class GamesPage
    {
        public GamesPage()
        {
            InitializeComponent();

            //InfoList.ItemsSource = TestData.News;
        }
    }
}