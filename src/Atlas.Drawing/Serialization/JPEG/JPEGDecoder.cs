using System;
using System.IO;
using System.Linq;

namespace Atlas.Drawing.Serialization.JPEG
{
    public class JPEGDecoder
    {
        private ushort _xResolution;
        private ushort _yResolution;
        private ushort _resetInterval;
        private byte[][] _dqt = new byte[4][];
        private HuffmanTable[][] _dht = new HuffmanTable[4][];
        private readonly byte[] _naturalOrder = new byte[] 
        {
            0,   1,  8, 16,  9,  2,  3, 10,
            17, 24, 32, 25, 18, 11,  4,  5,
            12, 19, 26, 33, 40, 48, 41, 34,
            27, 20, 13,  6,  7, 14, 21, 28,
            35, 42, 49, 56, 57, 50, 43, 36,
            29, 22, 15, 23, 30, 37, 44, 51,
            58, 59, 52, 45, 38, 31, 39, 46,
            53, 60, 61, 54, 47, 55, 62, 63,
        };

        public byte[] Decode(ref byte[] bytes, out int width, out int height)
        {
            _resetInterval = 0;

            width = 0;
            height = 0;

            using (var reader = new BinaryReader(new MemoryStream(bytes)))
            {
                Parse(reader);
            }

            return bytes;
        }

        private void Parse(BinaryReader reader)
        {
            while (reader.PeekChar() >= 0)
            {
                var temp = reader.ReadByte();

                if (temp != 0xFF)
                    throw new BadImageFormatException($"Cannot find next marker");

                var marker = reader.ReadByte();

                switch (marker)
                {
                    case (byte)Marker.SOF0:
                        ParseSOF(reader);
                        break;
                    case (byte)Marker.DRI:
                        ParseDRI(reader);
                        break;
                    case (byte)Marker.DHT:
                        ParseDHT(reader);
                        break;
                    case (byte)Marker.SOI:
                        break;
                    case (byte)Marker.EOI:
                        break;
                    case (byte)Marker.SOS:
                        ParseSOS(reader);
                        break;
                    case (byte)Marker.DQT:
                        ParseDQT(reader);
                        break;
                    case (byte)Marker.APP0:
                    case (byte)Marker.APP1:
                    case (byte)Marker.APP2:
                    case (byte)Marker.APP3:
                    case (byte)Marker.APP4:
                    case (byte)Marker.APP5:
                    case (byte)Marker.APP6:
                    case (byte)Marker.APP7:
                    case (byte)Marker.APP8:
                    case (byte)Marker.APP9:
                    case (byte)Marker.APP10:
                    case (byte)Marker.APP11:
                    case (byte)Marker.APP12:
                    case (byte)Marker.APP13:
                    case (byte)Marker.APP14:
                    case (byte)Marker.APP15:
                        ParseAPP(reader, (int)marker);
                        break;

                    default:
                        throw new BadImageFormatException($"Unknown JPEG Marker: {marker.ToString("X4")}");

                }
            }
        }

