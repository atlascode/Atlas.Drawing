using Atlas.Drawing.Imaging;
using Atlas.Drawing.Serialization.BMP;
using Atlas.Drawing.Serialization.PNG;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Atlas.Drawing
{
    public class Image
    {
        public int Width { get; private set; }
        public int Height { get; private set; }
        private byte[] rawBytes;
        protected Image(byte[] rawBytes, int width, int height)
        {
            this.rawBytes = rawBytes;
            this.Width = width;
            this.Height = height;
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

            string header = System.Text.Encoding.ASCII.GetString(imageBytes, 0, 2);

            try
            {
                if (header == "BM")
                {
                    int width;
                    int height;
                    var bytes = new BMPDecoder().Decode(ref imageBytes, out width, out height);
                    return new Image(bytes, width, height);
                }
            }
            catch (Exception ex)
            {
                // We wrap exceptions reading images with an OutOfMemory Exception because the System.Drawing.Image class throws these exceptions from GDI+ so to maintain backwards compatability we throw the same
                throw new System.OutOfMemoryException("There was an error loading the bitmap. See inner exception for details", ex);
            }

            return null;
        }

        public void Save(string filename, ImageFormat format)
        {
            using (var stream = File.OpenWrite(filename))
            {
                Save(stream, format);
            }
        }

        public void Save(Stream stream, ImageFormat format)
        {
            if(format == ImageFormat.Bmp)
            {
                var bmpEncoder = new BMPEncoder();
                var bytes = bmpEncoder.Encode(rawBytes, Width, Height);
                stream.Write(bytes, 0, bytes.Length);
            }
            else if (format == ImageFormat.Png)
            {
                var pngEncoder = new PNGEncoder();
                var bytes = pngEncoder.Encode(rawBytes, Width, Height);
                stream.Write(bytes, 0, bytes.Length);
            }
            else if(format == ImageFormat.MemoryBmp)
            {
                stream.Write(this.rawBytes, 0, this.rawBytes.Length);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}
