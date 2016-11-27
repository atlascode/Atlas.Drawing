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
            byte[] finalBytes = new byte[width * height * 4];
            if (!hasAlpha)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int sourceIndex = ((width * 3) * y) + (x * 3);
                        int destinationIndex = ((width * 4) * y) + (x * 4);

                        finalBytes[destinationIndex++] = rawBytes[sourceIndex++];
                        finalBytes[destinationIndex++] = rawBytes[sourceIndex++];
                        finalBytes[destinationIndex++] = rawBytes[sourceIndex];
                        finalBytes[destinationIndex] = 255;
                    }
                }
            }

            return finalBytes;
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
