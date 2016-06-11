using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows;
using System.Collections.ObjectModel;

namespace Encryptor
{
    class VBDirectoryEnumerator
    {
        /// <summary>
        /// Enumerate directories in a certain file
        /// </summary>
        public static ObservableCollection<VBFiles> GetFilesFromDirectory(string directory)
        {
            ObservableCollection<VBFiles> filesCollection = new ObservableCollection<VBFiles>();

            try
            {
                var allFiles = Directory.EnumerateFiles(directory);
                foreach (string currentFile in allFiles)
                {
                    string fileName = currentFile.Substring(directory.Length + 1);
                    filesCollection.Add( new VBFiles() { fileName = fileName, filePath = currentFile }) ;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Exception while enumerating filesCollection: "+e.Message);
                return null;
            }
            return filesCollection;
        }
    }
}
