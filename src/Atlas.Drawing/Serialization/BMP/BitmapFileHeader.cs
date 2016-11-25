using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Atlas.Drawing.Serialization.BMP
{
    public class BitmapFileHeader
    {
        public UInt32 DataLength { get { return 14; } }

        public UInt16 Signature { get; private set; }
        public UInt32 FileSize { get; private set; }
        public UInt16 Reserved1 { get; set; }
        public UInt16 Reserved2 { get; set; }
        public UInt32 PixelArrayOffset { get; private set; }

        public BitmapFileHeader()
        {

        }

        public void Deserialize(byte[] bytes)
        {
            Signature = BitConverter.ToUInt16(bytes, 0);
            FileSize = BitConverter.ToUInt32(bytes, 2);
            Reserved1 = BitConverter.ToUInt16(bytes, 6);
            Reserved2 = BitConverter.ToUInt16(bytes, 8);
            PixelArrayOffset = BitConverter.ToUInt32(bytes, 10);
        }

        public void Serialize()
        {

        }
    }
}
