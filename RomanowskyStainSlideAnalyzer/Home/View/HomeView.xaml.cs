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
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
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

        private bool _UseAutomaticSegmentation = true;
        public bool UseAutomaticSegmentation
        {
            get => _UseAutomaticSegmentation;
            set
            {
                _UseAutomaticSegmentation = value;
                OnPropertyChanged(nameof(UseAutomaticSegmentation));
                OnPropertyChanged(nameof(IsParameterSettingsEnabled));
                OnPropertyChanged(nameof(IsSelectPointEnabled));

                if (!_UseAutomaticSegmentation)
                {
                    btn_usePostProcess.IsEnabled = false;
                    btn_extractBBoxes.IsEnabled = false;
                    btn_extractMasks.IsEnabled = false;

                    ExtractBBoxes = false;
                    ExtractMasks = false;
                }
                else
                {
                    btn_usePostProcess.IsEnabled = true;
                    btn_extractBBoxes.IsEnabled = true;
                    btn_extractMasks.IsEnabled = true;
                }
            }
        }

        private bool _ExtractBBoxes = true;
        public bool ExtractBBoxes
        {
            get => _ExtractBBoxes;
            set
            {
                _ExtractBBoxes = value;

                if(value)
                {
                    UseAutomaticSegmentation = true;
                    UsePostProcess = false;
                }

                OnPropertyChanged(nameof(ExtractBBoxes));
            }
        }

        private bool _ExtractMasks = true;
        public bool ExtractMasks
        {
            get => _ExtractMasks;
            set
            {
                _ExtractMasks = value;

                if (value)
                {
                    UseAutomaticSegmentation = true;
                    UsePostProcess = false;
                }
                OnPropertyChanged(nameof(ExtractMasks));
            }
        }

        public bool IsParameterSettingsEnabled
        {
            get => _UseAutomaticSegmentation;
        }

        public bool IsSelectPointEnabled
        {
            get => !_UseAutomaticSegmentation;
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

        private bool _UsePostProcess = false;
        public bool UsePostProcess
        {
            get => _UsePostProcess;
            set
            {
                _UsePostProcess = value;

                if(value)
                {
                    ExtractBBoxes = false;
                    ExtractMasks = false;
                }

                OnPropertyChanged(nameof(UsePostProcess));
            }
        }

        private ParametersViewModel viewModel = new();
        private LabelingViewModel labelingViewModel = new();
        private EnvironmentHelper helper = new();
        private SegmentationHelper segmentationHelper = new();
        private string filePath = "";
        private string outputFileName = "";
        private string finalPreFix = "";
        private string finalPostFix = "";
        private string finalFileName = "";
        private string finalExt = "";
        private string targetPath = "";

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
            else if(helper.GetLibrariesVersion("Entry Point Version") != Assembly.GetExecutingAssembly().GetName().Version.ToString() || helper.GetLibrariesVersion("Feature Activator Version") != Assembly.GetExecutingAssembly().GetName().Version.ToString())
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
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void SetFileName()
        {
            var originalFileName = filePath.Split(@"\");

            if (radio_prefix.IsChecked == true)
            {
                if (dropDown_extensions != null && dropDown_extensions.Visibility == Visibility.Visible)
                {
                    dropDown_extensions.Visibility = Visibility.Collapsed;
                }

                PlaceHolderText = "Prefix";
                HelpText = $"The file will be saved as {FileName}_{originalFileName[originalFileName.Length - 1]}";
            } else if(radio_postfix.IsChecked == true)
            {
                if (dropDown_extensions != null && dropDown_extensions.Visibility == Visibility.Visible)
                {
                    dropDown_extensions.Visibility = Visibility.Collapsed;
                }

                PlaceHolderText = "Postfix";
                var name = originalFileName[originalFileName.Length - 1].Split(".");
                var ext = name[name.Length - 1];

                HelpText = $"The file will be saved as {name[0]}_{FileName}.{ext}";
            } else
            {
                if (dropDown_extensions != null && dropDown_extensions.Visibility == Visibility.Collapsed)
                {
                    dropDown_extensions.Visibility = Visibility.Visible;
                }

                PlaceHolderText = "File Name";
                HelpText = $"The file will be saved as {FileName}.{Extension}";
                dropDown_extensions.Visibility = Visibility.Visible;
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
                if(!helper.isWSLActivated || !helper.isVirtualMachinePlatformActivated)
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
            else {
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

            if(!helper.GetStatus("Entry Point Status"))
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
            if(helper.GetLibrariesVersion("Feature Activator Version") != Assembly.GetExecutingAssembly().GetName().Version.ToString())
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

            if(helper.GetLibrariesVersion("Entry Point Version") != Assembly.GetExecutingAssembly().GetName().Version.ToString())
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

                if(result == ContentDialogResult.Primary)
                {
                    helper.reboot();
                } 
                else if(result == ContentDialogResult.Secondary)
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

            var file = await filePicker.PickSingleFileAsync();

            if(file != null)
            {
                filePath = file.Path;

                var source = new BitmapImage(new Uri(filePath));
                btn_loadImage.Visibility = Visibility.Collapsed;
                imageTutorialView.Visibility = Visibility.Collapsed;
                btn_clear.Visibility = Visibility.Visible;
                selectedImagePanel.Visibility = Visibility.Visible;
                selectedImageView.Source = source;
                labelingViewModel.Source = source;
                controlsView.Visibility = Visibility.Visible;
                viewModel.Source = source;
                SetFileName();
            }
        }

        private async void OnClick(object sender, RoutedEventArgs e) {
            switch((sender as Button).Name)
            {
                case "btn_reboot":
                    helper.reboot();
                    break;

                case "btn_loadImage":
                    ShowFilePicker();
                    break;

                case "btn_clear":
                    filePath = "";

                    btn_loadImage.Visibility = Visibility.Visible;
                    imageTutorialView.Visibility = Visibility.Visible;
                    btn_clear.Visibility = Visibility.Collapsed;
                    selectedImagePanel.Visibility = Visibility.Collapsed;
                    controlsView.Visibility = Visibility.Collapsed;
                    resultImageView.Visibility = Visibility.Collapsed;
                    btn_labeling.Visibility = Visibility.Collapsed;
                    btn_customizeParams.IsEnabled = true;
                    btn_segment.IsEnabled = true;
                    btn_usePostProcess.IsEnabled = true;
                    btn_useAutomaticSegmentation.IsEnabled = true;
                    btn_selectPoint.IsEnabled = true;
                    progressView.Visibility = Visibility.Collapsed;
                    btn_save.Visibility = Visibility.Collapsed;

                    SegmentHelpText = "Click the Segment button to start segmentation.";
                    FileName = "output";
                    Extension = "png";
                    break;

                case "btn_customizeParams":
                    ParameterControlWindow paramsControlWindow = new(viewModel);
                    paramsControlWindow.Activate();
                    break;

                case "btn_selectPoint":
                    if(filePath == "")
                    {
                        await MainWindow.ShowContentDialogAsync("No Image", "Please select an image.", "OK");
                        return;
                    }

                    PointSelectionView pointSelectionView = new(viewModel);
                    pointSelectionView.Activate();

                    break;

                case "btn_labeling":
                    LabelingWindow labelingWindow = new(labelingViewModel, $@"C:\RomanowskyStainSlideAnalyzer\{targetPath}\{outputFileName}.txt", btn_extractMasks.IsChecked == true, btn_extractBBoxes.IsChecked == true);
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
                            File.Copy($@"C:\RomanowskyStainSlideAnalyzer\{targetPath}\{outputFileName}", $@"{folder.Path}\{outputFileName}");
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

                case "btn_segment":
                    if (filePath == "")
                    {
                        await MainWindow.ShowContentDialogAsync("No Image", "Please select an image.", "OK");
                        return;
                    }
                    else if (FileName == "" || (radio_newName.IsChecked == true && Extension == ""))
                    {
                        await MainWindow.ShowContentDialogAsync("Warning", "Please enter file name.", "OK");
                        return;
                    }
                    else if (filePath.Contains(" "))
                    {
                        await MainWindow.ShowContentDialogAsync("Warning", $"There is a space in the file path. If there is a space, unexpected actions may occur.\nPlease remove the space.\nFile path: {filePath}", "OK");
                        return;
                    } else if (FileName == "input")
                    {
                        await MainWindow.ShowContentDialogAsync("Warning", $"input cannot be used as a file name.\nPlease choose a different file name.", "OK");
                        return;
                    } else if(FileName.Contains("."))
                    {
                        await MainWindow.ShowContentDialogAsync("Warning", $"The file name cannot contain a '.'\nPlease try again with a different name.", "OK");
                        return;
                    }

                    commandBar.IsEnabled = false;
                    Indeterminate = true;
                    controlsView.Visibility = Visibility.Collapsed;
                    progressBar.Visibility = Visibility.Visible;
                    progressView.Visibility = Visibility.Visible;
                    segmentHelpPanel.Visibility = Visibility.Collapsed;
                    StatusText = "Romanowsky Stain Slide Analyzer is processing your request.\nPlease wait.";

                    finalPostFix = radio_postfix.IsChecked == true ? FileName : "";
                    finalPreFix = radio_prefix.IsChecked == true ? FileName : "";
                    finalFileName = radio_newName.IsChecked == true ? FileName : "";
                    finalExt = radio_newName.IsChecked == true ? Extension : "";

                    Thread thread = new Thread(Segment);
                    thread.Start();
                    break;
            }
        }

        private void Segment()
        {
            var isGPUInstalled = helper.GetGPU();

            if (!isGPUInstalled)
            {
                MainWindow.ShowContentDialogAsync(
                    "No GPU Installed",
                    "Windows cannot detect your GPU.\nIf you have a GPU installed, make sure that it is connected and that the GPU drivers are properly installed. If you do not have a GPU installed, segmentation speed may be very slow.",
                    "OK"
                );
            }

            List<double> coords = new();
            List<int> labels = new();

            if(!UseAutomaticSegmentation)
            {
                foreach(var item in viewModel.Points)
                {
                    coords.Add(item.X);
                    coords.Add(item.Y);
                    labels.Add((int)item.ClassType);
                }
            }

            var result = segmentationHelper.segment(
                filePath,
                finalFileName,
                finalPreFix,
                finalPostFix,
                finalExt,
                _UseAutomaticSegmentation,
                _ExtractBBoxes,
                _ExtractMasks,
                coords,
                labels,
                _UsePostProcess,
                viewModel.PointsPerSide.Value,
                viewModel.PointsPerBatch.Value,
                viewModel.PredIoUThresh.Value,
                viewModel.StabilityScoreThresh.Value,
                viewModel.StabilityScoreOffset.Value,
                viewModel.MaskThreshold.Value,
                viewModel.BoxNMSThresh.Value,
                viewModel.CropNLayers.Value,
                viewModel.CropNMSThresh.Value,
                viewModel.CropOverlapRatio.Value,
                viewModel.CropNPointsDownScaleFactor.Value,
                viewModel.MinMaskRegionArea.Value
            );

            if (!result)
            {
                DispatcherQueue.TryEnqueue(async () =>
                {
                    commandBar.IsEnabled = true;
                    btn_customizeParams.IsEnabled = false;
                    btn_segment.IsEnabled = false;
                    btn_usePostProcess.IsEnabled = false;
                    btn_useAutomaticSegmentation.IsEnabled = false;
                    btn_selectPoint.IsEnabled = false;
                    
                    progressBar.Visibility = Visibility.Collapsed;
                    resultImageView.Visibility = Visibility.Collapsed;
                    StatusText = "Romanowsky Stain Slide Analyzer encountered an error while processing the requested operation.\nClick the Clear button to try again.";
                });

                MainWindow.ShowContentDialogAsync("Error", "Romanowsky Stain Slide Analyzer encountered an error while processing the requested operation.\nPlease check that your PC environment is configured properly or try adjusting the parameters.", "OK");
                return;
            }

            var originalFileName = filePath.Split(@"\");

            if (finalPreFix != "")
            {
                outputFileName = $"{FileName}_{originalFileName[originalFileName.Length - 1]}";
            }
            else if (finalPostFix != "")
            {
                var name = originalFileName[originalFileName.Length - 1].Split(".");
                var ext = name[name.Length - 1];

                outputFileName = $"{name[0]}_{FileName}.{ext}";
            }
            else
            {
                outputFileName = $"{FileName}.{Extension}";
            }

            targetPath = DateTime.Now.ToString("MM_dd_yyyy_HH_mm_ss");
            updateStatus("Finishing up...");

            var createHistoryResult = segmentationHelper.CreateHistory(
                filePath,
                targetPath,
                outputFileName,
                _UsePostProcess,
                _UseAutomaticSegmentation,
                _ExtractBBoxes,
                _ExtractMasks,
                coords,
                labels,
                viewModel.PointsPerSide.Value,
                viewModel.PointsPerBatch.Value,
                viewModel.PredIoUThresh.Value,
                viewModel.StabilityScoreThresh.Value,
                viewModel.StabilityScoreOffset.Value,
                viewModel.MaskThreshold.Value,
                viewModel.BoxNMSThresh.Value,
                viewModel.CropNLayers.Value,
                viewModel.CropNMSThresh.Value,
                viewModel.CropOverlapRatio.Value,
                viewModel.CropNPointsDownScaleFactor.Value,
                viewModel.MinMaskRegionArea.Value
            );

            if (!createHistoryResult)
            {
                DispatcherQueue.TryEnqueue(async () =>
                {
                    commandBar.IsEnabled = true;
                    btn_customizeParams.IsEnabled = false;
                    btn_segment.IsEnabled = false;
                    btn_usePostProcess.IsEnabled = false;
                    progressView.Visibility = Visibility.Collapsed;
                    resultImageView.Visibility = Visibility.Collapsed;
                });

                MainWindow.ShowContentDialogAsync("Error", "Romanowsky Stain Slide Analyzer encountered an error while processing the requested operation.\nPlease check that your PC environment is configured properly or try adjusting the parameters.", "OK");
                return;
            }

            DispatcherQueue.TryEnqueue(async () =>
            {
                commandBar.IsEnabled = true;
                btn_customizeParams.IsEnabled = false;
                btn_segment.IsEnabled = false;
                btn_usePostProcess.IsEnabled = false;
                progressView.Visibility = Visibility.Collapsed;
                resultImageView.Visibility = Visibility.Visible;
                resultImageView.Source = new BitmapImage(new Uri(@$"C:\RomanowskyStainSlideAnalyzer\{targetPath}\{outputFileName}"));
                segmentHelpPanel.Visibility = Visibility.Visible;
                btn_labeling.Visibility = ExtractBBoxes ? Visibility.Visible : Visibility.Collapsed;
                SegmentHelpText = "The Romanowsky Stain Slide Analyzer has completed the task you requested, and the results are as above.\nFor detailed results, check the results in the History tab.";
            
                if(_ExtractBBoxes)
                {
                    btn_save.Visibility = Visibility.Visible;
                    btn_labeling.Visibility = Visibility.Visible;
                    SegmentHelpText += "\nYou can use the extracted Bounding Boxes to label the data.";
                }
            });
        }

        private void OnMenuItemClick(object sender, RoutedEventArgs e)
        {
            Extension = (sender as MenuFlyoutItem).Text;
        }
    }
}
