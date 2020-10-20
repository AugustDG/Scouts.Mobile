using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Scouts.Dev;
using Scouts.Events;
using Scouts.Models;
using Xamarin.Essentials;
using Xamarin.Forms;
using XF.Material.Forms.Resources;
using XF.Material.Forms.UI.Dialogs.Configurations;

namespace Scouts.Settings
{
    public static class AppSettings
    {
        public static UserDataModel CurrentUser
        {
            get => _currentUser;
            set
            {
                AppEvents.UserTypeChanged.Invoke(null, EventArgs.Empty);
                SavedPassword = value?.Password;
                SavedUserId = value?.UserId;
                _currentUser = value;
            }
        }

        private static UserDataModel _currentUser;
        
        public static string SavedUserId
        {
            get => Preferences.Get(nameof(SavedUserId), null);
            set => Preferences.Set(nameof(SavedUserId), value);
        }
        
        public static string SavedPassword
        {
            get => Preferences.Get(nameof(SavedPassword), null);
            set => Preferences.Set(nameof(SavedPassword), value);
        }

        public static DeviceInstallationModel DeviceInstallation { get; set; } = new DeviceInstallationModel
            {InstallationId = Guid.NewGuid().ToString("N")};

        public static List<bool> SavedNotificationSubscriptions { get; set; }

        public static string PnsId
        {
            get => Preferences.Get(nameof(PnsId), null);
            set => Preferences.Set(nameof(PnsId), value);
        }

        private static Queue<Action> WriteReadOperations { get; set; } = new Queue<Action>();
        private static bool _timerStarted;

        public static void Init()
        {
            QueueLoadSavedObjects();
        }

        public static void QueueLoadSavedObjects()
        {
            WriteReadOperations.Enqueue(LoadSavedObjects());

            StartQueueTimerAsync();
            _timerStarted = true;
        }

        public static void QueueSaveChangedObjects(string[] objectsToSave)
        {
            WriteReadOperations.Enqueue(SaveChangedObjects(objectsToSave));

            StartQueueTimerAsync();
            _timerStarted = true;
        }

        public static void QueueClearSavedObjects(string[] objectsToDelete)
        {
            WriteReadOperations.Enqueue(ClearSavedObjects(objectsToDelete));

            StartQueueTimerAsync();
            _timerStarted = true;
        }

        private static void StartQueueTimerAsync()
        {
            if (_timerStarted) return;

            Device.StartTimer(TimeSpan.FromSeconds(0.5), () =>
            {
                Action action;

                if (WriteReadOperations.TryDequeue(out action))
                {
                    action.Invoke();

                    _timerStarted = WriteReadOperations.TryPeek(out action);

                    return _timerStarted;
                }
                else
                {
                    _timerStarted = false;
                    return _timerStarted;
                }
            });
        }

        private static Action LoadSavedObjects()
        {
            return async() =>
            {
                var installationSavedLoc = $"{FileSystem.AppDataDirectory}/settings/savedInstallation.json";
                var notifSubsSavedLoc = $"{FileSystem.AppDataDirectory}/settings/savedNotifSubs.json";

                if (File.Exists(installationSavedLoc))
                {
                    var text = await File.ReadAllTextAsync(installationSavedLoc);

                    DeviceInstallation = JsonConvert.DeserializeObject<DeviceInstallationModel>(text);
                }

                if (File.Exists(notifSubsSavedLoc))
                {
                    var text = await File.ReadAllTextAsync(notifSubsSavedLoc);

                    SavedNotificationSubscriptions = JsonConvert.DeserializeObject<List<bool>>(text);
                }
                else
                {
                    SavedNotificationSubscriptions = new List<bool>();
                    for (var i = 0; i < Helpers.NotificationSubscriptionsQuantity; i++)
                    {
                        SavedNotificationSubscriptions.Add(false);
                    }
                }
            };
        }

        public static Action SaveChangedObjects(string[] objectsToSave)
        {
            return async () =>
            {
                if (objectsToSave.Contains(nameof(CurrentUser)) && !(CurrentUser is null))
                {
                    try
                    {
                        Preferences.Set(nameof(CurrentUser), JsonConvert.SerializeObject(CurrentUser));
                    }
                    catch (Exception e)
                    {
                        Helpers.DisplayMessage(e.Message);
                    }
                    
                    /*
                    try
                    {
                        var userSavedLoc = $"{FileSystem.AppDataDirectory}/settings";

                        Directory.CreateDirectory(userSavedLoc);

                        userSavedLoc += "/savedUser.json";

                        await using var file = File.CreateText(userSavedLoc);
                        var serializer = new JsonSerializer();
                        serializer.Serialize(file, CurrentUser);
                    }
                    catch (Exception e)
                    {
                        Helpers.DisplayMessage(e.Message);
                    }*/
                }

                if (objectsToSave.Contains(nameof(DeviceInstallation)) && !(DeviceInstallation is null))
                {
                    try
                    {
                        var installationSavedLoc = $"{FileSystem.AppDataDirectory}/settings";

                        Directory.CreateDirectory(installationSavedLoc);

                        installationSavedLoc += "/savedInstallation.json";

                        await using var file = File.CreateText(installationSavedLoc);
                        var serializer = new JsonSerializer();
                        serializer.Serialize(file, DeviceInstallation);
                    }
                    catch (Exception e)
                    {
                        Helpers.DisplayMessage(e.Message);
                    }
                }

                if (objectsToSave.Contains(nameof(SavedNotificationSubscriptions)) &&
                    !(SavedNotificationSubscriptions is null))
                {
                    try
                    {
                        var notifSubsSavedLoc = $"{FileSystem.AppDataDirectory}/settings";

                        Directory.CreateDirectory(notifSubsSavedLoc);

                        notifSubsSavedLoc += "/savedNotifSubs.json";

                        await using var file = File.CreateText(notifSubsSavedLoc);
                        var serializer = new JsonSerializer();
                        serializer.Serialize(file, SavedNotificationSubscriptions);
                    }
                    catch (Exception e)
                    {
                        Helpers.DisplayMessage(e.Message);
                    }
                }
            };
        }

        private static Action ClearSavedObjects(string[] objectsToDelete)
        {
            return () =>
            {
                var userSavedLoc = $"{FileSystem.AppDataDirectory}/settings/savedUser.json";
                var installationSavedLoc = $"{FileSystem.AppDataDirectory}/settings/savedInstallation.json";
                var notifSubsSavedLoc = $"{FileSystem.AppDataDirectory}/settings/savedNotifSubs.json";

                if (objectsToDelete.Contains(nameof(CurrentUser)) || objectsToDelete.Length == 0)
                    File.Delete(userSavedLoc);
                if (objectsToDelete.Contains(nameof(DeviceInstallation)) || objectsToDelete.Length == 0)
                    File.Delete(installationSavedLoc);
                if (objectsToDelete.Contains(nameof(SavedNotificationSubscriptions)) || objectsToDelete.Length == 0)
                    File.Delete(notifSubsSavedLoc);
            };
        }
    }
}