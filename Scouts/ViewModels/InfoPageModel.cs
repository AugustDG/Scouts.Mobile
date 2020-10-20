using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AppCenter.Crashes;
using MongoDB.Driver;
using MvvmHelpers;
using Rg.Plugins.Popup.Services;
using Scouts.Dev;
using Scouts.Events;
using Scouts.Fetchers;
using Scouts.Models;
using Scouts.Models.Enums;
using Scouts.Settings;
using Scouts.View.Pages;
using Scouts.View.Popups;
using Xamarin.Forms;
using FileSystem = Xamarin.Essentials.FileSystem;
using MongoClient = Scouts.Fetchers.MongoClient;
using Command = MvvmHelpers.Commands.Command;

namespace Scouts.ViewModels
{
    public class InfoPageModel : BaseViewModel
    {
        public ObservableRangeCollection<InfoModel> InfoCollection { get; set; } =
            new ObservableRangeCollection<InfoModel>();

        public DateTime LastRefreshed;
        
        public bool CanShowDeets = true;

        public FilterDefinition<InfoModel> CurrentFilterDefinition = new FilterDefinitionBuilder<InfoModel>().Empty;

        public bool IsSearching
        {
            get => _isSearching;
            set => SetProperty(ref _isSearching, value);
        }

        private bool _isSearching;
        
        public Color FilterButtColor
        {
            get => _filterButtColor;
            set => SetProperty(ref _filterButtColor, value);
        }

        private Color _filterButtColor;

        public Command ShowAddItemCommand => new Command(ShowAddPopup);
        public Command ShowSearchCommand => new Command(ToggleSearch);
        public Command SearchItemsCommand => new MvvmHelpers.Commands.Command<string>(SearchItems); 
        public Command ShowDetailsCommand => new MvvmHelpers.Commands.Command<InfoModel>(ShowDetailsPopup);
        public Command ShowFilterCommand => new Command(ShowFilter);
        public Command ShowOptionsCommand => new Command(ShowDrop);
        public Command RefreshCommand => new Command(RefreshNewsList);
        public Command LoadMoreItemsCommand => new Command(LoadMoreItems);
        public Command DeleteItemCommand => new MvvmHelpers.Commands.Command<InfoModel>(DeleteItem);

        private InfoPage _page;
        private InfoDetailsPopup _detailsPopup;
        private AddItemPopup _addItemPopup;
        private FilterPopup _filterPopup;

        private int _iteration;

