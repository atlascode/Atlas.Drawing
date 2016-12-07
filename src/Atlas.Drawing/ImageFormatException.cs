using System;

namespace Atlas.Drawing
{
    internal class ImageFormatException : Exception
    {
        public ImageFormatException() { }
        public ImageFormatException(string message) : base(message) { }
        public ImageFormatException(string message, Exception innerException) : base(message, innerException) { }
    }
}
