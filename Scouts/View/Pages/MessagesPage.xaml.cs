using System;
using System.Threading.Tasks;
using Scouts.Dev;
using Scouts.Events;
using Scouts.Models;
using Scouts.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Scouts.View.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MessagesPage
    {
        private MessagesPageModel _pageModel;

        public MessagesPage()
        {
            InitializeComponent();

            _pageModel = new MessagesPageModel {Navigation = Navigation};
            BindingContext = _pageModel;

            AppEvents.PageIndexChanged += OnPageIndexChanged;
        }

        private async void OnPageIndexChanged(object sender, EventArgs e)
        {
            if (DateTime.Now > _pageModel.lastRefreshed.Add(new TimeSpan(0, 15, 0)))
                _pageModel.RefreshContactsCommand.Execute(null);
            else
            {
                _pageModel.IsBusy = true;
                await Task.Delay(Helpers.ArtificialWaitTime);   
                _pageModel.IsBusy = false;
            }
        }

        private void RefreshView_OnRefreshing(object sender, EventArgs e)
        {
            RefreshCircle.IsRefreshing = false;
        }

        private void CollectionView_OnItemTapped(object sender, EventArgs e)
        {
            _pageModel.EnterChatRoomCommand.Execute((ChatUserModel)((Frame)sender).BindingContext);
        }
    }
}