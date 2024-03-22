using System.Drawing;
using System.IO;
using System.Windows.Media.Imaging;

namespace Client;

public static class DrawingHelpers
{
    public static BitmapImage ToImageSource(this Bitmap bitmap)
    {
        using MemoryStream memory = new MemoryStream();
        bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
        memory.Position = 0;

        BitmapImage bitMapImage = new BitmapImage();
        bitMapImage.BeginInit();
        bitMapImage.StreamSource = memory;
        bitMapImage.CacheOption = BitmapCacheOption.OnLoad;
        bitMapImage.EndInit();

        return bitMapImage;
    }
}