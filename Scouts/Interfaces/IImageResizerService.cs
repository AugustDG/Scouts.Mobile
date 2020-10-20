using System.Threading.Tasks;

namespace Scouts.Interfaces
{
    public interface IImageResizerService
    {
        public Task<byte[]> ResizeImage(byte[] imageData, float width, float height);
    }
}