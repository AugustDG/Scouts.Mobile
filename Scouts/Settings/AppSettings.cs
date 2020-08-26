using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Scouts.Dev;
using Scouts.Models;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Scouts.Settings
{
    public static class AppSettings
    {
        public static UserDataModel CurrentUser { get; set; }

        public static DeviceInstallationModel DeviceInstallation { get; set; } = new DeviceInstallationModel
            {InstallationId = Guid.NewGuid().ToString("N")};

        public static List<bool> SavedNotificationSubscriptions { get; set; }

        public static bool IsSaveUsername
        {
            get => Preferences.Get(nameof(IsSaveUsername), false);
            set => Preferences.Set(nameof(IsSaveUsername), value);
        }

        public static bool IsLoginAutomatic
        {
            get => Preferences.Get(nameof(IsLoginAutomatic), false);
            set => Preferences.Set(nameof(IsLoginAutomatic), value);
        }

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
            return async () =>
            {
                var userSavedLoc = $"{FileSystem.AppDataDirectory}/settings/savedUser.json";
                var installationSavedLoc = $"{FileSystem.AppDataDirectory}/settings/savedInstallation.json";
                var notifSubsSavedLoc = $"{FileSystem.AppDataDirectory}/settings/savedNotifSubs.json";

                if (File.Exists(userSavedLoc))
                {
                    var text = await File.ReadAllTextAsync(userSavedLoc);

                    CurrentUser = JsonConvert.DeserializeObject<UserDataModel>(text);
                }

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

        private static Action SaveChangedObjects(string[] objectsToSave)
        {
            return async () =>
            {
                var canWriteToStorage = await MainThread.InvokeOnMainThreadAsync(async () =>
                    await Permissions.RequestAsync<Permissions.StorageWrite>());

                while (canWriteToStorage != PermissionStatus.Granted)
                {
                    await Helpers.DisplayMessageAsync("Les permissions suivantes sont requises!");
                    try
                    {
                        canWriteToStorage = await MainThread.InvokeOnMainThreadAsync(async () =>
                            await Permissions.RequestAsync<Permissions.StorageWrite>());
                    }
                    catch (Exception e)
                    {
                        Helpers.DisplayMessage(e.Message);
                    }
                }

                if (objectsToSave.Contains(nameof(CurrentUser)) && !(CurrentUser is null))
                {
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
                    }
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