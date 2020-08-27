using System;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Firebase.Iid;
using Firebase.Messaging;
using Scouts.Droid.Dev;
using Scouts.Droid.Services;
using Scouts.Interfaces;
using Scouts.Services;
using Scouts.Settings;
using Xamarin.Forms;

[assembly: Dependency(typeof(DroidMessagingService))]

namespace Scouts.Droid.Services
{
    [Service]
    [IntentFilter(new[] {"com.google.firebase.MESSAGING_EVENT"})]
    public class DroidMessagingService : FirebaseMessagingService, IDroidMessagingService
    {
        public override void OnNewToken(string token)
        {
            AppSettings.PnsId = token;
            
            InstallationService.CreateOrUpdateServerInstallation();
        }

        public override void OnMessageReceived(RemoteMessage message)
        {
            base.OnMessageReceived(message);
            string messageBody;

            messageBody = message.GetNotification() != null ? message.GetNotification().Body : message.Data.Values.First();

            // convert the incoming message to a local notification
            SendLocalNotification(messageBody);

            // send the incoming message directly to the MainPage
            // SendMessageToMainPage(messageBody);
        }

        public void WipeToken()
        {
            Task.Run(() =>
            {
                FirebaseInstanceId.Instance.DeleteInstanceId();
                FirebaseInstanceId.Instance.UnregisterFromRuntime();
                FirebaseInstanceId.Instance.GetInstanceId();
            });
        }

        void SendLocalNotification(string body)
        {
            var intent = new Intent(this, typeof(MainActivity));
            intent.AddFlags(ActivityFlags.ClearTop);
            intent.PutExtra("message", body);

            //Unique request code to avoid PendingIntent collision.
            var requestCode = new Random().Next();
            var pendingIntent = PendingIntent.GetActivity(this, requestCode, intent, PendingIntentFlags.OneShot);

            if (!body.Contains("^^^")) return;
            
            var splitBody = body.Split("^^^", StringSplitOptions.RemoveEmptyEntries);

            var notificationBuilder = new Notification.Builder(this, Constants.NotificationChannelName[0]);

            switch (splitBody[0])
            {
                case "INF-NOPIC":
                    notificationBuilder.SetStyle(new Notification.BigTextStyle()
                        .SetBigContentTitle(splitBody[1])
                        .SetSummaryText("Nouvelle publication!")
                        .BigText(splitBody[2]))
                        .SetSubText("Nouvelle publication!")
                        .SetContentTitle(splitBody[1]);
                    break;
                
                case "MSG-SING":
                    notificationBuilder.SetSubText("Message!")
                        .SetContentTitle(splitBody[1])
                        .SetChannelId(Constants.NotificationChannelName[1]);
                    break;
            }

            notificationBuilder.SetSmallIcon(Resource.Drawable.logo_200)
                .SetAutoCancel(true)
                .SetShowWhen(false)
                .SetContentIntent(pendingIntent);

            var notificationManager = NotificationManager.FromContext(this);
            notificationManager.Notify(0, notificationBuilder.Build());
        }

        /*void SendMessageToMainPage(string body)
        {
            (App.Current.MainPage as AppShell)?.AddMessage(body);
        }*/
    }
}