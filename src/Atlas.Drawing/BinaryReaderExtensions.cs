using System;
using System.IO;

namespace Atlas.Drawing
{
    internal static class BinaryReaderExtensions
    {
        public static ushort ReadUInt16BE(this BinaryReader source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            var temp = source.ReadBytes(2);

            return (ushort)(temp[0] << 8 | temp[1]);
        }
    }
}
