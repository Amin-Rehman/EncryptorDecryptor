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

namespace Encryptor
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private BackgroundWorker bwEncryptionWork = new BackgroundWorker();

        private WaitForm waitForm;

        public MainWindow()
        {
            InitializeComponent();

            bwEncryptionWork.DoWork += new DoWorkEventHandler(bw_MountDoWork);
            bwEncryptionWork.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw_RunWorkerMountCompleted);


            filesInDirListView.SelectionMode = System.Windows.Controls.SelectionMode.Multiple;
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
                    object[] parameters = new object[] { enteredPassword };
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

        // Background workers 

        private void bw_MountDoWork(object sender, DoWorkEventArgs e)
        {
            object[] parameters = e.Argument as object[];
            string password = parameters[0].ToString();

            SharedProject.TrueCryptHelper.CloseAllExplorers();

            string pathToContainer = Directory.GetCurrentDirectory() + "\\TCRYPT";
            TrueCryptHelper.MountContainer("vb", pathToContainer);

            string driveLetter = TrueCryptHelper.GetDriveLetterOfMountedDrive();
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
        }

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

    }

}

