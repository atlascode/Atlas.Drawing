using System;

namespace Atlas.Drawing.Serialization.BMP
{
    public class BitmapFileHeader
    {
        public uint DataLength { get; } = 14;
        public ushort Signature { get; private set; }
        public uint FileSize { get; private set; }
        public ushort Reserved1 { get; set; }
        public ushort Reserved2 { get; set; }
        public uint PixelArrayOffset { get; private set; }

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
            throw new NotImplementedException();
        }

        public BitmapFileHeader() { }
    }
}
