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
        private bool _hasTransparency = false;

        private byte[] colorPalette = new byte[256*3];
        private byte[] alphaPalette = new byte[256];

        private UInt16[] _transparentColor;

        private uint _gama = 0;

        private int CalculateScanlineLength(int width)
        {
            int scanlineLength = width * (this._useColorTable ? _bitsPerChannel : this._bitsPerChannel * ((_hasColor ? 3 : 1) + (_hasAlpha ? 1 : 0)));

            int amount = scanlineLength % 8;
            if (amount != 0)
            {
                scanlineLength += 8 - amount;
            }

            return scanlineLength / 8;
        }

        public byte[] Decode(ref byte[] bytes, out int width, out int height)
        {
            byte[] rawBytes = null;
            // Skip the first 8 bytes
            int byteIndex = 8;
            bool end = false;
            using (var ms = new MemoryStream())
            {
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
                        case "tRNS":
                            ReadTransparencyPalette(ref bytes, byteIndex, chunkLength);
                            break;
                        case "gAMA":
                            _gama = EndianBitConverter.Big.ToUInt32(bytes, byteIndex);
                            break;
                        case "IDAT":
                            ms.Write(bytes, byteIndex, chunkLength);
                            break;
                        case "IEND":
                            end = true;
                            break;
                    }

                    byteIndex += chunkLength;
                    uint chunkCRC = EndianBitConverter.Big.ToUInt32(bytes, byteIndex); byteIndex += 4;
                }

                ms.Position = 2;
                using (var compressor = new DeflateStream(ms, CompressionMode.Decompress))
                using (var resultStream = new MemoryStream())
                {
                    compressor.CopyTo(resultStream);
                    rawBytes = resultStream.ToArray();
                }
            }

            width = _width;
            height = _height;

            int scanLineCount = height;

            int bitsPerPixel = _bitsPerChannel * ((_hasColor ? 3 : 1) + (_hasAlpha ? 1 : 0));
            int bytesPerPixel = (int)Math.Ceiling(bitsPerPixel / 8m);
            int sourceStride = (int)Math.Ceiling(((width * (this._useColorTable ? _bitsPerChannel : bitsPerPixel)) / 8d)) + 1;
            int destinationStride = (width * 4);

            int unfilteredBytesLength = (sourceStride - 1) * height;

            int interlacePass = 0;
            int[] interlaceStrides = new int[7];
            int[] interlaceLineCounts = new int[7];

            if (_interlaceMethod == 1)
            {
                unfilteredBytesLength = 0;
                for (int i = 0; i < 7; i++)
                {
                    if (i == 0)
                    {
                        interlaceStrides[i] = CalculateScanlineLength((width + 7) / 8) + 1; //(int)Math.Ceiling(((width * (this._useColorTable ? _bitsPerChannel : bitsPerPixel)) / 8) /8d) + 1;
                        interlaceLineCounts[i] = (height + 7) / 8; // (int)Math.Ceiling(height / 8d);
                    }
                    if (i == 1)
                    {
                        interlaceStrides[i] = CalculateScanlineLength((width + 3) / 8) + 1; //(int)Math.Ceiling(((width * (this._useColorTable ? _bitsPerChannel : bitsPerPixel)) / 8) / 8d) + 1;
                        interlaceLineCounts[i] = (height + 7) / 8; // (int)Math.Ceiling(height / 8d);
                        if(width < 5)
                        {
                            interlaceLineCounts[i] = 0;
                        }
                    }
                    if (i == 2)
                    {
                        interlaceStrides[i] = CalculateScanlineLength((width + 3) / 4) + 1; //(int)Math.Ceiling(((width * (this._useColorTable ? _bitsPerChannel : bitsPerPixel)) / 8) / 4d) + 1;
                        interlaceLineCounts[i] = (height + 3) / 8; //(height / 8) + (height % 8 > 3 ? 1 : 0);
                        if(height < 5)
                        {
                            interlaceStrides[i] = 1;
                        }
                    }
                    if (i == 3)
                    {
                        interlaceStrides[i] = CalculateScanlineLength((width + 1) / 4) + 1; //(int)Math.Ceiling(((width * (this._useColorTable ? _bitsPerChannel : bitsPerPixel)) / 8) / 4d) + 1;
                        interlaceLineCounts[i] = (height + 3) / 4; //(int)Math.Ceiling(height / 4d);
                        if (width < 3)
                        {
                            interlaceLineCounts[i] = 0;
                        }
                    }
                    if (i == 4)
                    {
                        interlaceStrides[i] = CalculateScanlineLength((width + 1) / 2) + 1; //(int)Math.Ceiling((((width / 2d) * (this._useColorTable ? _bitsPerChannel : bitsPerPixel)) / 8)) + 1;
                        interlaceLineCounts[i] = (height + 1) / 4; // ((height / 8) * 2) + (height % 8 > 6 ? 2 : height % 8 > 2 ? 1 : 0);
                        if (height < 3)
                        {
                            interlaceStrides[i] = 1;
                        }
                    }
                    if (i == 5)
                    {
                        interlaceStrides[i] = CalculateScanlineLength(width / 2) + 1; //(int)Math.Ceiling(((Math.Floor(width / 2d) * (this._useColorTable ? _bitsPerChannel : bitsPerPixel)) / 8)) + 1;
                        interlaceLineCounts[i] = (int)Math.Ceiling(height / 2d);
                        if (width < 2)
                        {
                            interlaceLineCounts[i] = 0;
                        }
                    }
                    if (i == 6)
                    {
                        interlaceStrides[i] = CalculateScanlineLength(width) + 1; //(int)Math.Ceiling(((width * (this._useColorTable ? _bitsPerChannel : bitsPerPixel)) / 8d)) + 1;
                        interlaceLineCounts[i] = (int)Math.Floor(height / 2d);
                        if (height < 2)
                        {
                            interlaceStrides[i] = 1;
                        }
                    }

                    unfilteredBytesLength += (interlaceStrides[i] - 1) * interlaceLineCounts[i];
                }

                sourceStride = interlaceStrides[0];
                scanLineCount = interlaceLineCounts[0];
            }

            byte[] unfilteredBytes = new byte[unfilteredBytesLength];
            int w = 0;
            int h = 0;
            
            byte currentFilter = 0;
            for (int i = 0, j = 0; i < rawBytes.Length; i++)
            {
                if(h == scanLineCount)
                {
                    interlacePass += 1;
                    sourceStride = interlaceStrides[interlacePass];
                    scanLineCount = interlaceLineCounts[interlacePass];

                    while(scanLineCount == 0)
                    {
                        interlacePass += 1;
                        sourceStride = interlaceStrides[interlacePass];
                        scanLineCount = interlaceLineCounts[interlacePass];
                    }

                    h = 0;
                }
                if(w == sourceStride)
                {
                    w = 0;
                    h += 1;
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
                        if (h > 0)
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
                        if (h > 0)
                        {
                            b = unfilteredBytes[j - (sourceStride - 1)];
                        }

                        unfilteredBytes[j++] = (byte)((rawBytes[i] + ((a + b) >> 1)) % 256);
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
                        if (h > 0)
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
                int pass = 0;

                int[] passXIncrement = new int[] { 8, 8, 4, 4, 2, 2, 1 };
                int[] passXOffset = new int[]    { 0, 4, 0, 2, 0, 1, 0 };
                int[] passYIncrement = new int[] { 8, 8, 8, 4, 4, 2, 2 };
                int[] passYOffset = new int[]    { 0, 0, 4, 0, 2, 0, 1 };


                if (_useColorTable)
                {
                    if (bitsPerPixel == 3)
                    {
                        for (; i < unfilteredBytes.Length; )
                        {
                            if (x < width && y < height)
                            {
                                var bits = new BitArray(new byte[] { unfilteredBytes[i++] });
                                for (int b = 7; b >= 0; b--)
                                {
                                    j = (x * 4) + (y * destinationStride);
                                    int colorIndex = bits[b] ? 1 : 0;
                                    finalBytes[j++] = colorPalette[colorIndex * 3];
                                    finalBytes[j++] = colorPalette[(colorIndex * 3) + 1];
                                    finalBytes[j++] = colorPalette[(colorIndex * 3) + 2];
                                    finalBytes[j++] = 255;

                                    x += passXIncrement[pass];
                                    if (x >= width)
                                    {
                                        // throw away unused bits in the last byte
                                        break;
                                    }
                                }
                            }

                            if (x >= width)
                            {
                                x = passXOffset[pass];
                                y += passYIncrement[pass];
                            }

                            if (y >= height)
                            {
                                pass += 1;
                                if (pass == 7)
                                {
                                    break;
                                }
                                x = passXOffset[pass];
                                y = passYOffset[pass];
                            }
                        }
                    }
                    else if (bitsPerPixel == 6)
                    {
                        for (; i < unfilteredBytes.Length; i++)
                        {
                            var bits = new BitArray(new byte[] { unfilteredBytes[i] });
                            for (int b = 7; b >= 0; b -= 2)
                            {
                                j = (x * 4) + (y * destinationStride);
                                int colorIndex = ((bits[b] ? 1 : 0) * 2) + (bits[b - 1] ? 1 : 0);
                                finalBytes[j++] = colorPalette[colorIndex * 3];
                                finalBytes[j++] = colorPalette[(colorIndex * 3) + 1];
                                finalBytes[j++] = colorPalette[(colorIndex * 3) + 2];
                                finalBytes[j++] = 255;

                                x += passXIncrement[pass];
                                if (x >= width)
                                {
                                    // throw away unused bits in the last byte
                                    break;
                                }
                            }

                            if (x >= width)
                            {
                                x = passXOffset[pass];
                                y += passYIncrement[pass];
                            }

                            if (y >= height)
                            {
                                pass += 1;
                                if (pass == 7)
                                {
                                    break;
                                }
                                x = passXOffset[pass];
                                y = passYOffset[pass];
                            }
                        }
                    }
                    else if (bitsPerPixel == 12)
                    {
                        for (; i < unfilteredBytes.Length; i++)
                        {
                            uint b = unfilteredBytes[i];

                            j = (x * 4) + (y * destinationStride);
                            int colorIndex = (byte)((b & 0xF0) >> 4);
                            finalBytes[j++] = colorPalette[colorIndex * 3];
                            finalBytes[j++] = colorPalette[(colorIndex * 3) + 1];
                            finalBytes[j++] = colorPalette[(colorIndex * 3) + 2];
                            finalBytes[j++] = 255; // alphaPalette[colorIndex];

                            x += passXIncrement[pass];
                            if (x < width)
                            {
                                // throw away unused bits in the last byte
                                j = (x * 4) + (y * destinationStride);
                                int colorIndex2 = (byte)(b & 0x0F);
                                finalBytes[j++] = colorPalette[colorIndex2 * 3];
                                finalBytes[j++] = colorPalette[(colorIndex2 * 3) + 1];
                                finalBytes[j++] = colorPalette[(colorIndex2 * 3) + 2];
                                finalBytes[j++] = 255; // alphaPalette[colorIndex];
                            }

                            

                            x += passXIncrement[pass];

                            if (x >= width)
                            {
                                x = passXOffset[pass];
                                y += passYIncrement[pass];
                            }

                            if (y >= height)
                            {
                                pass += 1;
                                if (pass == 7)
                                {
                                    break;
                                }
                                x = passXOffset[pass];
                                y = passYOffset[pass];
                            }
                        }
                    }
                    else
                    {
                        for (; i < unfilteredBytes.Length; i++)
                        {
                            j = (x * 4) + (y * destinationStride);
                            int colorIndex = unfilteredBytes[i];
                            finalBytes[j++] = colorPalette[colorIndex * 3];
                            finalBytes[j++] = colorPalette[(colorIndex * 3) + 1];
                            finalBytes[j++] = colorPalette[(colorIndex * 3) + 2];
                            finalBytes[j++] = 255; // alphaPalette[colorIndex];

                            x += passXIncrement[pass];

                            if (x >= width)
                            {
                                x = passXOffset[pass];
                                y += passYIncrement[pass];
                            }

                            if (y >= height)
                            {
                                pass += 1;
                                if (pass == 7)
                                {
                                    break;
                                }
                                x = passXOffset[pass];
                                y = passYOffset[pass];
                            }
                        }
                    }
                }
                else
                {

                    if (bitsPerPixel == 1)
                    {
                        for (; i < unfilteredBytes.Length;)
                        {
                            var bits = new BitArray(new byte[] { unfilteredBytes[i++] });
                            for (int b = 7; b >= 0; b--)
                            {
                                j = (x * 4) + (y * destinationStride);

                                finalBytes[j++] = (byte)(bits[b] ? 255 : 0);
                                finalBytes[j++] = (byte)(bits[b] ? 255 : 0);
                                finalBytes[j++] = (byte)(bits[b] ? 255 : 0);
                                finalBytes[j++] = 255;

                                x += passXIncrement[pass];
                                if (x >= width)
                                {
                                    // throw away unused bits in the last byte
                                    break;
                                }
                            }

                            if (x >= width)
                            {
                                x = passXOffset[pass];
                                y += passYIncrement[pass];
                            }

                            if (y >= height)
                            {
                                pass += 1;
                                if (pass == 7)
                                {
                                    break;
                                }
                                x = passXOffset[pass];
                                y = passYOffset[pass];
                            }
                        }
                    }
                    else if (bitsPerPixel == 2)
                    {
                        for (; i < unfilteredBytes.Length;)
                        {
                            var bits = new BitArray(new byte[] { unfilteredBytes[i++] });

                            for (int b = 7; b >= 0; b -= 2)
                            {
                                j = (x * 4) + (y * destinationStride);

                                int value = ((bits[b] ? 1 : 0) * 2) + (bits[b - 1] ? 1 : 0);
                                byte color = (byte)(value * 85);
                                finalBytes[j++] = color;
                                finalBytes[j++] = color;
                                finalBytes[j++] = color;
                                finalBytes[j++] = 255;

                                x += passXIncrement[pass];
                                if (x >= width)
                                {
                                    // throw away unused bits in the last byte
                                    break;
                                }
                            }

                            if (x >= width)
                            {
                                x = passXOffset[pass];
                                y += passYIncrement[pass];
                            }

                            if (y >= height)
                            {
                                pass += 1;
                                if (pass == 7)
                                {
                                    break;
                                }
                                x = passXOffset[pass];
                                y = passYOffset[pass];
                            }
                        }
                    }
                    else if (bitsPerPixel == 4)
                    {
                        for (; i < unfilteredBytes.Length;)
                        {
                            j = (x * 4) + (y * destinationStride);
                            uint b = unfilteredBytes[i++];
                            byte color = (byte)(((b & 0xF0) >> 4) * 17);
                            finalBytes[j++] = color;
                            finalBytes[j++] = color;
                            finalBytes[j++] = color;
                            finalBytes[j++] = 255;

                            x += passXIncrement[pass];
                            if (x >= width)
                            {
                                // throw away unused bits in the last byte
                                break;
                            }

                            j = (x * 4) + (y * destinationStride);
                            byte color2 = (byte)((b & 0x0F) * 17);
                            finalBytes[j++] = color2;
                            finalBytes[j++] = color2;
                            finalBytes[j++] = color2;
                            finalBytes[j++] = 255;

                            x += passXIncrement[pass];

                            if (x >= width)
                            {
                                x = passXOffset[pass];
                                y += passYIncrement[pass];
                            }

                            if (y >= height)
                            {
                                pass += 1;
                                if (pass == 7)
                                {
                                    break;
                                }
                                x = passXOffset[pass];
                                y = passYOffset[pass];
                            }
                        }
                    }
                    else if (bitsPerPixel == 8)
                    {
                        for (; i < unfilteredBytes.Length;)
                        {
                            j = (x * 4) + (y * destinationStride);
                            byte color = unfilteredBytes[i++];
                            finalBytes[j++] = color;
                            finalBytes[j++] = color;
                            finalBytes[j++] = color;
                            finalBytes[j++] = 255;

                            x += passXIncrement[pass];

                            if (x >= width)
                            {
                                x = passXOffset[pass];
                                y += passYIncrement[pass];
                            }

                            if (y >= height)
                            {
                                pass += 1;
                                if (pass == 7)
                                {
                                    break;
                                }
                                x = passXOffset[pass];
                                y = passYOffset[pass];
                            }
                        }
                    }
                    else if (bitsPerPixel == 16)
                    {
                        if (_hasAlpha) // 8 bit 2 channels
                        {
                            for (; i < unfilteredBytes.Length;)
                            {
                                byte color = unfilteredBytes[i++];
                                byte alpha = unfilteredBytes[i++];

                                j = (x * 4) + (y * destinationStride);
                                finalBytes[j++] = color;
                                finalBytes[j++] = color;
                                finalBytes[j++] = color;
                                finalBytes[j++] = alpha;

                                x += passXIncrement[pass];

                                if (x >= width)
                                {
                                    x = passXOffset[pass];
                                    y += passYIncrement[pass];
                                }

                                if (y >= height)
                                {
                                    pass += 1;
                                    if (pass == 7)
                                    {
                                        break;
                                    }
                                    x = passXOffset[pass];
                                    y = passYOffset[pass];
                                }
                            }
                        }
                        else
                        {
                            for (; i < unfilteredBytes.Length; )
                            {
                                byte color = unfilteredBytes[i++];
                                byte color2 = unfilteredBytes[i++];

                                byte color3 = (byte)(((int)(color << 8) + color2) / 256);

                                j = (x * 4) + (y * destinationStride);
                                finalBytes[j++] = color3;
                                finalBytes[j++] = color3;
                                finalBytes[j++] = color3;
                                finalBytes[j++] = 255;

                                x += passXIncrement[pass];

                                if (x >= width)
                                {
                                    x = passXOffset[pass];
                                    y += passYIncrement[pass];
                                }

                                if (y >= height)
                                {
                                    pass += 1;
                                    if (pass == 7)
                                    {
                                        break;
                                    }
                                    x = passXOffset[pass];
                                    y = passYOffset[pass];
                                }
                            }
                        }
                    }

                    else if (bitsPerPixel == 24)
                    {
                        for (; i < unfilteredBytes.Length;)
                        {
                            j = (x * 4) + (y * destinationStride);
                            finalBytes[j++] = unfilteredBytes[i++];
                            finalBytes[j++] = unfilteredBytes[i++];
                            finalBytes[j++] = unfilteredBytes[i++];
                            finalBytes[j++] = 255;

                            x += passXIncrement[pass];

                            if (x >= width)
                            {
                                x = passXOffset[pass];
                                y += passYIncrement[pass];
                            }

                            if (y >= height)
                            {
                                pass += 1;
                                if (pass == 7)
                                {
                                    break;
                                }
                                x = passXOffset[pass];
                                y = passYOffset[pass];
                            }
                        }
                    }
                    else if (bitsPerPixel == 32)
                    {
                        if (!_hasColor && _hasAlpha) // 16 bit 2 channels
                        {
                            for (; i < unfilteredBytes.Length;)
                            {
                                byte color = (byte)(((unfilteredBytes[i++] << 8) | unfilteredBytes[i++]) / 256);
                                byte alpha = (byte)(((unfilteredBytes[i++] << 8) | unfilteredBytes[i++]) / 256);
                                j = (x * 4) + (y * destinationStride);
                                finalBytes[j++] = color;
                                finalBytes[j++] = color;
                                finalBytes[j++] = color;
                                finalBytes[j++] = alpha;

                                x += passXIncrement[pass];

                                if (x >= width)
                                {
                                    x = passXOffset[pass];
                                    y += passYIncrement[pass];
                                }

                                if (y >= height)
                                {
                                    pass += 1;
                                    if (pass == 7)
                                    {
                                        break;
                                    }
                                    x = passXOffset[pass];
                                    y = passYOffset[pass];
                                }
                            }
                        }
                        else // 8 bit color with alpha
                        {
                            for (; i < unfilteredBytes.Length;)
                            {
                                j = (x * 4) + (y * destinationStride);
                                finalBytes[j++] = unfilteredBytes[i++];
                                finalBytes[j++] = unfilteredBytes[i++];
                                finalBytes[j++] = unfilteredBytes[i++];
                                finalBytes[j++] = unfilteredBytes[i++];

                                x += passXIncrement[pass];

                                if (x >= width)
                                {
                                    x = passXOffset[pass];
                                    y += passYIncrement[pass];
                                }

                                if (y >= height)
                                {
                                    pass += 1;
                                    if (pass == 7)
                                    {
                                        break;
                                    }
                                    x = passXOffset[pass];
                                    y = passYOffset[pass];
                                }
                            }
                        }
                    }
                    else if (bitsPerPixel == 48) // 16 bit color
                    {
                        for (; i < unfilteredBytes.Length;)
                        {
                            j = (x * 4) + (y * destinationStride);
                            finalBytes[j++] = (byte)(((unfilteredBytes[i++] << 8) | unfilteredBytes[i++]) / 256);
                            finalBytes[j++] = (byte)(((unfilteredBytes[i++] << 8) | unfilteredBytes[i++]) / 256);
                            finalBytes[j++] = (byte)(((unfilteredBytes[i++] << 8) | unfilteredBytes[i++]) / 256);
                            finalBytes[j++] = 255;

                            x += passXIncrement[pass];

                            if (x >= width)
                            {
                                x = passXOffset[pass];
                                y += passYIncrement[pass];
                            }

                            if (y >= height)
                            {
                                pass += 1;
                                if (pass == 7)
                                {
                                    break;
                                }
                                x = passXOffset[pass];
                                y = passYOffset[pass];
                            }
                        }
                    }
                    else if (bitsPerPixel == 64) // 16 bit color with alpha
                    {
                        for (; i < unfilteredBytes.Length;)
                        {
                            j = (x * 4) + (y * destinationStride);
                            finalBytes[j++] = (byte)(((unfilteredBytes[i++] << 8) | unfilteredBytes[i++]) / 256);
                            finalBytes[j++] = (byte)(((unfilteredBytes[i++] << 8) | unfilteredBytes[i++]) / 256);
                            finalBytes[j++] = (byte)(((unfilteredBytes[i++] << 8) | unfilteredBytes[i++]) / 256);
                            finalBytes[j++] = (byte)(((unfilteredBytes[i++] << 8) | unfilteredBytes[i++]) / 256);

                            x += passXIncrement[pass];

                            if (x >= width)
                            {
                                x = passXOffset[pass];
                                y += passYIncrement[pass];
                            }

                            if (y >= height)
                            {
                                pass += 1;
                                if (pass == 7)
                                {
                                    break;
                                }
                                x = passXOffset[pass];
                                y = passYOffset[pass];
                            }
                        }
                    }
                }

            }
            else // Non interlaced
            {
                if (_useColorTable)
                {
                    if (bitsPerPixel == 3)
                    {
                        int x = 0;
                        for (int i = 0, j = 0; i < unfilteredBytes.Length; i++)
                        {
                            var bits = new BitArray(new byte[] { unfilteredBytes[i] });
                            for (int b = 7; b >= 0; b--)
                            {
                                int colorIndex = bits[b] ? 1 : 0;
                                finalBytes[j++] = colorPalette[colorIndex * 3];
                                finalBytes[j++] = colorPalette[(colorIndex * 3) + 1];
                                finalBytes[j++] = colorPalette[(colorIndex * 3) + 2];
                                finalBytes[j++] = _hasTransparency ? alphaPalette[colorIndex] : (byte)255;

                                x += 1;

                                if (x >= width)
                                {
                                    // throw away unused bits in the last byte
                                    x = 0;
                                    break;
                                }
                            }
                        }
                    }
                    else if (bitsPerPixel == 6)
                    {
                        int x = 0;
                        for (int i = 0, j = 0; i < unfilteredBytes.Length; i++)
                        {
                            var bits = new BitArray(new byte[] { unfilteredBytes[i] });
                            for (int b = 7; b >= 0; b -= 2)
                            {
                                int colorIndex = ((bits[b] ? 1 : 0) * 2) + (bits[b - 1] ? 1 : 0);
                                finalBytes[j++] = colorPalette[colorIndex * 3];
                                finalBytes[j++] = colorPalette[(colorIndex * 3) + 1];
                                finalBytes[j++] = colorPalette[(colorIndex * 3) + 2];
                                finalBytes[j++] = _hasTransparency ? alphaPalette[colorIndex] : (byte)255; ;

                                x += 1;

                                if (x >= width)
                                {
                                    // throw away unused bits in the last byte
                                    x = 0;
                                    break;
                                }
                            }
                        }
                    }
                    else if (bitsPerPixel == 12)
                    {
                        int x = 0;
                        for (int i = 0, j = 0; i < unfilteredBytes.Length; i++)
                        {
                            uint b = unfilteredBytes[i];

                            int colorIndex = (byte)((b & 0xF0) >> 4);
                            finalBytes[j++] = colorPalette[colorIndex * 3];
                            finalBytes[j++] = colorPalette[(colorIndex * 3) + 1];
                            finalBytes[j++] = colorPalette[(colorIndex * 3) + 2];
                            finalBytes[j++] = _hasTransparency ? alphaPalette[colorIndex] : (byte)255; ; // alphaPalette[colorIndex];

                            x += 1;

                            if (x >= width)
                            {
                                // throw away unused bits in the last byte
                                x = 0;
                                continue;
                            }

                            int colorIndex2 = (byte)(b & 0x0F);
                            finalBytes[j++] = colorPalette[colorIndex2 * 3];
                            finalBytes[j++] = colorPalette[(colorIndex2 * 3) + 1];
                            finalBytes[j++] = colorPalette[(colorIndex2 * 3) + 2];
                            finalBytes[j++] = _hasTransparency ? alphaPalette[colorIndex] : (byte)255; ; // alphaPalette[colorIndex];

                            x += 1;
                            if (x >= width)
                            {
                                x = 0;
                            }
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
                            finalBytes[j++] = _hasTransparency ? alphaPalette[colorIndex] : (byte)255; ; // alphaPalette[colorIndex];
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
                            finalBytes[j++] = !_hasTransparency ? (byte)255 : (b & 0xF0) >> 4 == _transparentColor[0] ? (byte)0 : (byte)255;

                            byte color2 = (byte)((b & 0x0F) * 17);
                            finalBytes[j++] = color2;
                            finalBytes[j++] = color2;
                            finalBytes[j++] = color2;
                            finalBytes[j++] = !_hasTransparency ? (byte)255 : (b & 0x0F) == _transparentColor[0] ? (byte)0 : (byte)255;
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
                            for (int i = 0, j = 0; i < unfilteredBytes.Length;)
                            {
                                UInt16 c = (UInt16)((unfilteredBytes[i++] << 8) | unfilteredBytes[i++]);
                                byte color3 = (byte)(c / 256);

                                finalBytes[j++] = color3;
                                finalBytes[j++] = color3;
                                finalBytes[j++] = color3;
                                finalBytes[j++] = !_hasTransparency ? (byte)255 : c == _transparentColor[0] ? (byte)0 : (byte)255;
                            }
                        }
                    }
                    else if (bitsPerPixel == 24)
                    {
                        for (int i = 0, j = 0; i < unfilteredBytes.Length;)
                        {
                            byte r = unfilteredBytes[i++];
                            byte g = unfilteredBytes[i++];
                            byte b = unfilteredBytes[i++];

                            finalBytes[j++] = r;
                            finalBytes[j++] = g;
                            finalBytes[j++] = b;

                            if (!_hasTransparency || r != _transparentColor[0] || g != _transparentColor[1] || b != _transparentColor[2])
                            {
                                finalBytes[j++] = 255;
                            }
                            else
                            {
                                finalBytes[j++] = 0;
                            }
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
                            UInt16 r = (UInt16)((unfilteredBytes[i++] << 8) | unfilteredBytes[i++]);
                            UInt16 g = (UInt16)((unfilteredBytes[i++] << 8) | unfilteredBytes[i++]);
                            UInt16 b = (UInt16)((unfilteredBytes[i++] << 8) | unfilteredBytes[i++]);


                            finalBytes[j++] = (byte)(r / 256);
                            finalBytes[j++] = (byte)(g / 256);
                            finalBytes[j++] = (byte)(b / 256);

                            if (!_hasTransparency || r != _transparentColor[0] || g != _transparentColor[1] || b != _transparentColor[2])
                            {
                                finalBytes[j++] = 255;
                            }
                            else
                            {
                                finalBytes[j++] = 0;
                            }
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

            if(_gama != 0)
            {
                ApplyGama(ref finalBytes);
            }

            return finalBytes;
        }

        private void ApplyGama(ref byte[] bytes)
        {
            // This is not performant at all and will be switched to a lookup table later.
            double gama = 1.0 / (_gama / 100000.0);
            for (int i = 0; i < bytes.Length;)
            {
                bytes[i] = (byte)Math.Round(Math.Pow(bytes[i++] / 255d, gama) * 255); //r
                bytes[i] = (byte)Math.Round(Math.Pow(bytes[i++] / 255d, gama) * 255); //g
                bytes[i] = (byte)Math.Round(Math.Pow(bytes[i++] / 255d, gama) * 255); //b
                i++; // Gama doesnt apply to alpha
            }
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

        private void ReadTransparencyPalette(ref byte[] bytes, int offset, int length)
        {
            _hasTransparency = true;
            if (_colorType == 0) // Grayscale
            {
                _transparentColor = new UInt16[] { EndianBitConverter.Big.ToUInt16(bytes, offset) };
            }
            else if (_colorType == 2) // Color
            {
                _transparentColor = new UInt16[] 
                {
                    EndianBitConverter.Big.ToUInt16(bytes, offset), //R
                    EndianBitConverter.Big.ToUInt16(bytes, offset + 2), //G
                    EndianBitConverter.Big.ToUInt16(bytes, offset + 4) //B
                };
            }
            else if (_colorType == 3) // indexed
            {
                int i = 0;
                for (i = 0; i < length; i++)
                {
                    alphaPalette[i] = bytes[offset++];
                }

                for (; i < alphaPalette.Length; i++)
                {
                    alphaPalette[i] = 255;
                }
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
