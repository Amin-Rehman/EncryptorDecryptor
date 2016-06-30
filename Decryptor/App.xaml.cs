using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using SharedProject;

namespace Decryptor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            TrueCryptHelper.StartDeviceDriver();
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            TrueCryptHelper.StopDeviceDriver();
        }
    }
}
