using System;
using System.Threading.Tasks;
using Scouts.Dev;
using Scouts.ViewModels;
using Syncfusion.SfSchedule.XForms;
using Xamarin.Forms.Xaml;

namespace Scouts.View.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CalendarPage 
    {
        private CalendarPageModel _pageModel;

        public CalendarPage()
        {
            InitializeComponent();

            _pageModel = new CalendarPageModel(this);
            BindingContext = _pageModel;
        }

        protected async void OnAppearing()
        {
            if (DateTime.Now > _pageModel.lastRefreshed.Add(new TimeSpan(0, 15, 0)))
                _pageModel.RefreshCommand.Execute(null);
            else
            {
                _pageModel.IsRefreshing = true;
                await Task.Delay(Helpers.ArtificialWaitTime);   
                _pageModel.IsRefreshing = false;
            }
        }

        private void EventsSchedule_OnMonthInlineAppointmentTapped(object sender,
            MonthInlineAppointmentTappedEventArgs e)
        {
            _pageModel.DisplayPopup(e);
        }

        private void RefreshView_OnRefreshing(object sender, EventArgs e)
        {
            RefreshCircle.IsRefreshing = false;
        }
    }
}