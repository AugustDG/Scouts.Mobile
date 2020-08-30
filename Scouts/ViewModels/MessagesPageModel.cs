using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MvvmHelpers;
using Scouts.Events;
using Scouts.Fetchers;
using Scouts.Models;
using Scouts.Settings;
using Scouts.View;
using Scouts.View.Pages;
using Xamarin.Forms;

namespace Scouts.ViewModels
{
    class MessagesPageModel : BaseViewModel
    {
        public ObservableRangeCollection<ChatUserModel> UserCollection { get; set; } = new ObservableRangeCollection<ChatUserModel>();
        
        public DateTime lastRefreshed;

        public Command ShowOptionsCommand => new Command(ShowOptions);
        public Command RefreshContactsCommand => new Command(RefreshContacts);
        public Command EnterChatRoomCommand => new Command<ChatUserModel>(EnterChatRoom);

        public MessagesPageModel()
        {
            AppEvents.WipeAllUserData += WipeAllData;
        }
        
        private void WipeAllData(object sender, EventArgs e)
        {
            UserCollection.Clear();
            lastRefreshed = DateTime.Now.Subtract(new TimeSpan(0, 15, 0));
        }
        
        private async void EnterChatRoom(ChatUserModel user)
        {
            IsBusy = true;
            
            await Task.Run(() =>
            {
                var userIds = new List<string> {AppSettings.CurrentUser.UserId, user.UserId};
            
                userIds.Sort();
            
                ChatSettings.CurrentGroup = userIds[0] + "." + userIds[1];
                ChatSettings.ConnectedUser = user;
                ChatSettings.LocalUser ??= new ChatUserModel
                {
                    Username = AppSettings.CurrentUser.Username,
                    UserId = AppSettings.CurrentUser.UserId,
                    UserType = AppSettings.CurrentUser.UserType,
                    Color = AppSettings.CurrentUser.Color
                };
            });
            
            await Shell.Current.Navigation.PushModalAsync(new ChatPage(), true);
            
            IsBusy = false;
        }

        private async void RefreshContacts()
        {
            UserCollection.Clear();

            IsBusy = true;

            var userLists = await MongoClient.Instance.GetAllUserModelsTask();
            var chatUserList = new List<ChatUserModel>();
            
            await Task.Run(() =>
            {
                lastRefreshed = DateTime.Now;
                
                userLists.Remove(userLists.Find(x => x.id == AppSettings.CurrentUser.id));

                userLists.ForEach(user=>
                {
                    chatUserList.Add(new ChatUserModel
                    {
                        Username = user.Username,
                        UserId = user.UserId,
                        UserType = user.UserType,
                        Color = user.Color
                    });    
                });
            });
            
            UserCollection.AddRange(chatUserList);
            IsBusy = false;
        }
        
        private async void ShowOptions()
        {
            OptionsDropdown.DropdownInstance ??= new OptionsDropdown();
            await Shell.Current.Navigation.PushModalAsync(OptionsDropdown.DropdownInstance, false);
        }
    }
}
