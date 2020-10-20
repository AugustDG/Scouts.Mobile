using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using Scouts.Settings;
using Xamarin.Essentials;
using Xamarin.Forms;
using XF.Material.Forms.UI.Dialogs;
using XF.Material.Forms.UI.Dialogs.Configurations;

namespace Scouts.Dev
{
    public static class Helpers
    {
        /// <summary>
        /// In millisecondsssss :D
        /// </summary>
        public static int ArtificialWaitTime = 000;

        /// <summary>
        /// Capacity of the Notification Subscriptions array
        /// </summary>
        public static int NotificationSubscriptionsQuantity = 8;

        /// <summary>
        /// Generates  a random alphanumeric string!
        /// </summary>
        /// <param name="mult">Number of  iterations!</param>
        /// <returns></returns>
        public static string GenerateRandomString(int mult)
        {
            var path = String.Empty;
            for (var i = 0; i < mult; i++)
            {
                path += Path.GetRandomFileName();
            }

            return path.Replace(".", ""); // Remove period.
        }

        /// <summary>
        /// Generates  a random color!
        /// </summary>
        /// <returns></returns>
        public static Color GenerateRandomColor()
        {
            var rand = new Random();
            return new Color(rand.NextDouble(), rand.NextDouble(), rand.NextDouble());
        }

        /// <summary>
        /// Loads an array of preferences from array name!
        /// </summary>
        /// <returns></returns>
        public static ObservableCollection<bool> LoadPreferencesArray(string arrayName, int capacity)
        {
            var array = new ObservableCollection<bool>(new bool[capacity]);

            for (var i = 0; i < capacity; i++)
            {
                array[i] = Preferences.Get($"{arrayName}.{i}", false);
            }

            return array;
        }

        /// <summary>
        /// Loads an array of preferences from array name!
        /// </summary>
        /// <returns></returns>
        public static void SavesPreferencesArray(string arrayName, ObservableCollection<bool> array)
        {
            for (var i = 0; i < array.Count; i++)
            {
                Preferences.Set($"{arrayName}.{i}", array[i]);
            }
        }

        /// <summary>
        /// Displays message to user, using the Shell framework
        /// </summary>
        /// <returns></returns>
        public static void DisplayMessage(string msg)
        {
            MainThread.BeginInvokeOnMainThread(async () => await MaterialDialog.Instance.ConfirmAsync(msg, "Message", "Ok", configuration: ColorSettings.DefaultMaterialAlertDialogConfiguration));
        }
        
        /// <summary>
        /// Displays message to user, using the Shell framework
        /// </summary>
        /// <returns></returns>
        public static Task DisplayMessageAsync(string msg)
        {
            return MainThread.InvokeOnMainThreadAsync(async () => await MaterialDialog.Instance.ConfirmAsync(msg,"Message" , "Ok", configuration: ColorSettings.DefaultMaterialAlertDialogConfiguration));
        }
    }
}