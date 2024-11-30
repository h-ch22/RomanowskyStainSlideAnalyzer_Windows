using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
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
        [DllImport("Kernel32.dll", CharSet = CharSet.Auto)]
        public static extern bool TerminateProcess(IntPtr proc, uint uExit);

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

        private string _output = "";
        public string output
        {
            get { return _output; }
            set
            {
                _output += $"\n{value}";
                OnPropertyChanged(nameof(output));
            }
        }

        private string[] features = ["Microsoft-Hyper-V-All", "Microsoft-Windows-Subsystem-Linux", "VirtualMachinePlatform"];

        public event PropertyChangedEventHandler PropertyChanged;
        public bool IsRunning { get; set; }

        private bool AutoScroll = true;

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
            Dispatcher.Invoke(() => { progress += value; });
        }

        private void updateStatus(string newStatus)
        {
            Dispatcher.Invoke(() => { status = newStatus; });
        }

        private void getPackages()
        {
            updateStatus("Collecting Packages...");
            updateProgress();

            foreach(var feature in features)
            {
                updateStatus($"Activating Package {feature}");
                output = $"****** Output for {feature} ******";

                var result = runCommand("cmd.exe", $"/C ECHO N | powershell Enable-WindowsOptionalFeature -Online -FeatureName {feature}");

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

        private void ScrollViewer_ScrollChanged(Object sender, ScrollChangedEventArgs e)
        {
            if (e.ExtentHeightChange == 0)
            {  
                if (scrollViewer.VerticalOffset == scrollViewer.ScrollableHeight)
                {   
                    AutoScroll = true;
                }
                else
                {   
                    AutoScroll = false;
                }
            }

            if (AutoScroll && e.ExtentHeightChange != 0)
            {   
                scrollViewer.ScrollToVerticalOffset(scrollViewer.ExtentHeight);
            }
        }

        private bool runCommand(string command, string arguments)
        {
            try
            {
                using(var process = new Process())
                {
                    process.StartInfo.FileName = command;
                    process.StartInfo.Arguments = $"-Command \"[Console]::OutputEncoding = [System.Text.Encoding]::UTF8; {arguments}\"";
                    process.StartInfo.RedirectStandardError = true;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardInput = true;
                    process.StartInfo.CreateNoWindow = true;
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.StandardOutputEncoding = Encoding.UTF8;
                    process.StartInfo.StandardErrorEncoding = Encoding.UTF8;

                    process.OutputDataReceived += (sender, args) =>
                    {
                        if (!string.IsNullOrWhiteSpace(args.Data))
                        {
                            Dispatcher.Invoke(() => OnStandardTextReceived(args.Data));
                        }
                    };

                    process.ErrorDataReceived += (sender, args) =>
                    {
                        if (!string.IsNullOrWhiteSpace(args.Data))
                        {
                            Dispatcher.Invoke(() => OnErrorTextReceived(args.Data));
                        }
                    };

                    process.Start();

                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    process.WaitForExit();

                    return process.ExitCode == 0;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return false;
            }
        }

        protected virtual void OnStandardTextReceived(string message)
        {
            output = message;
        }

        protected virtual void OnErrorTextReceived(string message)
        {
            output = $"Error: {message}";
        }
    }
}