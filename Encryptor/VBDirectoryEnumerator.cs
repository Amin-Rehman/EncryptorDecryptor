using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows;
using System.Collections.ObjectModel;
using SharedProject;

namespace Encryptor
{
    class VBDirectoryEnumerator
    {
        /// <summary>
        /// Enumerate directories in a certain file
        /// </summary>
        public static ObservableCollection<VBFile> GetFilesFromDirectory(string directory)
        {
            ObservableCollection<VBFile> filesCollection = new ObservableCollection<VBFile>();

            try
            {
                var allFiles = Directory.EnumerateFiles(directory);
                foreach (string currentFile in allFiles)
                {
                    string fileName = currentFile.Substring(directory.Length + 1);
                    filesCollection.Add( new VBFile() { fileName = fileName, filePath = currentFile }) ;
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
