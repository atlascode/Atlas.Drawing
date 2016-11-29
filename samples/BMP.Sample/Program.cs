using System;
using System.IO;
using Atlas.Drawing;
using Atlas.Drawing.Imaging;

namespace BMP.Sample
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Directory.CreateDirectory("output");

            foreach (var file in Directory.GetFiles(@"bmpsuite-2.4\g\", "*.bmp"))
            {
                var bmp = Image.FromFile(file);
                bmp.Save(Path.Combine("output", Path.GetFileNameWithoutExtension(file) + ".png"), ImageFormat.Png);
            }

            foreach (var file in Directory.GetFiles(@"bmpsuite-2.4\q\", "*.bmp"))
            {
                var bmp = Image.FromFile(file);
                bmp.Save(Path.Combine("output", Path.GetFileNameWithoutExtension(file) + ".png"), ImageFormat.Png);
            }

            foreach (var file in Directory.GetFiles(@"bmpsuite-2.4\b\", "*.bmp"))
            {
                try
                {
                    var bmp = Image.FromFile(file);
                    bmp.Save(Path.Combine("output", Path.GetFileNameWithoutExtension(file) + ".png"), ImageFormat.Png);
                }
                catch (Exception)
                {

                }
            }
        }
    }
}
