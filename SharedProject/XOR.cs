using System;
using System.Collections.Generic;
using System.Text;

namespace SharedProject
{
    public static class XOR
    {
        /// <summary>
        /// Helper method to XOR two strings which are input
        /// </summary>
        public static string XORStrings(string a, string b)
        {
            int baseLength;

            // a is smaller than b, pad a from left
            if (a.Length < b.Length)
            {
                a = a.PadLeft(b.Length, (char)(0));
                baseLength = b.Length;
            }
            else
            {
                b = b.PadLeft(a.Length, (char)(0));
                baseLength = a.Length;
            }

            char[] charAArray = a.ToCharArray();
            char[] charBArray = b.ToCharArray();

            char[] result = new char[baseLength];


            for (int i = 0; i < baseLength; i++)
            {
                result[i] = (char)((short)(charAArray[i]) ^ (short)(charBArray[i]));
            }

            string strResult = new string(result);
            strResult = strResult.Trim((char)(0));

            return strResult;

        }
    }
}
