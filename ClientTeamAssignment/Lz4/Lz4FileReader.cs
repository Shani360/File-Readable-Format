using System;
using System.IO;

namespace ClientTeamAssignment.Lz4
{
    static class Lz4FileReader
    {

        public static byte[] ReadLz4File(string filePath)
        {
            try
            {
                byte[] fileBytes = File.ReadAllBytes(filePath);
                int uncompressedSize = fileBytes.Length * 3;
                byte[] output;
                do
                {
                    output = new byte[uncompressedSize];
                    int actualSize = DecodeLz4Block(fileBytes, output, 8 + 4);

                    if (actualSize > output.Length)
                    {
                        uncompressedSize *= 2;
                    }
                    else
                    {
                        Array.Resize(ref output, actualSize);
                        break;
                    }
                } while (true);

                return output;
            }
            catch (Exception ex)
            {
                return new byte[0];
            }

        }

        private static int DecodeLz4Block(byte[] input, byte[] output, int sIdx = 0)
        {
            int eIdx = input.Length;
            int j = 0;

            for (int i = sIdx; i < eIdx;)
            {
                byte token = input[i++];
                int literalsLength = (token >> 4);
                if (literalsLength > 0)
                {
                    int l1 = literalsLength + 240;
                    while (l1 == 255)
                    {
                        l1 = input[i++];
                        literalsLength += l1;
                    }

                    int end = i + literalsLength;
                    while (i < end)
                    {
                        output[j++] = input[i++];
                    }

                    if (i == eIdx)
                    {
                        return j;
                    }
                }

                int offset = input[i++] | (input[i++] << 8);
                if (offset == 0 || offset > j)
                {
                    return -(i - 2);
                }

                int matchLength = (token & 0xf);
                int l = matchLength + 240;
                while (l == 255)
                {
                    l = input[i++];
                    matchLength += l;
                }

                int pos = j - offset; 
                int endMatch = j + matchLength + 4;
                while (j < endMatch)
                {
                    output[j++] = output[pos++];
                }
            }

            return j;
        }
    }
}
