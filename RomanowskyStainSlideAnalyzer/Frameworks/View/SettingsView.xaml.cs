using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using RomanowskyStainSlideAnalyzer.Frameworks.Helper;
using RomanowskyStainSlideAnalyzer.Frameworks.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using Windows.Storage;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace RomanowskyStainSlideAnalyzer.Frameworks.View
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsView : Page, INotifyPropertyChanged
    {
        private string _AppName = $"Romanowsky Stain Slide Analyzer 1.0.0.0";
        public string AppName
        {
            get => _AppName;
            set
            {
                _AppName = $"Romanowsky Stain Slide Analyzer {value}";
                OnPropertyChanged(nameof(AppName));
            }
        }

        private string _IsEnvironmentSet = "Not Set";
        public string IsEnvironmentSet
        {
            get => _IsEnvironmentSet;
            set
            {
                _IsEnvironmentSet = value;
                OnPropertyChanged(nameof(IsEnvironmentSet));
            }
        }

        private Symbol _EnvironmentSymbol = Symbol.Cancel;
        public Symbol EnvironmentSymbol
        {
            get => _EnvironmentSymbol;
            set
            {
                _EnvironmentSymbol = value;
                OnPropertyChanged(nameof(EnvironmentSymbol));
            }
        }

        private Visibility _ShowProgress = Visibility.Collapsed;
        public Visibility ShowProgress
        {
            get => _ShowProgress;
            set
            {
                _ShowProgress = value;
                OnPropertyChanged(nameof(ShowProgress));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private EnvironmentHelper helper = new();
        public ObservableCollection<EnvironmentDataModel> Datas
        {
            get; private set;
        }

        private Dictionary<int, string> Titles = new()
        {
            { 0, "WSL Status" },
            { 1, "Windows Additional Features Status" },
            { 2, "Feature Activator Status" },
            { 3, "Linux Status" },
            { 4, "Essential Packages Status" },
            { 5, "Project Status" },
            { 6, "Python Packages Status" },
            { 7, "Entry Point Status" }
        };

        public SettingsView()
        {
            this.InitializeComponent();
            DataContext = this;

            AppName = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            Init();
        }

        private void Init()
        {
            Datas = new();

            foreach (var title in Titles)
            {
                Datas.Add(new EnvironmentDataModel(title.Key, title.Value, helper.GetStatus(title.Value)));
            }

            IsEnvironmentSet = helper.GetFinalStatus() ? "Set" : "Not Set";
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

            if(propertyName == nameof(IsEnvironmentSet))
            {
                SolidColorBrush brush;

                switch(IsEnvironmentSet)
                {
                    case "Set":
                        EnvironmentSymbol = Symbol.Accept;
                        break;

                    default:
                        EnvironmentSymbol = Symbol.Cancel;
                        break;
                }
            }
        }

        private void Button_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            switch((sender as Button).Name)
            {
                case "btn_clearHistory":
                    DirectoryInfo di = new DirectoryInfo(@"C:\RomanowskyStainSlideAnalyzer");

                    foreach (FileInfo file in di.GetFiles())
                    {
                        file.Delete();
                    }
                    foreach (DirectoryInfo dir in di.GetDirectories())
                    {
                        dir.Delete(true);
                    }

                    ShowAlert("Done", "All history has been removed.");

                    break;
            }
        }

        private void Refresh(object sender, RoutedEventArgs e)
        {
            EnvironmentDataModel dataModel = (sender as HyperlinkButton).DataContext as EnvironmentDataModel;

            DispatcherQueue.TryEnqueue(async () =>
            {
                EnvironmentExpander.Visibility = Visibility.Collapsed;
                ShowProgress = Visibility.Visible;
                dataModel.ProgressBarVisibility = Visibility.Visible;
            });

            Thread thread;

            switch(dataModel.id)
            {
                case 0:
                case 1:
                    thread = new Thread(delegate ()
                    {
                        var wslResult = helper.GetWSLStatus();

                        DispatcherQueue.TryEnqueue(async () =>
                        {
                            dataModel.IsActivated = helper.GetStatus(dataModel.Title);
                        });
                    });

                    thread.Start();

                    break;

                case 2:
                    thread = new Thread(delegate ()
                    {
                        var featureActivatorStatus = helper.GetFeatureActivatorInstalledStatus();

                        if(!featureActivatorStatus)
                        {
                            helper.InstallFeatureActivator();
                        }

                        DispatcherQueue.TryEnqueue(async () =>
                        {
                            dataModel.IsActivated = helper.GetStatus(dataModel.Title);
                        });
                    });

                    thread.Start();

                    break;

                case 3:
                    thread = new Thread(delegate ()
                    {
                        var linuxStatus = helper.GetInstalledLinux();

                        DispatcherQueue.TryEnqueue(async () =>
                        {
                            dataModel.IsActivated = helper.GetStatus(dataModel.Title);
                        });
                    });

                    thread.Start();

                    break;

                case 4:
                    thread = new Thread(delegate ()
                    {
                        var packagesStatus = helper.InstallEssentialPackages();

                        DispatcherQueue.TryEnqueue(async () =>
                        {
                            dataModel.IsActivated = helper.GetStatus(dataModel.Title);
                        });
                    });

                    thread.Start();

                    break;

                case 5:
                    thread = new Thread(delegate ()
                    {
                        var projectStatus = helper.DownloadSAMProject();

                        DispatcherQueue.TryEnqueue(async () =>
                        {
                            dataModel.IsActivated = helper.GetStatus(dataModel.Title);
                        });
                    });

                    thread.Start();

                    break;

                case 6:
                    thread = new Thread(delegate ()
                    {
                        var venvStatus = helper.CreateVirtualEnv();
                        var pythonPackagesStatus = helper.InstallPythonPackages();

                        DispatcherQueue.TryEnqueue(async () =>
                        {
                            dataModel.IsActivated = helper.GetStatus(dataModel.Title);
                        });
                    });

                    thread.Start();

                    break;

                case 7:
                    thread = new Thread(delegate ()
                    {
                        var projectStatus = helper.CopyEntryPoint();

                        DispatcherQueue.TryEnqueue(async () =>
                        {
                            dataModel.IsActivated = helper.GetStatus(dataModel.Title);
                        });
                    });

                    thread.Start();

                    break;
            }

            DispatcherQueue.TryEnqueue(async () =>
            {
                ShowProgress = Visibility.Collapsed;
                EnvironmentExpander.Visibility = Visibility.Visible;
                dataModel.ProgressBarVisibility = Visibility.Collapsed;
                IsEnvironmentSet = helper.GetFinalStatus() ? "Set" : "Not Set";
                Init();
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
    }
}
