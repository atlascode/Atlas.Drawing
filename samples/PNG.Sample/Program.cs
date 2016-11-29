using System;
using System.IO;
using Atlas.Drawing;
using Atlas.Drawing.Imaging;

namespace PNG.Sample
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Directory.CreateDirectory("output");

            foreach (var file in Directory.GetFiles(@"pngsuite", "*.png"))
            {
                var png = Image.FromFile(file);
                png.Save(Path.Combine("output", Path.GetFileNameWithoutExtension(file) + ".png"), ImageFormat.Png);
            }

            foreach (var file in Directory.GetFiles(@"pngsuite", "*.png"))
            {
                var png = Image.FromFile(file);
                png.Save(Path.Combine("output", Path.GetFileNameWithoutExtension(file) + ".png"), ImageFormat.Png);
            }

            foreach (var file in Directory.GetFiles(@"pngsuite", "*.png"))
            {
                try
                {
                    var png = Image.FromFile(file);
                    png.Save(Path.Combine("output", Path.GetFileNameWithoutExtension(file) + ".png"), ImageFormat.Png);
                }
                catch (Exception)
                {

                }
            }
        }
    }
}
