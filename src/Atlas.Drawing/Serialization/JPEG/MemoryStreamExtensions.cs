using System;
using System.Collections.Generic;
using System.IO;

namespace Atlas.Drawing.Serialization.JPEG
{
    internal static class MemoryStreamExtensions
    {
        public static void Write(this MemoryStream source, Component component, int componentIndex)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (component == null)
                throw new ArgumentNullException(nameof(component));

            if (componentIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(componentIndex), "Component Index cannot be less than 0");

            WriteImpl(source, component, componentIndex);
        }

        private static void WriteImpl(MemoryStream stream, Component component, int componentIndex)
        {
            // TODO(Dan): 
        }
    }
}

           