        public InfoPageModel(InfoPage pg)
        {
            _page = pg;
            
            FilterButtColor = (Color) Application.Current.Resources["SecondaryForegroundColor"];

            AppEvents.WipeAllUserData += WipeAllData;

            _filterPopup ??= new FilterPopup ();

            AppEvents.FilterInfos += FilterInfos;
            AppEvents.ClearFilter += ClearFilter;

            _detailsPopup ??= new InfoDetailsPopup();

            AppEvents.RefreshInfoFeed += delegate { RefreshNewsList(); };
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

        private async void ShowDrop()
        {
            if (!PopupNavigation.Instance.PopupStack.Contains(OptionsDropdown.DropdownInstance))
                await PopupNavigation.Instance.PushAsync(OptionsDropdown.DropdownInstance);
        }

        private async void ShowAddPopup()
        {
            IsBusy = true;
            
            _addItemPopup = new AddItemPopup();
            
            if (!PopupNavigation.Instance.PopupStack.Contains(_addItemPopup))
                await PopupNavigation.Instance.PushAsync(_addItemPopup);

            IsBusy = false;
        }

        private async void ShowDetailsPopup(InfoModel infoModel)
        {
            if (!PopupNavigation.Instance.PopupStack.Contains(_detailsPopup) && CanShowDeets)
            {
                AppEvents.OpenInfoDetails.Invoke(this, infoModel);
                await PopupNavigation.Instance.PushAsync(_detailsPopup);
            }
        }

        private async void ShowFilter()
        {
            if (!PopupNavigation.Instance.PopupStack.Contains(_filterPopup))
                await PopupNavigation.Instance.PushAsync(_filterPopup);
        }

        private void WipeAllData(object sender, EventArgs e)
        {
            InfoCollection.Clear();
            LastRefreshed = DateTime.Now.Subtract(new TimeSpan(0, 15, 0));
        }

        private async void DeleteItem(InfoModel modelToDelete)
        {
            var deleteResult =
                await MongoClient.Instance.NewsCollection.DeleteOneAsync(x => x.id == modelToDelete.id);

            if (deleteResult.DeletedCount == 1) Helpers.DisplayMessage("Article supprimé avec succès!");

            RefreshNewsList();
            
            CanShowDeets = true;
        }

        private async void RefreshNewsList()
        {
            IsBusy = true;

            InfoCollection.Clear();

            LastRefreshed = DateTime.Now;

            var foundDocs = await MongoClient.Instance.NewsCollection.Find(CurrentFilterDefinition).Limit(15)
                .SortByDescending(x => x.PostedTime).ToListAsync();

            var taskList = new List<Task>();

            foundDocs.ForEach((model) => { taskList.Add(GetInfoImageUrl(model)); });

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

            foundDocs.ForEach((model) => { taskList.Add(GetInfoImageUrl(model)); });

            await Task.WhenAll(taskList);

            InfoCollection.AddRange(foundDocs);

            _iteration = InfoCollection.Count;
        }

        private Task GetInfoImageUrl(InfoModel model)
        {
            return Task.Run(async () =>
            {
                if (model.Image == null && model.InfoAttachType == FileType.Image)
                {
                    var folderName = model.id.ToString();

                    model.Image = await DropboxClient.Instance.GetImageUrl(folderName);   
                }
            });
        }

        private void SearchItems(string query)
        {
            var textOptions = new TextSearchOptions
            {
                CaseSensitive = false,
                DiacriticSensitive = false,
                Language = "fr"
            };

            CurrentFilterDefinition = new FilterDefinitionBuilder<InfoModel>().Where(x => x.Title.Contains(query) || x.Summary.Contains(query));

            RefreshNewsList();
        }

        private void FilterInfos(object sender, FilterEventArgs args)
        {
            FilterButtColor = (Color) Application.Current.Resources["SyncPrimaryColor"];

            var filterDefinitions = new List<FilterDefinition<InfoModel>>();

            if (args.PublicType != -1)
                filterDefinitions.Add(new FilterDefinitionBuilder<InfoModel>().Eq(x => x.InfoPublicType,
                    (TargetPublicType) args.PublicType));
            if (args.EventType != -1)
                filterDefinitions.Add(new FilterDefinitionBuilder<InfoModel>().Eq(x => x.InfoEventType,
                    (EventType) args.EventType));

            filterDefinitions.Add(new FilterDefinitionBuilder<InfoModel>().Eq(x => x.IsUrgent, args.IsUrgent));


            CurrentFilterDefinition = new FilterDefinitionBuilder<InfoModel>().And(filterDefinitions);

            RefreshNewsList();
        }

        private void ClearFilter(object sender, EventArgs args)
        {
            var filterModel = (FilterPopupModel) sender;
            
            filterModel.ScriptChange = true;

            filterModel.IsUrgent = false;
            filterModel.InfoEventType = -1;
            filterModel.InfoPublicType = -1;

            filterModel.ScriptChange = false;

            FilterButtColor = (Color) Application.Current.Resources["SecondaryForegroundColor"];

            CurrentFilterDefinition = FilterDefinition<InfoModel>.Empty;

            RefreshNewsList();
        }

        #region Obsolete

        private async Task GetInfoImages(InfoModel model)
        {
            var path = string.Empty;
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
                Crashes.TrackError(e,
                    new Dictionary<string, string> {{"Model Id", $"{model?.id}"}, {"Image Path", $"{path}"}},
                    ErrorAttachmentLog.AttachmentWithText(e.StackTrace, "StackTrace"));
            }
        }

        #endregion
    }
}