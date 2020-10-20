using System;
using Scouts.Interfaces;
using Scouts.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Scouts.View.Popups
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AddItemPopup
    {
        private AddItemPopupModel _pageModel;

        public AddItemPopup()
        {
            InitializeComponent();
            
            _pageModel = new AddItemPopupModel(this);
            BindingContext = _pageModel;
        }

        public void FocusContentLayout()
        {
            //ContentEditor.Focus();
        }
    }
}