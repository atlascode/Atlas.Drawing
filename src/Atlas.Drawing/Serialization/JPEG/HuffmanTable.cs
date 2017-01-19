using System;

namespace Atlas.Drawing.Serialization.JPEG
{
    internal class HuffmanTable
    {
        //public const int FAST_BITS = 9;
        //public const int FAST_MASK = (1 << FAST_BITS) - 1;
        public const int HUFFMAN_MAX_TABLES = 4;
        public const byte HUFFMAN_DC_TABLE = 0;
        public const byte HUFFMAN_AC_TABLE = 1;

        public int[] Code { get; set; } = new int[256];
        public int[] Size { get; set; } = new int[256];
        public int[] Values { get; set; }
        public int[] MaxCode { get; set; } = { -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 };
        public byte[] Length { get; set; }

        public HuffmanTable() { }
        public HuffmanTable(byte[] data, byte[] values)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            if (values == null)
                throw new ArgumentNullException(nameof(data));

            Values = new int[values.Length];
            Size = new int[values.Length];

            for (int x = 0; x < values.Length; x++)
                Values[x] = values[x];

            int i, j, k = 0, code;
            // build size list for each symbol (from JPEG spec)
            for (i = 0; i < 16; i++)
            {
                for (j = 0; j < data[i]; j++)
                {
                    Size[k++] = (byte)(i + 1);
                }
            }

            //Size[k] = 0;

            // compute actual symbols (from jpeg spec)
            code = 0;
            k = 0;
            for (j = 1; j <= 16; ++j)
            {
                // compute delta to add to code to compute symbol id
                //h->delta[j] = k - code;
                if (Size[k] == j)
                {
                    do
                    {
                        Code[k++] = code++;
                    }
                    while (k < values.Length && Size[k] == i);

                    if (code - 1 >= (1 << i))
                        throw new ImageFormatException("Bad huffman code length");
                }
                // compute largest code + 1 for this size, preshifted as needed later
                MaxCode[j] = code << (16 - j);
                code <<= 1;
            }
            MaxCode[j] = int.MaxValue;

            // TODO(Dan): build non-spec acceleration table; 255 is flag for not-accelerated?!?
        }
    }
}
