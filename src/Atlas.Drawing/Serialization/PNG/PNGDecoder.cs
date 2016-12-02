using System;
using System.Collections;
using System.IO;
using System.IO.Compression;
using Atlas.Drawing.Conversion;

namespace Atlas.Drawing.Serialization.PNG
{
    public class PNGDecoder
    {
        private int _width;
        private int _height;
        private byte _bitsPerChannel;
        private byte _colorType;
        private byte _compressionMethod;
        private byte _filterMethod;
        private byte _interlaceMethod;
        private bool _useColorTable = false;
        private bool _hasColor = false;
        private bool _hasAlpha = false;

        private byte[] colorPalette = new byte[256*3];
        private byte[] alphaPalette = new byte[256];

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
                    case "PLTE":
                        ReadPalette(ref bytes, byteIndex, chunkLength);
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

            width = _width;
            height = _height;

            // add an alpha channel
            int bitsPerPixel = _bitsPerChannel * ((_hasColor ? 3 : 1) + (_hasAlpha ? 1 : 0));
            int bytesPerPixel = (bitsPerPixel / 8);
            int sourceStride = ((width * (this._useColorTable ? _bitsPerChannel : bitsPerPixel)) /8) + 1;
            int destinationStride = (width * 4);
            byte[] unfilteredBytes = new byte[((width * (this._useColorTable ? _bitsPerChannel : bitsPerPixel)) / 8) * height];

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

