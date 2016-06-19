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

namespace Encryptor
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();

            filesInDirListView.SelectionMode = System.Windows.Controls.SelectionMode.Multiple;
        }

        private void disableAllControls()
        {
            filesInDirListView.IsEnabled = false;
            filesToBeBurnedListView.IsEnabled = false;
            addFilesButton.IsEnabled = false;
            openCaseFolderButton.IsEnabled = false;
            encryptButton.IsEnabled = false;
        }

        private void enableAllControls()
        {
            filesInDirListView.IsEnabled = true;
            filesToBeBurnedListView.IsEnabled = true;
            addFilesButton.IsEnabled = true;
            openCaseFolderButton.IsEnabled = true;
            encryptButton.IsEnabled = true;
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
        }

        private void encryptButton_Click(object sender, RoutedEventArgs e)
        {
            PopupForm popup = new PopupForm();
            DialogResult dialogresult = popup.ShowDialog();

            if (dialogresult == System.Windows.Forms.DialogResult.OK)
            {
                string enteredPassword = popup.password;
                popup.Dispose();
                bootStrapEncryption(enteredPassword);
            }
            else if (dialogresult == System.Windows.Forms.DialogResult.Cancel)
            {
                popup.Dispose();
                return;
            }

        }

        private async void bootStrapEncryption(string password)
        {
            disableAllControls();

            // Step 1: Generate Key1 (Password XOR GUID)
            string guid = GUIDGenerator.getGuid();

            string keyOne = XOR.XORStrings(guid, password);

            // Step 2: Generate Key2 (md5 XOR GUID)
            string fileName = Directory.GetCurrentDirectory() + "\\TCRYPT";
            string md5 = await MD5Generator.GetMD5HashFromFile(fileName);
            string keyTwo = XOR.XORStrings(md5, guid);

            keyOne = KeyNormalizer.ToHex(keyOne);
            keyTwo = KeyNormalizer.ToHex(keyTwo);

            string jsonFilePath = Directory.GetCurrentDirectory() + "\\settings.json";

            KeyObject keyObj = new KeyObject { key1 = keyOne, key2 = keyTwo };

            JSONFactory.writeJSONFile(jsonFilePath, keyObj);


            System.Windows.MessageBox.Show("Encryption complete!");

            enableAllControls();

        }

    }

}

