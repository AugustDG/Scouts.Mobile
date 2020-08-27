using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using Scouts.Models;
using Scouts.Settings;
using Xamarin.Forms;

namespace Scouts.Services
{
    public static class InstallationService
    {
        public static async void CreateOrUpdateServerInstallation(List<string> tagsAdd = null,
            List<string> tagsRemove = null)
        {
            var installationModel = new DeviceInstallationModel
            {
                InstallationId = AppSettings.DeviceInstallation?.InstallationId,
                PushChannel = AppSettings.PnsId,
                Tags = AppSettings.DeviceInstallation?.Tags ?? new List<string>()
            };

            tagsAdd?.ForEach(s =>
            {
                if (!installationModel.Tags.Contains(s))
                    installationModel.Tags.Add(s);
            });

            tagsRemove?.ForEach(s =>
            {
                if (installationModel.Tags.Contains(s))
                    installationModel.Tags.Remove(s);
            });

            AppSettings.DeviceInstallation = installationModel;

            switch (Device.RuntimePlatform)
            {
                case Device.Android:
                {
                    installationModel.Platform = "Fcm";
                    break;
                }
                default: throw new NotImplementedException();
            }

            HttpClient client = new HttpClient();

            var updateUri = new Uri(
                "https://scouts-chat.azurewebsites.net/Installation/CreateOrUpdateInstallation",
                UriKind.Absolute);

            var json = JsonConvert.SerializeObject(installationModel);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var updateResult = await client.PutAsync(updateUri, content);

            if (updateResult.IsSuccessStatusCode) AppSettings.QueueSaveChangedObjects(new []{nameof(AppSettings.DeviceInstallation)});
        }

        public static async void DeleteServerInstallation()
        {
            DeviceInstallationModel installationModel = new DeviceInstallationModel
            {
                InstallationId = AppSettings.DeviceInstallation?.InstallationId,
                PushChannel = AppSettings.PnsId,
            };

            HttpClient client = new HttpClient();

            var uri = new Uri(
                $"https://scouts-chat.azurewebsites.net/Installation/DeleteInstallation?installationId={installationModel.InstallationId}",
                UriKind.Absolute);

            var result = await client.DeleteAsync(uri);

            if (result.IsSuccessStatusCode)
            {
                //AppSettings.ClearSavedObjects(new []{nameof(AppSettings.DeviceInstallation)});
            }
        }
    }
}