using System;
using MvvmHelpers;
using Scouts.View.Popups;
using Xamarin.Forms;

namespace Scouts.ViewModels
{
    public class UserDialogPopupModel : BaseViewModel
    {
        public EventHandler SubmitTextEvent;
        
        public string EntryText
        {
            get => _entryText;
            set => SetProperty(ref _entryText, value);
        }

        private string _entryText;

        public Color TextColor
        {
            get => _textColor;
            set => SetProperty(ref _textColor, value);
        }

        private Color _textColor;

        public Command<string> OnTextChangedCommand { get; set; }
        public Command ExitPopupCommand => new Command(ExitPopup);
        public Command SubmitPopupCommand => new Command(SubmitPopup);

        private UserDialogPopup _page;
        
        public UserDialogPopupModel(UserDialogPopup pg)
        {
            _page = pg;
        }
        
        private void ExitPopup()
        {
            _page.ClosePopup();
        }

        private void SubmitPopup()
        {
            SubmitTextEvent.Invoke(this, EventArgs.Empty);
        }
    }
}