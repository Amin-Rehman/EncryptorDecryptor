using System;
using System.Collections.Generic;
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
using SharedProject;
using System.IO;
using System.Windows.Forms;
using System.Diagnostics;
using System.ComponentModel;

namespace Decryptor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private BackgroundWorker bwDecryptionWork = new BackgroundWorker();

        private BackgroundWorker bwShutdownWork = new BackgroundWorker();

        private PleaseWaitForm waitForm;

        private PleaseWaitForm waitShutdownForm;

        public MainWindow()
        {
            InitializeComponent();
            bwDecryptionWork.DoWork += new DoWorkEventHandler(bw_DoWork);
            bwDecryptionWork.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw_RunWorkerMountCompleted);

            bwShutdownWork.DoWork += new DoWorkEventHandler(bw_Shutdown_DoWork);
            bwShutdownWork.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw_Shutdown_Completed);


        }

        private void decryptButton_Click(object sender, RoutedEventArgs e)
        {
            PasswordEntryForm popup = new PasswordEntryForm();
            DialogResult dialogresult = popup.ShowDialog();

            if (dialogresult == System.Windows.Forms.DialogResult.OK)
            {
                string enteredPassword = popup.password;
                popup.Dispose();

                bootStrapDecryption(enteredPassword);
            }
            else if (dialogresult == System.Windows.Forms.DialogResult.Cancel)
            {
                popup.Dispose();
                return;
            }
        }

        private void bootStrapDecryption(string password)
        {
            try
            {
                waitForm = new PleaseWaitForm();
                object[] parameters = new object[] { password };
                bwDecryptionWork.RunWorkerAsync(parameters);
                waitForm.Show();
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show("Error bootstrapping decryptor:" + e.Message, "Error");
            }
        }

        private void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            object[] parameters = e.Argument as object[];
            string password = parameters[0].ToString();
            string jsonFilePath = Directory.GetCurrentDirectory() + "\\settings.json";
            KeyObject keyObj = JSONFactory.readJSONFile(jsonFilePath);
            string containerFileName = Directory.GetCurrentDirectory() + "\\TCRYPT";
            string md5 = MD5Generator.GetMD5HashFromFile(containerFileName);
            string key2 = KeyNormalizer.ToAscii(keyObj.key2);
            string guid = XOR.XORStrings(key2, md5);
            string key1 = KeyNormalizer.ToAscii(keyObj.key1);

            string evaluatedPassword = XOR.XORStrings(key1, guid);

            if (evaluatedPassword == password)
            {
                //System.Windows.MessageBox.Show("Access granted!", "Decryptor", MessageBoxButton.OK, MessageBoxImage.Information);

                string pathToContainer = Directory.GetCurrentDirectory() + "\\TCRYPT";

                SharedProject.TrueCryptHelper.MountContainer("vb", pathToContainer);
            }
            else
            {
                System.Windows.MessageBox.Show("Access denied!", "Decryptor", MessageBoxButton.OK, MessageBoxImage.Hand);
            }
        }

        private void bw_RunWorkerMountCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            waitForm.Hide();
            if (SharedProject.TrueCryptHelper.GetDriveLetterOfMountedDrive() !="")
            {
                string mountedDrive = SharedProject.TrueCryptHelper.GetDriveLetterOfMountedDrive();
                Process.Start(mountedDrive);
            }

        }


        private void bw_Shutdown_DoWork(object sender, DoWorkEventArgs e)
        {
            TrueCryptHelper.CloseAllExplorers();
            TrueCryptHelper.UnMountContainer();
            TrueCryptHelper.StopDeviceDriver();
        }
        private void bw_Shutdown_Completed(object sender, RunWorkerCompletedEventArgs e)
        {
            waitShutdownForm.Hide();
            Environment.Exit(0);
        }

        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            bwShutdownWork.RunWorkerAsync();
            waitShutdownForm = new PleaseWaitForm();
            waitShutdownForm.Show();
        }
    }
}
