using System;
using System.Diagnostics;

namespace Atlas.Drawing.Serialization.JPEG
{
    internal class ZigZag
    {
        public const bool ZIGZAG_FORWARD = true;
        public const bool ZIGZAG_BACKWARD = false;
        public readonly static byte[] Map =
        {
           0,   1,  8, 16,  9,  2,  3, 10,
           17, 24, 32, 25, 18, 11,  4,  5,
           12, 19, 26, 33, 40, 48, 41, 34,
           27, 20, 13,  6,  7, 14, 21, 28,
           35, 42, 49, 56, 57, 50, 43, 36,
           29, 22, 15, 23, 30, 37, 44, 51,
           58, 59, 52, 45, 38, 31, 39, 46,
           53, 60, 61, 54, 47, 55, 62, 63
        };

        /// <summary>
        /// Encodes a matrix of equal width and height to a byte array
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns></returns>
        public static byte[] Encode(byte[][] matrix)
        {
            byte[] buffer = new byte[matrix.Length ^ 2];
            bool direction = ZIGZAG_FORWARD;

            int x = 0, y = 0, index = 0;
            for (int zigIndex = 0; zigIndex < (matrix.Length * 2 - 1); zigIndex++, direction = !direction)
            {
                if (direction == ZIGZAG_FORWARD)
                {
                    while (x >= 0 && y != matrix.Length)
                    {
                        if (x == matrix.Length)
                        {
                            x--;
                            y++;
                        }
                        buffer[index] = matrix[x][y];
                        y++;
                        x--;
                        index++;
                    }
                    x++;
                }
                else
                {
                    while (y >= 0 && x != matrix.Length)
                    {
                        if (y == matrix.Length)
                        {
                            y--;
                            x++;
                        }
                        buffer[index] = matrix[x][y];
                        y--;
                        x++;
                        index++;
                    }
                    y++;
                }
            }

            return buffer;
        }
        /// <summary>
        /// Encodes a matrix of equal width and height to a double array
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns></returns>
        public static double[] Encode(double[][] matrix)
        {
            double[] buffer = new double[matrix.Length * matrix.Length];
            bool direction = ZIGZAG_FORWARD;

            int x = 0, y = 0, index = 0;
            for (int zigIndex = 0; zigIndex < (matrix.Length * 2 - 1); zigIndex++, direction = !direction)
            {
                if (direction == ZIGZAG_FORWARD)
                {
                    while (x >= 0 && y != matrix.Length)
                    {
                        if (x == matrix.Length)
                        {
                            x--;
                            y++;
                        }
                        buffer[index] = matrix[x][y];
                        y++;
                        x--;
                        index++;
                    }
                    x++;
                }
                else
                {
                    while (y >= 0 && x != matrix.Length)
                    {
                        if (y == matrix.Length)
                        {
                            y--;
                            x++;
                        }
                        buffer[index] = matrix[x][y];
                        y--;
                        x++;
                        index++;
                    }
                    y++;
                }
            }

            return buffer;
        }
        /// <summary>
        /// Encodes a matrix of equal width and height to a float array
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns></returns>
        public static float[] Encode(float[][] matrix)
        {
            float[] buffer = new float[matrix.Length * matrix.Length];
            bool direction = ZIGZAG_FORWARD;

            int x = 0, y = 0, index = 0;
            for (int zigIndex = 0; zigIndex < (matrix.Length * 2 - 1); zigIndex++, direction = !direction)
            {
                if (direction == ZIGZAG_FORWARD)
                {
                    while (x >= 0 && y != matrix.Length)
                    {
                        if (x == matrix.Length)
                        {
                            x--;
                            y++;
                        }
                        buffer[index] = matrix[x][y];
                        y++;
                        x--;
                        index++;
                    }
                    x++;
                }
                else
                {
                    while (y >= 0 && x != matrix.Length)
                    {
                        if (y == matrix.Length)
                        {
                            y--;
                            x++;
                        }
                        buffer[index] = matrix[x][y];
                        y--;
                        x++;
                        index++;
                    }
                    y++;
                }
            }

            return buffer;
        }
        /// <summary>
        /// Encodes a matrix of equal width and height to a short array
        /// </summary>
        /// <param name="matrix"></param>
        /// <returns></returns>
        public static short[] Encode(short[][] matrix)
        {
            short[] buffer = new short[matrix.Length * matrix.Length];
            bool direction = ZIGZAG_FORWARD;

            int x = 0, y = 0, index = 0;
            for (int zigIndex = 0; zigIndex < (matrix.Length * 2 - 1); zigIndex++, direction = !direction)
            {
                if (direction == ZIGZAG_FORWARD)
                {
                    while (x >= 0 && y != matrix.Length)
                    {
                        if (x == matrix.Length)
                        {
                            x--;
                            y++;
                        }
                        buffer[index] = matrix[x][y];
                        y++;
                        x--;
                        index++;
                    }
                    x++;
                }
                else
                {
                    while (y >= 0 && x != matrix.Length)
                    {
                        if (y == matrix.Length)
                        {
                            y--;
                            x++;
                        }
                        buffer[index] = matrix[x][y];
                        y--;
                        x++;
                        index++;
                    }
                    y++;
                }
            }

            return buffer;
        }

