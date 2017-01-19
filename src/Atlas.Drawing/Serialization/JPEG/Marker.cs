using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Atlas.Drawing.Serialization.JPEG
{
    internal static class Marker
    {
        /// <summary>
        /// JFIF identifier.
        /// </summary>
        public const byte JFIF_J = 0x4a;
        /// <summary>
        /// JFIF identifier.
        /// </summary>
        public const byte JFIF_F = 0x46;
        /// <summary>
        /// JFIF identifier.
        /// </summary>
        public const byte JFIF_I = 0x49;
        /// <summary>
        /// JFIF identifier.
        /// </summary>
        public const byte JFIF_X = 0x46;

        /// <summary>
        /// JFIF extension code.
        /// </summary>
        public const byte JFXX_JPEG = 0x10;
        /// <summary>
        /// JFIF extension code.
        /// </summary>
        public const byte JFXX_ONE_BPP = 0x11;
        /// <summary>
        /// JFIF extension code.
        /// </summary>
        public const byte JFXX_THREE_BPP = 0x13;

        /// <summary>
        /// Marker prefix byte.
        /// </summary>
        public const byte XFF = 0xff;
        /// <summary>
        /// Marker byte that represents a literal 0xff.
        /// </summary>
        public const byte X00 = 0x00;

        /// <summary>
        /// Application Reserved Keyword.
        /// </summary>
        public const byte APP0 = 0xe0;
        /// <summary>
        /// Application Reserved Keyword.
        /// </summary>
        public const byte APP1 = 0xe1;
        /// <summary>
        /// Application Reserved Keyword.
        /// </summary>
        public const byte APP2 = 0xe2;
        /// <summary>
        /// Application Reserved Keyword.
        /// </summary>
        public const byte APP3 = 0xe3;
        /// <summary>
        /// Application Reserved Keyword.
        /// </summary>
        public const byte APP4 = 0xe4;
        /// <summary>
        /// Application Reserved Keyword.
        /// </summary>
        public const byte APP5 = 0xe5;
        /// <summary>
        /// Application Reserved Keyword.
        /// </summary>
        public const byte APP6 = 0xe6;
        /// <summary>
        /// Application Reserved Keyword.
        /// </summary>
        public const byte APP7 = 0xe7;
        /// <summary>
        /// Application Reserved Keyword.
        /// </summary>
        public const byte APP8 = 0xe8;
        /// <summary>
        /// Application Reserved Keyword.
        /// </summary>
        public const byte APP9 = 0xe9;
        /// <summary>
        /// Application Reserved Keyword.
        /// </summary>
        public const byte APP10 = 0xea;
        /// <summary>
        /// Application Reserved Keyword.
        /// </summary>
        public const byte APP11 = 0xeb;
        /// <summary>
        /// Application Reserved Keyword.
        /// </summary>
        public const byte APP12 = 0xec;
        /// <summary>
        /// Application Reserved Keyword.
        /// </summary>
        public const byte APP13 = 0xed;
        /// <summary>
        /// Application Reserved Keyword.
        /// </summary>
        public const byte APP14 = 0xee;
        /// <summary>
        /// Application Reserved Keyword.
        /// </summary>
        public const byte APP15 = 0xef;

        /// <summary>
        /// Modulo Restart Interval.
        /// </summary>
        public const byte RST0 = 0xd0;
        /// <summary>
        /// Modulo Restart Interval.
        /// </summary>
        public const byte RST1 = 0xd1;
        /// <summary>
        /// Modulo Restart Interval.
        /// </summary>
        public const byte RST2 = 0xd2;
        /// <summary>
        /// Modulo Restart Interval.
        /// </summary>
        public const byte RST3 = 0xd3;
        /// <summary>
        /// Modulo Restart Interval.
        /// </summary>
        public const byte RST4 = 0xd4;
        /// <summary>
        /// Modulo Restart Interval.
        /// </summary>
        public const byte RST5 = 0xd5;
        /// <summary>
        /// Modulo Restart Interval.
        /// </summary>
        public const byte RST6 = 0xd6;
        /// <summary>
        /// Modulo Restart Interval.
        /// </summary>
        public const byte RST7 = 0xd7;

        /// <summary>
        /// Nondifferential Huffman-coding frame (baseline dct).
        /// </summary>
        public const byte SOF0 = 0xc0;
        /// <summary>
        /// Nondifferential Huffman-coding frame (extended dct).
        /// </summary>
        public const byte SOF1 = 0xc1;
        /// <summary>
        /// Nondifferential Huffman-coding frame (progressive dct).
        /// </summary>
        public const byte SOF2 = 0xc2;
        /// <summary>
        /// Nondifferential Huffman-coding frame Lossless (Sequential).
        /// </summary>
        public const byte SOF3 = 0xc3;
        /// <summary>
        /// Differential Huffman-coding frame Sequential DCT.
        /// </summary>
        public const byte SOF5 = 0xc5;
        /// <summary>
        /// Differential Huffman-coding frame Progressive DCT.
        /// </summary>
        public const byte SOF6 = 0xc6;
        /// <summary>
        /// Differential Huffman-coding frame lossless.
        /// </summary>
        public const byte SOF7 = 0xc7;
        /// <summary>
        /// Nondifferential Arithmetic-coding frame (extended dct).
        /// </summary>
        public const byte SOF9 = 0xc9;
        /// <summary>
        /// Nondifferential Arithmetic-coding frame (progressive dct)
        /// </summary>
        public const byte SOF10 = 0xca;
        /// <summary>
        /// Nondifferential Arithmetic-coding frame (lossless).
        /// </summary>
        public const byte SOF11 = 0xcb;
        /// <summary>
        /// Differential Arithmetic-coding frame (sequential dct).
        /// </summary>
        public const byte SOF13 = 0xcd;
        /// <summary>
        /// Differential Arithmetic-coding frame (progressive dct).
        /// </summary>
        public const byte SOF14 = 0xce;
        /// <summary>
        /// Differential Arithmetic-coding frame (lossless).
        /// </summary>
        public const byte SOF15 = 0xcf;

        /// <summary>
        /// Huffman Table.
        /// </summary>
        public const byte DHT = 0xc4;
        /// <summary>
        /// Quantization Table.
        /// </summary>
        public const byte DQT = 0xdb;
        /// <summary>
        /// Start of Scan.
        /// </summary>
        public const byte SOS = 0xda;
        /// <summary>
        /// Defined Restart Interval.
        /// </summary>
        public const byte DRI = 0xdd;
        /// <summary>
        /// Comment in JPEG.
        /// </summary>
        public const byte COM = 0xfe;
        /// <summary>
        /// Start of Image.
        /// </summary>
        public const byte SOI = 0xd8;
        /// <summary>
        /// End of Image.
        /// </summary>
        public const byte EOI = 0xd9;
    }
}
