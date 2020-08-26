using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Scouts.Events;

namespace Scouts.Services
{
    public class ChatService
    {
        public event EventHandler<MessageEventArgs> OnReceivedMessage;
        public event EventHandler<MessageEventArgs> OnEnteredOrExited;
        public event EventHandler<MessageEventArgs> OnConnectionClosed;

        HubConnection _hubConnection;

        bool IsConnected { get; set; }
        Dictionary<string, string> ActiveChannels { get; } = new Dictionary<string, string>();

        public void Init()
        {
            var url = "https://scouts-chat.azurewebsites.net/chat";

            _hubConnection = new HubConnectionBuilder()
                .WithUrl(url)
                .Build();

            _hubConnection.Closed += async (error) =>
            {
                OnConnectionClosed?.Invoke(this, new MessageEventArgs("Connection closed...", string.Empty));
                IsConnected = false;
                await Task.Delay(500);
                try
                {
                    await ConnectAsync();
                }
                catch (Exception ex)
                {
                    // Exception!
                    Debug.WriteLine(ex);
                    Debug.WriteLine(_hubConnection.State);
                    throw;
                }
            };

            _hubConnection.On<string, string>("ReceiveMessage",
                (userId, message) => { OnReceivedMessage?.Invoke(this, new MessageEventArgs(message, userId)); });

            _hubConnection.On<string>("Entered",
                (userId) => { OnEnteredOrExited?.Invoke(this, new MessageEventArgs($"{userId} entered.", userId)); });


            _hubConnection.On<string>("Left",
                (userId) => { OnEnteredOrExited?.Invoke(this, new MessageEventArgs($"{userId} left.", userId)); });


            _hubConnection.On<string>("EnteredOrLeft",
                (message) => { OnEnteredOrExited?.Invoke(this, new MessageEventArgs(message, message)); });
        }

        public async Task ConnectAsync()
        {
            if (IsConnected)
                return;

            await _hubConnection.StartAsync();
            IsConnected = true;
        }

        public async Task DisconnectAsync()
        {
            if (!IsConnected)
                return;

            try
            {
                await _hubConnection.DisposeAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            ActiveChannels.Clear();
            IsConnected = false;
        }

        public async Task LeaveChannelAsync(string group, string userId)
        {
            if (!IsConnected || !ActiveChannels.ContainsKey(group))
                return;

            await _hubConnection.SendAsync("RemoveFromGroup", group, userId);

            ActiveChannels.Remove(group);
        }

        public async Task JoinChannelAsync(string group, string userId)
        {
            if (!IsConnected || ActiveChannels.ContainsKey(group))
                return;

            await _hubConnection.SendAsync("AddToGroup", group, userId);
            ActiveChannels.Add(group, userId);
        }

        public async Task SendMessageAsync(string group, string userId, string message)
        {
            if (!IsConnected)
                throw new InvalidOperationException("Not connected");

            await _hubConnection.InvokeAsync("SendMessageGroup",
                group,
                userId,
                message);
        }
    }
}