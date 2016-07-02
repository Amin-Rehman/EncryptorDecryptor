using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SharedProject
{
    class FileCopier
    {
        /// <summary>
        /// Helper method to copy files to a destination folder
        /// </summary>
        /// <param name="sourceFiles"> Array of VBFiles which need to be copied</param>
        /// <param name="destPath"> Path where these files need to be copied to</param>
        public static void CopyFiles(VBFile []sourceFiles,string destPath)
        {
            foreach (VBFile sourceFile in sourceFiles)
            {
                File.Copy(sourceFile.filePath, Path.Combine(destPath, sourceFile.fileName),true);
            }
        }
    }
}
