using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AppCenter.Analytics;
using MvvmHelpers;
using Scouts.Models;
using Scouts.Services;
using Scouts.Settings;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Scouts.ViewModels
{
    public class ChatPageModel : BaseViewModel
    {
        public bool IsConnected { get; set; }
        public bool IsSending { get; set; }

        public string ConnectedRoomName
        {
            get => _connectedRoomName;
            set => SetProperty(ref _connectedRoomName, value);
        }

        private string _connectedRoomName;
        public ChatMessageModel CurrentChatMessage { get; set; }

        public ObservableRangeCollection<ChatMessageModel> Messages { get; set; } =
            new ObservableRangeCollection<ChatMessageModel>();

        public ObservableRangeCollection<ChatUserModel> Users { get; set; } =
            new ObservableRangeCollection<ChatUserModel>();

        public Command ConnectToRoomCommand => new Command(async () => await ConnectToRoom());
        public Command DisonnectFromRoomCommand => new Command(DisonnectFromRoom);

        public Command SendMessageCommand => new Command(async () => await SendMessage());

        private ChatService _chatService;

        public ChatPageModel()
        {
            CurrentChatMessage = new ChatMessageModel();
            _chatService = new ChatService();

            _chatService.OnReceivedMessage += (sender, args) =>
            {
                if (args.Message != string.Empty)
                    SendLocalMessage(args.Message, GetUserInRoomFromId(args.UserId));
                else
                    AddRemoveConnectedUser(args.UserId, true);
            };

            _chatService.OnEnteredOrExited += async (sender, args) =>
            {
                AddRemoveConnectedUser(args.UserId, args.Message.Contains("entered"));

                if (args.UserId != ChatSettings.LocalUser.UserId)
                    await _chatService.SendMessageAsync(ChatSettings.CurrentGroup, ChatSettings.LocalUser.UserId,
                        "");
            };

            _chatService.OnConnectionClosed += (sender, args) =>
            {
                SendLocalMessage(args.Message, GetUserInRoomFromId(args.UserId));
            };
        }

        private async Task ConnectToRoom()
        {
            Analytics.TrackEvent("UserConnectedToRoom",
                new Dictionary<string, string>
                {
                    {"User", ChatSettings.LocalUser.Username + "/" + ChatSettings.LocalUser.UserId},
                    {"ConnectedTo", ChatSettings.ConnectedUser.Username + "/" + ChatSettings.LocalUser.UserId},
                    {"UserType", ChatSettings.LocalUser.UserType.ToString()},
                    {"App Version", AppInfo.VersionString + "/" + AppInfo.BuildString}
                });

            if (IsConnected)
                return;

            IsBusy = true;
            _chatService.Init();
            await _chatService.ConnectAsync();

            //NotificationHubConnectionService.Initialize();
            await NotificationHubConnectionService.ConnectAsync();

            await _chatService.JoinChannelAsync(ChatSettings.CurrentGroup,
                AppSettings.CurrentUser.UserId);
            IsConnected = true;

            Users.Add(ChatSettings.LocalUser);
            SendLocalMessage("Connecté!", ChatSettings.LocalUser);

            ConnectedRoomName = ChatSettings.ConnectedUser.Username;

            await Task.Delay(250);

            IsBusy = false;
        }

        private async void DisonnectFromRoom()
        {
            if (!IsConnected)
                return;
            await _chatService.LeaveChannelAsync(ChatSettings.CurrentGroup, ChatSettings.LocalUser.UserId);
            await _chatService.DisconnectAsync();

            ChatSettings.ConnectedUser = null;

            IsConnected = false;
        }

        async Task SendMessage()
        {
            if (!IsConnected)
            {
                await Shell.Current.DisplayAlert("Déconnecté!", "Reconnectez au serveur, puis réessayez svp!", "OK");
                return;
            }

            try
            {
                IsSending = true;
                await _chatService.SendMessageAsync(ChatSettings.CurrentGroup,
                    AppSettings.CurrentUser.UserId,
                    CurrentChatMessage.Message);

                var msgToSend = $"MSG-SING^^^{ChatSettings.LocalUser.Username} vous a envoyé un message!";

                var finalTagExpression =
                    $"(NotificationType:AllMessages-{ChatSettings.ConnectedUser.UserId} && !NotificationType:AllMessages-{AppSettings.CurrentUser.UserId})";

                await NotificationHubConnectionService.NotifyAsync(msgToSend, finalTagExpression);

                CurrentChatMessage.Message = string.Empty;
            }
            catch (Exception ex)
            {
                SendLocalMessage($"Échec: {ex.Message}", ChatSettings.LocalUser);
            }
            finally
            {
                IsSending = false;
            }
        }

        private void SendLocalMessage(string message, ChatUserModel user)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                if (user is null)
                    Messages.Add(new ChatMessageModel
                    {
                        Message = message,
                        Username = "System",
                        Color = Color.Black
                    });
                else
                    Messages.Add(new ChatMessageModel
                    {
                        Message = message,
                        Username = user.Username,
                        Color = user.Color
                    });
            });
        }

        void AddRemoveConnectedUser(string userId, bool add)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return;

            if (add)
            {
                if (Users.All(x => x.UserId != userId))
                {
                    Users.Add(userId == ChatSettings.LocalUser.UserId
                        ? ChatSettings.LocalUser
                        : ChatSettings.ConnectedUser);

                    SendLocalMessage("Connecté!", GetUserInRoomFromId(userId));
                }
            }
            else
            {
                if (Users.Any(x => x.UserId == userId))
                {
                    SendLocalMessage("Déconnecté...", GetUserInRoomFromId(userId));

                    Users.Remove(userId == ChatSettings.LocalUser.UserId
                        ? ChatSettings.LocalUser
                        : ChatSettings.ConnectedUser);
                }
            }
        }

        private ChatUserModel GetUserInRoomFromId(string userId)
        {
            return Users.FirstOrDefault(x => x.UserId == userId);
        }
    }
}