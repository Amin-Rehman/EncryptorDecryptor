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
                    System.Windows.MessageBox.Show("Error adding items to the burn list : "+ ex.Message);
                    return;
                }
            }
        }


    }
}
