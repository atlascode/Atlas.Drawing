using System;
using System.IO;
using Atlas.Drawing;
using Atlas.Drawing.Imaging;

namespace GIF.Sample
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Directory.CreateDirectory("output");

            foreach (var file in Directory.GetFiles(@"imagetestsuite", "*.gif"))
            {
                if (Path.GetFileName(file).EndsWith("rgbbwg.gif"))
                {
                    //try
                    {
                        var gif = Image.FromFile(file);
                        gif.Save(Path.Combine("output", Path.GetFileNameWithoutExtension(file) + ".png"), ImageFormat.Png);
                    }
                    //catch (Exception)
                    //{
                    //    System.IO.File.WriteAllBytes(Path.Combine("output", Path.GetFileNameWithoutExtension(file) + ".png"), new byte[] { });
                    //}
                }
            }
        }
    }
}
