﻿using System;
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
                if (Path.GetFileName(file).EndsWith(".png"))
                {
                    try
                    {
                        var png = Image.FromFile(file);
                        png.Save(Path.Combine("output", Path.GetFileNameWithoutExtension(file) + ".png"), ImageFormat.Png);
                    }
                    catch (Exception)
                    {
                        System.IO.File.WriteAllBytes(Path.Combine("output", Path.GetFileNameWithoutExtension(file) + ".png"), new byte[] { });
                    }
                }
            }
        }
    }
}
