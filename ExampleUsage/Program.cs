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
            foreach (var file in System.IO.Directory.GetFiles("..\\..\\bmpsuite-2.4\\g\\", "*.bmp"))
            {
                var bmp = Image.FromFile(file);
                bmp.Save(System.IO.Path.GetFileNameWithoutExtension(file) + ".png", ImageFormat.Png);
            }

            foreach (var file in System.IO.Directory.GetFiles("..\\..\\bmpsuite-2.4\\q\\", "*.bmp"))
            {
                var bmp = Image.FromFile(file);
                bmp.Save(System.IO.Path.GetFileNameWithoutExtension(file) + ".png", ImageFormat.Png);
            }

            foreach (var file in System.IO.Directory.GetFiles("..\\..\\bmpsuite-2.4\\b\\", "*.bmp"))
            {
                try
                {
                    var bmp = Image.FromFile(file);
                    bmp.Save(System.IO.Path.GetFileNameWithoutExtension(file) + ".png", ImageFormat.Png);
                }
                catch (Exception)
                {
                    
                }
            }

            //var bmp = Image.FromFile("..\\..\\bmpsuite-2.4\\g\\rgb24.bmp");
            //bmp.Save("rgb24.png", ImageFormat.Png);


            //var bmp2 = Image.FromFile("..\\..\\bmpsuite-2.4\\g\\pal8.bmp");
            //bmp2.Save("pal8.png", ImageFormat.Png);

            //var bmp3 = Image.FromFile("..\\..\\bmpsuite-2.4\\g\\pal8-0.bmp");
            //bmp3.Save("pal8-0.png", ImageFormat.Png);

            //var bmp4 = Image.FromFile("..\\..\\bmpsuite-2.4\\g\\pal8gs.bmp");
            //bmp4.Save("pal8gs.png", ImageFormat.Png);

            //var bmp5 = Image.FromFile("..\\..\\bmpsuite-2.4\\g\\pal8rle.bmp");
            //bmp5.Save("pal8rle.png", ImageFormat.Png);

            //var bmp6 = Image.FromFile("..\\..\\bmpsuite-2.4\\g\\pal8w126.bmp");
            //bmp6.Save("pal8w126.png", ImageFormat.Png);
            //var bmp7 = Image.FromFile("..\\..\\bmpsuite-2.4\\g\\pal8w125.bmp");
            //bmp7.Save("pal8w125.png", ImageFormat.Png);
            //var bmp8 = Image.FromFile("..\\..\\bmpsuite-2.4\\g\\pal8w124.bmp");
            //bmp8.Save("pal8w124.png", ImageFormat.Png);

            //var bmp91 = Image.FromFile("..\\..\\bmpsuite-2.4\\g\\pal8topdown.bmp");
            //bmp91.Save("pal8topdown.png", ImageFormat.Png);

            //var bmp9 = Image.FromFile("..\\..\\bmpsuite-2.4\\g\\pal8os2.bmp");
            //bmp9.Save("pal8os2.png", ImageFormat.Png);

            //var bmp10 = Image.FromFile("..\\..\\bmpsuite-2.4\\g\\pal8v4.bmp");
            //bmp10.Save("pal8v4.png", ImageFormat.Png);
            //var bmp11 = Image.FromFile("..\\..\\bmpsuite-2.4\\g\\pal8v5.bmp");
            //bmp11.Save("pal8v5.png", ImageFormat.Png);
            //var bmp12 = Image.FromFile("..\\..\\bmpsuite-2.4\\g\\rgb16.bmp");
            //bmp12.Save("rgb16.png", ImageFormat.Png);
            //var bmp13 = Image.FromFile("..\\..\\bmpsuite-2.4\\g\\rgb16-565.bmp");
            //bmp13.Save("rgb16-565.png", ImageFormat.Png);
            //var bmp14 = Image.FromFile("..\\..\\bmpsuite-2.4\\g\\rgb16-565pal.bmp");
            //bmp14.Save("rgb16-565pal.png", ImageFormat.Png);

            //var bmp15 = Image.FromFile("..\\..\\bmpsuite-2.4\\g\\rgb24pal.bmp");
            //bmp15.Save("rgb24pal.png", ImageFormat.Png);

            //var bmp16 = Image.FromFile("..\\..\\bmpsuite-2.4\\g\\rgb32.bmp");
            //bmp16.Save("rgb32.png", ImageFormat.Png);
            //var bmp17 = Image.FromFile("..\\..\\bmpsuite-2.4\\g\\rgb32bf.bmp");
            //bmp17.Save("rgb32bf.png", ImageFormat.Png);


            //var bmp18 = Image.FromFile("..\\..\\bmpsuite-2.4\\g\\pal4.bmp");
            //bmp18.Save("pal4.png", ImageFormat.Png);
            //var bmp19 = Image.FromFile("..\\..\\bmpsuite-2.4\\g\\pal4gs.bmp");
            //bmp19.Save("pal4gs.png", ImageFormat.Png);
            //var bmp20 = Image.FromFile("..\\..\\bmpsuite-2.4\\g\\pal4rle.bmp");
            //bmp20.Save("pal4rle.png", ImageFormat.Png);

            //var bmp21 = Image.FromFile("..\\..\\bmpsuite-2.4\\g\\pal1.bmp");
            //bmp21.Save("pal1.png", ImageFormat.Png);
            //var bmp22 = Image.FromFile("..\\..\\bmpsuite-2.4\\g\\pal1wb.bmp");
            //bmp22.Save("pal1wb.png", ImageFormat.Png);
            //var bmp23 = Image.FromFile("..\\..\\bmpsuite-2.4\\g\\pal1bg.bmp");
            //bmp23.Save("pal1bg.png", ImageFormat.Png);

            //var bmp = Image.FromFile("..\\..\\bmpsuite-2.4\\q\\pal2.bmp");
            //bmp.Save("pal2.png", ImageFormat.Png);
            //var bmp = Image.FromFile("..\\..\\bmpsuite-2.4\\q\\pal2color.bmp");
            //bmp.Save("pal2.png", ImageFormat.Png);

            //var bmp = Image.FromFile("..\\..\\bmpsuite-2.4\\q\\pal4rletrns.bmp");
            //bmp.Save("pal4rletrns.png", ImageFormat.Png);
            //var bmp2 = Image.FromFile("..\\..\\bmpsuite-2.4\\q\\pal4rlecut.bmp");
            //bmp2.Save("pal4rlecut.png", ImageFormat.Png);
            //var bmp3 = Image.FromFile("..\\..\\bmpsuite-2.4\\q\\pal8rletrns.bmp");
            //bmp3.Save("pal8rletrns.png", ImageFormat.Png);
            //var bmp4 = Image.FromFile("..\\..\\bmpsuite-2.4\\q\\pal8rlecut.bmp");
            //bmp4.Save("pal8rlecut.png", ImageFormat.Png);

            //var bmp = Image.FromFile("..\\..\\bmpsuite-2.4\\q\\rgba16-1924.bmp");
            //bmp.Save("rgba16-1924.png", ImageFormat.Png);
            //var bmp2 = Image.FromFile("..\\..\\bmpsuite-2.4\\q\\rgba16-4444.bmp");
            //bmp2.Save("rgba16-4444.png", ImageFormat.Png);

            //var bmp = Image.FromFile("..\\..\\bmpsuite-2.4\\q\\rgba32-61754.bmp");
            //bmp.Save("rgba32-61754.png", ImageFormat.Png);
            //var bmp2 = Image.FromFile("..\\..\\bmpsuite-2.4\\q\\rgba32-81284.bmp");
            //bmp2.Save("rgba32-81284.png", ImageFormat.Png);
            //var bmp3 = Image.FromFile("..\\..\\bmpsuite-2.4\\q\\rgba32abf.bmp");
            //bmp3.Save("rgba32abf.png", ImageFormat.Png);
        }
    }
}