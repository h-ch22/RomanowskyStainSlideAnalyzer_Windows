using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FeatureInstaller
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private double _progress = 0;
        public double progress
        {
            get { return _progress; }
            set
            {
                _progress = value;
                OnPropertyChanged(nameof(progress));
            }
        }

        private string _status = "Initializing...";
        public string status
        {
            get { return _status; }
            set
            {
                _status = value;
                OnPropertyChanged(nameof(status));
            }
        }

        private string[] features = ["Microsoft-Hyper-V-All", "Microsoft-Windows-Subsystem-Linux", "VirtualMachinePlaform"];
        private string error = "";

        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            Thread thread = new Thread(getPackages);
            thread.Start();
        }

        protected void OnPropertyChanged(string propertyName)
        { 
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));    
        }

        private void updateProgress(double value=25)
        {
            progress += value;
        }

        private void updateStatus(string newStatus)
        {
            status = newStatus;
        }

        private void getPackages()
        {
            updateStatus("Collecting Packages...");
            updateProgress();

            foreach(var feature in features)
            {
                Debug.WriteLine($"Installing {feature}");
                updateStatus($"Activating Package {feature}");

                var result = runCommand("powershell", $"dism /online /enable-feature /featurename:{feature}");

                if(!result)
                {
                    MessageBox.Show($"An error occurred while installing package {feature}.\nPlease check your network status or make sure that some of your Windows system files are not corrupted and try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    Environment.Exit(-1);
                }

                updateProgress();
            }

            MessageBox.Show("All packages have been installed successfully.\nContinue with the main application.");
            Environment.Exit(0);
        }

        private bool runCommand(string command, string arguments)
        {
            try
            {
                Process process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = command,
                        Arguments = arguments,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        RedirectStandardInput = true,
                        UseShellExecute = false,
                        CreateNoWindow = false
                    }
                };

                process.Start();

                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();

                Debug.WriteLine(output);
                Debug.WriteLine(error);

                if(error != "" || error != null)
                {
                    this.error = error;
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                error = ex.Message;
                return false;
            }
        }
    }
}