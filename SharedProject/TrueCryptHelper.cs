using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SharedProject
{
    class TrueCryptHelper
    {

        [DllImport("Crypto.dll")]
        private static extern void startDeviceDriver();

        [DllImport("Crypto.dll")]
        private static extern void stopDeviceDriver();


        [DllImport("Crypto.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void mountContainer(int iDrive, string password, int passwordLength, string pathToContainer);
        [DllImport("Crypto.dll")]
        private static extern void unMountContainer();

        public static string pathToTrueCryptContainer = "";
        static bool isContainerMountedCalled = false;


        private static int mountedDrivePath = -1;


        /// <summary>
        /// Helper method to mount a container.
        /// </summary>
        /// <param name="password"> Password for the TrueCrypt container</param>
        /// <param name="pathToContainer">Path to the container</param>
        public static void MountContainer(string password, string pathToContainer)
        {
            int driveNumber = GetDriveToMount();
            if (driveNumber == -1 || password.Length == 0)
            {
                // Error evaluating available driver to mount
                return;
            }

            mountContainer(driveNumber, password, password.Length, pathToContainer);


            isContainerMountedCalled = true;
            pathToTrueCryptContainer = pathToContainer;
            mountedDrivePath = driveNumber;

        }


        public static bool CloseAllExplorers()
        {

            foreach (Process p in Process.GetProcessesByName("explorer"))
            {
                p.Kill();
            }

            // Wait here till all the explorer processes have ended
            DateTime startTime = DateTime.Now;

            while (true)
            {
                DateTime currentTime = DateTime.Now;
                var timeDiff = currentTime.Subtract(startTime);

                // Break the check if its been more than 5 seconds
                if (timeDiff.Seconds > 5)
                {
                    break;
                }
            }
            return true;
        }


        /// <summary>
        /// Unmount the container if one is mounted.
        /// </summary>
        public static void UnMountContainer()
        {
            if (isContainerMountedCalled == true)
            {
                unMountContainer();
                isContainerMountedCalled = false;
                mountedDrivePath = -1;
            }
        }

        /// <summary>
        /// Start TrueCrypt drivers. Ideally call this on App Startup
        /// </summary>
        public static void StartDeviceDriver()
        {
            startDeviceDriver();
        }

        /// <summary>
        /// Stop TrueCrypt drivers. Ideally call this on App Exit
        /// </summary>
        public static void StopDeviceDriver()
        {
            stopDeviceDriver();
        }

        /// <summary>
        /// Helper Method to iterate through all the drives and returns the drive integer for the TrueCrypt container
        /// </summary>
        /// <returns>
        /// Integer of the drive number which can be mounted
        /// </returns>
        private static int GetDriveToMount()
        {
            const int ASCII_OFFSET = 65;
            int driveToMount = -1;
            DriveInfo[] allDrives = DriveInfo.GetDrives();
            List<char> driveLetters = new List<char>();
            List<int> trueCryptDriveLetter = new List<int>();


            foreach (DriveInfo d in allDrives)
            {
                // Retrieve just the drive letter
                char filteredName = d.Name.ElementAt<char>(0);
                driveLetters.Add(filteredName);
            }

            foreach (char driveChar in driveLetters)
            {
                trueCryptDriveLetter.Add((int)driveChar - ASCII_OFFSET);
            }

            // Chose random drive which is available
            // Chose a number between 0 and 25 excluding the drives already in use
            Random random = new Random();
            do
            {
                driveToMount = random.Next(0, 25);
            } while (trueCryptDriveLetter.Contains(driveToMount));

            return driveToMount;
        }

        public static string GetDriveLetterOfMountedDrive() {
            // Ascii of A is 65. 
            // Whatever the integer number is, increment it by 64 and we would get the drive path;
            // However it seems there is an extra offset of 1 inside TrueCrypt driver
            // So lets keep it 65 for the moment

            const int asciiOffSet = 65;
            int asciiDec = mountedDrivePath + asciiOffSet;
            string driveLetter = Convert.ToChar(asciiDec).ToString();
            driveLetter = driveLetter + ":\\";
            return driveLetter;
        }


    }
}
