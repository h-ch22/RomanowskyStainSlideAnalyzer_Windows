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

            Thread thread = new Thread(checkEnvironment);
            thread.Start();
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
                if(!helper.isWSLActivated || !helper.isVirtualMachinePlatformActivated || !helper.isHyperVActivated)
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
                        }
                    }

                    if(!isError)
                    {

                        updateStatus("Activating Features");
                        var activateResult = helper.ActivateFeatures();

                        if (activateResult)
                        {
                            isRebootRequired = true;
                            increaseProgress();
                            showRebootMessageBox();
                        }
                        else
                        {
                            showErrorMessage();
                        }
                    }

                }
            }
            else {
                increaseProgress(30);
            }

            if(!isRebootRequired && !isError)
            {
                updateStatus("Checking Installed Linux");

                var installedLinux = helper.GetInstalledLinux();
                increaseProgress();

                if (installedLinux == null)
                {

                }
                else if (installedLinux == "")
                {
                    updateStatus("Installing Linux (Ubuntu)...");
                    helper.InstallUbuntu();
                    increaseProgress();
                }
                else
                {
                    increaseProgress();
                }

                updateStatus("Installing Python, CUDA, cuDNN...");
            }
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
                };

                contentDialog.CloseButtonClick += (_s, _e) => {
                    InitializeText.Text = "Please reboot to continue";
                    progressBar.Visibility = Visibility.Collapsed;
                    txt_status.Visibility = Visibility.Collapsed;
                    btn_reboot.Visibility = Visibility.Visible;
                    ic_status.Symbol = Symbol.Refresh;
                };

                await contentDialog.ShowAsync();
            });
        }

        private void OnClick(object sender, RoutedEventArgs e) {
            if((sender as Button).Name == "btn_reboot")
            {
                helper.reboot();
            }
        }
    }
}
