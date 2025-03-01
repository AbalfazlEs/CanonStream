using AdjasmentCamera.View;
using AdjasmentCamera.ViewModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace AdjasmentCamera
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            AdjasmentCameraV win = new AdjasmentCameraV();          
            win.ShowDialog ();  
        }
    }
}
