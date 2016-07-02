using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Threading;


namespace SharedProject
{
    class MD5Generator
    {
        /// <summary>
        /// Helper method to Generate MD5 of a file. This ideally needs to be called in a background worker thread
        /// 
        /// </summary>
        public static string GetMD5HashFromFile(string fileName)
        {
            try
            {
                    using (var md5 = MD5.Create())
                    {
                        var stream = File.OpenRead(fileName);
                        return BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", string.Empty);
                    }
            }
            catch (Exception e)
            {
                MessageBox.Show("Exception while writing JSON file: " + e.Message);
                return null;
            }
            
        }
    }
}
