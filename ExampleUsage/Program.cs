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
            bmp.Save("rgb24.png", ImageFormat.Png);


            var bmp2 = Image.FromFile("..\\..\\bmpsuite-2.4\\g\\pal8.bmp");
            bmp2.Save("pal8.png", ImageFormat.Png);
        }
    }
}