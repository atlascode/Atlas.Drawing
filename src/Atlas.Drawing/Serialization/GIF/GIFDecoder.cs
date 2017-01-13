using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Atlas.Drawing.Serialization.GIF
{
    public class GIFDecoder
    {
        byte[] _globalColorTable;

        public byte[] Decode(ref byte[] bytes, out int width, out int height)
        {
            string version = System.Text.Encoding.ASCII.GetString(bytes, 3, 3);

            int byteIndex = 6;

            UInt16 logicalScreenWidth = BitConverter.ToUInt16(bytes, byteIndex); byteIndex += 2;
            UInt16 logicalScreenHeight = BitConverter.ToUInt16(bytes, byteIndex); byteIndex += 2;
            byte packedFields = bytes[byteIndex++];

            int sizeOfGlobalColorTable = 2 << (packedFields & 0x07);

            //< Packed Fields >  = 
            //              Global Color Table Flag       1 Bit
            //              Color Resolution              3 Bits
            //              Sort Flag                     1 Bit
            //              Size of Global Color Table    3 Bits

            byte backgroundColorIndex = bytes[byteIndex++];
            byte pixelAspectRatio = bytes[byteIndex++];

            if (((packedFields & 0x80) >> 7) == 1)
            {
                ReadGlobalColorTable(ref bytes, sizeOfGlobalColorTable, ref byteIndex);
            }

            bool end = false;
            while (byteIndex < bytes.Length && !end)
            {
                byte blockIdentifier = bytes[byteIndex++];

                switch (blockIdentifier)
                {
                    case 0:
                        end = true;
                        break;
                    case 0x21:
                        byte extension = bytes[byteIndex++];
                        switch (extension)
                        {
                            case 0xF9:
                                ReadGraphicalControlExtension(ref bytes, ref byteIndex);
                                break;
                            case 0xFE:
                                byteIndex += bytes[byteIndex++];
                                break;
                            case 0xFF:
                                byteIndex += 12;
                                break;
                            case 0x01:
                                byteIndex += 13;
                                break;
                            default:
                                byteIndex += bytes[byteIndex++];
                                break;
                        }
                        break;
                    case 0x2C:
                        ReadImageDescriptor(ref bytes, ref byteIndex);
                        break;
                }
            }

            width = logicalScreenWidth;
            height = logicalScreenHeight;
            return new byte[] { };
        }

        private void ReadGlobalColorTable(ref byte[] bytes, int size, ref int byteIndex)
        {
            _globalColorTable = new byte[size * 3];
            Array.Copy(bytes, byteIndex, _globalColorTable, 0, size * 3);
            byteIndex += size * 3;
        }

        private void ReadGraphicalControlExtension(ref byte[] bytes, ref int byteIndex)
        {
            byte size = bytes[byteIndex++];

            byteIndex += size;

            byteIndex++; // block terminator

            //byte packed = bytes[byteIndex++];


            //var DelayTime = BitConverter.ToInt16(bytes, byteIndex),
            //var TransparencyIndex = this.buffer[4],
            //var TransparencyFlag = (packed & 0x01) == 1,
            //var DisposalMethod = (DisposalMethod)((packed & 0x1C) >> 2)

        }

        private byte[] ReadLocalColorTable(ref byte[] bytes, int size, ref int byteIndex)
        {
            byte[] localColorTable = new byte[size * 3];
            Array.Copy(bytes, byteIndex, localColorTable, 0, size * 3);
            byteIndex += size * 3;
            return localColorTable;
        }

        private void ReadImageDescriptor(ref byte[] bytes, ref int byteIndex)
        {
            UInt16 imageLeft = BitConverter.ToUInt16(bytes, byteIndex); byteIndex += 2;
            UInt16 imageTop = BitConverter.ToUInt16(bytes, byteIndex); byteIndex += 2;
            UInt16 imageWidth = BitConverter.ToUInt16(bytes, byteIndex); byteIndex += 2;
            UInt16 imageHeight = BitConverter.ToUInt16(bytes, byteIndex); byteIndex += 2;
            byte packedFields = bytes[byteIndex++];
            //< Packed Fields >  = 
            //              Local Color Table Flag        1 Bit
            //              Interlace Flag                1 Bit
            //              Sort Flag                     1 Bit
            //              Reserved                      2 Bits
            //              Size of Local Color Table     3 Bits

            int sizeOfLocalColorTable = 2 << (packedFields & 0x07); ;

            if (((packedFields & 0x80) >> 7) == 1)
            {
                ReadLocalColorTable(ref bytes, sizeOfLocalColorTable, ref byteIndex);
            }

            bool interlaced = ((packedFields & 0x40) >> 6) == 1;

            byte dataSize = bytes[byteIndex++];
            ReadImageData(ref bytes, ref byteIndex);
        }

        private void ReadImageData(ref byte[] bytes, ref int byteIndex)
        {
            var bufferSize = bytes[byteIndex++];
            while (bufferSize > 0)
            {
                byteIndex += bufferSize;
                bufferSize = bytes[byteIndex++];
            }
        }
    }
}
