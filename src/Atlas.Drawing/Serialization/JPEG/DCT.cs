using System;

namespace Atlas.Drawing.Serialization.JPEG
{
    internal class DCT
    {
        public double[][] CosineMatrix { get; set; }
        public double[][] TransformedCosineMatrix { get; set; }

        public double[][] FastIdct(double[][] input)
        {
            double[][] output = new double[8][];
            double[][] temp = new double[8][];
            double temp1;

            for (int x = 0; x < output.Length; x++)
                output[x] = new double[8];

            for (int y = 0; y < temp.Length; y++)
                temp[y] = new double[8];

            int i, j, k;
            for (i = 0; i< 8; i++)
            {
                for (j = 0; j< 8; j++)
                {
                    temp[i][j] = 0.0;
                    for (k = 0; k< 8; k++)
                    {
                        temp[i][j] += input[i][k] * CosineMatrix[k][j];
                    }
                }
            }

            for (i = 0; i< 8; i++)
            {
                for (j = 0; j< 8; j++)
                {
                    temp1 = 0.0;

                    for (k = 0; k< 8; k++)
                        temp1 += TransformedCosineMatrix[i][k] * temp[k][j];

                    temp1 += 128.0;

                    output[i][j] = (int)Math.Round(temp1).Clamp(0, 255);
                }
            }

            return output;
        }
        public float[][] FastFdct(float[][] input)
        {
            float[][] output = new float[8][];
            double[][] temp = new double[8][];
            double temp1;
            int i;
            int j;
            int k;

            for (int y = 0; y < 8; y++)
            {
                output[y] = new float[8];
                temp[y] = new double[8];
            }

            for (i = 0; i < 8; i++)
            {
                for (j = 0; j < 8; j++)
                {
                    temp[i][j] = 0.0;
                    for (k = 0; k < 8; k++)
                    {
                        temp[i][j] += (((int)(input[i][k]) - 128) * TransformedCosineMatrix[k][j]);
                    }
                }
            }

            for (i = 0; i < 8; i++)
            {
                for (j = 0; j < 8; j++)
                {
                    temp1 = 0.0;

                    for (k = 0; k < 8; k++)
                    {
                        temp1 += (CosineMatrix[i][k] * temp[k][j]);
                    }

                    output[i][j] = (int)Math.Round(temp1) * 8;
                }
            }

            return output;
        }

        public DCT()
        {
            CosineMatrix = new double[8][];
            TransformedCosineMatrix = new double[8][];

            for (int i = 0; i < CosineMatrix.Length; i++)
                CosineMatrix[i] = new double[8];

            for (int i = 0; i < TransformedCosineMatrix.Length; i++)
                TransformedCosineMatrix[i] = new double[8];

            for (int j = 0; j < 8; j++)
            {
                double nn = 8;
                CosineMatrix[0][j] = 1.0 / Math.Sqrt(nn);
                TransformedCosineMatrix[j][0] = CosineMatrix[0][j];
            }

            for (int i = 1; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    double jj = j;
                    double ii = i;
                    CosineMatrix[i][j] = Math.Sqrt(2.0 / 8.0) * Math.Cos(((2.0 * jj + 1.0) * ii * Math.PI) / (2.0 * 8.0));
                    TransformedCosineMatrix[j][i] = CosineMatrix[i][j];
                }
            }
        }
    }
}
