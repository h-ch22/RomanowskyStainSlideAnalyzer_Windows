using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using RomanowskyStainSlideAnalyzer.Frameworks.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace RomanowskyStainSlideAnalyzer.Home.View
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class HomeView : Page, INotifyPropertyChanged
    {
        private string _StatusText = "Initializing...";
        private bool isRebootRequired = false;
        private bool isError = false;

        public string StatusText
        {
            get => _StatusText;
            set {
                _StatusText = value;
                OnPropertyChanged(nameof(StatusText));
            }
        }

        private double _Progress = 0;
        public double Progress
        {
            get => _Progress;
            set
            {
                _Progress = value;
                OnPropertyChanged(nameof(Progress));
            }
        }

        private EnvironmentHelper helper = new();
        public event PropertyChangedEventHandler PropertyChanged;

        public HomeView()
        {
            InitializeComponent();
            DataContext = this;

            if(!helper.GetStatus("All Status"))
            {
                Thread thread = new Thread(checkEnvironment);
                thread.Start();
            }
            else
            {
                commandBar.IsEnabled = true;
                progressView.Visibility = Visibility.Collapsed;
                configurationView.Visibility = Visibility.Collapsed;
                imageView.Visibility = Visibility.Visible;
            }
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private async void checkEnvironment()
        {
            updateStatus("Checking WSL Status");

            var result = helper.GetWSLStatus();

            increaseProgress();

            if(!result)
            {
                if(!helper.isWSLActivated || !helper.isVirtualMachinePlatformActivated)
                {
                    updateStatus("Checking Feature Activator Installation Status");
                    var isActivatorInstalled = helper.GetFeatureActivatorInstalledStatus();
                    increaseProgress();

                    if (!isActivatorInstalled)
                    {
                        updateStatus("Installing Feature Activator");
                        var installResult = helper.InstallFeatureActivator();
                        increaseProgress();

                        if (!installResult)
                        {
                            showErrorMessage();
                            return;
                        }
                    }

                    updateStatus("Activating Features (This may take some time)");
                    var activateResult = helper.ActivateFeatures();

                    if (activateResult)
                    {
                        isRebootRequired = true;
                        increaseProgress();
                        showRebootMessageBox();
                        return;
                    }
                    else
                    {
                        showErrorMessage();
                        return;
                    }

                }
            }
            else {
                increaseProgress(30);
            }

            updateStatus("Checking Installed Linux");

            var isLinuxInstalled = helper.GetInstalledLinux();
            increaseProgress();

            if (!isLinuxInstalled)
            {
                updateStatus("Installing Ubuntu");
                var installationResult = helper.InstallUbuntu();

                if (!installationResult)
                {
                    showErrorMessage();
                    return;
                }
            }

            increaseProgress();
            updateStatus("Checking GPU");

            var isGPUInstalled = helper.GetGPU();

            if (!isGPUInstalled)
            {
                ShowAlert("No GPU Installed", "Windows cannot detect your GPU.\nIf you have a GPU installed, make sure that it is connected and that the GPU drivers are properly installed. If you do not have a GPU installed, segmentation speed may be very slow.\nSkipping GPU configuration.");
            }

            if (!helper.GetStatus("Essential Packages Status"))
            {
                updateStatus("Installing Essential Packages...");
                var packageInstallResult = helper.InstallEssentialPackages();

                if (!packageInstallResult)
                {
                    showErrorMessage();
                    return;
                }
            }

            increaseProgress();

            if (!helper.GetStatus("Project Status"))
            {
                updateStatus("Downloading SAM Project...");

                var SAMResult = helper.DownloadSAMProject();

                if (!SAMResult)
                {
                    showErrorMessage();
                    return;
                }
            }

            increaseProgress();

            if(!helper.GetStatus("Python Packages Status"))
            {
                updateStatus("Genearating Virtual Env...");

                var venvResult = helper.CreateVirtualEnv();

                if (!venvResult)
                {
                    showErrorMessage();
                    return;
                }

                increaseProgress();
                updateStatus("Installing Python Packages...");

                var packageResult = helper.InstallPythonPackages();

                if(!packageResult)
                {
                    showErrorMessage();
                    return;
                }
            }

            else
            {
                increaseProgress();
            }

            increaseProgress();

            if(!helper.GetStatus("All Status"))
            {
                updateStatus("Copying Romanowsky Stain Slide Analyzer for Python CLI...");
                var copyResult = helper.CopyEntryPoint();

                if (!copyResult)
                {
                    showErrorMessage();
                    return;
                }

            }

            DispatcherQueue.TryEnqueue(() =>
            {
                commandBar.IsEnabled = true;
                progressView.Visibility = Visibility.Collapsed;
                configurationView.Visibility = Visibility.Collapsed;
                imageView.Visibility = Visibility.Visible;
            });
        }

        private void showErrorMessage()
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                isError = true;

                ic_status.Visibility = Visibility.Collapsed;
                InitializeText.Visibility = Visibility.Collapsed;
                progressBar.Visibility = Visibility.Collapsed;
                txt_status.Visibility = Visibility.Collapsed;
                btn_reboot.Visibility = Visibility.Collapsed;

                ic_error.Visibility = Visibility.Visible;
                ErrorText.Visibility = Visibility.Visible;
            });
        }

        private void increaseProgress(double value = 10)
        {
            DispatcherQueue.TryEnqueue(() => Progress += value);
        }

        private void updateStatus(string status)
        {
            DispatcherQueue.TryEnqueue(() => StatusText = status);
        }

        private void showRebootMessageBox()
        {
            DispatcherQueue.TryEnqueue(async () =>
            {
                isRebootRequired = true;

                var contentDialog = new ContentDialog
                {
                    Title = "Reboot Required",
                    Content = "A new Windows feature has been added and requires a reboot to continue installing.",
                    PrimaryButtonText = "Reboot now",
                    SecondaryButtonText = "Reboot 1 hour later",
                    CloseButtonText = "Reboot Manually",
                    DefaultButton = ContentDialogButton.Primary,
                    XamlRoot = App.window.Content.XamlRoot
                };

                contentDialog.PrimaryButtonClick += (_s, _e) => { helper.reboot(); };
                contentDialog.SecondaryButtonClick += (_s, _e) => {
                    helper.reboot(3600);
                    ShowRebootScreen();
                };

                contentDialog.CloseButtonClick += (_s, _e) => {
                    ShowRebootScreen();
                };

                await contentDialog.ShowAsync();
            });
        }

        private void ShowAlert(string title, string message)
        {
            DispatcherQueue.TryEnqueue(async () =>
            {
                var contentDialog = new ContentDialog
                {
                    Title = title,
                    Content = message,
                    CloseButtonText = "OK",
                    DefaultButton = ContentDialogButton.Close,
                    XamlRoot = App.window.Content.XamlRoot
                };

                await contentDialog.ShowAsync();
            });
        }

        private void ShowRebootScreen()
        {
            InitializeText.Text = "Please reboot to continue";
            progressBar.Visibility = Visibility.Collapsed;
            txt_status.Visibility = Visibility.Collapsed;
            btn_reboot.Visibility = Visibility.Visible;
            ic_status.Symbol = Symbol.Refresh;
        }

        private void OnClick(object sender, RoutedEventArgs e) {
            if((sender as Button).Name == "btn_reboot")
            {
                helper.reboot();
            }
        }
    }
}
