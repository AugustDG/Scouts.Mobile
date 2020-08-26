using System.IO;
using System.Threading.Tasks;

namespace Scouts.Interfaces
{
    public interface IPhotoPickerService
    {
        Task<Stream> GetImageStreamAsync();
    }
}