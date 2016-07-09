using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SharedProject
{
    class FileUtility
    {
        public static void deleteAllFilesFromFolder(string folderPath)
        {
            System.IO.DirectoryInfo di = new DirectoryInfo(folderPath);
            // Delete all files in the mounted drive
            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete();
            }
        }

        public static bool isContainerSizeEnough(VBFile[] listOfFiles)
        {
            string pathToContainer = getPathToContainer();
            var containerSize = new System.IO.FileInfo(pathToContainer).Length;

            long allFilesSize = 0;

            foreach (VBFile file in listOfFiles)
            {
                allFilesSize += new System.IO.FileInfo(file.filePath).Length;
            }


            if (allFilesSize < containerSize)
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        public static string getPathToContainer()
        {
            return Directory.GetCurrentDirectory() + "\\" + nameOfContainer();
        }

        public static string nameOfContainer()
        {
            return "TCRYPT";
        }

    }
}
