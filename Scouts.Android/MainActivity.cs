﻿using System;
using System.IO;
using System.Threading.Tasks;
using Acr.UserDialogs;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Common;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using FFImageLoading.Forms.Platform;
using Rg.Plugins.Popup.Services;
using Scouts.Dev;
using Scouts.Droid.Dev;
using Sharpnado.Presentation.Forms.Droid;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Color = Android.Graphics.Color;
using CarouselViewRenderer = CarouselView.FormsPlugin.Android.CarouselViewRenderer;

namespace Scouts.Droid
{
    [Activity(Label = "Scouts", Icon = "@mipmap/icon", Theme = "@style/MainTheme", HardwareAccelerated = true, LaunchMode = LaunchMode.SingleTop)]
    public class MainActivity : FormsAppCompatActivity
    {
        internal static MainActivity Instance { get; private set; }

        public static readonly int PickImageId = 1000;
        public TaskCompletionSource<Stream> PickImageTaskCompletionSource { set; get; }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            Instance ??= this;

            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;
            
            base.Window?.RequestFeature(WindowFeatures.ActionBar);
            
            base.OnCreate(savedInstanceState);

            Rg.Plugins.Popup.Popup.Init(this, savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            CachedImageRenderer.Init(true);
            CachedImageRenderer.InitImageViewHandler();
            SharpnadoInitializer.Initialize();
            UserDialogs.Init(this);
            Stormlion.PhotoBrowser.Droid.Platform.Init(this);
            CarouselViewRenderer.Init();
            XF.Material.Droid.Material.Init(this, savedInstanceState);
            Forms.Init(this, savedInstanceState);
            
            SetStatusBarColor(Color.Transparent);

            if (!IsPlayServiceAvailable())
            {
                throw new Exception(
                    "This device does not have Google Play Services and cannot receive push notifications.");
            }

            CreateNotificationChannel();
            
            LoadApplication(new App());
        }

        public override async void OnBackPressed()
        {
            if (Rg.Plugins.Popup.Popup.SendBackPressed(base.OnBackPressed))
            {
                if (PopupNavigation.Instance.PopupStack.Count > 0)
                    await PopupNavigation.Instance.PopAsync();
            }
            else
            {
                if (Xamarin.Forms.Application.Current.MainPage.Navigation.ModalStack.Count > 0)
                    await Xamarin.Forms.Application.Current.MainPage.Navigation.PopModalAsync(false);
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions,
            [GeneratedEnum] Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent intent)
        {
            base.OnActivityResult(requestCode, resultCode, intent);

            if (requestCode == PickImageId)
            {
                if ((resultCode == Result.Ok) && (intent != null))
                {
                    var uri = intent.Data;
                    var stream = ContentResolver.OpenInputStream(uri);

                    // Set the Stream as the completion of the Task
                    PickImageTaskCompletionSource.SetResult(stream);
                }
                else
                {
                    PickImageTaskCompletionSource.SetResult(null);
                }
            }
        }

        protected override void OnNewIntent(Intent intent)
        {
            if (intent.Extras != null)
            {
                var message = intent.GetStringExtra("message");
                Helpers.DisplayMessage(message);
            }

            base.OnNewIntent(intent);
        }

        bool IsPlayServiceAvailable()
        {
            var resultCode = GoogleApiAvailability.Instance.IsGooglePlayServicesAvailable(this);
            if (resultCode != ConnectionResult.Success)
            {
                if (GoogleApiAvailability.Instance.IsUserResolvableError(resultCode))
                    Log.Debug(Constants.DebugTag, GoogleApiAvailability.Instance.GetErrorString(resultCode));
                else
                {
                    Log.Debug(Constants.DebugTag, "This device is not supported");
                }

                return false;
            }

            return true;
        }

        void CreateNotificationChannel()
        {
            // Notification channels are new as of "Oreo".
            // There is no need to create a notification channel on older versions of Android.
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                foreach (var channelName in Constants.NotificationChannelName)
                {
                    var channelDescription = String.Empty;
                    var channel = new NotificationChannel(channelName, channelName, NotificationImportance.Default)
                    {
                        Description = channelDescription
                    };

                    var notificationManager = (NotificationManager) GetSystemService(NotificationService);
                    notificationManager.CreateNotificationChannel(channel);
                }
            }
        }
    }
}