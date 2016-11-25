using Ionic.Zlib;
using MiscUtil.Conversion;
using MiscUtil.IO;
using System.IO;

namespace Atlas.Drawing.Serialization.PNG
{
    public class PNGEncoder
    {
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

            for (int i=0;i<height;i++) {
	            // no filter
	            IDAT.Write((byte)0);
                uint p;
	            int j;
	            if ( !transparent ) {
	                //for(j=0;j<width;j++) {
	                //    p = getPixel(img, j, i);
	                //    IDAT.Write((uint)(((p&0xFFFFFF) << 8)|0xFF));
	                //}
                } else {
	                for(j=0;j<width;j++) {
	                    p = getPixel32(img, width, height, j, i);
                        //IDAT.Write(p);
                        IDAT.Write(((p & 0xFFFFFF) << 8) | (p >> 24)); //>>>
                    }
	            }
	        }

            WriteChunk(png,0x49444154, Compress(idatms));
            // Build IEND chunk
            WriteChunk(png,0x49454E44,null);
            // return PNG
	        return ms.ToArray();
	    }

        private uint getPixel32(byte[] img, int width,int height, int x, int y)
        {
            int startIndex = ((width) * 4 * (height-1-y)) + (x * 4);
            //A
            uint value = img[startIndex + 3];
            //R
            value <<= 8;
            value |= img[startIndex + 2];
            //G
            value <<= 8;
            value |= img[startIndex + 1];
            //B
            value <<= 8;
            value |= img[startIndex];

            return value;
        }
	
	    private uint[] crcTable;
	    private bool crcTableComputed = false;

        private static byte[] Compress(Stream input)
        {
            input.Position = 0;
            using (var compressStream = new MemoryStream())
            { 
                using (var compressor = new ZlibStream(compressStream, Ionic.Zlib.CompressionMode.Compress))
                {
                    input.CopyTo(compressor);
                }
                return compressStream.ToArray();
            }
        }

        private void WriteChunk(EndianBinaryWriter png, uint type, byte[] data)
        {
            uint c;
            if (!crcTableComputed) {
	            crcTableComputed = true;
	            crcTable = new uint[256];
	            for (uint n = 0; n< 256; n++) {
	                c = n;
	                for (uint k = 0; k < 8; k++) {
	                    if ((c & 1u) == 1u) {
	                        c = 0xedb88320 ^ (c >> 1);
	                    } else {
	                        c = c >> 1;
	                    }
	                }
                    crcTable[n] = c;
                }
	        }
	        int len = 0;
	        if (data != null) {
	            len = data.Length;
	        }
	        png.Write((uint)len);
	        var p = png.BaseStream.Position;
	        png.Write(type);
	        if ( data != null ) {
	            png.Write(data);
	        }
            long e = png.BaseStream.Position;
	        png.BaseStream.Position = p;
	        c = 0xffffffff;
	        for (int i = 0; i< (e-p); i++) {
	            c = crcTable[((c ^ png.BaseStream.ReadByte()) & 0xff)] ^ c >> 8;
	        }
	        c = c ^ 0xffffffff;
	        png.BaseStream.Position = e;
            png.Write(c);
	    }
    }
}
