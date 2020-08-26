using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using Microsoft.AppCenter.Analytics;
using Scouts.Annotations;
using Scouts.Events;
using Scouts.Fetchers;
using Scouts.View;
using Scouts.View.Pages;
using Syncfusion.SfSchedule.XForms;
using Xamarin.Forms;

namespace Scouts.ViewModels
{
    public class CalendarPageModel : INotifyPropertyChanged
    {
        public ScheduleAppointmentCollection ScheduledEvents { get; } = new ScheduleAppointmentCollection();
        public DateTime lastRefreshed;

        public bool IsRefreshing
        {
            get => _isRefreshing;
            set
            {
                _isRefreshing = value;
                OnPropertyChanged(nameof(IsRefreshing));
            }
        }

        private bool _isRefreshing;

        public bool IsPopping
        {
            get => _isPopping;
            set
            {
                _isPopping = value;
                OnPropertyChanged(nameof(IsPopping));
            }
        }

        private bool _isPopping;

        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                OnPropertyChanged(nameof(Title));
            }
        }

        private string _title;

        public string Date
        {
            get => _date;
            set
            {
                _date = value;
                OnPropertyChanged(nameof(Date));
            }
        }

        private string _date;

        public string Details
        {
            get => _details;
            set
            {
                _details = value;
                OnPropertyChanged(nameof(Details));
            }
        }

        private string _details;

        public ImageSource PopupSource
        {
            get => _popupSource;
            set
            {
                _popupSource = value;
                OnPropertyChanged(nameof(PopupSource));
            }
        }

        private ImageSource _popupSource;

        public Command RefreshCommand => new Command(FetchAndDisplayCalendar);

        public Command ShowOptionsCommand => new Command(ShowOptionsDropdown);

        private CalendarPage _page;
        private InfoDetailsPopup _popup;

        public CalendarPageModel(CalendarPage pg)
        {
            _page = pg;

            AppEvents.WipeAllUserData += WipeAllData;
        }

        private void WipeAllData(object sender, EventArgs e)
        {
            ScheduledEvents.Clear();
            lastRefreshed = DateTime.Now.Subtract(new TimeSpan(0, 15, 0));
        }

        public async void DisplayPopup(MonthInlineAppointmentTappedEventArgs args)
        {
            ScheduleAppointment appointment = (ScheduleAppointment) args.Appointment;

            if (appointment is null)
                return;

            _popup ??= new InfoDetailsPopup {BindingContext = this};

            Title = appointment.Subject;
            Details = appointment.Notes;
            Date = appointment.StartTime.ToString("HH:mm", CultureInfo.CreateSpecificCulture("fr-FR")) + " - " +
                   appointment.EndTime.ToString("HH:mm", CultureInfo.CreateSpecificCulture("fr-FR"));
            PopupSource = null;

            await Shell.Current.Navigation.PushModalAsync(_popup, false);
        }

        private async void ShowOptionsDropdown()
        {
            OptionsDropdown.DropdownInstance ??= new OptionsDropdown();
            await Shell.Current.Navigation.PushModalAsync(OptionsDropdown.DropdownInstance, false);
        }

        #region CalendarLogic

        private async void FetchAndDisplayCalendar()
        {
            IsRefreshing = true;

            CalendarFetcher fetcher = new CalendarFetcher();

            var cal = await fetcher.GetCalendar();

            ScheduledEvents.Clear();

            lastRefreshed = DateTime.Now;

            for (int i = 0; i < 10; i++)
            {
                var app = new ScheduleAppointment
                {
                    Subject = cal.Events[i].Summary,
                    Notes = cal.Events[i].Description,
                    IsAllDay = cal.Events[i].IsAllDay
                };

                if (cal.Events[i].DtStart.AsSystemLocal == cal.Events[i].DtEnd.AsSystemLocal)
                {
                    Analytics.TrackEvent("Schedule Error: Same start and end time!",
                        new Dictionary<string, string>
                            {{"Event Name", cal.Events[i].Summary}, {"Event Id", cal.Events[i].Uid}});

                    continue;
                }

                app.StartTime = cal.Events[i].DtStart.AsSystemLocal;
                app.EndTime = cal.Events[i].DtEnd.AsSystemLocal;

                ScheduledEvents.Add(app);
            }

            IsRefreshing = false;
        }

        #endregion

        #region BindingImplementation

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}