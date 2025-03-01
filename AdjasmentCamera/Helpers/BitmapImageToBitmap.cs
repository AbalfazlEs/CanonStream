using System.Drawing;
using System.Windows.Media.Imaging;

namespace AdjasmentCamera.Helpers
{
    public static class BitmapImageToBitmap
    {
        public static Bitmap Execute(BitmapImage bitmapImage)
        {
            using (var stream = new System.IO.MemoryStream())
            {
                BitmapEncoder encoder = new BmpBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmapImage));
                encoder.Save(stream);
                Bitmap bitmap = new Bitmap(stream);
                return bitmap;
            }
        }
    }
}
