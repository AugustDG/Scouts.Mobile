using System;
using MvvmHelpers;
using Scouts.Events;
using Scouts.Models.Enums;
using Scouts.Settings;

namespace Scouts.ViewModels
{
    public class MainPageModel : BaseViewModel
    {
        public int SelectedPageIndex
        {
            get => _selectedPageIndex;
            set
            {
                AppEvents.PageIndexChanged.Invoke(this, EventArgs.Empty);
                SetProperty(ref _selectedPageIndex, value);
            }
        }

        private int _selectedPageIndex;

        public bool[] EnabledPages
        {
            get => _enabledPages;
            set => SetProperty(ref _enabledPages, value);
        }

        private bool[] _enabledPages = new bool[4];

        public MainPageModel()
        {
            for (var i = 0; i < 4; i++) EnabledPages[i] = true;

            AppEvents.SwitchHomePage += SwitchHomePage;
            AppEvents.UserTypeChanged += UserTypeChanged;
        }

        private void UserTypeChanged(object sender, EventArgs e)
        {
            switch (AppSettings.CurrentUser?.UserType)
            {
                case UserType.None:
                    break;
                case UserType.Parent:
                    EnabledPages[0] = true;
                    EnabledPages[1] = false;
                    EnabledPages[2] = false;
                    EnabledPages[3] = false;
                    break;
                case UserType.ChildC:
                    break;
                case UserType.ChildLL:
                    break;
                case UserType.ChildEG:
                    break;
                case UserType.Counselor:
                    EnabledPages[0] = true;
                    EnabledPages[1] = false;
                    EnabledPages[2] = true;
                    EnabledPages[3] = false;
                    break;
                case UserType.Clan:
                    break;
                case UserType.Admin:
                    EnabledPages[0] = true;
                    EnabledPages[1] = true;
                    EnabledPages[2] = true;
                    EnabledPages[3] = true;
                    break;
            }
        }

        private void SwitchHomePage(object sender, int targetPage)
        {
            SelectedPageIndex = targetPage;
        }
    }
}