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
using RomanowskyStainSlideAnalyzer.Analyze.Helper;

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
                else
                {
                    if(Datas.Count < 1)
                    {
                        emptyPanel.Visibility = Visibility.Visible;
                    }

                    else
                    {
                        listView.Visibility = Visibility.Visible;
                    }
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

        private async Task GetHistory()
        {
            ToggleAppBarProgress(true);
            if (Datas.Count > 0) Datas.Clear();

            ShowProgress = Visibility.Visible;

            var month = Date.Month < 10 ? $"0{Date.Month}" : Date.Month.ToString();
            var day = Date.Day < 10 ? $"0{Date.Day}" : Date.Day.ToString();
            var year = Date.Year;

            var date = $"{month}_{day}_{year}";

            await Task.Run(async () =>
            {
                await GetHistory(date);
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

                ToggleAppBarProgress(false);
            });
        }

        private async Task GetHistory(string date)
        {
            Datas = await helper.GetHistory(date);

            DispatcherQueue.TryEnqueue(() =>
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
            DispatcherQueue.TryEnqueue(() => ToggleAppBarProgress(true));

            var file = GetImage(dataModel.root);

            var folder = await MainWindow.ShowSaveDialog(
                type == 0 ? new List<string> { "Comma-Separated Values (CSV) File" } : new List<string> { "Portable Network Graphics (PNG) File", "Joint Photographic Experts Group (JPG) File", "Joint Photographic Experts Group (JPEG) File" },
                type == 0 ? new List<string> { ".csv" } : new List<string> { ".png", ".jpg", ".jpeg" }
            );

            if (folder != null)
            {                
                try
                {
                    var splitPath = (type == 0 ? dataModel.labelingDataPath : file).Split(@"\");
                    var fileName = splitPath[splitPath.Length - 1];

                    helper.Copy(type == 0 ? dataModel.labelingDataPath : file, folder.Path);
                }
                catch (Exception ex)
                {
                    await MainWindow.ShowContentDialogAsync(
                        "Error", $"An error occurred while saving the file.\nPlease check if the file already exists or re-run the software.\nError: {ex.Message}", "OK"
                    );
                }
            }

            DispatcherQueue.TryEnqueue(() => ToggleAppBarProgress(false));
        }

        private async void OnSaveImageClick(object sender, RoutedEventArgs e)
        {
            HistoryDataModel dataModel = (sender as Button).DataContext as HistoryDataModel;
            await Save(1, dataModel);
        }

        private void ToggleAppBarProgress(bool isShow)
        {
            appBarProgress.Visibility = isShow ? Visibility.Visible : Visibility.Collapsed;
            btn_refresh.IsEnabled = isShow ? false : true;
            datePicker.IsEnabled = isShow ? false : true;
        }

        private async void OnSaveLabelingDataClick(object sender, RoutedEventArgs e)
        {
            HistoryDataModel dataModel = (sender as MenuFlyoutItem).DataContext as HistoryDataModel;

            ToggleAppBarProgress(true);

            var folderPicker = new FolderPicker();
            var window = App.window;
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
            StorageFolder? folder = null;

            switch ((sender as MenuFlyoutItem).Name)
            {
                case "btn_saveLabelingData":
                    await Task.Run(async () => {
                        await Save(0, dataModel);
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
                        var result = await helper.CopyMaskLabelingData($@"{dataModel.root}\Masks\{imgFile[imgFile.Length - 1]}", folder.Path);

                        if (Directory.GetFiles($@"{dataModel.root}\Masks\{imgFile[imgFile.Length - 1]}", "Mask_Labeled_*.csv").Length > 0)
                        {
                            LabelingHelper.writePythonFile(false, folder.Path);
                        }

                        if (File.Exists($@"{dataModel.root}\Masks\{imgFile[imgFile.Length - 1]}\Mask_Labeled.csv"))
                        {
                            LabelingHelper.writePythonFile(true, folder.Path);
                        }

                        await MainWindow.ShowContentDialogAsync(
                            result == "" ? "Done" : "Error",
                            result == "" ? "The requested task has been completed." : $"An error occurred while saving data.\nError: {result}",
                            "OK"
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

            ToggleAppBarProgress(false);

        }

        private async void SaveImageWithBBoxes(object sender, RoutedEventArgs e)
        {
            DispatcherQueue.TryEnqueue(() => ToggleAppBarProgress(true));

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

                var imageFile = GetOriginalImage(dataModel.root);

                var csvFile = dataModel.labelingDataPath;

                var imgFileSplit = dataModel.imgFile.Split(@"\");
                var txtFile = $@"{dataModel.root}\{imgFileSplit[imgFileSplit.Length - 1]}.txt";

                if (imageFile == "")
                {
                    string[] allowedTypes = [".jpg", ".jpeg", ".png"];

                    imageFile = await ShowFilePickerDialog("No Input File", "The input file could not be found.\nIt appears that segmentation was performed using an older version of Romanowsky Stain Slide Analyzer, or the file was deleted.\nWould you like to load the input file manually?", allowedTypes);

                    if (imageFile == "")
                    {
                        return;
                    }

                    await Task.Run(() => {
                        var filePathSplit = imageFile.Split(".");
                        var ext = filePathSplit[filePathSplit.Length - 1];

                        File.Copy(imageFile, $@"{dataModel.root}\input.{ext}");
                    });
                }

                var folder = await MainWindow.ShowSaveDialog(
                    new List<string> { "Portable Network Graphics (PNG) File", "Joint Photographic Experts Group (JPG) File", "Joint Photographic Experts Group (JPEG) File" },
                    new List<string> { ".png", ".jpg", ".jpeg" }
                );

                if (folder != null)
                {
                    using (Bitmap _bmp = new(imageFile))
                    {
                        Bitmap bmp = new(_bmp, new Size(2048, 2048));
                        var exportResult = await helper.ExportWithBBoxes(bmp, exportWithClasses ? csvFile : txtFile, exportWithClasses, folder.Path, exportWithClasses);
                    }
                }

                DispatcherQueue.TryEnqueue(() => ToggleAppBarProgress(false));
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

                ActivateAnalyzeWindow(imageFile, csvFile, dataModel.LabeledMaskPropertiesVisibility == Visibility.Visible ? true : false);
            }
            else
            {
                ActivateAnalyzeWindow(imageFile, csvFile, dataModel.LabeledMaskPropertiesVisibility == Visibility.Visible ? true : false);
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
            DispatcherQueue.TryEnqueue(() => ToggleAppBarProgress(true));

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
                        "Choose how to load the Mask data.\nIf you want to analyze this data, select Multiple Files.",
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

            DispatcherQueue.TryEnqueue(() => ToggleAppBarProgress(false));
        }

        private async void SaveWithLabeledMasks(object sender, RoutedEventArgs e)
        {
            DispatcherQueue.TryEnqueue(() => ToggleAppBarProgress(true));

            HistoryDataModel dataModel = (sender as MenuFlyoutItem).DataContext as HistoryDataModel;

            ContentDialogResult result;

            if(dataModel.LabeledMaskPropertiesVisibility == Visibility.Visible)
            {
                result = await MainWindow.ShowContentDialogAsync(
                    "Warning",
                    "Is this file properly labeled?\nIf it is not labeled or not properly labeled, the mask may not be drawn properly.\nIf this file is not properly labeled, click the Export without class button.\nExport without class uses the same color for the mask, rather than differentiating colors by class.",
                    "Export with Class",
                    "Export without Class",
                    "Cancel"
                );
            }

            else
            {
                var withOutClassResult = await MainWindow.ShowContentDialogAsync(
                    "Save Image with Masks",
                    "Since there is no labeled mask data, the mask is inserted with the same class color.\nIf you want to insert different mask colors for each class, proceed with labeling.\nDo you want to continue?",
                    "Yes",
                    "No"
                );

                result = withOutClassResult ? ContentDialogResult.Secondary : ContentDialogResult.None;
            }


            if (result == ContentDialogResult.Primary || result == ContentDialogResult.Secondary)
            {
                var imageFile = GetOriginalImage(dataModel.root);

                var csvFile = dataModel.labelingDataPath;

                var imgFileSplit = dataModel.imgFile.Split(@"\");
                var imgFileWithOutExt = imgFileSplit[imgFileSplit.Length - 1].Split(dataModel.imgFile.Contains(".jpg") ? ".jpg" : ".jpeg");

                if (imgFileWithOutExt.Length == 1)
                {
                    imgFileWithOutExt = imgFileSplit[imgFileSplit.Length - 1].Split(".png");
                }

                var txtFile = $@"{dataModel.root}\{imgFileSplit[imgFileSplit.Length - 1]}.txt";

                if (imageFile == "")
                {
                    string[] allowedTypes = [".jpg", ".jpeg", ".png"];

                    imageFile = await ShowFilePickerDialog("No Input File", "The input file could not be found.\nIt appears that segmentation was performed using an older version of Romanowsky Stain Slide Analyzer, or the file was deleted.\nWould you like to load the input file manually?", allowedTypes);

                    if (imageFile == "")
                    {
                        return;
                    }

                    await Task.Run(() => {
                        var filePathSplit = imageFile.Split(".");
                        var ext = filePathSplit[filePathSplit.Length - 1];

                        File.Copy(imageFile, $@"{dataModel.root}\input.{ext}");
                    });
                }

                var folder = await MainWindow.ShowSaveDialog(
                    new List<string> { "Portable Network Graphics (PNG) File", "Joint Photographic Experts Group (JPG) File", "Joint Photographic Experts Group (JPEG) File" },
                    new List<string> { ".png", ".jpg", ".jpeg" }
                );

                if (folder != null)
                {
                    using (Bitmap _bmp = new(imageFile))
                    {
                        Bitmap bmp = new(_bmp, new Size(2048, 2048));

                        var exportResult = await helper.ExportWithMasks(bmp, $@"{dataModel.root}\Masks\{imgFileSplit[imgFileSplit.Length - 1]}", result == ContentDialogResult.Primary);
                        var postfix = result == ContentDialogResult.Primary ? "with_classes" : "without_classes";

                        if (exportResult != null)
                        {
                            exportResult.Save(folder.Path);
                            exportResult.Dispose();
                        }

                        await MainWindow.ShowContentDialogAsync(
                            exportResult != null ? "Done" : "Error",
                            exportResult != null ? $@"Image Saved to {folder.Path}" : $"An error occurred while exporting the image.\nPlease restart the software, or segment it again.",
                            "OK"
                        );
                    }
                }

            }

            DispatcherQueue.TryEnqueue(() => ToggleAppBarProgress(false));
        }

        private async void Delete(object sender, RoutedEventArgs e)
        {
            HistoryDataModel dataModel = (sender as MenuFlyoutItem).DataContext as HistoryDataModel;

            var result = await MainWindow.ShowContentDialogAsync("Delete", "Deleting data will remove this data and cannot be undone.\nDo you want to continue?", "Yes", "No");

            if(result)
            {
                DirectoryInfo di = new DirectoryInfo(dataModel.root);

                foreach (FileInfo file in di.GetFiles())
                {
                    file.Delete();
                }
                foreach (DirectoryInfo dir in di.GetDirectories())
                {
                    dir.Delete(true);
                }

                di.Delete(true);                
                await MainWindow.ShowContentDialogAsync("Done", "History has been removed.", "OK");
                await GetHistory();
            }

        }

        private async void OnRelabelingOptionClick(object sender, RoutedEventArgs e)
        {
            HistoryDataModel dataModel = (sender as MenuFlyoutItem).DataContext as HistoryDataModel;

            var labelingViewModel = new LabelingViewModel();
            string originalImage = GetOriginalImage(dataModel.root);
            var imgFileSplit = dataModel.imgFile.Split(@"\");

            var csvFile = dataModel.labelingDataPath;

            if (originalImage == "")
            {
                string[] allowedTypes = [".jpg", ".jpeg", ".png"];

                originalImage = await ShowFilePickerDialog("No Input File", "The input file could not be found.\nIt appears that segmentation was performed using an older version of Romanowsky Stain Slide Analyzer, or the file was deleted.\nWould you like to load the input file manually?", allowedTypes);

                if (originalImage == "")
                {
                    return;
                }

                await Task.Run(() => {
                    var filePathSplit = originalImage.Split(".");
                    var ext = filePathSplit[filePathSplit.Length - 1];

                    File.Copy(originalImage, $@"{dataModel.root}\input.{ext}");
                });
            }

            var isMaskAvailable = helper.IsMaskAvailable(dataModel.root, imgFileSplit[imgFileSplit.Length - 1]);
            var isBBoxAvailable = helper.IsBBoxAvailable(dataModel.root, imgFileSplit[imgFileSplit.Length - 1]);

            labelingViewModel.Source = new BitmapImage(new Uri(originalImage));
            LabelingWindow labelingWindow = new(labelingViewModel, $@"{dataModel.root}\{imgFileSplit[imgFileSplit.Length - 1]}.txt", isMaskAvailable, isBBoxAvailable);
            labelingWindow.Activate();
            labelingWindow.Closed += async (_, __) =>
            {
                await GetHistory();
            };
        }

        private async void Refresh(object sender, RoutedEventArgs e)
        {
            await GetHistory();
        }

        private void ActivateAnalyzeWindow(string imageFile, string csvFile, bool IsMaskAvailable)
        {
            AnalyzeBBoxView bBoxView = new(imageFile, csvFile, IsMaskAvailable);
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

        private async void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            HistoryDataModel dataModel = (sender as MenuFlyoutItem).DataContext as HistoryDataModel;
            var name = (sender as MenuFlyoutItem).Name;

            string target = name == "btn_original" ? GetOriginalImage(dataModel.root) : dataModel.imgFile;

            if(name == "btn_original" && target == "")
            {
                string[] allowedTypes = [".jpg", ".jpeg", ".png"];

                target = await ShowFilePickerDialog("No Input File", "The input file could not be found.\nIt appears that segmentation was performed using an older version of Romanowsky Stain Slide Analyzer, or the file was deleted.\nWould you like to load the input file manually?", allowedTypes);

                if (target == "")
                {
                    return;
                }

                await Task.Run(() => {
                    var filePathSplit = target.Split(".");
                    var ext = filePathSplit[filePathSplit.Length - 1];

                    File.Copy(target, $@"{dataModel.root}\input.{ext}");
                });
            }

            ImageView imgView = new(target, dataModel.log);
            imgView.Activate();
        }
    }
}
