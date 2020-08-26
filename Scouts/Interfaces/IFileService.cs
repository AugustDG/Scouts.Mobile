using System.IO;

namespace Scouts.Interfaces
{
    public interface IFileService
    {
        void SavePicture(string name, Stream data, string location = "temp");
    }
}
