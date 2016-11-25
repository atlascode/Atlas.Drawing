using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Atlas.Drawing.Serialization.BMP
{
    public class BMPDecoder
    {
        public byte[] Decode(ref byte[] bytes, out int width, out int height)
        {
            var header = new BitmapFileHeader();
            header.Deserialize(bytes);

            var info = new BitmapInformationHeader();
            info.Deserialize(bytes);

            // Unpack any bytes into 32bpp RGBA
            var standardBytes = new byte[info.Width * info.Height * 4];

            uint bytesPerPixel = info.BitsPerPixel / 8u;
            uint stride = info.Width * info.BitsPerPixel;
            // Round up to a multiple of 32;
            stride += 32 - (stride % 32);
            stride /= 8;

            uint destinationStride = info.Width * 4;

            for (uint y = 0; y < info.Height; y++)
            {
                for (uint x = 0; x < info.Width; x++)
                {
                    ConvertPixel(ref bytes, header.PixelArrayOffset, stride, bytesPerPixel, x, y, ref standardBytes, destinationStride);
                }
            }

            width = (int)info.Width;
            height = (int)info.Height;
            
            return standardBytes;
        }

        private void ConvertPixel(ref byte[] sourceBytes, uint sourceOffset, uint sourceStride, uint sourceBytesPerPixel, uint x, uint y, ref byte[] destinationBytes, uint destinationStride)
        {
            uint sourcePixelOffset = (y * sourceStride) + (x * sourceBytesPerPixel) + sourceOffset;
            uint destinationPixelOffset = (y * destinationStride) + (x * 4);

            destinationBytes[destinationPixelOffset] = sourceBytes[sourcePixelOffset];       //R
            destinationBytes[destinationPixelOffset + 1] = sourceBytes[sourcePixelOffset+1]; //G
            destinationBytes[destinationPixelOffset + 2] = sourceBytes[sourcePixelOffset+2]; //B
            destinationBytes[destinationPixelOffset + 3] = 255;                              //A
        }
    }
}
