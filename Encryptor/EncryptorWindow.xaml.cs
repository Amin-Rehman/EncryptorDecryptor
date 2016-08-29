using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;
using SharedProject;
using System.IO;
using System.ComponentModel;
using IMAPI2.Interop;
using IMAPI2.MediaItem;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.InteropServices;

namespace Encryptor
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private BackgroundWorker bwEncryptionWork = new BackgroundWorker();
        private BackgroundWorker backgroundBurnWorker = new BackgroundWorker();

        private WaitForm waitForm;

        // List of all drives
        private const string ClientName = "BurnMedia";

        private List<MsftDiscRecorder2> listOfDrives;

        private BurnData _burnData = new BurnData();

        public MainWindow()
        {
            InitializeComponent();


            listOfDrives = new List<MsftDiscRecorder2>();

            // Background worer to start bootstrapping
            bwEncryptionWork.DoWork += new DoWorkEventHandler(bw_MountDoWork);
            bwEncryptionWork.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw_RunWorkerMountCompleted);

            // Background burn worker - To be called once the bootstrapping is completed
            backgroundBurnWorker.WorkerReportsProgress = true;
            backgroundBurnWorker.WorkerSupportsCancellation = true;
            backgroundBurnWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundBurnWorker_DoWork);
            backgroundBurnWorker.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.backgroundBurnWorker_ProgressChanged);
            backgroundBurnWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundBurnWorker_RunWorkerCompleted);

            filesInDirListView.SelectionMode = System.Windows.Controls.SelectionMode.Multiple;
            evaluateWritableDrives();
        }

        private void disableAllControls()
        {
            filesInDirListView.IsEnabled = false;
            filesToBeBurnedListView.IsEnabled = false;
            addFilesButton.IsEnabled = false;
            openCaseFolderButton.IsEnabled = false;
        }

        private void enableAllControls()
        {
            filesInDirListView.IsEnabled = true;
            filesToBeBurnedListView.IsEnabled = true;
            addFilesButton.IsEnabled = true;
            openCaseFolderButton.IsEnabled = true;
        }

        private void openCaseFolderButton_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog;
            folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.ShowNewFolderButton = false;
            folderBrowserDialog.RootFolder = Environment.SpecialFolder.MyComputer;
            DialogResult result = folderBrowserDialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                filesInDirListView.ItemsSource =
                    VBDirectoryEnumerator.GetFilesFromDirectory(folderBrowserDialog.SelectedPath);
            }
        }

        private void addFilesButton_Click(object sender, RoutedEventArgs e)
        {
            filesToBeBurnedListView.Items.Clear();

            // Iterate through all the items in the list view and add to the new list view
            foreach (var item in filesInDirListView.SelectedItems)
            {
                try
                {
                    if (filesToBeBurnedListView.Items.Add(item) == -1)
                    {
                        System.Windows.MessageBox.Show("Error adding items to the burn list");
                        return;
                    }
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show("Error adding items to the burn list : " + ex.Message);
                    return;
                }
            }

            // Enable or disable the encrypt button
            if (filesToBeBurnedListView.Items.Count > 0)
                encryptButton.IsEnabled = true;
            else
                encryptButton.IsEnabled = false;
        }

        private void encryptButton_Click(object sender, RoutedEventArgs e)
        {
            if (driveComboBox.SelectedItem == null)
            {
                System.Windows.MessageBox.Show(" No drive detected / inserted. Please select a drive to encrypt to before continuing. ","Error");
                return;
            }

            if ( !FileUtility.isContainerSizeEnough(filesToBeBurnedListView.Items.OfType<VBFile>().ToArray()))
            {
                System.Windows.MessageBox.Show("Encryped folder is not big enough.", "Error");
                return;
            }

            if (System.Windows.MessageBox.Show("Any open instances of Windows Explorer will have to close. Do you wish to continue?",
                "Closing Windows Explorer",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {

                PopupForm popup = new PopupForm();
                DialogResult dialogresult = popup.ShowDialog();
                string enteredPassword = "";

                if (dialogresult == System.Windows.Forms.DialogResult.OK)
                {
                    enteredPassword = popup.password;
                    popup.Dispose();
                }
                else if (dialogresult == System.Windows.Forms.DialogResult.Cancel)
                {
                    popup.Dispose();
                    return;
                }


                waitForm = new WaitForm();

                if (bwEncryptionWork.IsBusy != true)
                {
                    // Copy files to the drive
                    var selectedDestDrive = driveComboBox.Text;
                    object[] parameters = new object[] { enteredPassword , selectedDestDrive};
                    bwEncryptionWork.RunWorkerAsync(parameters);
                    disableAllControls();
                    waitForm.ShowDialog();
                }

            }
            else
            {
                return;
            }

        }

        #region Bootstrapping Background workers 

        private void bw_MountDoWork(object sender, DoWorkEventArgs e)
        {
            object[] parameters = e.Argument as object[];
            string password = parameters[0].ToString();
            string destinationDrive = parameters[1].ToString();

            SharedProject.TrueCryptHelper.CloseAllExplorers();

            string pathToContainer = FileUtility.getPathToContainer();
            TrueCryptHelper.MountContainer("vb", pathToContainer);


            string driveLetter = TrueCryptHelper.GetDriveLetterOfMountedDrive();
            FileUtility.deleteAllFilesFromFolder(driveLetter);

            VBFile[] listItems = filesToBeBurnedListView.Items.OfType<VBFile>().ToArray();
            // Copy files here
            SharedProject.FileCopier.CopyFiles(listItems, driveLetter);

            TrueCryptHelper.UnMountContainer();

            bootStrapEncryption(password);


        }

        private void bw_RunWorkerMountCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            waitForm.Hide();

            System.Windows.MessageBox.Show("Encryption complete!");
            SharedProject.TrueCryptHelper.pathToTrueCryptContainer = "";
            enableAllControls();

            burnToDiscButton.IsEnabled = true;


        }
        #endregion

        #region DVD Burning Background workers
        private void backgroundBurnWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            MsftDiscRecorder2 discRecorder = null;
            MsftDiscFormat2Data discFormatData = null;

            try
            {
                //
                // Create and initialize the IDiscRecorder2 object
                //
                discRecorder = new MsftDiscRecorder2();
                var burnData = (BurnData)e.Argument;
                discRecorder.InitializeDiscRecorder(burnData.uniqueRecorderId);

                //
                // Create and initialize the IDiscFormat2Data
                //
                discFormatData = new MsftDiscFormat2Data
                {
                    Recorder = discRecorder,
                    ClientName = ClientName,
                    ForceMediaToBeClosed = true
                };

                //
                // Set the verification level
                //
                var burnVerification = (IBurnVerification)discFormatData;
                burnVerification.BurnVerificationLevel = IMAPI_BURN_VERIFICATION_LEVEL.IMAPI_BURN_VERIFICATION_NONE;

                //
                // Check if media is blank, (for RW media)
                //
                object[] multisessionInterfaces = null;
                if (!discFormatData.MediaHeuristicallyBlank)
                {
                    multisessionInterfaces = discFormatData.MultisessionInterfaces;
                }

                //
                // Create the file system
                //
                IStream fileSystem;
                if (!CreateMediaFileSystem(discRecorder, multisessionInterfaces, out fileSystem))
                {
                    e.Result = -1;
                    return;
                }

                //
                // add the Update event handler
                //
                discFormatData.Update += discFormatData_Update;

                //
                // Write the data here
                //
                try
                {
                    discFormatData.Write(fileSystem);
                    e.Result = 0;
                }
                catch (COMException ex)
                {
                    e.Result = ex.ErrorCode;
                    System.Windows.Forms.MessageBox.Show(ex.Message, "IDiscFormat2Data.Write failed",
                        MessageBoxButtons.OK, MessageBoxIcon.Stop);
                }
                finally
                {
                    if (fileSystem != null)
                    {
                        Marshal.FinalReleaseComObject(fileSystem);
                    }
                }

                //
                // remove the Update event handler
                //
                discFormatData.Update -= discFormatData_Update;

                if (true)
                {
                    discRecorder.EjectMedia();
                }
            }
            catch (COMException exception)
            {
                //
                // If anything happens during the format, show the message
                //
                System.Windows.Forms.MessageBox.Show(exception.Message);
                e.Result = exception.ErrorCode;
            }
            finally
            {
                if (discRecorder != null)
                {
                    Marshal.ReleaseComObject(discRecorder);
                }

                if (discFormatData != null)
                {
                    Marshal.ReleaseComObject(discFormatData);
                }
            }
        }

        /// <summary>
        /// Event receives notification from the Burn thread of an event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void backgroundBurnWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            //int percent = e.ProgressPercentage;
            var burnData = (BurnData)e.UserState;

            if (burnData.task == BURN_MEDIA_TASK.BURN_MEDIA_TASK_FILE_SYSTEM)
            {
                labelStatusText.Content = burnData.statusMessage;
            }
            else if (burnData.task == BURN_MEDIA_TASK.BURN_MEDIA_TASK_WRITING)
            {
                switch (burnData.currentAction)
                {
                    case IMAPI_FORMAT2_DATA_WRITE_ACTION.IMAPI_FORMAT2_DATA_WRITE_ACTION_VALIDATING_MEDIA:
                        labelStatusText.Content = "Validating current media...";
                        break;

                    case IMAPI_FORMAT2_DATA_WRITE_ACTION.IMAPI_FORMAT2_DATA_WRITE_ACTION_FORMATTING_MEDIA:
                        labelStatusText.Content = "Formatting media...";
                        break;

                    case IMAPI_FORMAT2_DATA_WRITE_ACTION.IMAPI_FORMAT2_DATA_WRITE_ACTION_INITIALIZING_HARDWARE:
                        labelStatusText.Content = "Initializing hardware...";
                        break;

                    case IMAPI_FORMAT2_DATA_WRITE_ACTION.IMAPI_FORMAT2_DATA_WRITE_ACTION_CALIBRATING_POWER:
                        labelStatusText.Content = "Optimizing laser intensity...";
                        break;

                    case IMAPI_FORMAT2_DATA_WRITE_ACTION.IMAPI_FORMAT2_DATA_WRITE_ACTION_WRITING_DATA:
                        long writtenSectors = burnData.lastWrittenLba - burnData.startLba;

                        if (writtenSectors > 0 && burnData.sectorCount > 0)
                        {
                            var percent = (int)((100 * writtenSectors) / burnData.sectorCount);
                            labelStatusText.Content = string.Format("Progress: {0}%", percent);
                            //statusProgressBar.Value = percent;
                        }
                        else
                        {
                            labelStatusText.Content = "Progress 0%";
                            //statusProgressBar.Value = 0;
                        }
                        break;

                    case IMAPI_FORMAT2_DATA_WRITE_ACTION.IMAPI_FORMAT2_DATA_WRITE_ACTION_FINALIZATION:
                        labelStatusText.Content = "Finalizing writing...";
                        break;

                    case IMAPI_FORMAT2_DATA_WRITE_ACTION.IMAPI_FORMAT2_DATA_WRITE_ACTION_COMPLETED:
                        labelStatusText.Content = "Ready";
                        break;

                    case IMAPI_FORMAT2_DATA_WRITE_ACTION.IMAPI_FORMAT2_DATA_WRITE_ACTION_VERIFYING:
                        labelStatusText.Content = "Verifying";
                        break;
                }
            }
        }


        private void backgroundBurnWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            burnToDiscButton.IsEnabled = false;
            encryptButton.IsEnabled = true;
            enableAllControls();
            System.Windows.MessageBox.Show("Disc Burning complete!");
        }

        #endregion

        #region DVD Writing Helper methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="progress"></param>
        void discFormatData_Update([In, MarshalAs(UnmanagedType.IDispatch)] object sender, [In, MarshalAs(UnmanagedType.IDispatch)] object progress)
        {
            //
            // Check if we've cancelled
            //
            if (backgroundBurnWorker.CancellationPending)
            {
                var format2Data = (IDiscFormat2Data)sender;
                format2Data.CancelWrite();
                return;
            }

            var eventArgs = (IDiscFormat2DataEventArgs)progress;

            _burnData.task = BURN_MEDIA_TASK.BURN_MEDIA_TASK_WRITING;

            // IDiscFormat2DataEventArgs Interface
            _burnData.elapsedTime = eventArgs.ElapsedTime;
            _burnData.remainingTime = eventArgs.RemainingTime;
            _burnData.totalTime = eventArgs.TotalTime;

            // IWriteEngine2EventArgs Interface
            _burnData.currentAction = eventArgs.CurrentAction;
            _burnData.startLba = eventArgs.StartLba;
            _burnData.sectorCount = eventArgs.SectorCount;
            _burnData.lastReadLba = eventArgs.LastReadLba;
            _burnData.lastWrittenLba = eventArgs.LastWrittenLba;
            _burnData.totalSystemBuffer = eventArgs.TotalSystemBuffer;
            _burnData.usedSystemBuffer = eventArgs.UsedSystemBuffer;
            _burnData.freeSystemBuffer = eventArgs.FreeSystemBuffer;

            //
            // Report back to the UI
            //
            backgroundBurnWorker.ReportProgress(0, _burnData);
        }

        private bool CreateMediaFileSystem(IDiscRecorder2 discRecorder, object[] multisessionInterfaces, out IStream dataStream)
        {
            MsftFileSystemImage fileSystemImage = null;
            try
            {
                fileSystemImage = new MsftFileSystemImage();
                fileSystemImage.ChooseImageDefaults(discRecorder);
                fileSystemImage.FileSystemsToCreate =
                    FsiFileSystems.FsiFileSystemJoliet | FsiFileSystems.FsiFileSystemISO9660;

                DateTime dt = DateTime.Now;
                String timeString = String.Format("{0:d_M_yyyy_HH_mm}", dt);


                fileSystemImage.VolumeName = timeString;

                fileSystemImage.Update += fileSystemImage_Update;

                //
                // If multisessions, then import previous sessions
                //
                if (multisessionInterfaces != null)
                {
                    fileSystemImage.MultisessionInterfaces = multisessionInterfaces;
                    fileSystemImage.ImportFileSystem();
                }

                //
                // Get the image root
                //
                IFsiDirectoryItem rootItem = fileSystemImage.Root;

                //
                // Add Files and Directories to File System Image
                var listFiles = getListOfFiles();

                foreach (IMediaItem mediaItem in listFiles)
                {
                    //
                    // Check if we've cancelled
                    //
                    if (backgroundBurnWorker.CancellationPending)
                    {
                        break;
                    }

                    //
                    // Add to File System
                    //
                    mediaItem.AddToFileSystem(rootItem);
                }

                fileSystemImage.Update -= fileSystemImage_Update;

                //
                // did we cancel?
                //
                if (backgroundBurnWorker.CancellationPending)
                {
                    dataStream = null;
                    return false;
                }

                dataStream = fileSystemImage.CreateResultImage().ImageStream;
            }
            catch (COMException exception)
            {
                Console.WriteLine(exception.ToString());
                dataStream = null;
                return false;
            }
            finally
            {
                if (fileSystemImage != null)
                {
                    Marshal.ReleaseComObject(fileSystemImage);
                }
            }

            return true;
        }

        /// <summary>
        /// Event Handler for File System Progress Updates
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="currentFile"></param>
        /// <param name="copiedSectors"></param>
        /// <param name="totalSectors"></param>
        void fileSystemImage_Update([In, MarshalAs(UnmanagedType.IDispatch)] object sender,
            [In, MarshalAs(UnmanagedType.BStr)]string currentFile, [In] int copiedSectors, [In] int totalSectors)
        {
            var percentProgress = 0;
            if (copiedSectors > 0 && totalSectors > 0)
            {
                percentProgress = (copiedSectors * 100) / totalSectors;
            }

            if (!string.IsNullOrEmpty(currentFile))
            {
                var fileInfo = new FileInfo(currentFile);
                _burnData.statusMessage = "Adding \"" + fileInfo.Name + "\" to image...";

                //
                // report back to the ui
                //
                _burnData.task = BURN_MEDIA_TASK.BURN_MEDIA_TASK_FILE_SYSTEM;
                backgroundBurnWorker.ReportProgress(percentProgress, _burnData);
            }

        }

        #endregion


        private void bootStrapEncryption(string password)
        {
            try
            {

                // Step 1: Generate Key1 (Password XOR GUID)
                string guid = GUIDGenerator.getGuid();

                string keyOne = XOR.XORStrings(guid, password);

                // Step 2: Generate Key2 (md5 XOR GUID)
                string fileName = SharedProject.TrueCryptHelper.pathToTrueCryptContainer;
                string md5 = MD5Generator.GetMD5HashFromFile(fileName);
                string keyTwo = XOR.XORStrings(md5, guid);

                keyOne = KeyNormalizer.ToHex(keyOne);
                keyTwo = KeyNormalizer.ToHex(keyTwo);

                string jsonFilePath = Directory.GetCurrentDirectory() + "\\settings.json";

                KeyObject keyObj = new KeyObject { key1 = keyOne, key2 = keyTwo };

                JSONFactory.writeJSONFile(jsonFilePath, keyObj);

            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show("Error bootstraping encryption: "+e.Message);
            }

        }

        private List<FileItem> getListOfFiles()
        {
            List<FileItem> listOfFilesToBeCopied = new List<FileItem>();
            string fileToBeCopied;

            fileToBeCopied = FileUtility.nameOfContainer();
            listOfFilesToBeCopied.Add(new FileItem(Directory.GetCurrentDirectory() +"\\"+ fileToBeCopied));

            fileToBeCopied = "Crypto.dll";
            listOfFilesToBeCopied.Add(new FileItem(Directory.GetCurrentDirectory() + "\\" + fileToBeCopied));

            fileToBeCopied = "Decryptor.exe";
            listOfFilesToBeCopied.Add(new FileItem(Directory.GetCurrentDirectory() + "\\" + fileToBeCopied));

            fileToBeCopied = "Newtonsoft.Json.dll";
            listOfFilesToBeCopied.Add(new FileItem(Directory.GetCurrentDirectory() + "\\" + fileToBeCopied));

            fileToBeCopied = "Decryptor.exe";
            listOfFilesToBeCopied.Add(new FileItem(Directory.GetCurrentDirectory() + "\\" + fileToBeCopied));

            fileToBeCopied = "settings.json";
            listOfFilesToBeCopied.Add(new FileItem(Directory.GetCurrentDirectory() + "\\" + fileToBeCopied));

            fileToBeCopied = "truecrypt-x64.sys";
            listOfFilesToBeCopied.Add(new FileItem(Directory.GetCurrentDirectory() + "\\" + fileToBeCopied));

            fileToBeCopied = "truecrypt.sys";
            listOfFilesToBeCopied.Add(new FileItem(Directory.GetCurrentDirectory() + "\\" + fileToBeCopied));

            return listOfFilesToBeCopied;

        }

        private void RefreshDrives_Click(object sender, RoutedEventArgs e)
        {
            evaluateWritableDrives();
        }

        public void evaluateWritableDrives()
        {
            driveComboBox.Items.Clear();
            listOfDrives.Clear();

            var discMaster = new MsftDiscMaster2();

            if (!discMaster.IsSupportedEnvironment)
                return;
            foreach (string uniqueRecorderId in discMaster)
            {
                var discRecorder2 = new MsftDiscRecorder2();
                discRecorder2.InitializeDiscRecorder(uniqueRecorderId);

                string devicePaths = string.Empty;
                string volumePath = (string)discRecorder2.VolumePathNames.GetValue(0);
                foreach (string volPath in discRecorder2.VolumePathNames)
                {
                    if (!string.IsNullOrEmpty(devicePaths))
                    {
                        devicePaths += ",";
                    }
                    devicePaths += volumePath;
                }

                driveComboBox.Items.Add(string.Format("{0}", devicePaths));
                listOfDrives.Add(discRecorder2);

            }


            if (driveComboBox.HasItems)
            {
                driveComboBox.SelectedIndex = 0;
                driveComboBox.IsEnabled = true;
            }
            else
            {
                driveComboBox.IsEnabled = false;
            }

        }

        private void driveComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void burnToDisc_button_Click(object sender, RoutedEventArgs e)
        {
            encryptButton.IsEnabled = false;
            burnToDiscButton.IsEnabled = false;
            disableAllControls();

            var selectedDrive = driveComboBox.SelectedIndex;
            var discRecorder = (IDiscRecorder2) listOfDrives.ElementAt<MsftDiscRecorder2>(selectedDrive);

            _burnData.uniqueRecorderId = discRecorder.ActiveDiscRecorder;
            backgroundBurnWorker.RunWorkerAsync(_burnData);

        }
    }

}

