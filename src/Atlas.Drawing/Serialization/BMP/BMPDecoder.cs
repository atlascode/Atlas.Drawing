using System;
using System.Collections;

namespace Atlas.Drawing.Serialization.BMP
{
    public class BMPDecoder
    {
        private byte[] colorTable;

        private BitmapInformationHeader _info;
        private int _redChannelBitmaskOffset = 0;
        private int _greenChannelBitmaskOffset = 0;
        private int _blueChannelBitmaskOffset = 0;
        private int _alphaChannelBitmaskOffset = 0;
        private decimal _redChannelBitmaskMaxValue = 255;
        private decimal _greenChannelBitmaskMaxValue = 255;
        private decimal _blueChannelBitmaskMaxValue = 255;
        private decimal _alphaChannelBitmaskMaxValue = 255;

        public byte[] Decode(ref byte[] bytes, out int width, out int height)
        {
            var header = new BitmapFileHeader();
            header.Deserialize(bytes);

            _info = new BitmapInformationHeader();
            _info.Deserialize(bytes);

            if(_info.BitsPerPixel <= 8 && _info.ColorsInColorTable == 0)
            {
                _info.ColorsInColorTable = 256;
            }

            if(_info.BlueChannelBitmask == 0 && _info.GreenChannelBitmask == 0 && _info.RedChannelBitmask == 0)
            {
                if (_info.BitsPerPixel == 16)
                {
                    _info.RedChannelBitmask = 0x7C00;
                    _info.GreenChannelBitmask = 0x3E0;
                    _info.BlueChannelBitmask = 0x1F;
                }
                else if (_info.BitsPerPixel == 24)
                {
                    _info.RedChannelBitmask = 0xFF0000;
                    _info.GreenChannelBitmask = 0x00FF00;
                    _info.BlueChannelBitmask = 0x0000FF;
                }
                else if (_info.BitsPerPixel == 32)
                {
                    _info.RedChannelBitmask = 0xFF0000;
                    _info.GreenChannelBitmask = 0xFF00; 
                    _info.BlueChannelBitmask = 0xFF; 
                    _info.AlphaChannelBitmask = unchecked((int)0xFF000000);
                }
            }

            if(_info.RedChannelBitmask != 0)
            {
                _redChannelBitmaskOffset = CalculateBitmaskOffset(_info.RedChannelBitmask);
                _redChannelBitmaskMaxValue = (uint)_info.RedChannelBitmask >> _redChannelBitmaskOffset;
            }
            if (_info.GreenChannelBitmask != 0)
            {
                _greenChannelBitmaskOffset = CalculateBitmaskOffset(_info.GreenChannelBitmask);
                _greenChannelBitmaskMaxValue = (uint)_info.GreenChannelBitmask >> _greenChannelBitmaskOffset;
            }
            if (_info.BlueChannelBitmask != 0)
            {
                _blueChannelBitmaskOffset = CalculateBitmaskOffset(_info.BlueChannelBitmask);
                _blueChannelBitmaskMaxValue = (uint)_info.BlueChannelBitmask >> _blueChannelBitmaskOffset;
            }
            if (_info.AlphaChannelBitmask != 0)
            {
                _alphaChannelBitmaskOffset = CalculateBitmaskOffset(_info.AlphaChannelBitmask);
                _alphaChannelBitmaskMaxValue = (uint)_info.AlphaChannelBitmask >> _alphaChannelBitmaskOffset;
            }

            if (_info.ColorsInColorTable > 0) {
                colorTable = new byte[_info.ColorsInColorTable * 4];

                int colorOffset = 0;
                bool supportsAlpha = _info.SupportsAlpha;
                for (uint i = 0; i < _info.ColorsInColorTable; i++)
                {
                    uint sourceColorOffset = header.DataLength + _info.DataLength + (i * (_info.HeaderSize == 12 ? 3u : 4u));
                    colorTable[colorOffset++] = bytes[sourceColorOffset + 0]; //R
                    colorTable[colorOffset++] = bytes[sourceColorOffset + 1]; //G
                    colorTable[colorOffset++] = bytes[sourceColorOffset + 2]; //B
                    colorTable[colorOffset++] = supportsAlpha ? bytes[sourceColorOffset + 3] : (byte)255; //A
                }
            }

            bool useColorTable = _info.BitsPerPixel < 16 && _info.ColorsInColorTable > 0;

            // Unpack any bytes into 32bpp RGBA
            var standardBytes = new byte[_info.Width * Math.Abs(_info.Height) * 4];

            // Round up to a 4 byte boundary;
            uint stride = (((uint)_info.Width * _info.BitsPerPixel+31)/32) * 4;

            uint destinationStride = (uint)_info.Width * 4;

            // decompress bytes
            uint pixelArrayOffset = header.PixelArrayOffset;
            byte[] decompressedBytes = bytes;
            if(_info.Compression > 0)
            {
                if(_info.Compression == 1) // RLE8
                {
                    decompressedBytes = new byte[stride * _info.Height];
                    int currentIndex = 0;
                    for (int i = 0; i < _info.ImageSize;)
                    {
                        byte b = bytes[pixelArrayOffset + i++];

                        if (b == 0)
                        {
                            byte a = bytes[pixelArrayOffset + i++];
                            if (a == 0)
                            {
                                currentIndex += (int)stride - (currentIndex % (int)stride);
                            }
                            else  if(a == 1)
                            {
                                break;
                            }
                            else if (a == 2)
                            {
                                byte x = bytes[pixelArrayOffset + i++];
                                currentIndex += x;
                                byte y = bytes[pixelArrayOffset + i++];
                                currentIndex += ((int)stride * y);
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
                if (_info.Compression == 2) // RLE4
                {
                    decompressedBytes = new byte[stride * _info.Height];
                    int currentIndex = 0;
                    for (int i = 0; i < _info.ImageSize;)
                    {
                        byte b = bytes[pixelArrayOffset + i++];

                        if (b == 0)
                        {
                            byte a = bytes[pixelArrayOffset + i++];
                            if (a == 0)
                            {
                                currentIndex += (int)(stride*2) - (currentIndex % (int)(stride*2));
                            }
                            else if (a == 1)
                            {
                                break;
                            }
                            else if (a == 2)
                            {
                                byte x = bytes[pixelArrayOffset + i++];
                                currentIndex += x;
                                byte y = bytes[pixelArrayOffset + i++];
                                currentIndex += ((int)stride * y);
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

            for (uint y = 0; y < Math.Abs(_info.Height); y++)
            {
                for (uint x = 0; x < _info.Width; x++)
                {
                    ConvertPixel(ref decompressedBytes, pixelArrayOffset, stride, _info.BitsPerPixel, x, y, ref standardBytes, destinationStride, useColorTable, _info.Height >= 0 ? (uint)Math.Abs(_info.Height) - 1 : y * 2);
                }
            }

            width = (int)_info.Width;
            height = Math.Abs(_info.Height);
            
            return standardBytes;
        }

        private int CalculateBitmaskOffset(int mask)
        {
            var bits = new BitArray(new int[] { mask });

            for (int i = 0; i < _info.BitsPerPixel; i++)
            {
                if(bits[i] == true)
                {
                    return i;
                }
            }

            return 0;
        }
        private void ConvertPixel(ref byte[] sourceBytes, uint sourceOffset, uint sourceStride, uint sourceBitsPerPixel, uint x, uint y, ref byte[] destinationBytes, uint destinationStride, bool useColorTable, uint flip)
        {
            uint sourcePixelOffset = (y * sourceStride) + (x * sourceBitsPerPixel / 8) + sourceOffset;
            uint destinationPixelOffset = ((flip - y) * destinationStride) + (x * 4);

            if (useColorTable)
            {
                uint colorTableIndex = 0;
                if(sourceBitsPerPixel == 1)
                {
                    var bits = new BitArray(new byte[] { sourceBytes[sourcePixelOffset] });
                    colorTableIndex = (bits[7-(int)(x % 8u)] ? 1u : 0) * 4u;
                }
                else if (sourceBitsPerPixel == 2)
                {
                    var bits = new BitArray(new byte[] { sourceBytes[sourcePixelOffset] });
                    uint bit1 = bits[7 - (int)(x * 2 % 8u)] ? 1u : 0;
                    uint bit2 = bits[7 - (int)(x * 2 % 8u + 1)] ? 1u : 0;
                    colorTableIndex = ((bit1 * 2) + bit2) * 4u;
                }
                else if (sourceBitsPerPixel == 4)
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
                else if (sourceBitsPerPixel == 16)
                {
                    colorTableIndex = sourceBytes[sourcePixelOffset] * 4u;
                }

                destinationBytes[destinationPixelOffset + 2] = colorTable[colorTableIndex++];     //R
                destinationBytes[destinationPixelOffset + 1] = colorTable[colorTableIndex++]; //G
                destinationBytes[destinationPixelOffset + 0] = colorTable[colorTableIndex++]; //B

                if (_info.SupportsAlpha)
                {
                    destinationBytes[destinationPixelOffset + 3] = colorTable[colorTableIndex++]; //A
                } else
                {
                    destinationBytes[destinationPixelOffset + 3] = 255;
                }
            }
            else
            {
                byte r;
                byte g;
                byte b;
                byte a = 255;

                uint pixel = BitConverter.ToUInt32(sourceBytes, (int)sourcePixelOffset);
                r = (byte)(((pixel & _info.RedChannelBitmask) >> _redChannelBitmaskOffset) / _redChannelBitmaskMaxValue * 255);
                g = (byte)(((pixel & _info.GreenChannelBitmask) >> _greenChannelBitmaskOffset) / _greenChannelBitmaskMaxValue * 255);
                b = (byte)(((pixel & _info.BlueChannelBitmask) >> _blueChannelBitmaskOffset) / _blueChannelBitmaskMaxValue * 255);

                if (_info.SupportsAlpha)
                {
                    a = (byte)(((pixel & _info.AlphaChannelBitmask) >> _alphaChannelBitmaskOffset) / _alphaChannelBitmaskMaxValue * 255);
                }

                // Set destination bytes
                destinationBytes[destinationPixelOffset] = r;
                destinationBytes[destinationPixelOffset + 1] = g;
                destinationBytes[destinationPixelOffset + 2] = b;
                destinationBytes[destinationPixelOffset + 3] = a;
            }
        }
    }
}
