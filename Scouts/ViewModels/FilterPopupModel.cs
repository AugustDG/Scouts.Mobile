using System;
using MvvmHelpers;
using Rg.Plugins.Popup.Services;
using Scouts.Events;
using Scouts.Settings;
using Scouts.View.Popups;
using Xamarin.Forms;
using Command = MvvmHelpers.Commands.Command;

namespace Scouts.ViewModels
{
    public class FilterPopupModel : BaseViewModel
    {
        public bool ScriptChange;

        public int InfoPublicType
        {
            get => _infoPublicType;
            set
            {
                SetProperty(ref _infoPublicType, value);
                if (!ScriptChange) FilterInfos();
            }
        }

        private int _infoPublicType = -1;

        public int InfoEventType
        {
            get => _infoEventType;
            set
            {
                SetProperty(ref _infoEventType, value);
                if (!ScriptChange) FilterInfos();
            }
        }

        private int _infoEventType = -1;

        public bool IsUrgent
        {
            get => _isUrgent;
            set
            {
                SetProperty(ref _isUrgent, value);
                if (!ScriptChange) FilterInfos();
            }
        }

        private bool _isUrgent;

        public Command CloseFilterCommand => new Command(_page.ClosePopup);
        public Command ClearFilterCommand => new Command(ClearFilter);

        private readonly FilterPopup _page;
        
        public FilterPopupModel(FilterPopup pg)
        {
            _page = pg;
        }
        
        private void FilterInfos()
        {
            var args = new FilterEventArgs(InfoEventType, InfoPublicType, IsUrgent);
            
            AppEvents.FilterInfos.Invoke(this, args);
        }

        private void ClearFilter()
        {
            AppEvents.ClearFilter.Invoke(this, EventArgs.Empty);
        }
    }
}