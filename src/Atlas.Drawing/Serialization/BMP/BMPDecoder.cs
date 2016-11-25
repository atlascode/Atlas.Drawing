using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Atlas.Drawing.Serialization.BMP
{
    public class BMPDecoder
    {
        private byte[] colorTable;

        public byte[] Decode(ref byte[] bytes, out int width, out int height)
        {
            var header = new BitmapFileHeader();
            header.Deserialize(bytes);

            var info = new BitmapInformationHeader();
            info.Deserialize(bytes);

            if (info.ColorsInColorTable > 0) {
                colorTable = new byte[info.ColorsInColorTable * 4];

                int colorOffset = 0;
                bool supportsAlpha = info.SupportsAlpha;
                for (uint i = 0; i < info.ColorsInColorTable; i++)
                {
                    uint sourceColorOffset = header.DataLength + info.DataLength + (i * 4u);
                    colorTable[colorOffset++] = bytes[sourceColorOffset + 0]; //R
                    colorTable[colorOffset++] = bytes[sourceColorOffset + 1]; //G
                    colorTable[colorOffset++] = bytes[sourceColorOffset + 2]; //B
                    colorTable[colorOffset++] = supportsAlpha ? bytes[sourceColorOffset + 3] : (byte)255; //A
                }
            }

            // Unpack any bytes into 32bpp RGBA
            var standardBytes = new byte[info.Width * info.Height * 4];

            uint bytesPerPixel = info.BitsPerPixel / 8u;
            uint stride = (uint)info.Width * info.BitsPerPixel;
            // Round up to a multiple of 32;
            stride += 32 - (stride % 32);
            stride /= 8;

            uint destinationStride = (uint)info.Width * 4;

            for (uint y = 0; y < info.Height; y++)
            {
                for (uint x = 0; x < info.Width; x++)
                {
                    ConvertPixel(ref bytes, header.PixelArrayOffset, stride, bytesPerPixel, x, y, ref standardBytes, destinationStride, info.ColorsInColorTable > 0);
                }
            }

            width = (int)info.Width;
            height = (int)info.Height;
            
            return standardBytes;
        }

        private void ConvertPixel(ref byte[] sourceBytes, uint sourceOffset, uint sourceStride, uint sourceBytesPerPixel, uint x, uint y, ref byte[] destinationBytes, uint destinationStride, bool useColorTable)
        {
            uint sourcePixelOffset = (y * sourceStride) + (x * sourceBytesPerPixel) + sourceOffset;
            uint destinationPixelOffset = (y * destinationStride) + (x * 4);

            if (useColorTable)
            {
                uint colorTableIndex = sourceBytes[sourcePixelOffset] * 4u;
                destinationBytes[destinationPixelOffset] = colorTable[colorTableIndex++];     //R
                destinationBytes[destinationPixelOffset + 1] = colorTable[colorTableIndex++]; //G
                destinationBytes[destinationPixelOffset + 2] = colorTable[colorTableIndex++]; //B
                destinationBytes[destinationPixelOffset + 3] = colorTable[colorTableIndex++]; //A
            }
            else
            {
                destinationBytes[destinationPixelOffset] = sourceBytes[sourcePixelOffset];       //R
                destinationBytes[destinationPixelOffset + 1] = sourceBytes[sourcePixelOffset + 1]; //G
                destinationBytes[destinationPixelOffset + 2] = sourceBytes[sourcePixelOffset + 2]; //B
                destinationBytes[destinationPixelOffset + 3] = 255;                              //A
            }
        }
    }
}
