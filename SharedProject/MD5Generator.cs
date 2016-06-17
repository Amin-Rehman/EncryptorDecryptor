using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Threading;

/*
Example usage:
        private async void GenerateMD5Async()
        {
            string fileToTest = Directory.GetCurrentDirectory() + "\\testFile.txt";

            var md5OfFile = await MD5Generator.GetMD5HashFromFile(fileToTest);

            Console.WriteLine();
        }
*/
namespace SharedProject
{
    class MD5Generator
    {
        /// <summary>
        /// Helper method to Generate MD5 of a file. This will do the file reading and MD5 Hashing in a separate task
        /// 
        /// </summary>
        public static async Task<string> GetMD5HashFromFile(string fileName)
        {
            try
            {
                var result = await Task.Run(() => {
                    using (var md5 = MD5.Create())
                    {
                        var stream = File.OpenRead(fileName);
                        return BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", string.Empty);
                    }
                });

                return result;

            }
            catch (Exception e)
            {
                MessageBox.Show("Exception while writing JSON file: " + e.Message);
                return null;
            }
            
        }
    }
}
