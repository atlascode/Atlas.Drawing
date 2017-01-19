using System;
using System.IO;
using Atlas.Drawing.Imaging;
using Atlas.Drawing.Serialization.BMP;
using Atlas.Drawing.Serialization.JPEG;
using Atlas.Drawing.Serialization.PNG;

namespace Atlas.Drawing
{
    public class Image
    {
        private byte[] _rawBytes;

        public int Width { get; private set; }
        public int Height { get; private set; }

        public void Save(string filename, ImageFormat format)
        {
            using (var stream = File.OpenWrite(filename))
            {
                Save(stream, format);
            }
        }
        public void Save(Stream stream, ImageFormat format)
        {
            if (format == ImageFormat.Bmp)
            {
                var bmpEncoder = new BMPEncoder();
                var bytes = bmpEncoder.Encode(_rawBytes, Width, Height);
                stream.Write(bytes, 0, bytes.Length);
            }
            else if (format == ImageFormat.Png)
            {
                var pngEncoder = new PNGEncoder();
                var bytes = pngEncoder.Encode(_rawBytes, Width, Height);
                stream.Write(bytes, 0, bytes.Length);
            }
            else if (format == ImageFormat.MemoryBmp)
            {
                stream.Write(this._rawBytes, 0, this._rawBytes.Length);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public static Image FromFile(string filename)
        {
            using (var stream = File.OpenRead(filename))
            {
                return Image.FromStream(stream);
            }
        }
        public static Image FromStream(Stream stream)
        {
            var ms = new MemoryStream();
            stream.CopyTo(ms);

            var imageBytes = ms.ToArray();

            string header = System.Text.Encoding.ASCII.GetString(imageBytes, 0, 4);

            //try
            //{
            if (header.Substring(0,2) == "BM")
            {
                int width;
                int height;
                var bytes = new BMPDecoder().Decode(ref imageBytes, out width, out height);
                return new Image(bytes, width, height);
            }
            else if (imageBytes[0] == 0x89 && header.Substring(1) == "PNG")
            {
                int width;
                int height;
                var bytes = new PNGDecoder().Decode(ref imageBytes, out width, out height);
                return new Image(bytes, width, height);
            }
            else if (IsJpeg(ref imageBytes))
            {
                int width = 0;
                int height = 0;

                var bytes = new JPEGDecoder().Decode(ref imageBytes, out width, out height);
                return new Image(bytes, width, height);
            }
            //}
            //catch (Exception ex)
            //{
            //    // We wrap exceptions reading images with an OutOfMemory Exception because the System.Drawing.Image class throws these exceptions from GDI+ so to maintain backwards compatability we throw the same
            //    throw new System.OutOfMemoryException("There was an error loading the bitmap. See inner exception for details", ex);
            //}

            return null;
        }

        private static bool IsJpeg(ref byte[] bytes)
        {
            // JPEG magic number
            return bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF;
        }

        protected Image(byte[] rawBytes, int width, int height)
        {
            this._rawBytes = rawBytes;
            this.Width = width;
            this.Height = height;
        }
    }
}