        /// <summary>
        /// Convert a byte array into a matrix with the same amount of columns and rows with length sqrt(byte array length)
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static byte[][] Decode(byte[] data)
        {
            return Decode(data, (int)Math.Sqrt(data.Length), (int)Math.Sqrt(data.Length));
        }
        /// <summary>
        /// Convert a int array into a matrix with the same amount of columns and rows with length sqrt(int array length)
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static int[][] Decode(int[] data)
        {
            return Decode(data, (int)Math.Sqrt(data.Length), (int)Math.Sqrt(data.Length));
        }
        public static byte[][] Decode(byte[] data, int width, int height)
        {
            byte[][] buffer = new byte[height][];

            for (int i = 0; i < buffer.Length; i++)
                buffer[i] = new byte[width];

            for (int v = 0; v < height; v++)
                for (int z = 0; z < width; z++)
                    buffer[v][z] = 11;

            bool dir = ZIGZAG_FORWARD;
            int xindex = 0, yindex = 0, dataindex = 0;

            while (xindex < width && yindex < height && dataindex < data.Length)
            {
                buffer[yindex][xindex] = data[dataindex];
                dataindex++;

#if DEBUG
                Debug.WriteLine($"Setting {dataindex} to row: {yindex} column: {xindex} yourval: {(yindex * 8 + xindex)}");
#endif

                if (dir == ZIGZAG_FORWARD)
                {
                    if (yindex == 0 || xindex == (width - 1))
                    {
                        dir = ZIGZAG_BACKWARD;
                        if (xindex == (width - 1))
                            yindex++;
                        else
                            xindex++;
                    }
                    else
                    {
                        yindex--;
                        xindex++;
                    }
                }
                else
                { 
                    /* Backwards */
                    if (xindex == 0 || yindex == (height - 1))
                    {
                        dir = ZIGZAG_FORWARD;
                        if (yindex == (height - 1))
                            xindex++;
                        else
                            yindex++;
                    }
                    else
                    {
                        yindex++;
                        xindex--;
                    }
                }
            }

            return buffer;
        }
        public static double[][] Decode(double[] data, int width, int height)
        {
            double[][] buffer = new double[height][];

            for (int i = 0; i < buffer.Length; i++)
                buffer[i] = new double[width];

            for (int v = 0; v < height; v++)
                for (int z = 0; z < width; z++)
                    buffer[v][z] = 11;

            bool dir = ZIGZAG_FORWARD;
            int xindex = 0, yindex = 0, dataindex = 0;

            while (xindex < width && yindex < height && dataindex < data.Length)
            {
                buffer[yindex][xindex] = data[dataindex];
                dataindex++;

#if DEBUG
                Debug.WriteLine($"Setting {dataindex} to row: {yindex} column: {xindex} yourval: {(yindex * 8 + xindex)}");
#endif

                if (dir == ZIGZAG_FORWARD)
                {
                    if (yindex == 0 || xindex == (width - 1))
                    {
                        dir = ZIGZAG_BACKWARD;
                        if (xindex == (width - 1))
                            yindex++;
                        else
                            xindex++;
                    }
                    else
                    {
                        yindex--;
                        xindex++;
                    }
                }
                else
                { 
                    /* Backwards */
                    if (xindex == 0 || yindex == (height - 1))
                    {
                        dir = ZigZag.ZIGZAG_FORWARD;
                        if (yindex == (height - 1))
                            xindex++;
                        else
                            yindex++;
                    }
                    else
                    {
                        yindex++;
                        xindex--;
                    }
                }
            }

            return buffer;
        }
        public static float[][] Decode(float[] data, int width, int height)
        {
            float[][] buffer = new float[height][];

            for (int i = 0; i < buffer.Length; i++)
                buffer[i] = new float[width];

            for (int v = 0; v < height; v++)
                for (int z = 0; z < width; z++)
                    buffer[v][z] = 11;

            bool dir = ZIGZAG_FORWARD;
            int xindex = 0, yindex = 0, dataindex = 0;

            while (xindex < width && yindex < height && dataindex < data.Length)
            {
                buffer[yindex][xindex] = data[dataindex];
                dataindex++;

#if DEBUG
                Debug.WriteLine($"Setting {dataindex} to row: {yindex} column: {xindex} yourval: {(yindex * 8 + xindex)}");
#endif

                if (dir == ZIGZAG_FORWARD)
                {
                    if (yindex == 0 || xindex == (width - 1))
                    {
                        dir = ZIGZAG_BACKWARD;
                        if (xindex == (width - 1))
                            yindex++;
                        else
                            xindex++;
                    }
                    else
                    {
                        yindex--;
                        xindex++;
                    }
                }
                else
                { 
                    /* Backwards */
                    if (xindex == 0 || yindex == (height - 1))
                    {
                        dir = ZIGZAG_FORWARD;
                        if (yindex == (height - 1))
                            xindex++;
                        else
                            yindex++;
                    }
                    else
                    {
                        yindex++;
                        xindex--;
                    }
                }
            }

            return buffer;
        }
        public static int[][] Decode(int[] data, int width, int height)
        {
            int[][] buffer = new int[height][];

            for (int i = 0; i < buffer.Length; i++)
                buffer[i] = new int[width];

            for (int v = 0; v < height; v++)
                for (int z = 0; z < width; z++)
                    buffer[v][z] = 11;

            bool dir = ZIGZAG_FORWARD;
            int xindex = 0, yindex = 0, dataindex = 0;

            while (xindex < width && yindex < height && dataindex < data.Length)
            {
                buffer[yindex][xindex] = data[dataindex];
                dataindex++;

#if DEBUG
                Debug.WriteLine($"Setting {dataindex} to row: {yindex} column: {xindex} yourval: {(yindex * 8 + xindex)}");
#endif

                if (dir == ZIGZAG_FORWARD)
                {
                    if (yindex == 0 || xindex == (width - 1))
                    {
                        dir = ZIGZAG_BACKWARD;
                        if (xindex == (width - 1))
                            yindex++;
                        else
                            xindex++;
                    }
                    else
                    {
                        yindex--;
                        xindex++;
                    }
                }
                else
                { 
                    /* Backwards */
                    if (xindex == 0 || yindex == (height - 1))
                    {
                        dir = ZIGZAG_FORWARD;
                        if (yindex == (height - 1))
                            xindex++;
                        else
                            yindex++;
                    }
                    else
                    {
                        yindex++;
                        xindex--;
                    }
                }
            }

            return buffer;
        }
        public static double[][] Decode8x8(double[] input)
        {
            double[][] output = new double[8][];

            for (int i = 0; i < output.Length; i++)
                output[i] = new double[8];

            for (int i = 0; i < 64; i++)
                output[Map[i] / 8][Map[i] % 8] = input[i];

            return output;
        }
    }
}
