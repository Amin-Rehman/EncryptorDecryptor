using System;
using System.Collections.Generic;
using System.Text;

namespace SharedProject
{
    /// <summary>
    /// Helper class to allow a key in Hex format to convert to ascii (for XORing) or vice versa
    /// </summary>
    class KeyNormalizer
    {
        public static string ToHex(string input)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in input)
                sb.AppendFormat("{0:X2} ", (int)c);
            return sb.ToString().Trim();
        }


        public static string ToAscii(string input)
        {
            char[] delimiterChars = { ' ' };
            var splitString = input.Split(delimiterChars);

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            foreach (string s in splitString)
            {
                sb.Append(System.Convert.ToChar(System.Convert.ToUInt32(s, 16)));
            }

            return sb.ToString();
        }
    }
}


