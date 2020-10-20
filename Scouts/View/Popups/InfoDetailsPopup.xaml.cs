using Rg.Plugins.Popup.Services;
using Scouts.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
#pragma warning disable 4014

namespace Scouts.View.Popups
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class InfoDetailsPopup
    {
        public InfoDetailsPopup()
        {
            InitializeComponent();

            BindingContext = new InfoDetailsPopupModel(this);
            
            PopupCachedImage.BindingContext = BindingContext;
        }
    }
}