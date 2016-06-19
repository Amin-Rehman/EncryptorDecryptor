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

namespace Decryptor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
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

        private async void bootStrapDecryption(string password)
        {
            try
            {
                string jsonFilePath = Directory.GetCurrentDirectory() + "\\settings.json";
                KeyObject keyObj = JSONFactory.readJSONFile(jsonFilePath);
                string containerFileName = Directory.GetCurrentDirectory() + "\\TCRYPT";
                string md5 = await MD5Generator.GetMD5HashFromFile(containerFileName);
                string key2 = KeyNormalizer.ToAscii(keyObj.key2);
                string guid = XOR.XORStrings(key2, md5);
                string key1 = KeyNormalizer.ToAscii(keyObj.key1);

                string evaluatedPassword = XOR.XORStrings(key1, guid);

                if (evaluatedPassword == password)
                {
                    System.Windows.MessageBox.Show("Access granted!", "Decryptor", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    System.Windows.MessageBox.Show("Access denied!", "Decryptor", MessageBoxButton.OK, MessageBoxImage.Hand);
                }
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show("Error bootstrapping decryptor:"+e.Message,"Error");
            }
        }
    }
}
