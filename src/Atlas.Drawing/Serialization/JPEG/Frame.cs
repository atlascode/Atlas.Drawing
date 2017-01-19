using System.Collections.Generic;

namespace Atlas.Drawing.Serialization.JPEG
{
    internal class Frame
    {
        public const byte JPEG_COLOR_GRAY = 1;
        public const byte JPEG_COLOR_RGB = 2;
        public const byte JPEG_COLOR_YCbCr = 3;
        public const byte JPEG_COLOR_CMYK = 4;

        public List<Component> Components { get; private set; }
        public int ComponentCount => Components.Count;
        public byte Precision { get; set; }
        public byte ColourMode { get; set; }
        public short Width { get; set; }
        public short Height { get; set; }  
        public short ScanLines { get; set; }
        public short SamplesPerScan { get; set; }

        public Frame()
        {
            Components = new List<Component>();
        }
    }
}
