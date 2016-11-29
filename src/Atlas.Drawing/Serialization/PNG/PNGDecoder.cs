using MiscUtil.Conversion;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace Atlas.Drawing.Serialization.PNG
{
    public class PNGDecoder
    {
        private int width;
        private int height;
        private byte bitsPerChannel;
        private byte colorType;
        private byte compressionMethod;
        private byte filterMethod;
        private byte interlaceMethod;

        private bool useColorTable = false;
        private bool hasColor = false;
        private bool hasAlpha = false;

        public byte[] Decode(ref byte[] bytes, out int width, out int height)
        {
            byte[] rawBytes = null;
            // Skip the first 8 bytes
            int byteIndex = 8;
            bool end = false;
            while (byteIndex < bytes.Length && !end)
            {
                int chunkLength = EndianBitConverter.Big.ToInt32(bytes, byteIndex); byteIndex += 4;
                string chunkType = System.Text.Encoding.ASCII.GetString(bytes, byteIndex, 4); byteIndex += 4;

                switch (chunkType)
                {
                    case "IHDR":
                        ReadHeader(ref bytes, byteIndex);
                        break;
                    case "IDAT":
                        rawBytes = UnpackData(ref bytes, byteIndex, chunkLength);
                        break;
                    case "IEND":
                        end = true;
                        break;
                }

                byteIndex += chunkLength;
                uint chunkCRC = EndianBitConverter.Big.ToUInt32(bytes, byteIndex); byteIndex += 4;
            }

            width = this.width;
            height = this.height;

            // add an alpha channel
            int bitsPerPixel = bitsPerChannel * ((hasColor ? 3 : 1) + (hasAlpha ? 1 : 0));
            int bytesPerPixel = (bitsPerPixel / 8);
            int sourceStride = ((width * bitsPerPixel)/8) + 1;
            int destinationStride = (width * 4);
            byte[] unfilteredBytes = new byte[((width * bitsPerPixel) / 8) * height];

            int w = 0;
            byte currentFilter = 0;
            for (int i = 0, j = 0; i < rawBytes.Length; i++)
            {
                if(w == sourceStride)
                {
                    w = 0;
                }
                if(w == 0)
                {
                    currentFilter = rawBytes[i];
                }
                else
                {
                    if (currentFilter == 0)
                    {
                        unfilteredBytes[j++] = rawBytes[i];
                    }
                    else if (currentFilter == 1) // SUB (pixel to the left)
                    {
                        byte a = 0;
                        if (w > bytesPerPixel)
                        {
                            a = unfilteredBytes[j - bytesPerPixel];
                        }

                        unfilteredBytes[j++] = (byte)((rawBytes[i] + a) % 256);
                    }
                    else if (currentFilter == 2) // UP (pixel above)
                    {
                        byte b = 0;
                        if (w > bytesPerPixel)
                        {
                            b = unfilteredBytes[j - (sourceStride - 1)];
                        }

                        unfilteredBytes[j++] = (byte)((rawBytes[i] + b) % 256);
                    }
                    else if (currentFilter == 3) // Average
                    {
                        byte a = 0;
                        byte b = 0;

                        if (w > bytesPerPixel)
                        {
                            a = unfilteredBytes[j - bytesPerPixel];
                        }
                        if (i > sourceStride)
                        {
                            b = unfilteredBytes[j - (sourceStride - 1)];
                        }

                        unfilteredBytes[j++] = (byte)((rawBytes[i] + Math.Floor(a + b / 2m)) % 256);
                    }
                    else if (currentFilter == 4) // Paeth
                    {
                        byte a = 0;
                        byte b = 0;
                        byte c = 0;

                        if (w > bytesPerPixel)
                        {
                            a = unfilteredBytes[j - bytesPerPixel];
                        }
                        if (i > sourceStride)
                        {
                            b = unfilteredBytes[j - (sourceStride-1)];
                        }
                        if (w > bytesPerPixel && i > sourceStride)
                        {
                            c = unfilteredBytes[j - bytesPerPixel - (sourceStride-1)];
                        }

                        unfilteredBytes[j++] = (byte)((rawBytes[i] + PaethPredictor(a, b, c)) % 256);
                    } else
                    {
                        // throw unknown filter type error
                    }
                }

                w++;
            }

            var finalBytes = new byte[width * height * 4];

            if (bitsPerPixel == 1)
            {
                for (int i = 0, j = 0; i < unfilteredBytes.Length;i++)
                {
                    var bits = new BitArray(new byte[] { unfilteredBytes[i] });
                    for (int b = 7; b >= 0; b--)
                    {
                        finalBytes[j++] = (byte)(bits[b] ? 255 : 0);
                        finalBytes[j++] = (byte)(bits[b] ? 255 : 0);
                        finalBytes[j++] = (byte)(bits[b] ? 255 : 0);
                        finalBytes[j++] = 255;
                    }
                }
            }
            else if (bitsPerPixel == 2)
            {
                for (int i = 0, j = 0; i < unfilteredBytes.Length; i++)
                {
                    var bits = new BitArray(new byte[] { unfilteredBytes[i] });
                    for (int b = 7; b >= 0;b-=2)
                    {
                        int value = ((bits[b] ? 1 : 0) * 2) + (bits[b - 1] ? 1 : 0);
                        byte color = (byte)(value * 85);
                        finalBytes[j++] = color;
                        finalBytes[j++] = color;
                        finalBytes[j++] = color;
                        finalBytes[j++] = 255;
                    }
                }
            }
            else if (bitsPerPixel == 4)
            {
                for (int i = 0, j = 0; i < unfilteredBytes.Length; i++)
                {
                    uint b = unfilteredBytes[i];
                    byte color = (byte)(((b & 0xF0) >> 4) * 17);
                    finalBytes[j++] = color;
                    finalBytes[j++] = color;
                    finalBytes[j++] = color;
                    finalBytes[j++] = 255;

                    byte color2 = (byte)((b & 0x0F) * 17);
                    finalBytes[j++] = color2;
                    finalBytes[j++] = color2;
                    finalBytes[j++] = color2;
                    finalBytes[j++] = 255;
                }
            }
            else if (bitsPerPixel == 8)
            {
                for (int i = 0, j = 0; i < unfilteredBytes.Length; i++)
                {
                    byte color = unfilteredBytes[i];
                    finalBytes[j++] = color;
                    finalBytes[j++] = color;
                    finalBytes[j++] = color;
                    finalBytes[j++] = 255;
                }
            }
            else if (bitsPerPixel == 16)
            {
                for (int i = 0, j = 0; i < unfilteredBytes.Length; i++)
                {
                    byte color = unfilteredBytes[i++];
                    byte color2 = unfilteredBytes[i];

                    byte color3 = (byte)(((int)(color << 8) + color2) / 256);

                    finalBytes[j++] = color3;
                    finalBytes[j++] = color3;
                    finalBytes[j++] = color3;
                    finalBytes[j++] = 255;
                }
            }
            else if (bitsPerPixel == 24)
            {
                for (int i = 0, j = 0; i < unfilteredBytes.Length;)
                {
                    finalBytes[j++] = unfilteredBytes[i++];
                    finalBytes[j++] = unfilteredBytes[i++];
                    finalBytes[j++] = unfilteredBytes[i++];
                    finalBytes[j++] = 255;
                }
            }

            return finalBytes;
        }

        private int PaethPredictor(int a, int b, int c)
        {
            // a = left, b = above, c = upper left
            var p = a + b - c;        // initial estimate
            var pa = Math.Abs(p - a);// distances to a, b, c
            var pb = Math.Abs(p - b);
            var pc = Math.Abs(p - c);
            // return nearest of a,b,c,
            // breaking ties in order a, b, c.
            if (pa <= pb && pa <= pc)
            {
                return a;
            }
            else if (pb <= pc)
            {
                return b;
            }
            else
            {
                return c;
            }
        }

        private void ReadHeader(ref byte[] bytes, int offset)
        {
            width = EndianBitConverter.Big.ToInt32(bytes, offset);
            height = EndianBitConverter.Big.ToInt32(bytes, offset + 4);
            bitsPerChannel = bytes[offset + 8];
            colorType = bytes[offset + 9];
            compressionMethod = bytes[offset + 10];
            filterMethod = bytes[offset + 11];
            interlaceMethod = bytes[offset + 12];

            var colorTypeBits = new BitArray(new byte[] { colorType });
            useColorTable = colorTypeBits[0];
            hasColor = colorTypeBits[1];
            hasAlpha = colorTypeBits[2];
        }

        private byte[] UnpackData(ref byte[] bytes, int offset, int length)
        {
            using (var ms = new MemoryStream(bytes, offset + 2, length - 4)) // Skip 2 bytes for the zlib header and the last 4 bytes which is the adler crc
            using (var compressor = new DeflateStream(ms, CompressionMode.Decompress))
            using (var resultStream = new MemoryStream())
            {
                compressor.CopyTo(resultStream);
                return resultStream.ToArray();
            }
        }
    }
}