            if (_interlaceMethod == 1)
            {
                //1 6 4 6 2 6 4 6
                //7 7 7 7 7 7 7 7
                //5 6 5 6 5 6 5 6
                //7 7 7 7 7 7 7 7
                //3 6 4 6 3 6 4 6
                //7 7 7 7 7 7 7 7
                //5 6 5 6 5 6 5 6
                //7 7 7 7 7 7 7 7


                int x = 0;
                int y = 0;
                int i = 0;
                int j = 0;

                // Pass 1
                for (; i < unfilteredBytes.Length;)
                {
                    j = (x * 4) + (y * destinationStride);
                    finalBytes[j++] = unfilteredBytes[i++];
                    finalBytes[j++] = unfilteredBytes[i++];
                    finalBytes[j++] = unfilteredBytes[i++];
                    finalBytes[j++] = 255;
                    x += 8;
                    if (x >= width)
                    {
                        x = 0;
                        y += 8;
                    }

                    if (y >= height)
                    {
                        break;
                    }
                }

                // Pass 2
                x = 0;
                y = 0;
                for (; i < unfilteredBytes.Length;)
                {
                    j = (x * 4) + (y * destinationStride) + (4 * 4);
                    finalBytes[j++] = unfilteredBytes[i++];
                    finalBytes[j++] = unfilteredBytes[i++];
                    finalBytes[j++] = unfilteredBytes[i++];
                    finalBytes[j++] = 255;
                    x += 8;
                    if (x >= width)
                    {
                        x = 0;
                        y += 8;
                    }

                    if (y >= height)
                    {
                        break;
                    }
                }

                // Pass 3
                x = 0;
                y = 0;
                for (; i < unfilteredBytes.Length;)
                {
                    j = (x * 4) + ((y + 4) * destinationStride);
                    finalBytes[j++] = unfilteredBytes[i++];
                    finalBytes[j++] = unfilteredBytes[i++];
                    finalBytes[j++] = unfilteredBytes[i++];
                    finalBytes[j++] = 255;

                    j = (x * 4) + ((y + 4) * destinationStride) + (4 * 4);
                    finalBytes[j++] = unfilteredBytes[i++];
                    finalBytes[j++] = unfilteredBytes[i++];
                    finalBytes[j++] = unfilteredBytes[i++];
                    finalBytes[j++] = 255;

                    x += 8;
                    if (x >= width)
                    {
                        x = 0;
                        y += 8;
                    }

                    if (y >= height)
                    {
                        break;
                    }
                }

                // Pass 4
                x = 0;
                y = 0;
                for (; i < unfilteredBytes.Length;)
                {
                    j = (x * 4) + (y * destinationStride) + (2 * 4);
                    finalBytes[j++] = unfilteredBytes[i++];
                    finalBytes[j++] = unfilteredBytes[i++];
                    finalBytes[j++] = unfilteredBytes[i++];
                    finalBytes[j++] = 255;

                    j = (x * 4) + (y * destinationStride) + (6 * 4);
                    finalBytes[j++] = unfilteredBytes[i++];
                    finalBytes[j++] = unfilteredBytes[i++];
                    finalBytes[j++] = unfilteredBytes[i++];
                    finalBytes[j++] = 255;

                    j = (x * 4) + ((y + 4) * destinationStride) + (2 * 4);
                    finalBytes[j++] = unfilteredBytes[i++];
                    finalBytes[j++] = unfilteredBytes[i++];
                    finalBytes[j++] = unfilteredBytes[i++];
                    finalBytes[j++] = 255;

                    j = (x * 4) + ((y + 4) * destinationStride) + (6 * 4);
                    finalBytes[j++] = unfilteredBytes[i++];
                    finalBytes[j++] = unfilteredBytes[i++];
                    finalBytes[j++] = unfilteredBytes[i++];
                    finalBytes[j++] = 255;

                    x += 8;
                    if (x >= width)
                    {
                        x = 0;
                        y += 8;
                    }

                    if (y >= height)
                    {
                        break;
                    }
                }

                // Pass 5
                x = 0;
                y = 0;
                for (; i < unfilteredBytes.Length;)
                {
                    j = (x * 4) + ((y + 2) * destinationStride);
                    finalBytes[j++] = unfilteredBytes[i++];
                    finalBytes[j++] = unfilteredBytes[i++];
                    finalBytes[j++] = unfilteredBytes[i++];
                    finalBytes[j++] = 255;

                    j = (x * 4) + ((y + 2) * destinationStride) + (2 * 4);
                    finalBytes[j++] = unfilteredBytes[i++];
                    finalBytes[j++] = unfilteredBytes[i++];
                    finalBytes[j++] = unfilteredBytes[i++];
                    finalBytes[j++] = 255;

                    j = (x * 4) + ((y + 4) * destinationStride) + (4 * 4);
                    finalBytes[j++] = unfilteredBytes[i++];
                    finalBytes[j++] = unfilteredBytes[i++];
                    finalBytes[j++] = unfilteredBytes[i++];
                    finalBytes[j++] = 255;

                    j = (x * 4) + ((y + 4) * destinationStride) + (6 * 4);
                    finalBytes[j++] = unfilteredBytes[i++];
                    finalBytes[j++] = unfilteredBytes[i++];
                    finalBytes[j++] = unfilteredBytes[i++];
                    finalBytes[j++] = 255;

                    j = (x * 4) + ((y + 6) * destinationStride);
                    finalBytes[j++] = unfilteredBytes[i++];
                    finalBytes[j++] = unfilteredBytes[i++];
                    finalBytes[j++] = unfilteredBytes[i++];
                    finalBytes[j++] = 255;

                    j = (x * 4) + ((y + 6) * destinationStride) + (2 * 4);
                    finalBytes[j++] = unfilteredBytes[i++];
                    finalBytes[j++] = unfilteredBytes[i++];
                    finalBytes[j++] = unfilteredBytes[i++];
                    finalBytes[j++] = 255;

                    j = (x * 4) + ((y + 6) * destinationStride) + (4 * 4);
                    finalBytes[j++] = unfilteredBytes[i++];
                    finalBytes[j++] = unfilteredBytes[i++];
                    finalBytes[j++] = unfilteredBytes[i++];
                    finalBytes[j++] = 255;

                    j = (x * 4) + ((y + 6) * destinationStride) + (6 * 4);
                    finalBytes[j++] = unfilteredBytes[i++];
                    finalBytes[j++] = unfilteredBytes[i++];
                    finalBytes[j++] = unfilteredBytes[i++];
                    finalBytes[j++] = 255;

                    x += 8;
                    if (x >= width)
                    {
                        x = 0;
                        y += 8;
                    }

                    if (y >= height)
                    {
                        break;
                    }
                }

                // Pass 6
                x = 0;
                y = 0;
                for (; i < unfilteredBytes.Length;)
                {
                    j = (x * 4) + (y * destinationStride) + (1 * 4);
                    finalBytes[j++] = unfilteredBytes[i++];
                    finalBytes[j++] = unfilteredBytes[i++];
                    finalBytes[j++] = unfilteredBytes[i++];
                    finalBytes[j++] = 255;

                    j = (x * 4) + (y * destinationStride) + (3 * 4);
                    finalBytes[j++] = unfilteredBytes[i++];
                    finalBytes[j++] = unfilteredBytes[i++];
                    finalBytes[j++] = unfilteredBytes[i++];
                    finalBytes[j++] = 255;

                    j = (x * 4) + (y * destinationStride) + (5 * 4);
                    finalBytes[j++] = unfilteredBytes[i++];
                    finalBytes[j++] = unfilteredBytes[i++];
                    finalBytes[j++] = unfilteredBytes[i++];
                    finalBytes[j++] = 255;

                    j = (x * 4) + (y * destinationStride) + (7 * 4);
                    finalBytes[j++] = unfilteredBytes[i++];
                    finalBytes[j++] = unfilteredBytes[i++];
                    finalBytes[j++] = unfilteredBytes[i++];
                    finalBytes[j++] = 255;

                    j = (x * 4) + ((y + 2) * destinationStride) + (1 * 4);
                    finalBytes[j++] = unfilteredBytes[i++];
                    finalBytes[j++] = unfilteredBytes[i++];
                    finalBytes[j++] = unfilteredBytes[i++];
                    finalBytes[j++] = 255;

                    j = (x * 4) + ((y + 2) * destinationStride) + (3 * 4);
                    finalBytes[j++] = unfilteredBytes[i++];
                    finalBytes[j++] = unfilteredBytes[i++];
                    finalBytes[j++] = unfilteredBytes[i++];
                    finalBytes[j++] = 255;

                    j = (x * 4) + ((y + 2) * destinationStride) + (5 * 4);
                    finalBytes[j++] = unfilteredBytes[i++];
                    finalBytes[j++] = unfilteredBytes[i++];
                    finalBytes[j++] = unfilteredBytes[i++];
                    finalBytes[j++] = 255;

                    j = (x * 4) + ((y + 2) * destinationStride) + (7 * 4);
                    finalBytes[j++] = unfilteredBytes[i++];
                    finalBytes[j++] = unfilteredBytes[i++];
                    finalBytes[j++] = unfilteredBytes[i++];
                    finalBytes[j++] = 255;

                    j = (x * 4) + ((y + 4) * destinationStride) + (1 * 4);
                    finalBytes[j++] = unfilteredBytes[i++];
                    finalBytes[j++] = unfilteredBytes[i++];
                    finalBytes[j++] = unfilteredBytes[i++];
                    finalBytes[j++] = 255;

                    j = (x * 4) + ((y + 4) * destinationStride) + (3 * 4);
                    finalBytes[j++] = unfilteredBytes[i++];
                    finalBytes[j++] = unfilteredBytes[i++];
                    finalBytes[j++] = unfilteredBytes[i++];
                    finalBytes[j++] = 255;

                    j = (x * 4) + ((y + 4) * destinationStride) + (5 * 4);
                    finalBytes[j++] = unfilteredBytes[i++];
                    finalBytes[j++] = unfilteredBytes[i++];
                    finalBytes[j++] = unfilteredBytes[i++];
                    finalBytes[j++] = 255;

                    j = (x * 4) + ((y + 4) * destinationStride) + (7 * 4);
                    finalBytes[j++] = unfilteredBytes[i++];
                    finalBytes[j++] = unfilteredBytes[i++];
                    finalBytes[j++] = unfilteredBytes[i++];
                    finalBytes[j++] = 255;

                    j = (x * 4) + ((y + 6) * destinationStride) + (1 * 4);
                    finalBytes[j++] = unfilteredBytes[i++];
                    finalBytes[j++] = unfilteredBytes[i++];
                    finalBytes[j++] = unfilteredBytes[i++];
                    finalBytes[j++] = 255;

                    j = (x * 4) + ((y + 6) * destinationStride) + (3 * 4);
                    finalBytes[j++] = unfilteredBytes[i++];
                    finalBytes[j++] = unfilteredBytes[i++];
                    finalBytes[j++] = unfilteredBytes[i++];
                    finalBytes[j++] = 255;

                    j = (x * 4) + ((y + 6) * destinationStride) + (5 * 4);
                    finalBytes[j++] = unfilteredBytes[i++];
                    finalBytes[j++] = unfilteredBytes[i++];
                    finalBytes[j++] = unfilteredBytes[i++];
                    finalBytes[j++] = 255;

                    j = (x * 4) + ((y + 6) * destinationStride) + (7 * 4);
                    finalBytes[j++] = unfilteredBytes[i++];
                    finalBytes[j++] = unfilteredBytes[i++];
                    finalBytes[j++] = unfilteredBytes[i++];
                    finalBytes[j++] = 255;

                    x += 8;
                    if (x >= width)
                    {
                        x = 0;
                        y += 8;
                    }

                    if (y >= height)
                    {
                        break;
                    }
                }

                // Pass 7
                x = 0;
                y = 0;
                for (; i < unfilteredBytes.Length;)
                {
                    for (int k = 0; k < 8; k++)
                    {
                        j = (x * 4) + ((y + 1) * destinationStride) + (k * 4);
                        finalBytes[j++] = unfilteredBytes[i++];
                        finalBytes[j++] = unfilteredBytes[i++];
                        finalBytes[j++] = unfilteredBytes[i++];
                        finalBytes[j++] = 255;
                    }

                    for (int k = 0; k < 8; k++)
                    {
                        j = (x * 4) + ((y + 3) * destinationStride) + (k * 4);
                        finalBytes[j++] = unfilteredBytes[i++];
                        finalBytes[j++] = unfilteredBytes[i++];
                        finalBytes[j++] = unfilteredBytes[i++];
                        finalBytes[j++] = 255;
                    }

                    for (int k = 0; k < 8; k++)
                    {
                        j = (x * 4) + ((y + 5) * destinationStride) + (k * 4);
                        finalBytes[j++] = unfilteredBytes[i++];
                        finalBytes[j++] = unfilteredBytes[i++];
                        finalBytes[j++] = unfilteredBytes[i++];
                        finalBytes[j++] = 255;
                    }

                    for (int k = 0; k < 8; k++)
                    {
                        j = (x * 4) + ((y + 7) * destinationStride) + (k * 4);
                        finalBytes[j++] = unfilteredBytes[i++];
                        finalBytes[j++] = unfilteredBytes[i++];
                        finalBytes[j++] = unfilteredBytes[i++];
                        finalBytes[j++] = 255;
                    }


                    x += 8;
                    if (x >= width)
                    {
                        x = 0;
                        y += 8;
                    }

                    if (y >= height)
                    {
                        break;
                    }
                }
            }
            else
            {


                if (_useColorTable)
                {
                    if (bitsPerPixel == 3)
                    {
                        for (int i = 0, j = 0; i < unfilteredBytes.Length; i++)
                        {
                            var bits = new BitArray(new byte[] { unfilteredBytes[i] });
                            for (int b = 7; b >= 0; b--)
                            {
                                int colorIndex = bits[b] ? 1 : 0;
                                finalBytes[j++] = colorPalette[colorIndex * 3];
                                finalBytes[j++] = colorPalette[(colorIndex * 3) + 1];
                                finalBytes[j++] = colorPalette[(colorIndex * 3) + 2];
                                finalBytes[j++] = 255;
                            }
                        }
                    }
                    else if (bitsPerPixel == 6)
                    {
                        for (int i = 0, j = 0; i < unfilteredBytes.Length; i++)
                        {
                            var bits = new BitArray(new byte[] { unfilteredBytes[i] });
                            for (int b = 7; b >= 0; b -= 2)
                            {
                                int colorIndex = ((bits[b] ? 1 : 0) * 2) + (bits[b - 1] ? 1 : 0);
                                finalBytes[j++] = colorPalette[colorIndex * 3];
                                finalBytes[j++] = colorPalette[(colorIndex * 3) + 1];
                                finalBytes[j++] = colorPalette[(colorIndex * 3) + 2];
                                finalBytes[j++] = 255;
                            }
                        }
                    }
                    else if (bitsPerPixel == 12)
                    {
                        for (int i = 0, j = 0; i < unfilteredBytes.Length; i++)
                        {
                            uint b = unfilteredBytes[i];

                            int colorIndex = (byte)((b & 0xF0) >> 4);
                            finalBytes[j++] = colorPalette[colorIndex * 3];
                            finalBytes[j++] = colorPalette[(colorIndex * 3) + 1];
                            finalBytes[j++] = colorPalette[(colorIndex * 3) + 2];
                            finalBytes[j++] = 255; // alphaPalette[colorIndex];

                            int colorIndex2 = (byte)(b & 0x0F);
                            finalBytes[j++] = colorPalette[colorIndex * 3];
                            finalBytes[j++] = colorPalette[(colorIndex * 3) + 1];
                            finalBytes[j++] = colorPalette[(colorIndex * 3) + 2];
                            finalBytes[j++] = 255; // alphaPalette[colorIndex];

                        }
                    }
                    else
                    {
                        for (int i = 0, j = 0; i < unfilteredBytes.Length; i++)
                        {
                            int colorIndex = unfilteredBytes[i];
                            finalBytes[j++] = colorPalette[colorIndex * 3];
                            finalBytes[j++] = colorPalette[(colorIndex * 3) + 1];
                            finalBytes[j++] = colorPalette[(colorIndex * 3) + 2];
                            finalBytes[j++] = 255; // alphaPalette[colorIndex];
                        }
                    }
                }
                else
                {
                    if (bitsPerPixel == 1)
                    {
                        for (int i = 0, j = 0; i < unfilteredBytes.Length; i++)
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
                            for (int b = 7; b >= 0; b -= 2)
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
                        if (_hasAlpha) // 8 bit 2 channels
                        {
                            for (int i = 0, j = 0; i < unfilteredBytes.Length; i++)
                            {
                                byte color = unfilteredBytes[i++];
                                byte alpha = unfilteredBytes[i];
                                finalBytes[j++] = color;
                                finalBytes[j++] = color;
                                finalBytes[j++] = color;
                                finalBytes[j++] = alpha;
                            }
                        }
                        else
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
                    else if (bitsPerPixel == 32)
                    {
                        if (!_hasColor && _hasAlpha) // 16 bit 2 channels
                        {
                            for (int i = 0, j = 0; i < unfilteredBytes.Length;)
                            {
                                byte color = (byte)(((unfilteredBytes[i++] << 8) | unfilteredBytes[i++]) / 256);
                                byte alpha = (byte)(((unfilteredBytes[i++] << 8) | unfilteredBytes[i++]) / 256);
                                finalBytes[j++] = color;
                                finalBytes[j++] = color;
                                finalBytes[j++] = color;
                                finalBytes[j++] = alpha;
                            }
                        }
                        else // 8 bit color with alpha
                        {
                            for (int i = 0, j = 0; i < unfilteredBytes.Length;)
                            {
                                finalBytes[j++] = unfilteredBytes[i++];
                                finalBytes[j++] = unfilteredBytes[i++];
                                finalBytes[j++] = unfilteredBytes[i++];
                                finalBytes[j++] = unfilteredBytes[i++];
                            }
                        }
                    }
                    else if (bitsPerPixel == 48) // 16 bit color
                    {
                        for (int i = 0, j = 0; i < unfilteredBytes.Length;)
                        {
                            finalBytes[j++] = (byte)(((unfilteredBytes[i++] << 8) | unfilteredBytes[i++]) / 256);
                            finalBytes[j++] = (byte)(((unfilteredBytes[i++] << 8) | unfilteredBytes[i++]) / 256);
                            finalBytes[j++] = (byte)(((unfilteredBytes[i++] << 8) | unfilteredBytes[i++]) / 256);
                            finalBytes[j++] = 255;
                        }
                    }
                    else if (bitsPerPixel == 64) // 16 bit color with alpha
                    {
                        for (int i = 0, j = 0; i < unfilteredBytes.Length;)
                        {
                            finalBytes[j++] = (byte)(((unfilteredBytes[i++] << 8) | unfilteredBytes[i++]) / 256);
                            finalBytes[j++] = (byte)(((unfilteredBytes[i++] << 8) | unfilteredBytes[i++]) / 256);
                            finalBytes[j++] = (byte)(((unfilteredBytes[i++] << 8) | unfilteredBytes[i++]) / 256);
                            finalBytes[j++] = (byte)(((unfilteredBytes[i++] << 8) | unfilteredBytes[i++]) / 256);
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
            _width = EndianBitConverter.Big.ToInt32(bytes, offset);
            _height = EndianBitConverter.Big.ToInt32(bytes, offset + 4);
            _bitsPerChannel = bytes[offset + 8];
            _colorType = bytes[offset + 9];
            _compressionMethod = bytes[offset + 10];
            _filterMethod = bytes[offset + 11];
            _interlaceMethod = bytes[offset + 12];

            var colorTypeBits = new BitArray(new byte[] { _colorType });
            _useColorTable = colorTypeBits[0];
            _hasColor = colorTypeBits[1];
            _hasAlpha = colorTypeBits[2];
        }

        private void ReadPalette(ref byte[]bytes , int offset, int length)
        {
            for (int i = 0; i < length; i++)
            {
                colorPalette[i] = bytes[offset++];
            }
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
