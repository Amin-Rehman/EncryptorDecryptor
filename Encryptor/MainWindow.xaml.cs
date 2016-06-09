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

namespace Encryptor
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ObservableCollection<Files> files;

        public MainWindow()
        {
            InitializeComponent();

            files = new ObservableCollection<Files>()
            {
                new Files(){Name="Sample File 1",Address="Some sample path 1"},
                new Files(){Name="Sample File 2",Address="Some sample path 2"},
                new Files(){Name="Sample File 3",Address="Some sample path 3"}
            };

            filesInDirListView.ItemsSource = files;
        }
    }
}
