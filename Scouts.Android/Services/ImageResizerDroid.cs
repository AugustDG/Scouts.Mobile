using System.IO;
using System.Threading.Tasks;
using Android.Graphics;
using Scouts.Droid.Services;
using Scouts.Interfaces;
using Xamarin.Forms;

[assembly: Dependency(typeof(ImageResizerDroid))]

namespace Scouts.Droid.Services
{
    public class ImageResizerDroid : IImageResizerService
    {
        public Task<byte[]> ResizeImage(byte[] imageData, float width, float height)
        {
            return Task.Run(() =>
            {
                // Load the bitmap
                var originalImage = BitmapFactory.DecodeByteArray(imageData, 0, imageData.Length);

                if (originalImage == null) return new byte[0];
                
                var finalWidth = (int) originalImage.Width;
                var finalHeight = (int) originalImage.Height;

                if (originalImage.Width > width)
                {
                    finalWidth = (int) width;
                    finalHeight = (int) width / originalImage.Width  * originalImage.Height;
                }
                else if (originalImage.Height > height)
                {
                    finalHeight = (int) height;
                    finalWidth = (int) height / originalImage.Height * originalImage.Width;
                }

                var resizedImage = Bitmap.CreateScaledBitmap(originalImage, finalWidth, finalHeight, false);

                using var ms = new MemoryStream();
                resizedImage?.Compress(Bitmap.CompressFormat.Jpeg, 100, ms);
                return ms.ToArray();
            });
        }
    }
}