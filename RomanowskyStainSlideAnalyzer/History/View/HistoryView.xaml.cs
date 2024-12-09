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

        private void GetHistory()
        {
            if(Datas.Count > 0) Datas.Clear();

            ShowProgress = Visibility.Visible;

            var month = Date.Month < 10 ? $"0{Date.Month}" : Date.Month.ToString();
            var day = Date.Day < 10 ? $"0{Date.Day}" : Date.Day.ToString();
            var year = Date.Year;

            var date = $"{month}_{day}_{year}";

            Thread thread = new(() => GetHistory(date));
            thread.Start();

            DispatcherQueue.TryEnqueue(async () =>
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

        private async void OnClick(object sender, RoutedEventArgs e)
        {
            HistoryDataModel dataModel = (sender as Button).DataContext as HistoryDataModel;
            var file = "";

            var jpgFiles = Directory.GetFiles($@"{dataModel.root}\", "*.jpg");
            var jpegFiles = Directory.GetFiles($@"{dataModel.root}\", "*.jpeg");
            var pngFiles = Directory.GetFiles($@"{dataModel.root}\", "*.png");

            if (jpgFiles.Length > 0) file = jpgFiles[0];
            else if (jpegFiles.Length > 0) file = jpegFiles[0];
            else file = pngFiles[0];

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
                    helper.Copy((sender as Button).Name == "btn_saveLabelingData" ? dataModel.labelingDataPath : file, folder.Path);
                }
                catch (Exception ex)
                {
                    ShowAlert("Error", $"An error occurred while saving the file.\nPlease check if the file already exists or re-run the software.\nError: {ex.Message}");
                }
            }
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
