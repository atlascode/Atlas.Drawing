namespace Atlas.Drawing.Serialization.PNG
{
    public class Adler32Computer
    {
        private int _a = 1;
        private int _b = 0;
        private static readonly int Modulus = 65521;

        public int Checksum => ((_b * 65536) + _a);

        public void Update(byte[] data, int offset, int length)
        {
            for (int counter = 0; counter < length; ++counter)
            {
                _a = (_a + (data[offset + counter])) % Modulus;
                _b = (_b + _a) % Modulus;
            }
        }
    }
}
