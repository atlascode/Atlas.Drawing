using System;
using System.IO;

namespace Atlas.Drawing.Serialization.JPEG
{
    internal class BitReader : IDisposable
    {
        private int _count;
        private uint _buffer;
        private Stream _stream;

        public int Peak(int bitCount)
        {
            EnsureData(bitCount);

            var mask = ((1 << _count) - 1) ^ ((1 << (_count - bitCount)) - 1);

            return (int)((_buffer & mask) >> (_count - bitCount));
        }
        public int Read(int bitCount)
        {
            EnsureData(bitCount);

            var mask = ((1 << _count) - 1) ^ ((1 << (_count - bitCount)) - 1);
            var value = (int)(_buffer & mask) >> (_count - bitCount);

            _count -= bitCount;

            return value;
        }
        public void Skip(int bitCount)
        {
            _count -= bitCount;
        }
        public void Dispose()
        {
            if (_stream == null)
                return;

            _stream.Dispose();
        }
        
        private void EnsureData(int bitCount)
        {
            var todo = (((bitCount - _count) + 7) / 8);

            for (int i = 0; i < todo; i++)
                _buffer = (_buffer << 8) | (uint)_stream.ReadByte();

            _count += (todo > 0)
                        ? todo * 8
                        : 0;
        }

        public BitReader(byte[] data)
        {
            _stream = new MemoryStream(data);
        }
        public BitReader(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            _stream = stream;
        }
    }
}
