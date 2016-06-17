using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace SharedProject
{
    class MD5Generator
    {
        /// <summary>
        /// Helper method to Generate MD5 of a file
        /// </summary>
        public static string GetMD5HashFromFile(string fileName)
        {
            try
            {
                using (var md5 = MD5.Create())
                {
                    using (var stream = File.OpenRead(fileName))
                    {
                        return BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", string.Empty);
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Exception while writing JSON file: " + e.Message);
                return;
            }
            
        }
    }
}
