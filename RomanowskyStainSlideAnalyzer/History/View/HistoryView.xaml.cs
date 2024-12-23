using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Interop;
using Microsoft.UI.Xaml.Controls;
using RomanowskyStainSlideAnalyzer.History.Helper;
using RomanowskyStainSlideAnalyzer.History.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading;
using System.Diagnostics;
using RomanowskyStainSlideAnalyzer.Frameworks.Models;
using Windows.Storage.Pickers;
using System.IO;
using RomanowskyStainSlideAnalyzer.Labeling.View;
using Microsoft.UI.Xaml.Media.Imaging;
using RomanowskyStainSlideAnalyzer.Labeling.Models;
using static System.Net.Mime.MediaTypeNames;
using System.Threading.Tasks;
using RomanowskyStainSlideAnalyzer.Analyze.View;
using RomanowskyStainSlideAnalyzer.Labeling.Helper;
using System.Drawing;
using Microsoft.UI.Xaml.Media;
using RomanowskyStainSlideAnalyzer.Frameworks.View;
using Windows.Storage;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace RomanowskyStainSlideAnalyzer.History.View
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class HistoryView : Page, INotifyPropertyChanged
    {
        private HistoryHelper helper = new();

        private ObservableCollection<HistoryDataModel> Datas = new();

        private DateTimeOffset _Date = DateTimeOffset.Now;
        public DateTimeOffset Date
        {
            get => _Date;
            set
            {
                _Date = value;
                OnPropertyChanged(nameof(Date));
            }
        }

        private Visibility _ShowProgress = Visibility.Visible;
        public Visibility ShowProgress
        {
            get => _ShowProgress;
            set
            {
                if(value == Visibility.Visible)
                {
                    emptyPanel.Visibility = Visibility.Collapsed;
                    listView.Visibility = Visibility.Collapsed;
                }

                _ShowProgress = value;
                OnPropertyChanged(nameof(ShowProgress));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

            if (propertyName == nameof(Date)) GetHistory();
        }

        public HistoryView()
        {
            this.InitializeComponent();
            DataContext = this;
            GetHistory();
            historyListView.ItemsSource = Datas;
        }

        private async void GetHistory()
        {
            if(Datas.Count > 0) Datas.Clear();

            ShowProgress = Visibility.Visible;

            var month = Date.Month < 10 ? $"0{Date.Month}" : Date.Month.ToString();
            var day = Date.Day < 10 ? $"0{Date.Day}" : Date.Day.ToString();
            var year = Date.Year;

            var date = $"{month}_{day}_{year}";

            await Task.Run(() =>
            {
                GetHistory(date);
            });

            DispatcherQueue.TryEnqueue(() =>
            {
                ShowProgress = Visibility.Collapsed;

                if (Datas.Count == 0)
                {
                    emptyPanel.Visibility = Visibility.Visible;
                    listView.Visibility = Visibility.Collapsed;
                }
                else
                {
                    emptyPanel.Visibility = Visibility.Collapsed;
                    listView.Visibility = Visibility.Visible;
                }

            });
        }

        private void GetHistory(string date)
        {
            Datas = helper.GetHistory(date);

            DispatcherQueue.TryEnqueue(async () =>
            {
                historyListView.ItemsSource = Datas;
            });
        }

        private void ShowLog(object sender, RoutedEventArgs e)
        {
            HistoryDataModel dataModel = (sender as MenuFlyoutItem).DataContext as HistoryDataModel;

            SegmentationLogView logView = new(dataModel.log);
            logView.Activate();
        }

        private async Task Save(int type, HistoryDataModel dataModel)
        {
            var file = GetImage(dataModel.root);

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
                    var splitPath = (type == 0 ? dataModel.labelingDataPath : file).Split(@"\");
                    var fileName = splitPath[splitPath.Length - 1];

                    if (File.Exists($@"{folder.Path}\{fileName.Split(".txt")[0]}"))
                    {
                        DispatcherQueue.TryEnqueue(async () =>
                        {
                            var isCopy = await MainWindow.ShowContentDialogAsync(
                                "File Already Exists",
                                $@"The file {folder.Path}\{fileName.Split(".txt")[0]} already exists.\nDo you want to overwrite it?",
                                "Yes",
                                "No"
                            );

                            if(isCopy)
                            {
                                helper.Copy(type == 0 ? dataModel.labelingDataPath : file, folder.Path);
                            }
                        });
                    }
                    else
                    {
                        helper.Copy(type == 0 ? dataModel.labelingDataPath : file, folder.Path);
                    }
                }
                catch (Exception ex)
                {
                    await MainWindow.ShowContentDialogAsync(
                        "Error", $"An error occurred while saving the file.\nPlease check if the file already exists or re-run the software.\nError: {ex.Message}", "OK"
                    );
                }
            }
        }

        private async void OnSaveImageClick(object sender, RoutedEventArgs e)
        {
            HistoryDataModel dataModel = (sender as Button).DataContext as HistoryDataModel;
            await Save(1, dataModel);
        }

        private async void OnSaveLabelingDataClick(object sender, RoutedEventArgs e)
        {
            HistoryDataModel dataModel = (sender as MenuFlyoutItem).DataContext as HistoryDataModel;

            var folderPicker = new FolderPicker();
            var window = App.window;
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
            StorageFolder? folder = null;

            switch ((sender as MenuFlyoutItem).Name)
            {
                case "btn_saveLabelingData":
                    await Task.Run(async () => {
                        await Save(0, dataModel);
                        await MainWindow.ShowContentDialogAsync(
                            "Done", "The requested task has been completed.", "OK"
                        );
                    });

                    break;

                case "btn_saveMaskData":
                    WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, hWnd);

                    folderPicker.ViewMode = PickerViewMode.Thumbnail;
                    folderPicker.SuggestedStartLocation = PickerLocationId.Desktop;

                    folder = await folderPicker.PickSingleFolderAsync();

                    if (folder != null)
                    {
                        var imgFile = dataModel.imgFile.Split(@"\");
                        helper.CopyMaskLabelingData($@"{dataModel.root}\Masks\{imgFile[imgFile.Length - 1]}", folder.Path);
                        LabelingHelper.writePythonFile(helper.IsSingleLabeled($@"{dataModel.root}\Masks\{imgFile[imgFile.Length - 1]}"), folder.Path);

                        await MainWindow.ShowContentDialogAsync(
                            "Done", "The requested task has been completed.", "OK"
                        );
                    }

                    break;

                case "btn_saveOriginalMask":
                    WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, hWnd);

                    folderPicker.ViewMode = PickerViewMode.Thumbnail;
                    folderPicker.SuggestedStartLocation = PickerLocationId.Desktop;

                    folder = await folderPicker.PickSingleFolderAsync();

                    if (folder != null)
                    {
                        var imgFile = dataModel.imgFile.Split(@"\");
                        helper.CopyMaskLabelingData($@"{dataModel.root}\Masks\{imgFile[imgFile.Length - 1]}", folder.Path, false);

                        await MainWindow.ShowContentDialogAsync(
                            "Done", "The requested task has been completed.", "OK"
                        );
                    }

                    break;

            }

        }

        private async void SaveImageWithBBoxes(object sender, RoutedEventArgs e)
        {
            HistoryDataModel dataModel = (sender as MenuFlyoutItem).DataContext as HistoryDataModel;
            ContentDialogResult result;

            if (dataModel.labelingDataPath != "")
            {
                result = await MainWindow.ShowContentDialogAsync(
                    "Warning",
                    "Is this file properly labeled?\nIf the labeling is interrupted or the file is not labeled, the bounding box may not be drawn properly.\nIf the file is not properly labeled, click the Export without class.\nExport without class directly uses the coordinates of the bounding box exported by the model, while Export with class uses the result file labeled by the user.",
                    "Export with Class",
                    "Export without Class",
                    "Cancel"
                );
            }
            else
            {
                var isYes = await MainWindow.ShowContentDialogAsync(
                    "Warning",
                    "It appears that there is no labeling data for this file.\nIn this case, the bounding boxes for each class will not be displayed, and only the bounding boxes for the coordinates output directly from the model will be displayed.\r\nTo display bounding boxes for each class, replace the labeling data.\r\nDo you want to continue?",
                    "Yes",
                    "No"
                );

                result = isYes ? ContentDialogResult.Secondary : ContentDialogResult.None;
            }

            if (result == ContentDialogResult.Primary || result == ContentDialogResult.Secondary)
            {
                var exportWithClasses = result == ContentDialogResult.Primary;
                var postfix = exportWithClasses ? "with_classes" : "without_classes";

                var imageFile = GetOriginalImage(dataModel.root);

                var csvFile = dataModel.labelingDataPath;

                var resultFileName = "";
                var imgFileSplit = dataModel.imgFile.Split(@"\");
                var imgFileWithOutExt = imgFileSplit[imgFileSplit.Length - 1].Split(dataModel.imgFile.Contains(".jpg") ? ".jpg" : ".jpeg");

                if (imgFileWithOutExt.Length == 1)
                {
                    imgFileWithOutExt = imgFileSplit[imgFileSplit.Length - 1].Split(".png");
                }

                resultFileName = imgFileWithOutExt[0];

                var txtFile = $@"{dataModel.root}\{imgFileSplit[imgFileSplit.Length - 1]}.txt";

                dataModel.showProgress = Visibility.Visible;

                if (imageFile == "")
                {
                    string[] allowedTypes = [".jpg", ".jpeg", ".png"];

                    imageFile = await ShowFilePickerDialog("No Input File", "The input file could not be found.\nIt appears that segmentation was performed using an older version of Romanowsky Stain Slide Analyzer, or the file was deleted.\nWould you like to load the input file manually?", allowedTypes);

                    if (imageFile == "")
                    {
                        dataModel.showProgress = Visibility.Collapsed;
                        return;
                    }

                    await Task.Run(() => {
                        var filePathSplit = imageFile.Split(".");
                        var ext = filePathSplit[filePathSplit.Length - 1];

                        File.Copy(imageFile, $@"{dataModel.root}\input.{ext}");
                    });
                }

                var folderPicker = new FolderPicker();
                var window = App.window;
                var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);

                WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, hWnd);

                folderPicker.ViewMode = PickerViewMode.Thumbnail;
                folderPicker.SuggestedStartLocation = PickerLocationId.Desktop;

                var folder = await folderPicker.PickSingleFolderAsync();

                if (folder != null)
                {
                    using (Bitmap _bmp = new(imageFile))
                    {
                        Bitmap bmp = new(_bmp, new Size(2048, 2048));

                        var exportResult = await helper.ExportWithBBoxes(bmp, exportWithClasses ? csvFile : txtFile, exportWithClasses, folder.Path, $"{resultFileName}_{postfix}", exportWithClasses);

                        await MainWindow.ShowContentDialogAsync(
                            exportResult == "" ? "Done" : "Error",
                            exportResult == "" ? $@"Image Saved to {folder.Path}\{resultFileName}_{postfix}.png" : $"An error occurred while exporting the image.\nError: {exportResult}",
                            "OK"
                        );
                    }
                }

                dataModel.showProgress = Visibility.Collapsed;
            }
        }

        private async void OnLabelingOptionClick(object sender, RoutedEventArgs e)
        {
            HistoryDataModel dataModel = (sender as Button).DataContext as HistoryDataModel;
            var imageFile = GetOriginalImage(dataModel.root);
            var csvFile = dataModel.labelingDataPath;

            var csvFileSplit = csvFile.Split(@"\");
            var outputFile = csvFileSplit[csvFileSplit.Length - 1].Split(".csv")[0];

            if (imageFile == "")
            {
                string[] allowedTypes = [".jpg", ".jpeg", ".png"];

                imageFile = await ShowFilePickerDialog("No Input File", "The input file could not be found.\nIt appears that segmentation was performed using an older version of Romanowsky Stain Slide Analyzer, or the file was deleted.\nWould you like to load the input file manually?", allowedTypes);
                
                if(imageFile == "")
                {
                    return;
                }

                var filePathSplit = imageFile.Split(".");
                var ext = filePathSplit[filePathSplit.Length - 1];

                File.Copy(imageFile, $@"{dataModel.root}\input.{ext}");

                ActivateAnalyzeWindow(imageFile, csvFile, helper.IsMaskAvailable(dataModel.root, outputFile));
            }
            else
            {
                ActivateAnalyzeWindow(imageFile, csvFile, helper.IsMaskAvailable(dataModel.root, outputFile));
            }
        }

        private async Task<string> ShowFilePickerDialog(string title, string message, string[] allowedTypes)
        {
            var filePath = "";
            ContentDialogResult result;

            var dialogResult = await MainWindow.ShowContentDialogAsync
            (
                title,
                message,
                "Yes",
                "No"
            );

            result = dialogResult ? ContentDialogResult.Primary : ContentDialogResult.Secondary;

            if (result == ContentDialogResult.Primary)
            {
                var filePicker = new FileOpenPicker();
                var window = App.window;
                var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);

                WinRT.Interop.InitializeWithWindow.Initialize(filePicker, hWnd);

                filePicker.ViewMode = PickerViewMode.Thumbnail;
                filePicker.SuggestedStartLocation = PickerLocationId.Desktop;

                foreach (var type in allowedTypes)
                {
                    filePicker.FileTypeFilter.Add(type);
                }

                var file = await filePicker.PickSingleFileAsync();

                if (file != null)
                {
                    filePath = file.Path;
                }
            }

            return filePath;
        }

        private async void ChangeLabelingData(object sender, RoutedEventArgs e)
        {
            HistoryDataModel dataModel = (sender as MenuFlyoutItem).DataContext as HistoryDataModel;
            string[] allowedType = [".csv"];
            var originalCSVFileSplit = dataModel.imgFile.Split(@"\");
            var fileName = originalCSVFileSplit[originalCSVFileSplit.Length - 1];

            switch ((sender as MenuFlyoutItem).Name)
            {
                case "btn_changeLabelingData":
                    var newCSVPath = await ShowFilePickerDialog("Change Labeling Data", "Changing the labeling data file can cause unexpected errors.\nIf possible, use a modified copy of the file generated by Romanowsky Stain Slide Analyzer, or use a file of the same format as that file.\nDo you want to continue?", allowedType);

                    if (newCSVPath != "")
                    {
                        var validateResult = helper.ValidateLabeledData(newCSVPath);

                        if (!validateResult)
                        {
                            await MainWindow.ShowContentDialogAsync(
                                "Error",
                                "This file is either not in the same format as the csv file generated by Romanowsky Stain Slide Analyzer, or the file may be in use.\nCSV files with different formats can cause problems with various functions such as Analyze, Export, etc.\nIf the file is in use, close the file and try again.\nIf this file is not in the same format as the file generated by Romanowsky Stain Slide Analyzer, export the data from another labeled file and re-create the csv file using that file's format as reference.",
                                "OK"
                            );

                            return;
                        }

                        try
                        {
                            File.Copy(newCSVPath, $@"{dataModel.root}\{fileName}.csv", true);
                            await MainWindow.ShowContentDialogAsync("Done", "The file has been copied.", "OK");

                            GetHistory();
                        }
                        catch (Exception ex)
                        {
                            await MainWindow.ShowContentDialogAsync("Error", $"An error occurred while copying the file.\n{ex.Message}", "OK");
                        }
                    }

                    break;
                case "btn_changeLabelingMaskData":
                    var type = await MainWindow.ShowContentDialogAsync(
                        "Select Import Type",
                        "Choose how to load the Mask data.",
                        "Single File",
                        "Multiple Files",
                        "Cancel"
                    );

                    if (type == ContentDialogResult.Primary)
                    {
                        var newData = await ShowFilePickerDialog("Change Labeled Mask Data", "Changing the Mask Data may cause unexpected errors.\nIf possible, import the Mask Data automatically generated by Romanowsky Stain Slide Analyzer.\nIf the file name of the Mask Data is modified, the Mask Data will not be copied.\nThe file name format of the Mask Data should be as follows:\nMask_Labeled.csv", allowedType);
                        
                        try
                        {
                            helper.ChangeMaskData(newData, dataModel.root, fileName);
                            await MainWindow.ShowContentDialogAsync(
                                "Change Labeled Mask Data",
                                "The requested task has been completed.",
                                "OK"
                            );

                            GetHistory();
                        }

                        catch (Exception ex)
                        {
                            await MainWindow.ShowContentDialogAsync("Error", $"An error occurred while copying the file.\nRestart the software or, if the file is in use, close it and try again.\nError: {ex.Message}", "OK");
                        }
                    }
                    else if (type == ContentDialogResult.Secondary)
                    {
                        var isContinue = await MainWindow.ShowContentDialogAsync(
                            "Change Mask Data", "Changing the Mask Data may cause unexpected errors.\nIf possible, import the Mask Data automatically generated by Romanowsky Stain Slide Analyzer.\nIf the file name of the Mask Data is modified, the Mask Data will not be copied.\nThe file name format of the Mask Data should be as follows:\nMask_Labeled_index.csv", "OK", "Cancel"
                        );

                        if(isContinue)
                        {
                            var folderPicker = new FolderPicker();
                            var window = App.window;
                            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
                            StorageFolder? folder = null;

                            try
                            {
                                WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, hWnd);

                                folderPicker.ViewMode = PickerViewMode.Thumbnail;
                                folderPicker.SuggestedStartLocation = PickerLocationId.Desktop;

                                folder = await folderPicker.PickSingleFolderAsync();

                                if(folder != null)
                                {
                                    await helper.ChangeMaskData(folder.Path, dataModel.root, fileName, true);
                                    await MainWindow.ShowContentDialogAsync(
                                        "Change Labeled Mask Data",
                                        "The requested task has been completed.",
                                        "OK"
                                    );

                                    GetHistory();
                                }
                            }

                            catch (Exception ex)
                            {
                                await MainWindow.ShowContentDialogAsync("Error", $"An error occurred while copying the file.\nRestart the software or, if the file is in use, close it and try again.\nError: {ex.Message}", "OK");
                            }
                        }
                    }
                    break;

                case "btn_changeMaskData":
                    var isChange = await MainWindow.ShowContentDialogAsync(
                        "Change Mask Data",
                        "Changing the Mask Data may cause unexpected errors.\nIf possible, import the Mask Data automatically generated by Romanowsky Stain Slide Analyzer.\nIf the file name of the Mask Data is modified, the Mask Data will not be copied.\nThe file name format of the Mask Data should be as follows:\nmask_index.csv",
                        "Continue",
                        "Cancel"
                    );

                    if(isChange)
                    {
                        var folderPicker = new FolderPicker();
                        var window = App.window;
                        var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
                        StorageFolder? folder = null;

                        try
                        {
                            WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, hWnd);

                            folderPicker.ViewMode = PickerViewMode.Thumbnail;
                            folderPicker.SuggestedStartLocation = PickerLocationId.Desktop;

                            folder = await folderPicker.PickSingleFolderAsync();

                            if (folder != null)
                            {
                                await helper.ChangeMaskData(folder.Path, dataModel.root, fileName, false);
                                await MainWindow.ShowContentDialogAsync(
                                    "Change Mask Data",
                                    "The requested task has been completed.",
                                    "OK"
                                );

                                GetHistory();
                            }
                        }

                        catch(Exception exe)
                        {
                            await MainWindow.ShowContentDialogAsync("Error", $"An error occurred while copying the file.\nRestart the software or, if the file is in use, close it and try again.\nError: {exe.Message}", "OK");
                        }
                    }

                    break;
            }

            
        }

        private async void OnRelabelingOptionClick(object sender, RoutedEventArgs e)
        {
            HistoryDataModel dataModel = (sender as MenuFlyoutItem).DataContext as HistoryDataModel;

            var labelingViewModel = new LabelingViewModel();
            string originalImage = GetOriginalImage(dataModel.root);
            var imgFileSplit = dataModel.imgFile.Split(@"\");

            var isMaskAvailable = helper.IsMaskAvailable(dataModel.root, imgFileSplit[imgFileSplit.Length - 1]);
            var isBBoxAvailable = helper.IsBBoxAvailable(dataModel.root, imgFileSplit[imgFileSplit.Length - 1]);

            labelingViewModel.Source = new BitmapImage(new Uri(originalImage));
            LabelingWindow labelingWindow = new(labelingViewModel, $@"{dataModel.root}\{imgFileSplit[imgFileSplit.Length - 1]}.txt", isMaskAvailable, isBBoxAvailable);
            labelingWindow.Activate();
        }

        private void Refresh(object sender, RoutedEventArgs e)
        {
            GetHistory();
        }

        private void ActivateAnalyzeWindow(string imageFile, string csvFile, bool IsMaskAvailable)
        {
            AnalyzeBBoxView bBoxView = new(imageFile, csvFile);
            bBoxView.Activate();
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

        private string GetImage(string path)
        {
            var jpgFiles = Directory.GetFiles($@"{path}\", "*.jpg");
            var jpegFiles = Directory.GetFiles($@"{path}\", "*.jpeg");
            var pngFiles = Directory.GetFiles($@"{path}\", "*.png");

            var file = "";

            if (jpgFiles.Length == 1 && !jpgFiles[0].Contains(@"\input.jpg"))
            {
                file = jpgFiles[0];
            }
            else if (jpgFiles.Length == 2)
            {
                file = jpgFiles[0].Contains(@"\input.jpg") ? jpgFiles[1] : jpgFiles[0];
            }
            else if (jpegFiles.Length == 1 && !jpegFiles[0].Contains(@"\input.jpeg"))
            {
                file = jpegFiles[0];
            }
            else if (jpegFiles.Length == 2)
            {
                file = jpegFiles[0].Contains(@"\input.jpeg") ? jpegFiles[1] : jpegFiles[0];
            }

            else if (pngFiles.Length == 1 && !pngFiles[0].Contains(@"\input.png"))
            {
                file = pngFiles[0];
            }
            else if (pngFiles.Length == 2)
            {
                file = pngFiles[0].Contains(@"\input.png") ? pngFiles[1] : pngFiles[0];
            }

            return file;
        }
    }
}
