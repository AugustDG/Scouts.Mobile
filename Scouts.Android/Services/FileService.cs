using System;
using System.IO;
using Scouts.Droid.Services;
using Scouts.Interfaces;
using Xamarin.Forms;

[assembly: Dependency(typeof(FileService))]
namespace Scouts.Droid.Services
{
    public class FileService : IFileService
    {
        public void SavePicture(string name, Stream data, string location = "temp")
        {
            var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            documentsPath = Path.Combine(documentsPath, "Orders", location);
            Directory.CreateDirectory(documentsPath);

            var filePath = Path.Combine(documentsPath, name);

            var bArray = new byte[data.Length];

            using (var fs = new FileStream(filePath, FileMode.OpenOrCreate))
            {
                using (data)
                {
                    data.Read(bArray, 0, (int)data.Length);
                }
                var length = bArray.Length;
                fs.Write(bArray, 0, length);
            }
        }

        public void GetPicture(string name, string location)
        {

        }
    }
}