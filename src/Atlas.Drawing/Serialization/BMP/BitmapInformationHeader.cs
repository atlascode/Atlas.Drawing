using System;

namespace Atlas.Drawing.Serialization.BMP
{
    public class BitmapInformationHeader
    {
        public uint DataLength => HeaderSize;
        public uint HeaderSize { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public ushort Planes { get; set; }
        public ushort BitsPerPixel { get; set; }
        public uint Compression { get; set; }
        public uint ImageSize { get; set; }
        public uint XPixelsPerMeter { get; set; }
        public uint YPixelsPerMeter { get; set; }
        public uint ColorsInColorTable { get; set; }
        public uint ImportantColorCount { get; set; }
        public int RedChannelBitmask { get; set; }
        public int GreenChannelBitmask { get; set; }
        public int BlueChannelBitmask { get; set; }
        public int AlphaChannelBitmask { get; set; }
        public bool SupportsAlpha { get; private set; }

        public void Deserialize(byte[] bytes)
        {
            int headerPosition = 0;
            HeaderSize = BitConverter.ToUInt32(bytes, 14 + headerPosition); headerPosition += 4;

            if(HeaderSize == 12) // a 12 byte header is the smallest header possible and is for the OS/2 BMPv1 or Windows BMPv2 format
            {
                Width = BitConverter.ToInt16(bytes, 14 + headerPosition); headerPosition += 2;
                Height = BitConverter.ToInt16(bytes, 14 + headerPosition); headerPosition += 2;
                Planes = BitConverter.ToUInt16(bytes, 14 + headerPosition); headerPosition += 2;
                BitsPerPixel = BitConverter.ToUInt16(bytes, 14 + headerPosition); headerPosition += 2;
                return;
            }

            Width = BitConverter.ToInt32(bytes, 14 + headerPosition); headerPosition += 4;
            Height = BitConverter.ToInt32(bytes, 14 + headerPosition); headerPosition += 4;

            if (headerPosition >= HeaderSize)
                return;

            Planes = BitConverter.ToUInt16(bytes, 14 + headerPosition); headerPosition += 2;
            BitsPerPixel = BitConverter.ToUInt16(bytes, 14 + headerPosition); headerPosition += 2;

            if (headerPosition >= HeaderSize)
                return;

            Compression = BitConverter.ToUInt32(bytes, 14 + headerPosition); headerPosition += 4;
            ImageSize = BitConverter.ToUInt32(bytes, 14 + headerPosition); headerPosition += 4;
            XPixelsPerMeter = BitConverter.ToUInt32(bytes, 14 + headerPosition); headerPosition += 4;
            YPixelsPerMeter = BitConverter.ToUInt32(bytes, 14 + headerPosition); headerPosition += 4;
            ColorsInColorTable = BitConverter.ToUInt32(bytes, 14 + headerPosition); headerPosition += 4;
            ImportantColorCount = BitConverter.ToUInt32(bytes, 14 + headerPosition); headerPosition += 4;

            if (headerPosition >= HeaderSize && Compression != 3 && Compression != 6)
                return;

            RedChannelBitmask = BitConverter.ToInt32(bytes, 14 + headerPosition); headerPosition += 4;
            GreenChannelBitmask = BitConverter.ToInt32(bytes, 14 + headerPosition); headerPosition += 4;
            BlueChannelBitmask = BitConverter.ToInt32(bytes, 14 + headerPosition); headerPosition += 4;

            if (headerPosition >= HeaderSize && Compression != 6)
                return;

            AlphaChannelBitmask = BitConverter.ToInt32(bytes, 14 + headerPosition); headerPosition += 4;
            SupportsAlpha = AlphaChannelBitmask != 0;

            if (headerPosition >= HeaderSize)
                return;

            // ColourSpaceType
            //ColourSpaceEndpoints
            //GammaRed
            //GammaGreen
            //GammaBlue
            //Intent
            //ICCProfileData
            //ICCProfileSize
            //Reserved
        }
    }
}
