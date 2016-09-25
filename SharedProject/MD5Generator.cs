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
        const int FILE_SIZE_TO_READ = 1000000;

        private static string BytesToStringConverted(byte[] bytes)
        {
            using (var stream = new MemoryStream(bytes))
            {
                using (var streamReader = new StreamReader(stream))
                {
                    return streamReader.ReadToEnd();
                }
            }
        }

        /// <summary>
        /// Helper method to Generate MD5 of a file. This ideally needs to be called in a background worker thread
        /// 
        /// </summary>
        public static string GetMD5HashFromFile(string fileName)
        {
            try
            {
                string dataFromFile = "";
                using (var md5 = MD5.Create())
                {
                    FileStream fsSource = new FileStream(fileName,FileMode.Open, FileAccess.Read);

                    byte[] bytes = new byte[FILE_SIZE_TO_READ];
                    int n = fsSource.Read(bytes, 0, FILE_SIZE_TO_READ);
                    string data = BytesToStringConverted(bytes);

                    dataFromFile = data.Replace("-", string.Empty);

                }
                return dataFromFile;

            }

            catch (Exception e)
            {
                MessageBox.Show("Exception while writing JSON file: " + e.Message);
                return null;
            }
            
        }
    }
}
