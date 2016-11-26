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
        public Int32 Width { get; set; }
        public Int32 Height { get; set; }
        public UInt16 Planes { get; set; }
        public UInt16 BitsPerPixel { get; set; }
        public UInt32 Compression { get; set; }
        public UInt32 ImageSize { get; set; }
        public UInt32 XPixelsPerMeter { get; set; }
        public UInt32 YPixelsPerMeter { get; set; }
        public UInt32 ColorsInColorTable { get; set; }
        public UInt32 ImportantColorCount { get; set; }
        public Int32 RedChannelBitmask { get; set; }
        public Int32 GreenChannelBitmask { get; set; }
        public Int32 BlueChannelBitmask { get; set; }
        public Int32 AlphaChannelBitmask { get; set; }

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

            if (headerPosition >= HeaderSize) return;

            Planes = BitConverter.ToUInt16(bytes, 14 + headerPosition); headerPosition += 2;
            BitsPerPixel = BitConverter.ToUInt16(bytes, 14 + headerPosition); headerPosition += 2;

            if (headerPosition >= HeaderSize) return;

            Compression = BitConverter.ToUInt32(bytes, 14 + headerPosition); headerPosition += 4;
            ImageSize = BitConverter.ToUInt32(bytes, 14 + headerPosition); headerPosition += 4;
            XPixelsPerMeter = BitConverter.ToUInt32(bytes, 14 + headerPosition); headerPosition += 4;
            YPixelsPerMeter = BitConverter.ToUInt32(bytes, 14 + headerPosition); headerPosition += 4;
            ColorsInColorTable = BitConverter.ToUInt32(bytes, 14 + headerPosition); headerPosition += 4;
            ImportantColorCount = BitConverter.ToUInt32(bytes, 14 + headerPosition); headerPosition += 4;

            if (headerPosition >= HeaderSize && Compression != 3 && Compression != 4) return;

            RedChannelBitmask = BitConverter.ToInt32(bytes, 14 + headerPosition); headerPosition += 4;
            GreenChannelBitmask = BitConverter.ToInt32(bytes, 14 + headerPosition); headerPosition += 4;
            BlueChannelBitmask = BitConverter.ToInt32(bytes, 14 + headerPosition); headerPosition += 4;

            if (headerPosition >= HeaderSize && Compression != 4) return;

            AlphaChannelBitmask = BitConverter.ToInt32(bytes, 14 + headerPosition); headerPosition += 4;
            SupportsAlpha = true;

            if (headerPosition >= HeaderSize) return;

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
