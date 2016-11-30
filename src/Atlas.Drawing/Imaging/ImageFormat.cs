using System;

namespace Atlas.Drawing.Imaging
{
    //[TypeConverterAttribute(typeof(ImageFormatConverter))]
    public sealed class ImageFormat
    {
        private Guid _guid;

        private static ImageFormat _undefined = new ImageFormat(new Guid("{b96b3ca9-0728-11d3-9d7b-0000f81ef32e}"));
        private static ImageFormat _memoryBMP = new ImageFormat(new Guid("{b96b3caa-0728-11d3-9d7b-0000f81ef32e}"));
        private static ImageFormat _bmp = new ImageFormat(new Guid("{b96b3cab-0728-11d3-9d7b-0000f81ef32e}"));
        private static ImageFormat _emf = new ImageFormat(new Guid("{b96b3cac-0728-11d3-9d7b-0000f81ef32e}"));
        private static ImageFormat _wmf = new ImageFormat(new Guid("{b96b3cad-0728-11d3-9d7b-0000f81ef32e}"));
        private static ImageFormat _jpeg = new ImageFormat(new Guid("{b96b3cae-0728-11d3-9d7b-0000f81ef32e}"));
        private static ImageFormat _png = new ImageFormat(new Guid("{b96b3caf-0728-11d3-9d7b-0000f81ef32e}"));
        private static ImageFormat _gif = new ImageFormat(new Guid("{b96b3cb0-0728-11d3-9d7b-0000f81ef32e}"));
        private static ImageFormat _tiff = new ImageFormat(new Guid("{b96b3cb1-0728-11d3-9d7b-0000f81ef32e}"));
        private static ImageFormat _exif = new ImageFormat(new Guid("{b96b3cb2-0728-11d3-9d7b-0000f81ef32e}"));
        private static ImageFormat _photoCD = new ImageFormat(new Guid("{b96b3cb3-0728-11d3-9d7b-0000f81ef32e}"));
        private static ImageFormat _flashPIX = new ImageFormat(new Guid("{b96b3cb4-0728-11d3-9d7b-0000f81ef32e}"));
        private static ImageFormat _icon = new ImageFormat(new Guid("{b96b3cb5-0728-11d3-9d7b-0000f81ef32e}"));

        /// <summary>
        /// Specifies a global unique identifier (GUID) that represents this <see cref='ImageFormat'/>.
        /// </summary>
        public Guid Guid
        {
            get { return _guid; }
        }
        /// <summary>
        /// Specifies a memory bitmap image format.
        /// </summary>
        public static ImageFormat MemoryBmp
        {
            get { return _memoryBMP; }
        }
        /// <summary>
        ///    Specifies the bitmap image format.
        /// </summary>
        public static ImageFormat Bmp
        {
            get { return _bmp; }
        }
        /// <summary>
        ///    Specifies the enhanced Windows metafile image format.
        /// </summary>
        public static ImageFormat Emf
        {
            get { return _emf; }
        }
        /// <summary>
        ///    Specifies the Windows metafile image
        ///    format.
        /// </summary>
        public static ImageFormat Wmf
        {
            get { return _wmf; }
        }
        /// <summary>
        ///    Specifies the GIF image format.
        /// </summary>
        public static ImageFormat Gif
        {
            get { return _gif; }
        }
        /// <summary>
        ///    Specifies the JPEG image format.
        /// </summary>
        public static ImageFormat Jpeg
        {
            get { return _jpeg; }
        }
        /// <summary>
        ///    Specifies the W3C PNG image format.
        /// </summary>
        public static ImageFormat Png
        {
            get { return _png; }
        }
        /// <summary>
        ///    Specifies the Tag Image File Format (TIFF) image format.
        /// </summary>
        public static ImageFormat Tiff
        {
            get { return _tiff; }
        }
        /// <summary>
        ///    Specifies the Exchangable Image Format (EXIF).
        /// </summary>
        public static ImageFormat Exif
        {
            get { return _exif; }
        }
        /// <summary>
        ///     Specifies the Windows icon image format.
        /// </summary>
        public static ImageFormat Icon
        {
            get { return _icon; }
        }

        /// <summary>
        ///    Returns a value indicating whether the
        ///    specified object is an <see cref='ImageFormat'/> equivalent to this <see cref='ImageFormat'/>.
        /// </summary>
        public override bool Equals(object o)
        {
            ImageFormat format = o as ImageFormat;
            if (format == null)
                return false;
            return _guid == format._guid;
        }
        /// <summary>
        ///     Returns a hash code.
        /// </summary>
        public override int GetHashCode()
        {
            return _guid.GetHashCode();
        }
        /// <summary>
        ///    Converts this <see cref='ImageFormat'/> to a human-readable string.
        /// </summary>
        public override string ToString()
        {
            if (this == _memoryBMP) return "MemoryBMP";
            if (this == _bmp) return "Bmp";
            if (this == _emf) return "Emf";
            if (this == _wmf) return "Wmf";
            if (this == _gif) return "Gif";
            if (this == _jpeg) return "Jpeg";
            if (this == _png) return "Png";
            if (this == _tiff) return "Tiff";
            if (this == _exif) return "Exif";
            if (this == _icon) return "Icon";
            return "[ImageFormat: " + _guid + "]";
        }

        /// <summary>
        /// Initializes a new instance of the <see cref='ImageFormat'/> class with the specified GUID.
        /// </summary>
        /// <param name="guid"></param>
        public ImageFormat(Guid guid)
        {
            _guid = guid;
        }
    }
}
