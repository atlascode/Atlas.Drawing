﻿using System;
using System.IO;
using System.IO.Compression;
using Atlas.Drawing.Conversion;
using Atlas.Drawing.IO;

namespace Atlas.Drawing.Serialization.PNG
{
    public class PNGEncoder
    {
        private uint[] _crcTable;
        private bool _crcTableComputed = false;

        public byte[] Encode(byte[] img, int width, int height)
        {
            var ms = new MemoryStream();
            var ihdrms = new MemoryStream();
            var idatms = new MemoryStream();
            bool transparent = true;

            var converter = new BigEndianBitConverter();

            // Create output byte array
            var png = new EndianBinaryWriter(converter, ms);
            // Write PNG signature
            png.Write((uint)0x89504e47);
	        png.Write((uint)0x0D0A1A0A);
	        // Build IHDR chunk
	        var IHDR = new EndianBinaryWriter(converter, ihdrms);
            IHDR.Write(width);
	        IHDR.Write(height);
            //IHDR.Write((uint)0x08060000); // 32bit RGBA
            IHDR.Write((byte)8); // BitDepth
            IHDR.Write((byte)6); // Colour Type //https://www.w3.org/TR/PNG/#6Colour-values
            IHDR.Write((byte)0); // Compression Method. Always 0
            IHDR.Write((byte)0); // Filter Method. Always 0
            IHDR.Write((byte)0); // Interlace Method. 0 or 1

            WriteChunk(png,0x49484452, ihdrms.ToArray());
            // Build IDAT chunk
            var IDAT = new EndianBinaryWriter(converter, idatms);

            for (int y = 0; y < height; y++)
            {
	            // no filter
	            IDAT.Write((byte)0);
                uint p;
	            if ( !transparent )
                {
	                //for(j=0;j<width;j++) {
	                //    p = getPixel(img, j, i);
	                //    IDAT.Write((uint)(((p&0xFFFFFF) << 8)|0xFF));
	                //}
                }
                else
                {
	                for(int x = 0; x < width; x++)
                    {
	                    p = GetPixel32(img, width, x, y);
                        IDAT.Write(p);
                        //IDAT.Write(((p & 0xFFFFFF) << 8) | (p >> 24)); //>>>
                    }
	            }
	        }

            WriteChunk(png,0x49444154, Compress(idatms));
            // Build IEND chunk
            WriteChunk(png,0x49454E44,null);
            // return PNG

	        return ms.ToArray();
	    }

        private uint GetPixel32(byte[] img, int width,int x, int y)
        {
            int startIndex = ((width * 4) * y) + (x * 4);
            //R
            uint value = img[startIndex];
            //G
            value <<= 8;
            value |= img[startIndex + 1];
            //B
            value <<= 8;
            value |= img[startIndex + 2];
            //A
            value <<= 8;
            value |= img[startIndex + 3];

            return value;
        }
        private void WriteChunk(EndianBinaryWriter png, uint type, byte[] data)
        {
            uint c;
            if (!_crcTableComputed)
            {
                _crcTableComputed = true;
                _crcTable = new uint[256];
                for (uint n = 0; n < 256; n++)
                {
                    c = n;
                    for (uint k = 0; k < 8; k++)
                    {
                        if ((c & 1u) == 1u)
                        {
                            c = 0xedb88320 ^ (c >> 1);
                        }
                        else
                        {
                            c = c >> 1;
                        }
                    }
                    _crcTable[n] = c;
                }
            }
            int len = 0;
            if (data != null)
            {
                len = data.Length;
            }
            png.Write((uint)len);
            var p = png.BaseStream.Position;
            png.Write(type);
            if (data != null)
            {
                png.Write(data);
            }
            long e = png.BaseStream.Position;
            png.BaseStream.Position = p;
            c = 0xffffffff;
            for (int i = 0; i < (e - p); i++)
            {
                c = _crcTable[((c ^ png.BaseStream.ReadByte()) & 0xff)] ^ c >> 8;
            }
            c = c ^ 0xffffffff;
            png.BaseStream.Position = e;
            png.Write(c);
        }
        private static byte[] Compress(Stream input)
        {
            input.Position = 0;

            byte[] uncompressedBytes;
            using (var ms = new MemoryStream())
            {
                input.CopyTo(ms);
                uncompressedBytes = ms.ToArray();
                input.Position = 0;
            }

            using (var compressStream = new MemoryStream())
            {

                var adler = new Adler32Computer();
                adler.Update(uncompressedBytes, 0, uncompressedBytes.Length);

                byte[] compressedBytes;
                using (var compressor = new DeflateStream(compressStream, CompressionMode.Compress, true))
                {
                    input.CopyTo(compressor);
                    compressor.Flush();
                    compressedBytes = compressStream.ToArray();
                }

                var zlibBytes = new byte[compressedBytes.Length + 6];

                zlibBytes[0] = 0x78;
                zlibBytes[1] = 0x01;

                compressedBytes.CopyTo(zlibBytes, 2);

                byte[] intBytes = BitConverter.GetBytes(adler.Checksum);
                compressedBytes[compressedBytes.Length - 1] = intBytes[0];
                compressedBytes[compressedBytes.Length - 2] = intBytes[1];
                compressedBytes[compressedBytes.Length - 3] = intBytes[2];
                compressedBytes[compressedBytes.Length - 4] = intBytes[3];

                return zlibBytes;
            }
        }
    }
}
