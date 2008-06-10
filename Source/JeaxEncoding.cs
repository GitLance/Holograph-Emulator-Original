using System;
using System.Text;

namespace Holo
{
    /// <summary>
    /// Provides number encoding/decoding for the Habbo client. Features VL64 and Base64. Class written by Josh Comery. (Jeax)
    /// </summary>
    public static class Encoding
    {
        #region Base64
        public static string encodeB64(int value, int length)
        {
            string stack = "";
            for (int x = 1; x <= length; x++)
            {
                int offset = 6 * (length - x);
                byte val = (byte)(64 + (value >> offset & 0x3f));
                stack += (char)val;
            }
            return stack;
        }
        public static string encodeB64(string Val)
        {
            int value = Val.Length;
            int length = 2;
            string stack = "";
            for (int x = 1; x <= length; x++)
            {
                int offset = 6 * (length - x);
                byte val = (byte)(64 + (value >> offset & 0x3f));
                stack += (char)val;
            }
            return stack;
        }
        public static int decodeB64(string Val)
        {
            char[] val = Val.ToCharArray();
            int intTot = 0;
            int y = 0;
            for (int x = (val.Length - 1); x >= 0; x--)
            {
                int intTmp = (int)(byte)((val[x] - 64));
                if (y > 0)
                {
                    intTmp = intTmp * (int)(Math.Pow(64, y));
                }
                intTot += intTmp;
                y++;
            }
            return intTot;
        }
        #endregion
        #region VL64
        public static string encodeVL64(int i)
        {
            byte[] wf = new byte[6];
            int pos = 0;
            int startPos = pos;
            int bytes = 1;
            int negativeMask = i >= 0 ? 0 : 4;
            i = Math.Abs(i);
            wf[pos++] = (byte)(64 + (i & 3));
            for (i >>= 2; i != 0; i >>= 6)
            {
                bytes++;
                wf[pos++] = (byte)(64 + (i & 0x3f));
            }

            wf[startPos] = (byte)(wf[startPos] | bytes << 3 | negativeMask);

            System.Text.ASCIIEncoding encoder = new ASCIIEncoding();
            string tmp = encoder.GetString(wf);
            return tmp.Replace("\0", "");
        }
        public static int decodeVL64(string data)
        {
            return decodeVL64(data.ToCharArray());
        }
        public static int decodeVL64(char[] raw)
        {
            try
            {
                int pos = 0;
                int v = 0;
                bool negative = (raw[pos] & 4) == 4;
                int totalBytes = raw[pos] >> 3 & 7;
                v = raw[pos] & 3;
                pos++;
                int shiftAmount = 2;
                for (int b = 1; b < totalBytes; b++)
                {
                    v |= (raw[pos] & 0x3f) << shiftAmount;
                    shiftAmount = 2 + 6 * b;
                    pos++;
                }

                if (negative)
                    v *= -1;

                return v;
            }
            catch
            {
                return 0;
            }
        }
        #endregion
    }
}