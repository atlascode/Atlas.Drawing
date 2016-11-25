using Atlas.Drawing.Imaging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Atlas.Drawing
{
    public class Bitmap : Image
    {
        public Bitmap(int width, int height) : base(new byte[width*height*4], width, height)
        {
            
        }
    }
}