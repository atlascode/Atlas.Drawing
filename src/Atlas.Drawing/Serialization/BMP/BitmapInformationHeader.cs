using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Atlas.Drawing.Serialization.BMP
{
    public class BitmapInformationHeader
    {
        public UInt32 DataLength { get { return HeaderSize; } }
        public UInt32 HeaderSize { get; set; }
        public UInt32 Width { get; set; }
        public UInt32 Height { get; set; }
        public UInt16 Planes { get; set; }
        public UInt16 BitsPerPixel { get; set; }

        public void Deserialize(byte[] bytes)
        {
            HeaderSize = BitConverter.ToUInt32(bytes, 14);
            Width = BitConverter.ToUInt32(bytes, 18);
            Height = BitConverter.ToUInt32(bytes, 22);
            Planes = BitConverter.ToUInt16(bytes, 26);
            BitsPerPixel = BitConverter.ToUInt16(bytes, 28);
        }
    }
}
