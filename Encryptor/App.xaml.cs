using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using SharedProject;

namespace Encryptor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            SharedProject.TrueCryptHelper.StartDeviceDriver();
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            SharedProject.TrueCryptHelper.UnMountContainer();
            SharedProject.TrueCryptHelper.StopDeviceDriver();
        }
    }
}