        private void ParseSOF(BinaryReader reader)
        {
            var length = reader.ReadUInt16BE();
            var precision = reader.ReadByte();

            if (precision != 8)
                throw new NotImplementedException();

            _yResolution = reader.ReadUInt16BE();
            _xResolution = reader.ReadUInt16BE();

            var componentCount = reader.ReadByte();
            if (componentCount != 3)
                throw new NotImplementedException();

            for (int i = 0; i < 3; i++)
            {
                var id = reader.ReadByte();
                var sample = reader.ReadByte();

                if (id == 0 && sample != 0x22)
                    throw new NotImplementedException();

                if (id != 0 && sample != 0x11)
                    throw new NotImplementedException();

                // TODO(Dan): Build components here...

                reader.ReadByte();
            }
        }
        private void ParseAPP(BinaryReader reader, int app)
        {
            int length = reader.ReadUInt16BE();
            reader.BaseStream.Seek(length - 2, SeekOrigin.Current);
        }
        private void ParseDRI(BinaryReader reader)
        {
            reader.ReadUInt16BE();
            _resetInterval = reader.ReadUInt16BE();
        }
        private void ParseDQT(BinaryReader reader)
        {
            var tableLength = reader.ReadUInt16BE();
            var tableEnd = tableLength + reader.BaseStream.Position - 2;

            while (reader.BaseStream.Position < tableEnd)
            {
                var ident = reader.ReadByte();

                if (_dqt[ident] == null)
                    _dqt[ident] = new byte[64];

                // NOTE(Dan) 8-bit values
                if ((ident & 0xF0) >> 4 == 0)
                {
                    for (int i = 0; i < 64; i++)
                    {
                        _dqt[ident][_naturalOrder[i]] = reader.ReadByte();
                    }
                }
                else
                {
                    throw new NotImplementedException("Only 8-bit DQT is currently supported");
                }

                //// NOTE(Dan) 16-bit values
                //if ((indent & 0xF0) >> 4 == 1)
                //{
                //    for (int i = 0; i < 64; i++)
                //    {
                //        _dqt[indent][_naturalOrder[i]] = reader.ReadUInt16BE();
                //    }
                //}
            }
        }
        private void ParseDHT(BinaryReader reader)
        {
            // TODO(Dan): Do this so that we read all of the DHT tables not just the first.

            var start = reader.BaseStream.Position;
            int length = reader.ReadUInt16BE() - 2;
            var end = start + length;

            while (length > 0)
            {
                // q
                int select = reader.ReadByte();

                int lengthsTotal = 0;

                var tc = select >> 4;
                var th = select & 15;

                if (tc > 1 || th > 3)
                    throw new BadImageFormatException($"Bad DHT Header, JPEG is corrupt");

                byte[] lengths = reader.ReadBytes(16);

                for (int i = 0; i < 16; i++)
                    lengthsTotal += lengths[i];

                length -= 17;

                if (_dht[th] == null)
                    _dht[th] = new HuffmanTable[65536];

                // TODO(Dan): Build table
                int code = 0;
                for (int i = 1; i <= 16; i++)
                {
                    for (int j = 0; j < lengths[i - 1]; j++)
                    {
                        byte val = reader.ReadByte();

                        // Fill table
                        int x = 16 - i;
                        int lo = code << x;
                        int hi = code << x | ((1 << x) - 1);
                        for (int k = lo; k <= hi; k++)
                            _dht[th][k] = new HuffmanTable() { B = (ushort)i, V = val };

                        code++;
                    }
                    code <<= 1;
                }

                length -= lengthsTotal;
            }
        }
        private void ParseSOS(BinaryReader reader)
        {
            var length = reader.ReadUInt16BE();

            if (reader.ReadByte() != 3)
                throw new NotImplementedException();

            for (int i = 0; i < 3; i++)
            {
                reader.ReadByte();
                var sample = reader.ReadByte();

                if (i == 0 && sample != 0x00)
                    throw new NotImplementedException();

                if (i != 0 && sample != 0x11)
                    throw new NotImplementedException();

                reader.ReadByte();

                // TODO(Dan): Read scan data.
                ReadScanData(reader);
            }
        }
        
       
        private enum Marker : byte
        {
            DUMMY = 0x00,
            SOI = 0xD8,
            SOF0 = 0xC0,
            SOF1 = 0xC1,
            SOF2 = 0xC2,
            DHT = 0xC4,
            DQT = 0xDB,
            DRI = 0xDD,
            SOS = 0xDA,
            COM = 0xFE,
            EOI = 0xD9,

            APP0 = 0xE0,
            APP1 = 0xE1,
            APP2 = 0xE2,
            APP3 = 0xE3,
            APP4 = 0xE4,
            APP5 = 0xE5,
            APP6 = 0xE6,
            APP7 = 0xE7,
            APP8 = 0xE8,
            APP9 = 0xE9,
            APP10 = 0xEA,
            APP11 = 0xEB,
            APP12 = 0xEC,
            APP13 = 0xED,
            APP14 = 0xEE,
            APP15 = 0xFE
        }
    }
}
