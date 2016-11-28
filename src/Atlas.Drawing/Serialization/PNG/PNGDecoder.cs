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
                
                switch(chunkType)
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
            int sourceStride = (width * 3) + 1;
            int destinationStride = (width * 4);
            byte[] finalBytes = new byte[width * height * 4];
            if (!hasAlpha)
            {
                for (int y = 0; y < height; y++)
                {
                    byte filterType = rawBytes[sourceStride * y];
                    for (int x = 0; x < width; x++)
                    {
                        int sourceIndex = (sourceStride * y) + (x * 3) + 1;
                        int destinationIndex = ((width * 4) * y) + (x * 4);

                        if (filterType == 0)
                        {
                            finalBytes[destinationIndex++] = rawBytes[sourceIndex++];
                            finalBytes[destinationIndex++] = rawBytes[sourceIndex++];
                            finalBytes[destinationIndex++] = rawBytes[sourceIndex];
                            finalBytes[destinationIndex] = 255;
                        }
                        else if (filterType == 1) // SUB (pixel to the left)
                        {
                            byte a = 0;
                            if(x != 0)
                            {
                                a = finalBytes[destinationIndex - 4];
                            }

                            finalBytes[destinationIndex++] = (byte)((rawBytes[sourceIndex++] + a) % 256);

                            if (x != 0)
                            {
                                a = finalBytes[destinationIndex - 4];
                            }

                            finalBytes[destinationIndex++] = (byte)((rawBytes[sourceIndex++] + a) % 256);

                            if (x != 0)
                            {
                                a = finalBytes[destinationIndex - 4];
                            }

                            finalBytes[destinationIndex++] = (byte)((rawBytes[sourceIndex++] + a) % 256);
                            finalBytes[destinationIndex] = 255;
                        }
                        else if (filterType == 2) // UP (pixel above)
                        {
                            byte b = 0;
                            if (x != 0)
                            {
                                b = finalBytes[destinationIndex - destinationStride];
                            }

                            finalBytes[destinationIndex++] = (byte)((rawBytes[sourceIndex++] + b) % 256);

                            if (x != 0)
                            {
                                b = finalBytes[destinationIndex - destinationStride];
                            }

                            finalBytes[destinationIndex++] = (byte)((rawBytes[sourceIndex++] + b) % 256);

                            if (x != 0)
                            {
                                b = finalBytes[destinationIndex - destinationStride];
                            }

                            finalBytes[destinationIndex++] = (byte)((rawBytes[sourceIndex++] + b) % 256);
                            finalBytes[destinationIndex] = 255;
                        }
                        else if (filterType == 3) // Average
                        {
                            byte a = 0;
                            byte b = 0;

                            if (x != 0)
                            {
                                a = finalBytes[destinationIndex - 4];
                            }
                            if (y != 0)
                            {
                                b = finalBytes[destinationIndex - destinationStride];
                            }

                            finalBytes[destinationIndex++] = (byte)((rawBytes[sourceIndex++] + Math.Floor(a + b / 2m)) % 256);

                            if (x != 0)
                            {
                                a = finalBytes[destinationIndex - 4];
                            }
                            if (y != 0)
                            {
                                b = finalBytes[destinationIndex - destinationStride];
                            }
                            
                            finalBytes[destinationIndex++] = (byte)((rawBytes[sourceIndex++] + Math.Floor(a + b / 2m)) % 256);

                            if (x != 0)
                            {
                                a = finalBytes[destinationIndex - 4];
                            }
                            if (y != 0)
                            {
                                b = finalBytes[destinationIndex - destinationStride];
                            }

                            finalBytes[destinationIndex++] = (byte)((rawBytes[sourceIndex++] + Math.Floor(a + b / 2m)) % 256);
                            finalBytes[destinationIndex] = 255;
                        }
                        else if (filterType == 4) // Paeth
                        {
                            byte a = 0;
                            byte b = 0;
                            byte c = 0;

                            if(x != 0)
                            {
                                a = finalBytes[destinationIndex - 4];
                            }
                            if (y != 0)
                            {
                                b = finalBytes[destinationIndex - destinationStride];
                            }
                            if(x != 0 && y != 0)
                            {
                                c = finalBytes[destinationIndex - 4 - destinationStride];
                            }

                            finalBytes[destinationIndex++] = (byte)((rawBytes[sourceIndex++] + PaethPredictor(a, b, c)) % 256);

                            if (x != 0)
                            {
                                a = finalBytes[destinationIndex - 4];
                            }
                            if (y != 0)
                            {
                                b = finalBytes[destinationIndex - destinationStride];
                            }
                            if (x != 0 && y != 0)
                            {
                                c = finalBytes[destinationIndex - 4 - destinationStride];
                            }

                            finalBytes[destinationIndex++] = (byte)((rawBytes[sourceIndex++] + PaethPredictor(a, b, c)) % 256);

                            if (x != 0)
                            {
                                a = finalBytes[destinationIndex - 4];
                            }
                            if (y != 0)
                            {
                                b = finalBytes[destinationIndex - destinationStride];
                            }
                            if (x != 0 && y != 0)
                            {
                                c = finalBytes[destinationIndex - 4 - destinationStride];
                            }

                            finalBytes[destinationIndex++] = (byte)((rawBytes[sourceIndex++] + PaethPredictor(a, b, c)) % 256);
                            finalBytes[destinationIndex] = 255;
                        }
                    }
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
