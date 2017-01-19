using System;

namespace Atlas.Drawing.Serialization.JPEG
{
    internal static class ComponentExtensions
    {
        /// <summary>
        /// Run the Quantization backward method on all of the block data.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static double[] QuantitizeData(this Component source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            // TODO(Dan): The data needs to be available on the component

            var temp = new double[64];

            //for (int i = 0; i < 64; i++)
            //    temp[i] = source.Block[i] * source.QuantizationTable[i];

            return temp;
        }
        /// <summary>
        /// This scales up the component size based on the factor size. 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static double[][] Scale(this Component source, double[][] data)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (data == null)
                throw new ArgumentNullException(nameof(data));

            var vFactor = source.MaxV / source.VFactor;
            var hFactor = source.MaxH / source.HFactor;

            if (hFactor == 1 && vFactor == 1)
                return data;

            var output = new double[data.Length][];

            for (var i = 0; i < output.Length; i++)
                output[i] = new double[data[0].Length];

            if (vFactor > 1)
            {
                for (var i = 0; i < source.QuantizationTable.Length; i++)
                {
                    // TODO(Dan): Deal with Vertical upscaling
                    var x = data[i];

                    for (int j = 0; j < data[i].Length; j++)
                    {
                        for (int k = 0; k < hFactor; k++)
                        {
                            //dest[j * factorUpVertical + u] = src[j];
                        }
                    }

                    //data.add(i, dest);
                }
            }

            if (hFactor > 1)
            {
                for (var i = 0; i < source.QuantizationTable.Length; i++)
                {
                    // TODO(Dan): Deal with Horizontal upscaling
                }
            }

            return output;
        }
        public static double DecodeDcCoeficient(this Component source, BitReader reader)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            var t = source.DCTable.Decode(reader);

            int diff = reader.Read(t);

            diff = source.DCTable.Extend(diff, t);
            diff += source.PreviousDC;

            source.PreviousDC = diff;

            return diff;
        }
        public static double[] DecodeAcCoeficients(this Component source, BitReader reader)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            var zz = new double[64];

            for (int k = 1; k < 64; k++)
            {
                int s = source.ACTable.Decode(reader);

                int r = s >> 4;
                s &= 15;

                if (s != 0)
                {
                    k += r;
                    r = reader.Read(s);
                    s = source.DCTable.Extend(r, s);
                    zz[k] = s;
                }
                else
                {
                    if (r != 15)
                        return (zz);

                    k += 15;
                }
            }

            return zz;
        }
    }
}
