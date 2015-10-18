using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace MizTagger
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        public ShitAllInOne Data;

        private void Application_Exit_1(object sender, ExitEventArgs e)
        {

        }

        private void Application_Startup_1(object sender, StartupEventArgs e)
        {
            Data = new ShitAllInOne(ShitAllInOne.ConnectionString);
        }
    }
}
