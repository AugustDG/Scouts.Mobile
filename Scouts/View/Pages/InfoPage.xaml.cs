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
    public partial class InfoPage 
    {
        private InfoPageModel _pageModel;

        public InfoPage()
        {
            InitializeComponent();

            _pageModel = new InfoPageModel(this);
            BindingContext = _pageModel;
            
            AppEvents.PageIndexChanged += OnPageIndexChanged;
        }

        private async void OnPageIndexChanged(object sender, EventArgs e)
        {
            if (DateTime.Now > _pageModel.LastRefreshed.Add(new TimeSpan(0, 15, 0)))
                _pageModel.RefreshCommand.Execute(null);
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

        private void InputView_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            _pageModel.SearchItemsCommand.Execute(e.NewTextValue);
        }
        
        private void SwipeView_OnItemTapped(object sender, EventArgs e)
        {
            _pageModel.ShowDetailsCommand.Execute((InfoModel) ((SwipeView) sender).BindingContext);
        }

        private void SwipeItemView_OnInvoked(object sender, EventArgs e)
        {
            _pageModel.CanShowDeets = false;
            _pageModel.DeleteItemCommand.Execute((InfoModel)((SwipeItemView) sender).BindingContext);
        }
    }
}