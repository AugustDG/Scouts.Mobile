using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Threading.Tasks;

namespace Scouts.Services
{
    public static class NotificationHubConnectionService
    {
        private static HubConnection _hubConnection;

        private static bool IsConnected { get; set; }

        private static bool IsDisposed { get; set; } = true;

        public static void Init()
        {
            var url = "https://scouts-chat.azurewebsites.net/notifications";

            _hubConnection = new HubConnectionBuilder()
                .WithAutomaticReconnect( new []{new TimeSpan(10000), new TimeSpan(20000), new TimeSpan(30000), new TimeSpan(40000), new TimeSpan(50000) })
                .WithUrl(url)
                .Build();

            IsDisposed = false;

            /*_hubConnection.Closed += async (error) =>
            {
                await Task.Delay(1000);
                
                IsConnected = false;
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
            };*/
        }

        public static Task ConnectAsync()
        {
            if (IsConnected) return Task.CompletedTask;

            IsConnected = true;
            return _hubConnection.StartAsync();
        }

        public static Task DisposeAsync()
        {
            if (IsDisposed) return Task.CompletedTask;
            
            IsConnected = false;
            IsDisposed = true;

            return _hubConnection.DisposeAsync();
        }

        public static Task NotifyAsync(string msg, string destUsers)
        {
            if (!IsConnected || IsDisposed) return Task.CompletedTask;

            return _hubConnection.InvokeAsync("SendNotificationUser", msg, destUsers);
        }
    }
}