using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Crashes;
using MongoDB.Driver;
using MvvmHelpers;
using Scouts.Dev;
using Scouts.Events;
using Scouts.Fetchers;
using Scouts.Models;
using Scouts.Models.Enums;
using Scouts.Services;
using Scouts.View;
using Scouts.View.Pages;
using Scouts.View.Popups;
using Xamarin.Forms;
using FileSystem = Xamarin.Essentials.FileSystem;
using MongoClient = Scouts.Fetchers.MongoClient;

namespace Scouts.ViewModels
{
    public class InfoPageModel : BaseViewModel
    {
        public ObservableRangeCollection<InfoModel> InfoCollection { get; set; } =
            new ObservableRangeCollection<InfoModel>();

        public DateTime lastRefreshed;

        public FilterDefinition<InfoModel> CurrentFilterDefinition = new FilterDefinitionBuilder<InfoModel>().Empty;

        public bool IsSearching
        {
            get => _isSearching;
            set => SetProperty(ref _isSearching, value);
        }

        private bool _isSearching;

        public int InfoPublicType
        {
            get => _infoPublicType;
            set
            {
                SetProperty(ref _infoPublicType, value);
                if (!_scriptChange) FilterInfos();
            }
        }

        private int _infoPublicType = -1;

        public int InfoEventType
        {
            get => _infoEventType;
            set
            {
                SetProperty(ref _infoEventType, value);
                if (!_scriptChange) FilterInfos();
            }
        }

        private int _infoEventType = -1;

        public bool IsUrgent
        {
            get => _isUrgent;
            set
            {
                SetProperty(ref _isUrgent, value);
                if (!_scriptChange) FilterInfos();
            }
        }

        private bool _isUrgent;

        public Color FilterButtColor
        {
            get => _filterButtColor;
            set => SetProperty(ref _filterButtColor, value);
        }

        private Color _filterButtColor;

        public Command ShowAddItemCommand => new Command(ShowAddPopup);
        public Command ShowSearchCommand => new Command(ToggleSearch);
        public Command SearchItemsCommand => new Xamarin.Forms.Command<string>(SearchItems);
        public Command ShowFilterCommand => new Command(ShowFilter);
        public Command CloseFilterCommand => new Command(CloseFilter);
        public Command ClearFilterCommand => new Command(ClearFilter);
        public Command ShowDetailsCommand => new Xamarin.Forms.Command<InfoModel>(ShowDetailsPopup);
        public Command ShowOptionsCommand => new Command(ShowOptions);
        public Command RefreshCommand => new Command(RefreshNewsList);
        public Command LoadMoreItemsCommand => new Command(LoadMoreItems);
        public Command DeleteItemCommand => new Xamarin.Forms.Command<InfoModel>(DeleteItem);

        private InfoPage _page;
        private InfoDetailsPopup _detailsPopup;
        private AddItemPopup _addItemPopup;
        private FilterPopup _filterPopup;

        private int _iteration;
        private bool _scriptChange;

        public InfoPageModel(InfoPage pg)
        {
            _page = pg;

            FilterButtColor = (Color) Application.Current.Resources["SecondaryForegroundColor"];
            AppEvents.WipeAllUserData += WipeAllData;

            _filterPopup ??= new FilterPopup {BindingContext = this};
        }

        private void ToggleSearch()
        {
            if (IsSearching)
            {
                IsSearching = false;

                CurrentFilterDefinition = FilterDefinition<InfoModel>.Empty;

                RefreshNewsList();
            }
            else
            {
                IsSearching = true;
            }
        }

        private async void ShowOptions()
        {
            OptionsDropdown.DropdownInstance ??= new OptionsDropdown();
            await Shell.Current.Navigation.PushModalAsync(OptionsDropdown.DropdownInstance, false);
        }

        private async void ShowAddPopup()
        {
            _addItemPopup ??= new AddItemPopup(this);

            await NotificationHubConnectionService.ConnectAsync();

            await Shell.Current.Navigation.PushModalAsync(_addItemPopup, false);
        }

        private async void ShowDetailsPopup(InfoModel infoModel)
        {
            _detailsPopup ??= new InfoDetailsPopup {BindingContext = new InfoDetailsPopupModel()};

            ((InfoDetailsPopupModel) _detailsPopup.BindingContext).CurrentModel = infoModel;
            ((InfoDetailsPopupModel) _detailsPopup.BindingContext).Init();

            await Shell.Current.Navigation.PushModalAsync(_detailsPopup, false);
        }

