using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Atlas.Drawing.Serialization.JPEG
{
    internal class JPEGDecoder
    {
        private int _imageWidth;
        private int _imageHeight;
        private byte[] _scanData;
        private Frame _frame;
        private HuffmanTable[] _acTables;
        private HuffmanTable[] _dcTables;
        private int[][] _dequant;

        public byte[] Decode(ref byte[] bytes, out int width, out int height)
        {
            using (var reader = new BinaryReader(new MemoryStream(bytes)))
            {
                Parse(reader);
            }

            width = _imageWidth;
            height = _imageHeight;

            return DecodeScan();
        }

        private void Parse(BinaryReader reader)
        {
            while (reader.PeekChar() > 0)
            {
                var temp = reader.ReadByte();

                if (temp != 0xFF)
                    throw new ImageFormatException($"Cannot find next marker");

                var marker = reader.ReadByte();

                switch (marker)
                {
                    case Marker.SOF0:
                        ParseSOF(reader);
                        break;
                    case Marker.DRI:
                        ParseDRI(reader);
                        break;
                    case Marker.DHT:
                        ParseDHT(reader);
                        break;
                    case Marker.SOI:
                        break;
                    case Marker.EOI:
                        break;
                    case Marker.SOS:
                        if (marker == 0xC2)
                            throw new ImageFormatException("Progressive JPEGs are currently not supported");
                        ParseSOS(reader);
                        break;
                    case Marker.DQT:
                        ParseDQT(reader);
                        break;
                    case Marker.APP0:
                    case Marker.APP1:
                    case Marker.APP2:
                    case Marker.APP3:
                    case Marker.APP4:
                    case Marker.APP5:
                    case Marker.APP6:
                    case Marker.APP7:
                    case Marker.APP8:
                    case Marker.APP9:
                    case Marker.APP10:
                    case Marker.APP11:
                    case Marker.APP12:
                    case Marker.APP13:
                    case Marker.APP14:
                    case Marker.APP15:
                        ParseAPP(reader, marker);
                        break;

                    default:
                        throw new ImageFormatException($"Unknown JPEG Marker: {marker.ToString("X4")}");

                }
            }
        }
        private void ParseSOF(BinaryReader reader)
        {
            var length = reader.ReadUInt16BE();
            var precision = reader.ReadByte();

            if (precision != 8)
                throw new NotImplementedException();

            _imageHeight = reader.ReadUInt16BE();
            _imageWidth = reader.ReadUInt16BE();           

            if (_imageWidth <= 0 || _imageHeight <= 0)
                throw new ImageFormatException("Invalid image size");

            var componentCount = reader.ReadByte();

            // JFIF allows 1 component (Y)
            // Otherwise there should be 3 (1 = Y, 2 = Cb, 3 = Cr)
            if (componentCount != 3 && componentCount != 1)
                throw new ImageFormatException($"Invalid component count. Expecting either 1 or 3 but found {componentCount}");

            if (length != 8 + 3 * componentCount)
                throw new ImageFormatException("Bad SOF length");

            _frame = new Frame();
            _frame.Precision = precision;
            _frame.ScanLines = (short)_imageHeight;
            _frame.SamplesPerScan = (short)_imageWidth;
            _frame.ColourMode = (componentCount == 1)
                ? Frame.JPEG_COLOR_GRAY
                : Frame.JPEG_COLOR_YCbCr;

            for (int i = 0; i < componentCount; i++)
            {
                var componentId = reader.ReadByte();
                var sample = reader.ReadByte();
                var qId = reader.ReadByte();

                if (i == 0 && sample != 0x22)
                    throw new NotImplementedException();

                if (i != 0 && sample != 0x11)
                    throw new NotImplementedException();

                var component = new Component(componentId, (byte)(sample >> 4), (byte)(sample & 15), qId);

                if (component.HFactor == 0 || component.HFactor > 4)
                    throw new ImageFormatException("Bad component horizonatal factor");

                if (component.VFactor == 0 || component.VFactor > 4)
                    throw new ImageFormatException("Bad component vertical factor");

                if (qId > 3)
                    throw new ImageFormatException("Bad component quantization id");

                //component.Dequant = _dequant[qId];

                component.MaxH = Math.Max(1, (int)component.HFactor);
                component.MaxV = Math.Max(1, (int)component.VFactor);

                _frame.Components.Add(component);
            }

            //for (int i = 0; i < componentCount; i++)
            //{
            //    var component = _components[i];
            //    component.Width = (_imageWidth * component.BlocksPerMCUHorz + hMax - 1) / hMax;
            //    component.Height = (_imageHeight * component.BlocksPerMCUVert + vMax - 1) / vMax;
            //    component.MinReqWidth = _mcuCountX * component.BlocksPerMCUHorz * 8;
            //    component.MinReqHeight = _mcuCountY * component.BlocksPerMCUVert * 8;

            //    if (component.BlocksPerMCUHorz < hMax)
            //        component.Upsampler |= 1;

            //    if (component.BlocksPerMCUVert < vMax)
            //        component.Upsampler |= 2;
            //}
        }
        private void ParseSOS(BinaryReader reader)
        {
            // SOS non-SOF Marker - Start Of Scan Marker, this is where the
            // actual data is stored in a interlaced or non-interlaced with
            // from 1-4 components of color data, if three components most
            // likely a YCrCb model, this is a fairly complex process.
            var length = reader.ReadUInt16BE();
            var componentCount = reader.ReadByte();
            

            if (componentCount != 3)
                throw new ImageFormatException("Bad component count");

            if (length != 6 + 2 * componentCount)
                throw new ImageFormatException("Bad SOS length");

            for (int i = 0; i < componentCount; i++)
            {
                var id = reader.ReadByte();
                var table = reader.ReadByte();

                if ((table & 0xF) >= 4)
                    throw new ImageFormatException("Error, more than 2 AC Huffman tables are not supported");

                if ((table >> 4) >= 4)
                    throw new ImageFormatException("Error, more than 2 DC Huffman tables are not supported");

                foreach (var component in _frame.Components)
                {
                    if (component.Id == id)
                    {
                        if ((table >> 4) > 3 || (table & 15) > 3)
                            throw new ImageFormatException("Bad huffman table index");

                        component.DCTable = _dcTables[table >> 4];
                        component.ACTable = _acTables[table & 15];

                        if (component.DCTable == null || component.ACTable == null)
                            throw new ImageFormatException("Bad huffman table index");

                        break;
                    }
                }
            }

            if (reader.ReadByte() != 0)
                throw new ImageFormatException("Bad SOS");

            if (reader.ReadByte() != 63)
                throw new ImageFormatException("Bad SOS");

            if (reader.ReadByte() != 0)
                throw new ImageFormatException("Bad SOS");

            ReadScanData(reader);
        }
        private void ParseAPP(BinaryReader reader, int app)
        {
            int length = reader.ReadUInt16BE();
            reader.BaseStream.Seek(length - 2, SeekOrigin.Current);
        }
        private void ParseDRI(BinaryReader reader)
        {
            reader.ReadUInt16BE();

            // This is the reset interval
            // TODO(Dan): Need to store this for later use.
            reader.ReadUInt16BE();
            //_resetInterval = reader.ReadUInt16BE();
        }
        private void ParseDQT(BinaryReader reader)
        {
            // DQT non-SOF Marker - This defines the quantization
            // coeffecients, this allows us to figure out the quality of
            // compression and unencode the data. The data is loaded and
            // then stored in to an array.
            var tableLength = reader.ReadUInt16BE();
            var tableEnd = tableLength + reader.BaseStream.Position - 2;

            while (reader.BaseStream.Position < tableEnd)
            {
                var index = (int)reader.ReadByte();
                var type = index >> 4;
                var table = index & 15;

                if (type != 0)
                    throw new ImageFormatException("Bad DQT type");

                if (table > 3)
                    throw new ImageFormatException("Bad DQT table");

                if (_dequant[index] == null)
                    _dequant[index] = new int[64];

                //NOTE(Dan) 8 - bit values
                if ((index & 0xF0) >> 4 == 0)
                {
                    for (int i = 0; i < 64; i++)
                    {
                        _dequant[index & 0x0F][i] = reader.ReadByte();
                    }
                }
                // NOTE(Dan) 16-bit values
                else if ((index & 0xF0) >> 4 == 1)
                {
                    for (int i = 0; i < 64; i++)
                    {
                        _dequant[index & 0x0F][i] = reader.ReadUInt16BE();
                    }
                }
                else
                {
                    throw new NotImplementedException("Only 8-bit DQT is currently supported");
                }
            }
        }
        private void ParseDHT(BinaryReader reader)
        {
            // DHT non-SOF Marker - Huffman Table is required for decoding
            // the JPEG stream, when we receive a marker we load in first
            // the table length (16 bits), the table class (4 bits), table
            // identifier (4 bits), then we load in 16 bytes and each byte
            // represents the count of bytes to load in for each of the 16
            // bytes. We load this into an array to use later and move on 4
            // huffman tables can only be used in an image.
            var start = reader.BaseStream.Position;
            var length = reader.ReadUInt16BE() - 2;
            var end = start + length;

            // Multiple tables may be defined within a DHT marker.
            // This will keep reading until there are no tables left
            // Most of the time there are just one tables.
            while (length > 0)
            {
                // Read the identifier information and class
                // information about the Huffman table, then read the
                // 16 byte codelength in and read in the Huffman values
                // and put it into table info.
                var huffmanInfo = reader.ReadByte();
                var tableClass = huffmanInfo >> 4;
                var huffmanIndex = huffmanInfo & 15;

                if (tableClass > 1 || huffmanIndex > 3)
                    throw new ImageFormatException("Bad DHT header, JPEG is corrupt");

                var lengths = new byte[16];
                var lengthsTotal = 0;

                for (int i = 0; i < 16; i++)
                {
                    lengths[i] = reader.ReadByte();
                    lengthsTotal += lengths[i];
                }

                var table = new HuffmanTable(lengths, reader.ReadBytes(lengthsTotal));

                if (tableClass == HuffmanTable.HUFFMAN_DC_TABLE)
                    _dcTables[huffmanIndex] = table;

                if (tableClass == HuffmanTable.HUFFMAN_AC_TABLE)
                    _acTables[huffmanIndex] = table;

                length -= 17;
                length -= lengthsTotal;
            }

            if (length != 0)
                throw new ImageFormatException("Bad DHT length");
        }
        private void ReadScanData(BinaryReader reader)
        {
            // TODO(Dan): Can we do this with an upfront allocation?
            var scanData = new List<byte>();

            while (true)
            {
                var value = reader.ReadByte();

                if (value == 0xFF)
                {
                    var next = reader.ReadByte();

                    if (next == Marker.X00)
                    {
                        scanData.Add(0xFF);
                    }
                    //else if (next == (byte)Marker.RST0 || next == (byte)Marker.RST1 || next == (byte)Marker.RST2 ||
                    //         next == (byte)Marker.RST3 || next == (byte)Marker.RST4 || next == (byte)Marker.RST5 ||
                    //         next == (byte)Marker.RST6 || next == (byte)Marker.RST7)
                    //{
                    //    scanData.Add(value);
                    //}
                    else
                    {
                        reader.BaseStream.Seek(-2, SeekOrigin.Current);
                        break;
                    }
                }
                else
                {
                    scanData.Add(value);
                }
            }

            // NOTE(Dan): 2 padding bytes for huffman decoding
            scanData.Add(0);
            scanData.Add(0);

            _scanData = scanData.ToArray();
        }

        private byte[] DecodeScan()
        {

            // NOTE(Dan): Currently only single frame is supported.
            var dct = new DCT();

            // RGBA
            var image = new byte[_imageWidth * _imageHeight * 4];

            using (var output = new MemoryStream(image))
            using (var reader = new BitReader(new MemoryStream(_scanData)))
            {
                // TODO(Dan):  Decode MCU Stuff here...
                
                // TODO(Dan): Set the component quant tables and deal with decoding the component

                if (_frame.ComponentCount == 1)
                {
                    // TODO(Dan): Single component = greyscale
                }
                else if (_frame.ComponentCount == 3)
                {
                    // TODO(Dan): 3 Componentns = YCbCr

                }
                else
                {
                    throw new ImageFormatException($"Unsupported colour mode. 4 Component colour mode found");
                }
            }

            return image;
        }

        public JPEGDecoder()
        {
            _acTables = new HuffmanTable[HuffmanTable.HUFFMAN_MAX_TABLES];
            _dcTables = new HuffmanTable[HuffmanTable.HUFFMAN_MAX_TABLES];
            _dequant = new int[4][];
        }
    }
}
