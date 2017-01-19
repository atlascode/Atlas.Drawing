using System;

namespace Atlas.Drawing.Serialization.JPEG
{
    internal static class HuffmanTableExtensions
    {
        public static int Decode(this HuffmanTable source, BitReader reader)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (reader == null)
                throw new ArgumentNullException(nameof(source));

            var i = 0;
            var code = reader.Read(1);

            while (code > source.MaxCode[i])
            {
                i++;
                code <<= 1;
                code |= reader.Read(1);
            }

            int val = source.Values[code];

            if (val < 0)
                val = 256 + val;

            return val;
        }

        public static int Extend(this HuffmanTable source, int diff, int t)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            int Vt = (int)Math.Pow(2, (t - 1));

            if (diff < Vt)
            {
                Vt = (-1 << t) + 1;
                diff += Vt;
            }

            return diff;
        }

    }
}
