using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

using Bitmap = Atlas.Drawing.Bitmap;
using Image = Atlas.Drawing.Image;
using ImageFormat = Atlas.Drawing.Imaging.ImageFormat;

namespace ExampleUsage
{
    class Program
    {
        static void Main(string[] args)
        {
            var bmp = Image.FromFile("..\\..\\bmpsuite-2.4\\g\\rgb24.bmp");
            //var ms = new MemoryStream();
            //bmp.Save(ms, ImageFormat.MemoryBmp);
            //var img = Image.FromStream(ms);
            //img.Save("test.png", ImageFormat.Png);

            bmp.Save("rgb24.png", ImageFormat.Png);
        }
    }
}