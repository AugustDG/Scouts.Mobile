using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Dropbox.Api;
using Dropbox.Api.Files;
using Xamarin.Essentials;

namespace Scouts.Fetchers
{
    public class DropboxClient
    {
        private const string AccessToken = "{DROPBOX_TOKEN}";
        public static DropboxClient Instance { get; set; }
        
        private Dropbox.Api.DropboxClient _client;

        public DropboxClient()
        {
            _client = new Dropbox.Api.DropboxClient(AccessToken,
                new DropboxClientConfig() {HttpClient = new HttpClient(new HttpClientHandler())});
        }

        public async Task DownloadImage(string folderName)
        {
            try
            {
                await _client.Files.DownloadAsync($"/Scouts/Infos/{folderName}/Image1.jpg")
                    .ContinueWith(async (resp) =>
                    {
                        var stream = await resp.Result.GetContentAsStreamAsync();

                        var path = $"{FileSystem.CacheDirectory}/{folderName}";

                        Directory.CreateDirectory(path);

                        path += "/Image1.jpg";

                        await using var fs = File.Create(path);
                        await stream.CopyToAsync(fs);

                        return path;
                    }).Result;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }
        
        public async Task<string> GetImageUrl(string folderName)
        {
            try
            {
                return (await _client.Files.GetTemporaryLinkAsync($"/Scouts/Infos/{folderName}/Image1.jpg")).Link;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return "Failed";
            }
        }

        public async Task UploadImage(byte[] bytes, string folderName, string fileName = "Image1.jpg")
        {
            try
            {
                using var mem = new MemoryStream(bytes);

                var updated = await _client.Files.UploadAsync(
                    $"/Scouts/Infos/{folderName}/{fileName}",
                    WriteMode.Overwrite.Instance,
                    body: mem);

                Debug.WriteLine("Saved {0}/{1} rev {2}", folderName, "Image1.jpg", updated.Rev);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        #region DEBUG_FUNCTIONS

        public async Task<ListFolderResult> ListRootFolder()
        {
            var list = await _client.Files.ListFolderAsync("/Scouts/Infos");

            return list;
        }

        #endregion

        /*public async void GetImage(string inPath, FileType type = FileType.Image)
        {
            var task = _client.Files.DownloadAsync($"/Scouts/Infos/{inPath}");

            var array = task.Result.GetContentAsByteArrayAsync();

            var path =
                $"{Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory)}";

            var split = path + "/" + inPath.Split('/')[0];
            
            Directory.CreateDirectory(split);

            var fullPath = $"{path}/{inPath}";
            
            File.WriteAllBytes(fullPath, array.Result);
        }*/
    }
}
