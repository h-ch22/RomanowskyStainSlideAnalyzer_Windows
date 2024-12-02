using System.Configuration;
using System.Data;
using System.Windows;

namespace FeatureInstaller
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static int ConfigType = 0;

        protected override void OnStartup(StartupEventArgs e)
        {
            if(e.Args.Length == 1)
            {

                if (e.Args[0] == "/Install-Windows-Additional-Features" || e.Args[0] == "/Install-Ubuntu")
                {
                    ConfigType = e.Args[0] == "/Install-Windows-Additional-Features" ? 0 : 1;
                    base.OnStartup(e);
                }
                else
                {
                    MessageBox.Show($"The software cannot be run due to unknown Argument {e.Args[0]}.", "Unknown Argument", MessageBoxButton.OK, MessageBoxImage.Error);
                    Environment.Exit(0);
                }
            }
            else
            {
                MessageBox.Show("This software requires an Argument to run.\nRun it using the main software, or run it again by manually entering the Argument (/Install-Windows-Additional-Features or /Install-Ubuntu).", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Environment.Exit(0);
            }
        }
    }

}
