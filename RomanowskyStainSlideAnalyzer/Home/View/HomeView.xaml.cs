using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using RomanowskyStainSlideAnalyzer.Frameworks.Helper;
using RomanowskyStainSlideAnalyzer.Frameworks.View;
using RomanowskyStainSlideAnalyzer.History.Models;
using RomanowskyStainSlideAnalyzer.Home.Helper;
using RomanowskyStainSlideAnalyzer.Home.Models;
using RomanowskyStainSlideAnalyzer.Labeling.Helper;
using RomanowskyStainSlideAnalyzer.Labeling.Models;
using RomanowskyStainSlideAnalyzer.Labeling.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;

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
        private string _FileName = "output";
        public string FileName
        {
            get => _FileName;
            set
            {
                _FileName = value;
                OnPropertyChanged(nameof(FileName));
                SetFileName();
            }
        }

        private string _Extension = "png";
        public string Extension
        {
            get => _Extension;
            set
            {
                _Extension = value;
                OnPropertyChanged(nameof(Extension));
                SetFileName();
            }
        }

        private string _PlaceHolderText = "Postfix";

        public string PlaceHolderText
        {
            get => _PlaceHolderText;
            set
            {
                _PlaceHolderText = value;
                OnPropertyChanged(nameof(PlaceHolderText));
            }
        }

        private string _Device = "CPU";
        public string Device
        {
            get => _Device;
            set
            {
                _Device = value;
                OnPropertyChanged(nameof(Device));
            }
        }

        private string _HelpText = "";

        public string HelpText
        {
            get => _HelpText;
            set
            {
                _HelpText = value;
                OnPropertyChanged(nameof(HelpText));
            }
        }

        private bool _Indeterminate = false;

        public bool Indeterminate
        {
            get => _Indeterminate;
            set
            {
                _Indeterminate = value;
                OnPropertyChanged(nameof(Indeterminate));
            }
        }

        public bool UseAutomaticSegmentation
        {
            get => FilesToSegment.Count() > 0 ? FilesToSegment[CurrentIndex].useAutomaticSegmentation : true;
            set
            {
                if (FilesToSegment.Count() > 0)
                {
                    FilesToSegment[CurrentIndex].useAutomaticSegmentation = value;
                    OnPropertyChanged(nameof(UseAutomaticSegmentation));
                    ToggleButtonStatus();

                    if (!value)
                    {
                        ExtractBBoxes = false;
                        ExtractMasks = false;
                    }
                }
            }
        }

        public bool ExtractBBoxes
        {
            get => FilesToSegment.Count() > 0 ? FilesToSegment[CurrentIndex].extractBoundingBoxes : true;
            set
            {
                if (FilesToSegment.Count() > 0)
                {
                    FilesToSegment[CurrentIndex].extractBoundingBoxes = value;

                    ToggleButtonStatus();

                    OnPropertyChanged(nameof(ExtractBBoxes));
                }
            }
        }

        public bool ExtractMasks
        {
            get => FilesToSegment.Count() > 0 ? FilesToSegment[CurrentIndex].extractMasks : true;
            set
            {
                if (FilesToSegment.Count() > 0)
                {
                    FilesToSegment[CurrentIndex].extractMasks = value;

                    ToggleButtonStatus();

                    OnPropertyChanged(nameof(ExtractMasks));
                }
            }
        }

        private Symbol _SegmentHelpIcon = Symbol.Accept;
        public Symbol SegmentHelpIcon
        {
            get => _SegmentHelpIcon;
            set
            {
                _SegmentHelpIcon = value;
                OnPropertyChanged(nameof(SegmentHelpIcon));
            }
        }

        private string _SegmentHelpText = "Click the Segment button to start segmentation.";
        public string SegmentHelpText
        {
            get => _SegmentHelpText;
            set
            {
                _SegmentHelpText = value;
                OnPropertyChanged(nameof(SegmentHelpText));
            }
        }

        public string StatusText
        {
            get => _StatusText;
            set
            {
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

        public bool UsePostProcess
        {
            get => FilesToSegment.Count() > 0 ? FilesToSegment[CurrentIndex].usePostProcess : false;
            set
            {
                if (FilesToSegment.Count() > 0)
                {
                    FilesToSegment[CurrentIndex].usePostProcess = value;

                    if (value)
                    {
                        ExtractBBoxes = false;
                        ExtractMasks = false;
                    }

                    ToggleButtonStatus();
                    OnPropertyChanged(nameof(UsePostProcess));
                }
            }
        }

        private ObservableCollection<SegmentParameterDataModel> _FilesToSegment = new();
        public ObservableCollection<SegmentParameterDataModel> FilesToSegment
        {
            get => _FilesToSegment;
            set
            {
                _FilesToSegment = value;
                OnPropertyChanged(nameof(FilesToSegment));
            }
        }

        private int _CurrentIndex = 0;
        public int CurrentIndex
        {
            get => _CurrentIndex;
            set
            {
                IsChangingIndex = true;
                _CurrentIndex = value;
                OnPropertyChanged(nameof(CurrentIndex));
                OnPropertyChanged(nameof(UseAutomaticSegmentation));
                OnPropertyChanged(nameof(ExtractBBoxes));
                OnPropertyChanged(nameof(ExtractMasks));
                OnPropertyChanged(nameof(UsePostProcess));

                ToggleButtonStatus();

                if (FilesToSegment.Count() > 0 && value > -1)
                {
                    Device = FilesToSegment[value].Device;

                    if (FilesToSegment[value].prefix != "")
                    {
                        radio_prefix.IsChecked = true;
                        FileName = FilesToSegment[value].prefix;
                    }
                    else if (FilesToSegment[value].postfix != "")
                    {
                        radio_postfix.IsChecked = true;
                        FileName = FilesToSegment[value].postfix;
                    }
                    else if (FilesToSegment[value].newName != "")
                    {
                        radio_newName.IsChecked = true;
                        FileName = FilesToSegment[value].newName;
                        Extension = FilesToSegment[value].ext;
                    }
                    else
                    {
                        radio_postfix.IsChecked = true;
                        FileName = "output";
                    }

                    if (IsSegmentationComplete)
                    {
                        LabelingBtnVisibility = (FilesToSegment[value].extractBoundingBoxes || FilesToSegment[value].extractMasks) ? Visibility.Visible : Visibility.Collapsed;
                    }
                }

                IsChangingIndex = false;
            }
        }

        private bool _UseParallel = false;
        public bool UseParallel
        {
            get => _UseParallel;
            set
            {
                _UseParallel = value;
                dropdown_devices.IsEnabled = !value;
                txt_deviceHelp.Text = value ? "When the Parallel option is enabled, it automatically assigns tasks to each GPU." : "Only NVIDIA GPUs are displayed.";

                OnPropertyChanged(nameof(UseParallel));
            }
        }

        private bool _IsParallelEnabled = false;
        public bool IsParallelEnabled
        {
            get => _IsParallelEnabled;
            set
            {
                _IsParallelEnabled = value;
                OnPropertyChanged(nameof(IsParallelEnabled));
            }
        }

        private Visibility _LabelingBtnVisibility = Visibility.Collapsed;
        public Visibility LabelingBtnVisibility
        {
            get => _LabelingBtnVisibility;
            set
            {
                _LabelingBtnVisibility = value;
                OnPropertyChanged(nameof(LabelingBtnVisibility));
            }
        }

        private bool IsChangingIndex = false;
        private bool IsSegmentationComplete = false;
        private bool IsError = false;
        private List<string> DeviceList = new();

        private EnvironmentHelper helper = new();
        private SegmentationHelper segmentationHelper = new();

        public event PropertyChangedEventHandler PropertyChanged;

        public HomeView()
        {
            InitializeComponent();
            DataContext = this;

            if (!helper.GetFinalStatus())
            {
                Thread thread = new Thread(checkEnvironment);
                thread.Start();
            }
            else if (helper.GetLibrariesVersion("Entry Point Version") != Assembly.GetExecutingAssembly().GetName().Version.ToString() || helper.GetLibrariesVersion("Feature Activator Version") != Assembly.GetExecutingAssembly().GetName().Version.ToString())
            {
                Thread thread = new(updateLibraries);
                thread.Start();
            }
            else
            {
                commandBar.IsEnabled = true;
                progressView.Visibility = Visibility.Collapsed;
                configurationView.Visibility = Visibility.Collapsed;
                imageView.Visibility = Visibility.Visible;
            }

            DeviceList = EnvironmentHelper.GetGPUList();
            IsParallelEnabled = DeviceList.Count() > 1;

            foreach (var d in DeviceList)
            {
                var deviceFlyout = new MenuFlyoutItem();
                deviceFlyout.Text = d;
                deviceFlyout.Click += OnDeviceSelected;

                deviceMenu.Items.Add(
                    deviceFlyout
                );
            }

            Device = "Auto";

            FilesToSegment.CollectionChanged += (s, e) =>
            {
                btn_deleteImage.IsEnabled = FilesToSegment.Count() > 1;
                OnPropertyChanged(nameof(FilesToSegment.Count));
            };
        }

        private void ToggleButtonStatus()
        {
            btn_extractBBoxes.IsEnabled = FilesToSegment.Count() > 0 ? FilesToSegment[CurrentIndex].useAutomaticSegmentation && !FilesToSegment[CurrentIndex].usePostProcess : true;
            btn_extractMasks.IsEnabled = FilesToSegment.Count() > 0 ? FilesToSegment[CurrentIndex].useAutomaticSegmentation && !FilesToSegment[CurrentIndex].usePostProcess : true;
            btn_customizeParams.IsEnabled = FilesToSegment.Count() > 0 ? FilesToSegment[CurrentIndex].useAutomaticSegmentation : true;
            btn_usePostProcess.IsEnabled = FilesToSegment.Count() > 0 ? FilesToSegment[CurrentIndex].useAutomaticSegmentation : false;
            btn_selectPoint.IsEnabled = FilesToSegment.Count() > 0 ? !FilesToSegment[CurrentIndex].useAutomaticSegmentation : false;
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void SetFileName()
        {
            if (FilesToSegment.Count() > 0 && !IsChangingIndex)
            {
                var originalFileName = FilesToSegment[CurrentIndex].filePath.Split(@"\");

                if (radio_prefix.IsChecked == true)
                {
                    if (dropDown_extensions != null && dropDown_extensions.Visibility == Visibility.Visible)
                    {
                        dropDown_extensions.Visibility = Visibility.Collapsed;
                    }

                    PlaceHolderText = "Prefix";
                    HelpText = $"The file will be saved as {FileName}_{originalFileName[originalFileName.Length - 1]}";

                    FilesToSegment[CurrentIndex].prefix = FileName;
                    FilesToSegment[CurrentIndex].postfix = "";
                    FilesToSegment[CurrentIndex].ext = "";
                    FilesToSegment[CurrentIndex].newName = "";
                }
                else if (radio_postfix.IsChecked == true)
                {
                    if (dropDown_extensions != null && dropDown_extensions.Visibility == Visibility.Visible)
                    {
                        dropDown_extensions.Visibility = Visibility.Collapsed;
                    }

                    PlaceHolderText = "Postfix";
                    var name = originalFileName[originalFileName.Length - 1].Split(".");
                    var ext = name[name.Length - 1];

                    HelpText = $"The file will be saved as {name[0]}_{FileName}.{ext}";

                    FilesToSegment[CurrentIndex].postfix = FileName;
                    FilesToSegment[CurrentIndex].prefix = "";
                    FilesToSegment[CurrentIndex].ext = "";
                    FilesToSegment[CurrentIndex].newName = "";
                }
                else
                {
                    if (dropDown_extensions != null && dropDown_extensions.Visibility == Visibility.Collapsed)
                    {
                        dropDown_extensions.Visibility = Visibility.Visible;
                    }

                    PlaceHolderText = "File Name";
                    HelpText = $"The file will be saved as {FileName}.{Extension}";
                    dropDown_extensions.Visibility = Visibility.Visible;

                    FilesToSegment[CurrentIndex].newName = FileName;
                    FilesToSegment[CurrentIndex].postfix = "";
                    FilesToSegment[CurrentIndex].ext = Extension;
                    FilesToSegment[CurrentIndex].prefix = "";
                }
            }
        }

        private void OnRadioButtonChecked(object sender, RoutedEventArgs e)
        {
            SetFileName();
        }

        private async void checkEnvironment()
        {
            updateStatus("Checking WSL Status");

            var result = helper.GetWSLStatus();

            increaseProgress();

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

            if (!result)
            {
                if (!helper.isWSLActivated || !helper.isVirtualMachinePlatformActivated)
                {
                    updateStatus("Activating Features (This may take some time)");
                    var activateResult = helper.ActivateFeatures();

                    if (activateResult)
                    {
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
                else
                {
                    increaseProgress();
                }
            }
            else
            {
                increaseProgress(20);
            }

            updateStatus("Checking Installed Linux");

            var isLinuxInstalled = helper.GetInstalledLinux();
            increaseProgress();

            if (!isLinuxInstalled)
            {
                updateStatus("Installing Ubuntu");
                var installationResult = helper.ActivateFeatures(1);

                if (!installationResult)
                {
                    showErrorMessage();
                    return;
                }
            }

            increaseProgress();

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

            if (!helper.GetStatus("Python Packages Status"))
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

                if (!packageResult)
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

            if (!helper.GetStatus("Entry Point Status"))
            {
                updateStatus("Copying Romanowsky Stain Slide Analyzer for Python CLI...");
                var copyResult = helper.CopyEntryPoint();

                if (!copyResult)
                {
                    showErrorMessage();
                    return;
                }
            }

            var _ = helper.GetFinalStatus();

            DispatcherQueue.TryEnqueue(() =>
            {
                commandBar.IsEnabled = true;
                progressView.Visibility = Visibility.Collapsed;
                configurationView.Visibility = Visibility.Collapsed;
                imageView.Visibility = Visibility.Visible;
            });
        }

        private async void updateLibraries()
        {
            if (helper.GetLibrariesVersion("Feature Activator Version") != Assembly.GetExecutingAssembly().GetName().Version.ToString())
            {
                updateStatus("Updating Feature Activator...");
                var isActivatorInstalled = helper.InstallFeatureActivator();

                if (!isActivatorInstalled)
                {
                    showErrorMessage();
                    return;
                }
            }

            increaseProgress(50);

            if (helper.GetLibrariesVersion("Entry Point Version") != Assembly.GetExecutingAssembly().GetName().Version.ToString())
            {
                updateStatus("Updating Entry Point...");
                var isEntryPointCopied = helper.UpdateEntryPoint();

                if (!isEntryPointCopied)
                {
                    showErrorMessage();
                    return;
                }
            }

            increaseProgress(50);

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
                var result = await MainWindow.ShowContentDialogAsync(
                    "Reboot Required",
                    "A new Windows additional feature has been added and requires a reboot to continue installing.",
                    "Reboot now",
                    "Reboot 1 hour later",
                    "Reboot manually"
                );

                if (result == ContentDialogResult.Primary)
                {
                    helper.reboot();
                }
                else if (result == ContentDialogResult.Secondary)
                {
                    helper.reboot(3600);
                    ShowRebootScreen();
                }
                else
                {
                    ShowRebootScreen();
                }
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

        private async void ShowFilePicker()
        {
            var filePicker = new FileOpenPicker();
            var window = App.window;
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);

            WinRT.Interop.InitializeWithWindow.Initialize(filePicker, hWnd);

            filePicker.ViewMode = PickerViewMode.Thumbnail;
            filePicker.SuggestedStartLocation = PickerLocationId.Desktop;
            filePicker.FileTypeFilter.Add(".jpg");
            filePicker.FileTypeFilter.Add(".jpeg");
            filePicker.FileTypeFilter.Add(".png");

            var files = await filePicker.PickMultipleFilesAsync();

            if (files != null && files.Count() > 0)
            {
                foreach (var file in files)
                {
                    if (file.Path.Contains(" "))
                    {
                        await MainWindow.ShowContentDialogAsync("Warning", $"There is a space in the file path. If there is a space, unexpected actions may occur.\nPlease remove the space.\nFile path: {file.Path}", "OK");
                    }

                    else
                    {
                        FilesToSegment.Add(
                            new(file.Path, "", "", "output", "", new(), new())
                        );
                    }
                }

                if (FilesToSegment.Count() > 0)
                {
                    imageTutorialView.Visibility = Visibility.Collapsed;
                    btn_clear.Visibility = Visibility.Visible;
                    selectedImagePanel.Visibility = Visibility.Visible;
                    controlsView.Visibility = Visibility.Visible;
                    SetFileName();
                }
            }
        }

        private string GetOriginalImage(string path)
        {
            var jpgFiles = Directory.GetFiles($@"{path}\", "input.jpg");
            var jpegFiles = Directory.GetFiles($@"{path}\", "input.jpeg");
            var pngFiles = Directory.GetFiles($@"{path}\", "input.png");

            if (jpgFiles.Length == 1) return jpgFiles[0];
            else if (jpegFiles.Length == 1) return jpegFiles[0];
            else if (pngFiles.Length == 1) return pngFiles[0];
            else return "";
        }

        private async void OnClick(object sender, RoutedEventArgs e)
        {
            switch ((sender as Button).Name)
            {
                case "btn_reboot":
                    helper.reboot();
                    break;

                case "btn_segment":
                    if (FilesToSegment.Count() <= 0)
                    {
                        await MainWindow.ShowContentDialogAsync("No Image", "Please select an image.", "OK");
                        return;
                    }

                    else
                    {
                        foreach (var file in FilesToSegment)
                        {
                            if (file.prefix == "" && file.postfix == "" && file.newName == "")
                            {
                                await MainWindow.ShowContentDialogAsync("Warning", "Please enter file name.", "OK");
                                return;
                            }
                            else if (file.newName == "input" || file.postfix == "input" || file.prefix == "input")
                            {
                                await MainWindow.ShowContentDialogAsync("Warning", $"input cannot be used as a file name.\nPlease choose a different file name.", "OK");
                                return;
                            }
                            else if (file.newName.Contains(".") || file.prefix.Contains(".") || file.postfix.Contains("."))
                            {
                                await MainWindow.ShowContentDialogAsync("Warning", $"The file name cannot contain a '.'\nPlease try again with a different name.", "OK");
                                return;
                            }

                            var originalFileName = file.filePath.Split(@"\");
                            var ext = originalFileName[originalFileName.Length - 1].Split(".");

                            if (file.prefix != "") file.destination = $"{file.prefix}_{originalFileName[originalFileName.Length - 1]}";
                            else if (file.postfix != "") file.destination = $"{ext[0]}_{file.postfix}.{ext[1]}";
                            else file.destination = $@"{file.newName}.{file.ext}";
                        }
                    }

                    DispatcherQueue.TryEnqueue(() =>
                    {
                        commandBar.IsEnabled = false;
                        Indeterminate = true;
                        controlsView.Visibility = Visibility.Collapsed;
                        progressBar.IsIndeterminate = (FilesToSegment.Count() == 1 || UseParallel);
                        progressBar.Visibility = Visibility.Visible;
                        progressView.Visibility = Visibility.Visible;
                        segmentHelpPanel.Visibility = Visibility.Collapsed;
                        StatusText = "Romanowsky Stain Slide Analyzer is processing your request.\nPlease wait.";
                        Progress = 0;
                    });

                    if (UseParallel)
                    {
                        await Task.Run(() =>
                        {
                            List<ObservableCollection<SegmentParameterDataModel>> gpuAssignments = new();

                            for (int i = 0; i < DeviceList.Count(); i++)
                            {
                                gpuAssignments.Add(new());
                            }

                            for (int i = 0; i < FilesToSegment.Count(); i++)
                            {
                                var gpuIndex = i % DeviceList.Count();
                                FilesToSegment[i].Device = DeviceList[gpuIndex];
                                gpuAssignments[gpuIndex].Add(FilesToSegment[i]);
                            }

                            Parallel.ForEach(gpuAssignments, item =>
                            {
                                Segment(item);
                            });
                        });

                        if (!IsError) ShowCompleteView();
                    }

                    else
                    {
                        Thread thread = new Thread(() => Segment(null));
                        thread.Start();
                    }

                    break;

            }
        }

        private async void OnMenuFlyoutClick(object sender, RoutedEventArgs e)
        {
            switch ((sender as MenuFlyoutItem).Name)
            {
                case "btn_loadImage":
                    ShowFilePicker();
                    break;

                case "btn_deleteImage":
                    var isConfirm = await MainWindow.ShowContentDialogAsync("Delete", "Are you sure you want to remove this image?\nThe actual file will not be removed.", "Yes", "No");

                    if (isConfirm)
                    {
                        var isIndexAtEnd = false;

                        if (CurrentIndex == FilesToSegment.Count() - 1)
                        {
                            CurrentIndex -= 1;
                            isIndexAtEnd = true;
                        }

                        FilesToSegment.RemoveAt(isIndexAtEnd ? CurrentIndex + 1 : CurrentIndex);
                    }
                    break;

                case "btn_clear":
                    CurrentIndex = 0;
                    IsError = false;
                    FilesToSegment.Clear();
                    IsSegmentationComplete = false;
                    LabelingBtnVisibility = Visibility.Collapsed;

                    btn_loadImage.Visibility = Visibility.Visible;
                    imageTutorialView.Visibility = Visibility.Visible;
                    btn_clear.Visibility = Visibility.Collapsed;
                    selectedImagePanel.Visibility = Visibility.Collapsed;
                    controlsView.Visibility = Visibility.Collapsed;
                    btn_labeling.Visibility = Visibility.Collapsed;
                    btn_customizeParams.IsEnabled = true;
                    btn_segment.IsEnabled = true;
                    btn_usePostProcess.IsEnabled = true;
                    btn_useAutomaticSegmentation.IsEnabled = true;
                    btn_selectPoint.IsEnabled = false;
                    progressView.Visibility = Visibility.Collapsed;
                    btn_save.Visibility = Visibility.Collapsed;

                    SegmentHelpText = "Click the Segment button to start segmentation.";
                    FileName = "output";
                    Extension = "png";
                    break;

                case "btn_customizeParams":
                    if (FilesToSegment.Count() <= 0)
                    {
                        await MainWindow.ShowContentDialogAsync("No Image", "Please select an image.", "OK");
                        return;
                    }


                    ParameterControlWindow paramsControlWindow = new(
                        FilesToSegment[CurrentIndex]
                    );

                    paramsControlWindow.Activate();

                    break;

                case "btn_setParamsAll":
                    if (FilesToSegment.Count() > 0 && FilesToSegment[CurrentIndex].useAutomaticSegmentation)
                    {
                        var dialogResult = await MainWindow.ShowContentDialogAsync("Set All Parameters", "Sets all parameters to the same value based on the parameters of the currently displayed image.\nDo you want to continue?", "Yes", "No");

                        if (dialogResult)
                        {
                            var current = FilesToSegment[CurrentIndex];

                            for (var i = 0; i < FilesToSegment.Count(); i++)
                            {
                                var target = FilesToSegment[i];

                                if (i != CurrentIndex)
                                {
                                    target.PointsPerSide.Value = current.PointsPerSide.Value;
                                    target.PointsPerBatch.Value = current.PointsPerBatch.Value;
                                    target.PredIoUThresh.Value = current.PredIoUThresh.Value;
                                    target.StabilityScoreThresh.Value = current.StabilityScoreThresh.Value;
                                    target.StabilityScoreOffset.Value = current.StabilityScoreOffset.Value;
                                    target.MaskThreshold.Value = current.MaskThreshold.Value;
                                    target.BoxNMSThresh.Value = current.BoxNMSThresh.Value;
                                    target.CropNLayers.Value = current.CropNLayers.Value;
                                    target.CropNMSThresh.Value = current.CropNMSThresh.Value;
                                    target.CropOverlapRatio.Value = current.CropOverlapRatio.Value;
                                    target.CropNPointsDownScaleFactor.Value = current.CropNPointsDownScaleFactor.Value;
                                    target.MinMaskRegionArea.Value = current.MinMaskRegionArea.Value;
                                }
                            }

                            await MainWindow.ShowContentDialogAsync("Done", "All parameters are set.", "OK");
                        }
                    }
                    else
                    {
                        if (FilesToSegment.Count() <= 0)
                        {
                            await MainWindow.ShowContentDialogAsync("No Image", "Please select an image.", "OK");
                        }

                        else if (!FilesToSegment[CurrentIndex].useAutomaticSegmentation)
                        {
                            await MainWindow.ShowContentDialogAsync("Warning", "To use this feature, enable Use Automatic Segmentation.", "OK");
                        }
                    }

                    break;

                case "btn_selectPoint":
                    if (FilesToSegment.Count() <= 0)
                    {
                        await MainWindow.ShowContentDialogAsync("No Image", "Please select an image.", "OK");
                        return;
                    }

                    PointSelectionView pointSelectionView = new(FilesToSegment[CurrentIndex]);
                    pointSelectionView.Activate();

                    break;

                case "btn_labeling":
                    var viewModel = new LabelingViewModel();
                    viewModel.Source = new BitmapImage(new Uri(FilesToSegment[CurrentIndex].filePath));

                    LabelingWindow labelingWindow = new(viewModel, $@"{FilesToSegment[CurrentIndex].destination}.txt", FilesToSegment[CurrentIndex].extractMasks, FilesToSegment[CurrentIndex].extractBoundingBoxes);
                    labelingWindow.Activate();
                    break;

                case "btn_save":
                    var folderPicker = new FolderPicker();
                    var window = App.window;
                    var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);

                    WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, hWnd);

                    folderPicker.ViewMode = PickerViewMode.Thumbnail;
                    folderPicker.SuggestedStartLocation = PickerLocationId.Desktop;

                    var folder = await folderPicker.PickSingleFolderAsync();

                    if (folder != null)
                    {
                        try
                        {
                            var filePathSplit = FilesToSegment[CurrentIndex].destination.Split(@"\");
                            File.Copy($@"{FilesToSegment[CurrentIndex].destination}", $@"{folder.Path}\{filePathSplit[filePathSplit.Length - 1]}");
                            await MainWindow.ShowContentDialogAsync("Done", "The requested task has been completed.", "OK");
                        }
                        catch (Exception ex)
                        {
                            await MainWindow.ShowContentDialogAsync(
                                "Error",
                                $"An error occurred while saving the file.\nPlease check if the file already exists or re-run the software.\nError: {ex.Message}",
                                "OK"
                            );
                        }
                    }
                    break;

            }
        }

        private async void Segment(ObservableCollection<SegmentParameterDataModel>? files = null)
        {
            foreach (var file in files == null ? FilesToSegment : files)
            {
                List<double> coords = new();
                List<int> labels = new();

                if (!file.useAutomaticSegmentation)
                {
                    foreach (var item in file.Points)
                    {
                        coords.Add(item.X);
                        coords.Add(item.Y);
                        labels.Add((int)item.ClassType);
                    }
                }

                file.inputCoords = coords;
                file.inputLabels = labels;

                var selectedDevice = file.Device;

                if (selectedDevice != "Auto" && selectedDevice != "CPU")
                {
                    selectedDevice = selectedDevice.Split("Bus #")[1].Split(",")[0];
                    selectedDevice = (int.Parse(selectedDevice) - 1).ToString();
                }

                var result = segmentationHelper.segment(
                    file.filePath,
                    file.destination,
                    file.prefix,
                    file.postfix,
                    file.ext,
                    file.useAutomaticSegmentation,
                    file.extractBoundingBoxes,
                    file.extractMasks,
                    file.inputCoords,
                    file.inputLabels,
                    file.usePostProcess,
                    file.PointsPerSide.Value,
                    file.PointsPerBatch.Value,
                    file.PredIoUThresh.Value,
                    file.StabilityScoreThresh.Value,
                    file.StabilityScoreOffset.Value,
                    file.MaskThreshold.Value,
                    file.BoxNMSThresh.Value,
                    file.CropNLayers.Value,
                    file.CropNMSThresh.Value,
                    file.CropOverlapRatio.Value,
                    file.CropNPointsDownScaleFactor.Value,
                    file.MinMaskRegionArea.Value,
                    selectedDevice
                );

                if (!result)
                {
                    IsError = true;
                    ShowErrorView();
                    return;
                }

                var originalFileName = file.filePath.Split(@"\");
                var outputFileName = "";

                if (file.prefix != "")
                {
                    outputFileName = $"{file.prefix}_{originalFileName[originalFileName.Length - 1]}";
                }
                else if (file.postfix != "")
                {
                    var name = originalFileName[originalFileName.Length - 1].Split(".");
                    var ext = name[name.Length - 1];

                    outputFileName = $"{name[0]}_{file.postfix}.{ext}";
                }
                else
                {
                    outputFileName = $"{file.newName}.{file.ext}";
                }

                var targetPath = DateTime.Now.ToString("MM_dd_yyyy_HH_mm_ss");

                var createHistoryResult = segmentationHelper.CreateHistory(
                    file.filePath,
                    targetPath,
                    outputFileName,
                    file.usePostProcess,
                    file.useAutomaticSegmentation,
                    file.extractBoundingBoxes,
                    file.extractMasks,
                    file.inputCoords,
                    file.inputLabels,
                    file.PointsPerSide.Value,
                    file.PointsPerBatch.Value,
                    file.PredIoUThresh.Value,
                    file.StabilityScoreThresh.Value,
                    file.StabilityScoreOffset.Value,
                    file.MaskThreshold.Value,
                    file.BoxNMSThresh.Value,
                    file.CropNLayers.Value,
                    file.CropNMSThresh.Value,
                    file.CropOverlapRatio.Value,
                    file.CropNPointsDownScaleFactor.Value,
                    file.MinMaskRegionArea.Value,
                    file.Device
                );

                if (!createHistoryResult)
                {
                    IsError = true;
                    ShowErrorView();
                    return;
                }

                var rssaFolder = @"C:\RomanowskyStainSlideAnalyzer";
                var finalPath = Path.Combine(rssaFolder, targetPath);

                DispatcherQueue.TryEnqueue(() =>
                {
                    file.destination = @$"{finalPath}\{file.destination}";

                    var progressUnit = (Convert.ToDouble(1.0 / Convert.ToDouble(FilesToSegment.Count()))) * 100;
                    increaseProgress(progressUnit);
                });
            }

            if (files == null && !IsError) ShowCompleteView();
        }

        private void ShowErrorView()
        {
            DispatcherQueue.TryEnqueue(async () =>
            {
                commandBar.IsEnabled = true;
                btn_customizeParams.IsEnabled = false;
                btn_segment.IsEnabled = false;
                btn_usePostProcess.IsEnabled = false;
                progressBar.Visibility = Visibility.Collapsed;
                StatusText = "Romanowsky Stain Slide Analyzer encountered an error while processing the requested operation.\nClick the Clear button to try again.";
                await MainWindow.ShowContentDialogAsync("Error", "Romanowsky Stain Slide Analyzer encountered an error while processing the requested operation.\nPlease check that your PC environment is configured properly or try adjusting the parameters.", "OK");
            });
        }

        private void ShowCompleteView()
        {
            DispatcherQueue.TryEnqueue(async () =>
            {
                commandBar.IsEnabled = true;
                btn_customizeParams.IsEnabled = false;
                btn_segment.IsEnabled = false;
                btn_usePostProcess.IsEnabled = false;

                btn_save.Visibility = Visibility.Visible;
                progressView.Visibility = Visibility.Collapsed;
                segmentHelpPanel.Visibility = Visibility.Visible;
                SegmentHelpText = "The Romanowsky Stain Slide Analyzer has completed the task you requested, and the results are as above.\nFor detailed results, check the results in the History tab. Data with the Extract Bounding Boxes or Extract Masks options enabled can be labeled by clicking the Labeling button.";
                IsSegmentationComplete = true;

                LabelingBtnVisibility = (FilesToSegment[CurrentIndex].extractBoundingBoxes || FilesToSegment[CurrentIndex].extractMasks) ? Visibility.Visible : Visibility.Collapsed;

            });
        }

        private void OnMenuItemClick(object sender, RoutedEventArgs e)
        {
            Extension = (sender as MenuFlyoutItem).Text;
        }

        private void OnDeviceSelected(object sender, RoutedEventArgs e)
        {
            Device = (sender as MenuFlyoutItem).Text;

            if (!IsChangingIndex && FilesToSegment.Count() > 0)
            {
                FilesToSegment[CurrentIndex].Device = Device;
            }
        }

        private async void imageView_DragEnter(object sender, DragEventArgs e)
        {
            if (e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                var items = await e.DataView.GetStorageItemsAsync();

                if (items.Count > 0)
                {
                    foreach (var item in items)
                    {
                        if (item.GetType() == typeof(StorageFile))
                        {
                            if ((item as StorageFile).Path.Contains(" "))
                            {
                                await MainWindow.ShowContentDialogAsync("Warning", $"There is a space in the file path. If there is a space, unexpected actions may occur.\nPlease remove the space.\nFile path: {item.Path}", "OK");
                            }
                            else if ((item as StorageFile).FileType.ToLower() == ".jpg" || (item as StorageFile).FileType.ToLower() == ".jpeg" || (item as StorageFile).FileType.ToLower() == ".png")
                            {
                                FilesToSegment.Add(new(item.Path, "", "", "output", "", new(), new()));
                                btn_loadImage.Visibility = Visibility.Collapsed;
                                imageTutorialView.Visibility = Visibility.Collapsed;
                                btn_clear.Visibility = Visibility.Visible;
                                selectedImagePanel.Visibility = Visibility.Visible;
                                controlsView.Visibility = Visibility.Visible;
                                SetFileName();
                            }
                            else
                            {
                                e.AcceptedOperation = DataPackageOperation.None;
                                await MainWindow.ShowContentDialogAsync("Warning", $"Only .jpg, .jpeg, .png files can be loaded.\nFile {(item as StorageFile).Path} will be skipped.", "OK");
                            }
                        }
                    }
                }
            }
        }

        private async void imageView_DragOver(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = DataPackageOperation.Copy;
        }
    }
}