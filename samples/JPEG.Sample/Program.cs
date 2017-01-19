using System.IO;
using Atlas.Drawing;
using Atlas.Drawing.Imaging;

namespace JPEG.Sample
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Directory.CreateDirectory("output");

            //foreach (var file in Directory.GetFiles(@"samples", "*.jpg"))
            //{
            //    var jpg = Image.FromFile(file);
            //    jpg.Save(Path.Combine("output", Path.GetFileNameWithoutExtension(file) + ".png"), ImageFormat.Png);
            //}

            var jpg = Image.FromFile(@"samples\Untitled.jpg");

            jpg.Save(Path.Combine("output", Path.GetFileNameWithoutExtension(@"samples\Untitled.jpg") + ".png"), ImageFormat.Png);
        }
    }
}
