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

            if(info.BitsPerPixel <= 8 && info.ColorsInColorTable == 0)
            {
                info.ColorsInColorTable = 256;
            }

            if (info.ColorsInColorTable > 0) {
                colorTable = new byte[info.ColorsInColorTable * 4];

                int colorOffset = 0;
                bool supportsAlpha = info.SupportsAlpha;
                for (uint i = 0; i < info.ColorsInColorTable; i++)
                {
                    uint sourceColorOffset = header.DataLength + info.DataLength + (i * (info.HeaderSize == 12 ? 3u : 4u));
                    colorTable[colorOffset++] = bytes[sourceColorOffset + 0]; //R
                    colorTable[colorOffset++] = bytes[sourceColorOffset + 1]; //G
                    colorTable[colorOffset++] = bytes[sourceColorOffset + 2]; //B
                    colorTable[colorOffset++] = supportsAlpha ? bytes[sourceColorOffset + 3] : (byte)255; //A
                }
            }

            bool useColorTable = info.BitsPerPixel < 24 && info.ColorsInColorTable > 0;

            // Unpack any bytes into 32bpp RGBA
            var standardBytes = new byte[info.Width * Math.Abs(info.Height) * 4];

            // Round up to a 4 byte boundary;
            uint stride = (((uint)info.Width * info.BitsPerPixel+31)/32) * 4;

            uint destinationStride = (uint)info.Width * 4;

            // decompress bytes
            uint pixelArrayOffset = header.PixelArrayOffset;
            byte[] decompressedBytes = bytes;
            if(info.Compression > 0)
            {
                if(info.Compression == 1) // RLE8
                {
                    decompressedBytes = new byte[stride * info.Height];
                    int currentIndex = 0;
                    for (int i = 0; i < info.ImageSize;)
                    {
                        byte b = bytes[pixelArrayOffset + i++];

                        if (b == 0)
                        {
                            byte a = bytes[pixelArrayOffset + i++];
                            if (a == 0)
                            {
                                if (currentIndex % (int)stride != 0)
                                {
                                    currentIndex += (int)stride - (currentIndex % (int)stride);
                                }
                            }
                            else  if(a == 1)
                            {
                                break;
                            }
                            else if (a == 2)
                            {
                                byte x = bytes[pixelArrayOffset + i++];
                                byte y = bytes[pixelArrayOffset + i++];
                            }
                            else
                            {
                                for (int j = 0; j < a; j++)
                                {
                                    decompressedBytes[currentIndex++] = bytes[pixelArrayOffset + i++];
                                }

                                // align to full word (2 bytes)
                                if(a % 2 != 0)
                                {
                                    i++;
                                }
                            }
                        }
                        else
                        {
                            byte c = bytes[pixelArrayOffset + i++];
                            for (int j = 0; j < b; j++)
                            {
                                decompressedBytes[currentIndex++] = c;
                            }
                        }
                    }
                    pixelArrayOffset = 0;
                }
                if (info.Compression == 2) // RLE4
                {
                    decompressedBytes = new byte[stride * info.Height];
                    int currentIndex = 0;
                    for (int i = 0; i < info.ImageSize;)
                    {
                        byte b = bytes[pixelArrayOffset + i++];

                        if (b == 0)
                        {
                            byte a = bytes[pixelArrayOffset + i++];
                            if (a == 0)
                            {
                                if (currentIndex % (int)stride != 0)
                                {
                                    currentIndex += (int)stride - (currentIndex % (int)stride);
                                }
                            }
                            else if (a == 1)
                            {
                                break;
                            }
                            else if (a == 2)
                            {
                                byte x = bytes[pixelArrayOffset + i++];
                                byte y = bytes[pixelArrayOffset + i++];
                            }
                            else
                            {
                                for (int j = 0; j < a; j++)
                                {
                                    byte r = bytes[pixelArrayOffset + i++];
                                    byte n1 = (byte)((r & 0xF0) >> 4);
                                    byte n2 = (byte)(r & 0x0F);

                                    if(currentIndex % 2 == 0)
                                    {
                                        decompressedBytes[currentIndex / 2] = (byte)(n1 | n1 << 4);
                                    }
                                    else
                                    {
                                        decompressedBytes[currentIndex / 2] = (byte)(n1 | (decompressedBytes[currentIndex / 2]) << 4);
                                    }
                                    currentIndex++;

                                    j++;
                                    if (j < a)
                                    {
                                        if (currentIndex % 2 == 0)
                                        {
                                            decompressedBytes[currentIndex / 2] = (byte)(n2 | n2 << 4);
                                        }
                                        else
                                        {
                                            decompressedBytes[currentIndex / 2] = (byte)(n2 | (decompressedBytes[currentIndex / 2]) << 4);
                                        }
                                        currentIndex++;
                                    }
                                }

                                // align to full word (2 bytes)
                                if(a % 2 != 0)
                                {
                                    a += 1;
                                }

                                if ((a + 4) % 4 != 0)
                                {
                                    i++;
                                }
                            }
                        }
                        else
                        {
                            byte r = bytes[pixelArrayOffset + i++];
                            byte n1 = (byte)((r & 0xF0) >> 4);
                            byte n2 = (byte)(r & 0x0F);

                            for (int j = 0; j < b; j++)
                            {
                                if (currentIndex % 2 == 0)
                                {
                                    decompressedBytes[currentIndex / 2] = (byte)(n1 | n1 << 4);
                                }
                                else
                                {
                                    decompressedBytes[currentIndex / 2] = (byte)(n1 | (decompressedBytes[currentIndex / 2]) << 4);
                                }
                                currentIndex++;

                                j++;
                                if (j < b)
                                {
                                    if (currentIndex % 2 == 0)
                                    {
                                        decompressedBytes[currentIndex / 2] = (byte)(n2 | n2 << 4);
                                    }
                                    else
                                    {
                                        decompressedBytes[currentIndex / 2] = (byte)(n2 | (decompressedBytes[currentIndex / 2]) << 4);
                                    }
                                    currentIndex++;
                                }

                            }
                        }
                    }

                    pixelArrayOffset = 0;
                }
            }

            for (uint y = 0; y < Math.Abs(info.Height); y++)
            {
                for (uint x = 0; x < info.Width; x++)
                {
                    ConvertPixel(ref decompressedBytes, pixelArrayOffset, stride, info.BitsPerPixel, x, y, ref standardBytes, destinationStride, useColorTable, info.Height < 0 ? (uint)Math.Abs(info.Height) - 1 : y * 2);
                }
            }

            width = (int)info.Width;
            height = Math.Abs(info.Height);
            
            return standardBytes;
        }

        private void ConvertPixel(ref byte[] sourceBytes, uint sourceOffset, uint sourceStride, uint sourceBitsPerPixel, uint x, uint y, ref byte[] destinationBytes, uint destinationStride, bool useColorTable, uint flip)
        {
            uint sourcePixelOffset = (y * sourceStride) + (x * sourceBitsPerPixel / 8) + sourceOffset;
            uint destinationPixelOffset = ((flip - y) * destinationStride) + (x * 4);

            if (useColorTable)
            {
                uint colorTableIndex = 0;
                if (sourceBitsPerPixel == 4)
                {
                    if (x % 2 == 0)
                    {
                        colorTableIndex = (((uint)sourceBytes[sourcePixelOffset] & 0xF0) >> 4) * 4u; 
                    }
                    else
                    {
                        colorTableIndex = ((uint)sourceBytes[sourcePixelOffset] & 0x0F) * 4u;
                    }
                }
                else if (sourceBitsPerPixel == 8)
                {
                    colorTableIndex = sourceBytes[sourcePixelOffset] * 4u;
                }

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
