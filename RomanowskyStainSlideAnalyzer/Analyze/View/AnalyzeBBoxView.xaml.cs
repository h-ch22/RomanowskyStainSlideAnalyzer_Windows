using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using RomanowskyStainSlideAnalyzer.Labeling.Models;
using System.Threading;
using RomanowskyStainSlideAnalyzer.Analyze.Models;
using RomanowskyStainSlideAnalyzer.Labeling.Helper;
using System.Collections.ObjectModel;
using Microsoft.UI.Xaml.Media.Imaging;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;
using System.Diagnostics;
using RomanowskyStainSlideAnalyzer.History.Models;
using Windows.Storage.Pickers;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Windows.UI;
using RomanowskyStainSlideAnalyzer.Analyze.Helper;
using RomanowskyStainSlideAnalyzer.Home.Helper;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Numerics;
using RomanowskyStainSlideAnalyzer.Frameworks.View;
using System.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace RomanowskyStainSlideAnalyzer.Analyze.View
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AnalyzeBBoxView : Window
    {
        private AnalyzeBBoxViewModel viewModel = new();
        private LabelingHelper helper = new();
        private AnalyzeHelper analyzeHelper = new();
        private ObservableCollection<LabelingDataModel> Datas;
        private ObservableCollection<LabelingDataModel> MaskDatas;

        private string path = "";
        private string maskPath = "";
        private bool showBBoxColor = true;

        public AnalyzeBBoxView(string ImagePath, string CSVPath, bool IsMaskAvailable)
        {
            InitializeComponent();

            gridView.DataContext = viewModel;
            viewModel.ImagePath = ImagePath;
            viewModel.CSVPath = CSVPath;
            viewModel.IsMaskAvailable = IsMaskAvailable;

            if (!IsMaskAvailable)
            {
                viewModel.MaskDataVisibility = Visibility.Collapsed;
            }
            else
            {
                GetMaskPath();
            }

            IntPtr hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            var windowId = Win32Interop.GetWindowIdFromWindow(hwnd);

            AppWindow appWindow = AppWindow.GetFromWindowId(windowId);
            appWindow.Resize(new Windows.Graphics.SizeInt32(1350, 1000));
            Init();
        }

        private async void Init()
        {
            ExtendsContentIntoTitleBar = true;
            SystemBackdrop = new MicaBackdrop() { Kind = Microsoft.UI.Composition.SystemBackdrops.MicaKind.BaseAlt };
            SetTitleBar(AppTitleBar);

            await SetData();
        }

        private async Task SetData()
        {
            await Task.Run(() => GetData());
            GetAllAvg();
        }

        private async void GetData()
        {
            var data = helper.GetData(viewModel.CSVPath);

            DispatcherQueue.TryEnqueue(() =>
            {
                Datas = data;

                if (!viewModel.IsMaskAvailable)
                {
                    viewModel.ShowProgress = Visibility.Collapsed;
                    scrollView.Visibility = Visibility.Visible;
                    listView.ItemsSource = Datas;
                }
            });

            if (viewModel.IsMaskAvailable)
            {
                await Task.Run(() =>
                {
                    var maskPath = GetMaskPath();
                    var data = analyzeHelper.GetData(maskPath);

                    DispatcherQueue.TryEnqueue(() =>
                    {
                        MaskDatas = data;
                        viewModel.ShowProgress = Visibility.Collapsed;
                        scrollView.Visibility = Visibility.Visible;
                        listView.ItemsSource = Datas.Count > MaskDatas.Count ? Datas : MaskDatas;
                    });
                });

            }
        }

        private string GetMaskPath()
        {
            var csvPathSplit = viewModel.CSVPath.Split(@"\");
            var fileName = csvPathSplit[csvPathSplit.Length - 1].Split(".csv")[0];
            var root = csvPathSplit[csvPathSplit.Length - 2];
            maskPath = $@"C:\RomanowskyStainSlideAnalyzer\{root}\Masks\{fileName}";

            return maskPath;
        }

        private void listView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int idx = (sender as ListView).SelectedIndex;

            AnalyzeColor(idx);
        }

        private void AnalyzeColor(int idx)
        {
            if (idx > -1)
            {
                LabelingDataModel dataModel = idx < Datas.Count ? Datas[idx] : MaskDatas[idx];

                viewModel.Index = dataModel.id;
                viewModel.Class = dataModel.classId;
                viewModel.X = dataModel.x;
                viewModel.Y = dataModel.y;

                Analyze(dataModel.x, dataModel.y, dataModel.width, dataModel.height);

                if (btn_hideBBox.IsChecked == false)
                {
                    if (viewModel.UseBoundingBoxAsTarget || viewModel.ShowAllData)
                    {
                        CreateBBox(dataModel.x, dataModel.y, dataModel.width, dataModel.height, dataModel.classId);
                    }

                    if ((viewModel.ShowAllData && viewModel.IsMaskAvailable) || !viewModel.UseBoundingBoxAsTarget)
                    {
                        var data = AnalyzeHelper.GetMask(maskPath, listView.SelectedIndex.ToString());
                        DrawContours(data);
                    }
                }

                float xRatio = (float)viewModel.Size / 512;
                float yRatio = (float)viewModel.Size / 512;

                int newX = (int)(float.Parse(dataModel.x) * xRatio);
                int newY = (int)(float.Parse(dataModel.y) * yRatio);
                float newWidth = float.Parse(dataModel.width) * xRatio;
                float newHeight = float.Parse(dataModel.height) * yRatio;

                if (viewModel.IsZoomModeEnabled)
                {
                    double viewportWidth = img_scrollView.ViewportWidth;
                    double viewportHeight = img_scrollView.ViewportHeight;

                    if (img_scrollView.ZoomFactor <= 1F)
                    {
                        img_scrollView.ZoomTo(3F, new(newX, newY));
                    }
                    else
                    {
                        double zoomFactor = img_scrollView.ZoomFactor;
                        double scrollOffsetX = Convert.ToDouble(newX) * zoomFactor - (viewportWidth / 2);
                        double scrollOffsetY = Convert.ToDouble(newY) * zoomFactor - (viewportHeight / 2);
                        img_scrollView.ScrollTo(scrollOffsetX, scrollOffsetY);
                    }

                    if (img_maskScrollView.ZoomFactor <= 1F)
                    {
                        img_maskScrollView.ZoomTo(3F, new(newX, newY));
                    }
                    else
                    {
                        double zoomFactor = img_maskScrollView.ZoomFactor;
                        double scrollOffsetX = Convert.ToDouble(newX) * zoomFactor - (viewportWidth / 2);
                        double scrollOffsetY = Convert.ToDouble(newY) * zoomFactor - (viewportHeight / 2);
                        img_maskScrollView.ScrollTo(scrollOffsetX, scrollOffsetY);
                    }

                    if (img_originalScrollView.ZoomFactor <= 1F)
                    {
                        img_originalScrollView.ZoomTo(3F, new(newX, newY));
                    }
                    else
                    {
                        double zoomFactor = img_originalScrollView.ZoomFactor;
                        double scrollOffsetX = Convert.ToDouble(newX) * zoomFactor - (viewportWidth / 2);
                        double scrollOffsetY = Convert.ToDouble(newY) * zoomFactor - (viewportHeight / 2);
                        img_originalScrollView.ScrollTo(scrollOffsetX, scrollOffsetY);
                    }
                }
            }
        }

        private bool IsContour(int[,] data, int x, int y)
        {
            return data[y - 1, x] == 0 || data[y + 1, x] == 0 ||
                   data[y, x - 1] == 0 || data[y, x + 1] == 0;
        }

        private void ShowZeroWHPanel(bool isShow)
        {
            ZeroWHPanel.Visibility = isShow ? Visibility.Visible : Visibility.Collapsed;
            colorPanel.Visibility = isShow ? Visibility.Collapsed : Visibility.Visible;
            croppedImg.Visibility = isShow ? Visibility.Collapsed : Visibility.Visible;
            avgPanel.Visibility = isShow ? Visibility.Visible : Visibility.Collapsed;

            viewModel.ShowAnalyzeProgress = Visibility.Collapsed;
        }

        private async void Analyze(string x, string y, string width, string height)
        {
            NoSelectionPanel.Visibility = Visibility.Collapsed;

            if (int.Parse(width) == 0 || int.Parse(height) == 0)
            {
                ShowZeroWHPanel(true);
            }
            else
            {
                ShowZeroWHPanel(false);

                viewModel.ShowAnalyzeProgress = Visibility.Visible;

                using (Bitmap _bmp = new(viewModel.ImagePath))
                {
                    var bmpImage = new BitmapImage();
                    var maskedImage = new BitmapImage();

                    if (viewModel.UseBoundingBoxAsTarget)
                    {
                        Rectangle cropRect = new(int.Parse(x), int.Parse(y), int.Parse(width), int.Parse(height));

                        var analyzeData = await analyzeHelper.Analyze(x, y, width, height, viewModel.ImagePath);

                        using (var croppedBmp = analyzeData.Item2)
                        {
                            using (MemoryStream ms = new())
                            {
                                croppedBmp.Save(ms, ImageFormat.Png);
                                ms.Position = 0;
                                bmpImage.SetSource(ms.AsRandomAccessStream());
                                croppedBmp.Dispose();
                            }
                        }

                        if(viewModel.UseBoundingBoxAsTarget)
                        {
                            viewModel.Average = analyzeData.Item1;
                        }

                        viewModel.ShowAnalyzeProgress = Visibility.Collapsed;
                    }

                    if(!viewModel.UseBoundingBoxAsTarget || (viewModel.ShowAllData && viewModel.IsMaskAvailable))
                    {
                        var maskData = AnalyzeHelper.GetMask(maskPath, listView.SelectedIndex.ToString());
                        var analyzeData = await analyzeHelper.Analyze(maskData, viewModel.ImagePath);

                        if ((analyzeData.Item1 == null || analyzeData.Item2 == null) && !viewModel.ShowAllData)
                        {
                            ShowZeroWHPanel(true);
                        }

                        else
                        {
                            ShowZeroWHPanel(false);

                            if (analyzeData.Item2 != null)
                            {
                                using (var croppedBmp = analyzeData.Item2)
                                {
                                    using (MemoryStream ms = new())
                                    {
                                        croppedBmp.Save(ms, ImageFormat.Png);
                                        ms.Position = 0;

                                        if (viewModel.ShowAllData)
                                        {
                                            maskedImage.SetSource(ms.AsRandomAccessStream());
                                        }

                                        else
                                        {
                                            bmpImage.SetSource(ms.AsRandomAccessStream());
                                        }
                                        croppedBmp.Dispose();
                                    }
                                }
                            }

                            if (analyzeData.Item1 != null && !viewModel.UseBoundingBoxAsTarget)
                            {
                                viewModel.Average = analyzeData.Item1;
                            }
                        }
                    }

                    if (ZeroWHPanel.Visibility != Visibility.Visible)
                    {
                        viewModel.CroppedImage = bmpImage;
                        viewModel.CroppedMaskImage = maskedImage;
                        viewModel.ShowAnalyzeProgress = Visibility.Collapsed;
                        NoSelectionPanel.Visibility = Visibility.Collapsed;
                        croppedImg.Visibility = Visibility.Visible;

                        if (viewModel.ShowAllData && viewModel.IsMaskAvailable)
                        {
                            croppedMaskImg.Visibility = Visibility.Visible;
                        }
                        avgPanel.Visibility = Visibility.Visible;
                    }
                }
            }
        }

        private async void GetAllAvg()
        {
            statisticsProgressPanel.Visibility = Visibility.Visible;
            statisticsPanel.Visibility = Visibility.Collapsed;
            AllAvgDataModel avgData;

            if (viewModel.UseBoundingBoxAsTarget)
            {
                avgData = await Task.Run(() => analyzeHelper.Analyze(viewModel.ImagePath, Datas.ToList()));
            }

            else
            {
                avgData = await Task.Run(() => analyzeHelper.Analyze(viewModel.ImagePath, MaskDatas.ToList(), GetMaskPath()));
            }

            viewModel.AllAvg = avgData;

            statisticsProgressPanel.Visibility = Visibility.Collapsed;
            statisticsPanel.Visibility = Visibility.Visible;
            btn_exportData.IsEnabled = true;
            btn_useBBox.IsEnabled = viewModel.IsMaskAvailable;
        }

        private void DeleteBBox()
        {
            foreach (var child in canvas.Children)
            {
                if (child.GetType() == typeof(Microsoft.UI.Xaml.Shapes.Rectangle) && (child as Microsoft.UI.Xaml.Shapes.Rectangle).Name == "boundingBox")
                {
                    canvas.Children.Remove(child);
                }
            }
        }

        private void CreateBBox(string x, string y, string width, string height, string className)
        {
            DeleteBBox();

            float xRatio = (float)viewModel.Size / 512;
            float yRatio = (float)viewModel.Size / 512;

            int newX = (int)(float.Parse(x) * xRatio);
            int newY = (int)(float.Parse(y) * yRatio);
            int newWidth = (int)(float.Parse(width) * xRatio);
            int newHeight = (int)(float.Parse(height) * yRatio);

            Microsoft.UI.Xaml.Shapes.Rectangle bBox = new()
            {
                Width = newWidth,
                Height = newHeight,
                Stroke = new SolidColorBrush(showBBoxColor ? ToMediaColor(LabelingHelper.GetBoundingBoxColor(className)) : Windows.UI.Color.FromArgb(255, 219, 66, 66))
            };

            bBox.Name = "boundingBox";

            canvas.Children.Add(bBox);
            bBox.SetValue(Canvas.LeftProperty, Convert.ToDouble(newX));
            bBox.SetValue(Canvas.TopProperty, Convert.ToDouble(newY));
        }

        private void DrawContours(int[,] data)
        {
            DeleteContours();

            float xRatio = (float)viewModel.Size / 512;
            float yRatio = (float)viewModel.Size / 512;

            for (int y = 1; y < 511; y++)
            {
                for (int x = 1; x < 511; x++)
                {
                    if (data[y, x] == 1 && IsContour(data, x, y))
                    {
                        int newX = (int)(x * xRatio);
                        int newY = (int)(y * yRatio);

                        DrawContourPixel(newX, newY);
                    }
                }
            }
        }

        private void DrawContourPixel(int x, int y)
        {
            var rect = new Microsoft.UI.Xaml.Shapes.Rectangle
            {
                Width = 1,
                Height = 1,
                Stroke = new SolidColorBrush(showBBoxColor ? ToMediaColor(LabelingHelper.GetBoundingBoxColor(viewModel.Class)) : Windows.UI.Color.FromArgb(255, 219, 66, 66))
            };

            Canvas.SetLeft(rect, x);
            Canvas.SetTop(rect, y);
            rect.Name = "contour";

            if (viewModel.ShowAllData)
            {
                maskCanvas.Children.Add(rect);
            }
            else
            {
                canvas.Children.Add(rect);
            }
        }

        private void DeleteContours()
        {
            List<Microsoft.UI.Xaml.Shapes.Rectangle> toRemove;

            if (viewModel.ShowAllData)
            {
                toRemove = maskCanvas.Children.OfType<Microsoft.UI.Xaml.Shapes.Rectangle>()
                              .Where(rect => rect.Name == "contour")
                              .ToList();
            }

            else
            {
                toRemove = canvas.Children.OfType<Microsoft.UI.Xaml.Shapes.Rectangle>()
                              .Where(rect => rect.Name == "contour")
                              .ToList();
            }


            foreach (var rect in toRemove)
            {
                if (viewModel.ShowAllData) maskCanvas.Children.Remove(rect);
                else canvas.Children.Remove(rect);
            }
        }

        private Windows.UI.Color ToMediaColor(System.Drawing.Color color)
        {
            return Windows.UI.Color.FromArgb(color.A, color.R, color.G, color.B);
        }

        private void AppBarToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            if (btn_hideBBox.IsChecked == true)
            {
                if (canvas != null)
                {
                    DeleteContours();
                    DeleteBBox();
                }

            }

            else if (btn_hideBBox.IsChecked == false)
            {
                if (listView.SelectedIndex > -1)
                {
                    int idx = listView.SelectedIndex;

                    if (viewModel.UseBoundingBoxAsTarget && idx < Datas.Count)
                    {
                        LabelingDataModel dataModel = Datas[idx];
                        CreateBBox(dataModel.x, dataModel.y, dataModel.width, dataModel.height, dataModel.classId);
                    }

                    if (!viewModel.UseBoundingBoxAsTarget || (viewModel.ShowAllData && viewModel.IsMaskAvailable))
                    {
                        var data = AnalyzeHelper.GetMask(maskPath, idx.ToString());
                        DrawContours(data);
                    }
                }
            }

            btn_toggleBBoxColor.IsEnabled = btn_hideBBox.IsChecked == false;
        }

        private async void ToggleDataTarget(object sender, RoutedEventArgs e)
        {
            if(!viewModel.ShowAllData)
            {
                DeleteBBox();
                DeleteContours();

                listView.ItemsSource = viewModel.UseBoundingBoxAsTarget ? Datas : MaskDatas;
            }

            GetAllAvg();
        }

        private void ToggleBBoxColor(object sender, RoutedEventArgs e)
        {
            if (canvas != null)
            {
                DeleteBBox();
                DeleteContours();
            }

            showBBoxColor = btn_toggleBBoxColor.IsChecked == true;

            if (listView != null && listView.SelectedIndex > -1)
            {
                int idx = listView.SelectedIndex;

                if (viewModel.UseBoundingBoxAsTarget)
                {
                    LabelingDataModel dataModel = Datas[idx];

                    CreateBBox(dataModel.x, dataModel.y, dataModel.width, dataModel.height, dataModel.classId);
                }

                if (!viewModel.UseBoundingBoxAsTarget || (viewModel.ShowAllData && viewModel.IsMaskAvailable))
                {
                    var data = AnalyzeHelper.GetMask(maskPath, idx.ToString());
                    DrawContours(data);
                }
            }
        }

        private async void btn_exportData_Click(object sender, RoutedEventArgs e)
        {
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
                    path = folder.Path;
                    btn_exportData.Visibility = Visibility.Collapsed;
                    appBarProgress.Visibility = Visibility.Visible;

                    await Export();
                    await MainWindow.ShowContentDialogAsync(
                        "Done",
                        $"Analyze data was exported to {folder.Path}.",
                        "OK"
                    );

                    btn_exportData.Visibility = Visibility.Visible;
                    appBarProgress.Visibility = Visibility.Collapsed;
                }
                catch (Exception ex)
                {
                    await MainWindow.ShowContentDialogAsync(
                        "Error",
                        $"An error occurred while saving the file.\nPlease check if the file already exists or re-run the software.\nError: {ex.Message}",
                        "OK"
                    );
                    btn_exportData.Visibility = Visibility.Visible;
                    appBarProgress.Visibility = Visibility.Collapsed;
                }
            }
        }

        private async Task Export()
        {
            var dataToExport = await analyzeHelper.Export(viewModel.AllAvg, Datas, viewModel.ImagePath);
            await Task.Run(() => helper.CreateCSVFile(viewModel.CSVPath, path, dataToExport.Item1, dataToExport.Item2, viewModel.UseBoundingBoxAsTarget));
        }

        private void ToggleAllData(object sender, RoutedEventArgs e)
        {
            viewModel.AllDataVisibility = viewModel.ShowAllData ? Visibility.Visible : Visibility.Collapsed;
            viewModel.MaskDataVisibility = (viewModel.ShowAllData && viewModel.IsMaskAvailable) ? Visibility.Visible : Visibility.Collapsed;
            croppedMaskImg.Visibility = (viewModel.ShowAllData && viewModel.IsMaskAvailable) ? Visibility.Visible : Visibility.Collapsed;

            viewModel.Size = viewModel.ShowAllData ? 256 : 512;
            viewModel.Center = viewModel.ShowAllData ? 128 : 256;

            listView.SelectedIndex = -1;
            DeleteBBox();
            DeleteContours();

            if(viewModel.ShowAllData)
            {
                if (viewModel.IsMaskAvailable) listView.ItemsSource = Datas.Count > MaskDatas.Count ? Datas : MaskDatas;
                else listView.ItemsSource = Datas;
            }
            else
            {
                listView.ItemsSource = viewModel.UseBoundingBoxAsTarget ? Datas : MaskDatas;
            }
        }
    }
}