        private async void ShowFilter()
        {
            _filterPopup ??= new FilterPopup {BindingContext = this};
            await Shell.Current.Navigation.PushModalAsync(_filterPopup, false);
        }

        private void CloseFilter()
        {
            _filterPopup.ClosePopup();
        }

        private void WipeAllData(object sender, EventArgs e)
        {
            InfoCollection.Clear();
            lastRefreshed = DateTime.Now.Subtract(new TimeSpan(0, 15, 0));
        }

        private async void DeleteItem(InfoModel modelToDelete)
        {
            var deleteResult =
                await MongoClient.Instance.NewsCollection.DeleteOneAsync(x => x.id == modelToDelete.id);

            if (deleteResult.DeletedCount == 1)
            {
                Helpers.DisplayMessage("Article supprimé avec succès!");
            }

            RefreshNewsList();
        }

        private async void RefreshNewsList()
        {
            IsBusy = true;

            InfoCollection.Clear();

            lastRefreshed = DateTime.Now;

            var foundDocs = await MongoClient.Instance.NewsCollection.Find(CurrentFilterDefinition).Limit(15)
                .SortByDescending(x => x.PostedTime).ToListAsync();

            var taskList = new List<Task>();

            foundDocs.ForEach((model) => { taskList.Add(GetInfoImages(model)); });

            await Task.WhenAll(taskList);

            InfoCollection.AddRange(foundDocs);

            _iteration = InfoCollection.Count;

            IsBusy = false;
        }

        private async void LoadMoreItems()
        {
            var foundDocs = await MongoClient.Instance.NewsCollection.Find(CurrentFilterDefinition)
                .Skip(_iteration).Limit(15)
                .SortByDescending(x => x.PostedTime).ToListAsync();

            var taskList = new List<Task>();

            foundDocs.ForEach((model) => { taskList.Add(GetInfoImages(model)); });

            await Task.WhenAll(taskList);

            InfoCollection.AddRange(foundDocs);

            _iteration = InfoCollection.Count;
        }

        private async Task GetInfoImages(InfoModel model)
        {
            string path = string.Empty;
            try
            {
                var folderName = model.id.ToString();

                path = $"{FileSystem.CacheDirectory}/{folderName}";

                if (!Directory.Exists(path))
                {
                    if (model.InfoAttachType == FileType.Image)
                    {
                        await DropboxClient.Instance.DownloadImage(folderName);

                        path = $"{FileSystem.CacheDirectory}/{folderName}/Image1.jpg";
                        model.Image = ImageSource.FromFile(path);
                    }
                }
                else
                {
                    model.Image = ImageSource.FromFile($"{path}/Image1.jpg");
                }
            }
            catch (Exception e)
            {
                Crashes.TrackError(e, new Dictionary<string, string> {{"Model Id", $"{model?.id}" }, { "Image Path", $"{path}" } }, ErrorAttachmentLog.AttachmentWithText(e.StackTrace, "StackTrace"));
            }
        }

        private void SearchItems(string query)
        {
            var textOptions = new TextSearchOptions
            {
                CaseSensitive = false,
                DiacriticSensitive = false,
                Language = "fr"
            };

            CurrentFilterDefinition = new FilterDefinitionBuilder<InfoModel>().Text(query, textOptions);

            RefreshNewsList();
        }

        private void FilterInfos()
        {
            FilterButtColor = (Color) Application.Current.Resources["SyncPrimaryColor"];

            var filterDefinitions = new List<FilterDefinition<InfoModel>>();

            if (InfoPublicType != -1)
                filterDefinitions.Add(new FilterDefinitionBuilder<InfoModel>().Eq(x => x.InfoPublicType,
                    (TargetPublicType) _infoPublicType));
            if (InfoEventType != -1)
                filterDefinitions.Add(new FilterDefinitionBuilder<InfoModel>().Eq(x => x.InfoEventType,
                    (EventType) _infoEventType));

            filterDefinitions.Add(new FilterDefinitionBuilder<InfoModel>().Eq(x => x.IsUrgent, _isUrgent));


            CurrentFilterDefinition = new FilterDefinitionBuilder<InfoModel>().And(filterDefinitions);

            RefreshNewsList();
        }

        private void ClearFilter()
        {
            _scriptChange = true;

            IsUrgent = false;
            InfoEventType = -1;
            InfoPublicType = -1;

            _scriptChange = false;

            FilterButtColor = (Color) Application.Current.Resources["SecondaryForegroundColor"];

            CurrentFilterDefinition = FilterDefinition<InfoModel>.Empty;

            RefreshNewsList();
        }
    }
}