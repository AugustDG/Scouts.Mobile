using System;
using Scouts.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Scouts.View.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SettingsPage
    {
        private SettingsPageModel _pageModel;
        
        public static SettingsPage Instance { get; set; }
        
        public SettingsPage()
        {
            InitializeComponent();
            
            _pageModel = new SettingsPageModel(this);

            BindingContext = _pageModel;
            
            Instance = this;
        }

        private void AccExpander_OnTapped(object sender, EventArgs e)
        {
            _pageModel.ExpanderTapped((Expander) sender, AccExpanderImg);
        }
        
        private void NotifExpander_OnTapped(object sender, EventArgs e)
        {
            _pageModel.ExpanderTapped((Expander) sender, NotifExpanderImg);
        }
        
        private void PersExpander_OnTapped(object sender, EventArgs e)
        {
            _pageModel.ExpanderTapped((Expander) sender, PersExpanderImg);
        }

        private void N_Switch_OnToggled(object sender, ToggledEventArgs e)
        {
            if (((Switch)sender).IsPlatformEnabled && !_pageModel.ScriptChange)
                _pageModel.AllNotifCheckedChangedCommand.Execute(e.Value);
        }
        
        private void SwitchArray_OnToggled(object sender, ToggledEventArgs e)
        {
            if (((Switch)sender).IsPlatformEnabled)
                _pageModel.OnNotifCheckedChangedCommand.Execute(null);
        }
    }
}