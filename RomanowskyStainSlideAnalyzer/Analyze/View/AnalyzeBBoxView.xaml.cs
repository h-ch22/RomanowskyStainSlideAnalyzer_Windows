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
        private ObservableCollection<LabelingDataModel> Datas;
        private string csvPath;
        private string imagePath;
        private string path;
        private bool showBBoxColor = true;

        public AnalyzeBBoxView(string ImagePath, string CSVPath)
        {
            this.InitializeComponent();

            csvPath = CSVPath;
            imagePath = ImagePath;

            gridView.DataContext = viewModel;
            viewModel.ImagePath = ImagePath;
            viewModel.CSVPath = CSVPath;

            IntPtr hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            var windowId = Win32Interop.GetWindowIdFromWindow(hwnd);

            AppWindow appWindow = AppWindow.GetFromWindowId(windowId);
            appWindow.Resize(new Windows.Graphics.SizeInt32(1280, 1000));
            Init();
        }

        private async void Init()
        {
            ExtendsContentIntoTitleBar = true;
            SystemBackdrop = new MicaBackdrop() { Kind = Microsoft.UI.Composition.SystemBackdrops.MicaKind.BaseAlt };
            SetTitleBar(AppTitleBar);
            SetData();
        }

        private void SetData()
        {
            Thread thread = new Thread(GetData);
            thread.Start();

            Thread viewThread = new Thread(GetAllAvg);
            viewThread.Start();
        }

        private void GetData()
        {
            var data = helper.GetData(csvPath);

            DispatcherQueue.TryEnqueue(() =>
            {
                viewModel.ShowProgress = Visibility.Collapsed;
                scrollView.Visibility = Visibility.Visible;
                Datas = data;
                listView.ItemsSource = Datas;
            });
        }

        private void listView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int idx = (sender as ListView).SelectedIndex;
            LabelingDataModel dataModel = Datas[idx];

            viewModel.Index = dataModel.id;
            viewModel.Class = dataModel.classId;
            viewModel.X = dataModel.x;
            viewModel.Y = dataModel.y;
            viewModel.Size = (float.Parse(dataModel.width) * float.Parse(dataModel.height)).ToString();
            viewModel.Width = dataModel.width;
            viewModel.Height = dataModel.height;

            if(btn_hideBBox.IsChecked == false)
            {
                CreateBBox(dataModel.x, dataModel.y, dataModel.width, dataModel.height, dataModel.classId);
            }

            Analyze(dataModel.x, dataModel.y, dataModel.width, dataModel.height);

            if (viewModel.IsZoomModeEnabled && img_scrollView.ZoomFactor <= 1F)
            {
                img_scrollView.ZoomTo(3F, new(float.Parse(dataModel.x), float.Parse(dataModel.y)));
            }
            else if (viewModel.IsZoomModeEnabled)
            {
                double zoomFactor = img_scrollView.ZoomFactor;

                double viewportWidth = img_scrollView.ViewportWidth;
                double viewportHeight = img_scrollView.ViewportHeight;

                double scrollOffsetX = (double.Parse(dataModel.x)) * zoomFactor - (viewportWidth / 2);
                double scrollOffsetY = (double.Parse(dataModel.y)) * zoomFactor - (viewportHeight / 2);

                img_scrollView.ScrollTo(scrollOffsetX, scrollOffsetY);
            }
        }

        private void Analyze(string x, string y, string width, string height)
        {
            NoSelectionPanel.Visibility = Visibility.Collapsed;

            if (int.Parse(width) == 0 || int.Parse(height) == 0)
            {
                ZeroWHPanel.Visibility = Visibility.Visible;
                colorPanel.Visibility = Visibility.Collapsed;
                croppedImg.Visibility = Visibility.Collapsed;
                avgPanel.Visibility = Visibility.Visible;
            }
            else
            {
                ZeroWHPanel.Visibility = Visibility.Collapsed;
                colorPanel.Visibility = Visibility.Visible;

                viewModel.ShowAnalyzeProgress = Visibility.Visible;

                using (Bitmap _bmp = new(viewModel.ImagePath))
                {
                    Bitmap bmp = new(_bmp, new Size(512, 512));
                    Rectangle cropRect = new(int.Parse(x), int.Parse(y), int.Parse(width), int.Parse(height));

                    using (Bitmap croppedImage = bmp.Clone(cropRect, bmp.PixelFormat))
                    {
                        var avgData = GetAvg(croppedImage);
                        var croppedBmp = bmp.Clone(cropRect, bmp.PixelFormat);
                        var bmpImage = new BitmapImage();

                        using (MemoryStream ms = new())
                        {
                            croppedBmp.Save(ms, ImageFormat.Png);
                            ms.Position = 0;
                            bmpImage.SetSource(ms.AsRandomAccessStream());
                        }

                        viewModel.Average = avgData.Item1.ToString();
                        viewModel.AvgA = avgData.Item2.ToString();
                        viewModel.AvgR = avgData.Item3.ToString();
                        viewModel.AvgG = avgData.Item4.ToString();
                        viewModel.AvgB = avgData.Item5.ToString();
                        viewModel.AvgBrightness = avgData.Item6.ToString();
                        viewModel.AvgHue = avgData.Item7.ToString();
                        viewModel.AvgSaturation = avgData.Item8.ToString();

                        viewModel.CroppedImage = bmpImage;
                        viewModel.ShowAnalyzeProgress = Visibility.Collapsed;
                        NoSelectionPanel.Visibility = Visibility.Collapsed;
                        croppedImg.Visibility = Visibility.Visible;
                        avgPanel.Visibility = Visibility.Visible;
                    }
                }
            }
        }

        private void GetAllAvg()
        {
            DispatcherQueue.TryEnqueue(() =>
            {
                statisticsProgressPanel.Visibility = Visibility.Visible;
                statisticsPanel.Visibility = Visibility.Collapsed;
            });

            float a = 0f;
            float r = 0f;
            float g = 0f;
            float b = 0f;

            float brightness = 0.0f;
            float hue = 0.0f;
            float saturation = 0.0f;
            float total = 0f;

            float widthAll = 0f;
            float heightAll = 0f;
            float sizeAll = 0f;

            float aNone = 0f;
            float rNone = 0f;
            float gNone = 0f;
            float bNone = 0f;

            float brightnessNone = 0.0f;
            float hueNone = 0.0f;
            float saturationNone = 0.0f;
            float totalNone = 0f;

            float widthNone = 0f;
            float heightNone = 0f;
            float sizeNone = 0f;

            float aLCell = 0f;
            float rLCell = 0f;
            float gLCell = 0f;
            float bLCell = 0f;

            float brightnessLCell = 0.0f;
            float hueLCell = 0.0f;
            float saturationLCell = 0.0f;
            float totalLCell = 0f;

            float widthLCell = 0f;
            float heightLCell = 0f;
            float sizeLCell = 0f;

            float aSCell = 0f;
            float rSCell = 0f;
            float gSCell = 0f;
            float bSCell = 0f;

            float brightnessSCell = 0.0f;
            float hueSCell = 0.0f;
            float saturationSCell = 0.0f;
            float totalSCell = 0f;

            float widthSCell = 0f;
            float heightSCell = 0f;
            float sizeSCell = 0f;


            using (Bitmap _bmp = new(viewModel.ImagePath))
            {
                Bitmap bmp = new(_bmp, new Size(512, 512));

                foreach(var data in Datas)
                {
                    var x = int.Parse(data.x);
                    var y = int.Parse(data.y);
                    var width = int.Parse(data.width);
                    var height = int.Parse(data.height);

                    widthAll += float.Parse(data.width);
                    heightAll += float.Parse(data.height);
                    sizeAll += float.Parse(data.width) * float.Parse(data.height);

                    if(width == 0 || height == 0)
                    {
                        continue;
                    }

                    Rectangle cropRect = new(x, y, width, height);

                    using (Bitmap croppedImage = bmp.Clone(cropRect, bmp.PixelFormat))
                    {
                        for (int _x = 0; _x < bmp.Width; _x++)
                        {
                            for (int _y = 0; _y < bmp.Height; _y++)
                            {
                                System.Drawing.Color clr = bmp.GetPixel(_x, _y);
                                a += clr.A;
                                r += clr.R;
                                g += clr.G;
                                b += clr.B;
                                brightness += clr.GetBrightness();
                                hue += clr.GetHue();
                                saturation += clr.GetSaturation();

                                total++;

                                switch(data.classId)
                                {
                                    case "None":
                                        aNone += clr.A;
                                        rNone += clr.R;
                                        gNone += clr.G;
                                        bNone += clr.B;
                                        brightnessNone += clr.GetBrightness();
                                        hueNone += clr.GetHue();
                                        saturationNone += clr.GetSaturation();

                                        widthNone += float.Parse(data.width);
                                        heightNone += float.Parse(data.height);
                                        sizeNone += (float.Parse(data.width) * float.Parse(data.height));

                                        totalNone++;

                                        break;

                                    case "Large Cell":
                                        aLCell += clr.A;
                                        rLCell += clr.R;
                                        gLCell += clr.G;
                                        bLCell += clr.B;
                                        brightnessLCell += clr.GetBrightness();
                                        hueLCell += clr.GetHue();
                                        saturationLCell += clr.GetSaturation();

                                        widthLCell += float.Parse(data.width);
                                        heightLCell += float.Parse(data.height);
                                        sizeLCell += (float.Parse(data.width) * float.Parse(data.height));

                                        totalLCell++;

                                        break;

                                    case "Small Cell":
                                        aSCell += clr.A;
                                        rSCell += clr.R;
                                        gSCell += clr.G;
                                        bSCell += clr.B;
                                        brightnessSCell += clr.GetBrightness();
                                        hueSCell += clr.GetHue();
                                        saturationSCell += clr.GetSaturation();

                                        widthSCell += float.Parse(data.width);
                                        heightSCell += float.Parse(data.height);
                                        sizeSCell += (float.Parse(data.width) * float.Parse(data.height));

                                        totalSCell++;

                                        break;

                                    default: break;
                                }
                            }
                        }
                    }
                }

                brightness /= total;
                hue /= total;
                saturation /= total;

                brightnessNone /= totalNone;
                hueNone /= totalNone;
                saturationNone /= totalNone;

                brightnessLCell /= totalLCell;
                hueLCell /= totalLCell;
                saturationLCell /= totalLCell;

                brightnessSCell /= totalSCell;
                hueSCell /= totalSCell;
                saturationSCell /= totalSCell;

                sizeAll /= Datas.Count;
                sizeNone /= totalNone;
                sizeLCell /= totalLCell;
                sizeSCell /= totalSCell;

                widthAll /= Datas.Count;
                heightAll /= Datas.Count;

                widthNone /= totalNone;
                heightNone /= totalNone;

                widthLCell /= totalLCell;
                heightLCell /= totalLCell;

                widthSCell /= totalSCell;
                heightSCell /= totalSCell;

                int aAsInt = GetAvg(a, total);
                int rAsInt = GetAvg(r, total);
                int gAsInt = GetAvg(g, total);
                int bAsInt = GetAvg(b, total);

                int aNoneAsInt = GetAvg(aNone, totalNone);
                int rNoneAsInt = GetAvg(rNone, totalNone);
                int gNoneAsInt = GetAvg(gNone, totalNone);
                int bNoneAsInt = GetAvg(bNone, totalNone);

                int aLCellAsInt = GetAvg(aLCell, totalLCell);
                int rLCellAsInt = GetAvg(rLCell, totalLCell);
                int gLCellAsInt = GetAvg(gLCell, totalLCell);
                int bLCellAsInt = GetAvg(bLCell, totalLCell);

                int aSCellAsInt = GetAvg(aSCell, totalSCell);
                int rSCellAsInt = GetAvg(rSCell, totalSCell);
                int gSCellAsInt = GetAvg(gSCell, totalSCell);
                int bSCellAsInt = GetAvg(bSCell, totalSCell);

                DispatcherQueue.TryEnqueue(() =>
                {
                    viewModel.AverageAll = System.Drawing.Color.FromArgb(aAsInt, rAsInt, gAsInt, bAsInt).ToString();
                    viewModel.AvgAAll = aAsInt.ToString();
                    viewModel.AvgRAll = rAsInt.ToString();
                    viewModel.AvgGAll = gAsInt.ToString();
                    viewModel.AvgBAll = bAsInt.ToString();

                    viewModel.AvgBrightnessAll = brightness.ToString();
                    viewModel.AvgHueAll = hue.ToString();
                    viewModel.AvgSaturationAll = saturation.ToString();

                    viewModel.AverageNone = System.Drawing.Color.FromArgb(aNoneAsInt, rNoneAsInt, gNoneAsInt, bNoneAsInt).ToString();
                    viewModel.AvgANone = aNoneAsInt.ToString();
                    viewModel.AvgRNone = rNoneAsInt.ToString();
                    viewModel.AvgGNone = gNoneAsInt.ToString();
                    viewModel.AvgBNone = bNoneAsInt.ToString();

                    viewModel.AvgBrightnessNone = brightnessNone.ToString();
                    viewModel.AvgHueNone = hueNone.ToString();
                    viewModel.AvgSaturationNone = saturationNone.ToString();

                    viewModel.AverageLCell = System.Drawing.Color.FromArgb(aLCellAsInt, rLCellAsInt, gLCellAsInt, bLCellAsInt).ToString();
                    viewModel.AvgALCell = aLCellAsInt.ToString();
                    viewModel.AvgRLCell = rLCellAsInt.ToString();
                    viewModel.AvgGLCell = gLCellAsInt.ToString();
                    viewModel.AvgBLCell = bLCellAsInt.ToString();

                    viewModel.AvgBrightnessLCell = brightnessLCell.ToString();
                    viewModel.AvgHueLCell = hueLCell.ToString();
                    viewModel.AvgSaturationLCell = saturationLCell.ToString();

                    viewModel.AverageSCell = System.Drawing.Color.FromArgb(aSCellAsInt, rSCellAsInt, gSCellAsInt, bSCellAsInt).ToString();
                    viewModel.AvgASCell = aSCellAsInt.ToString();
                    viewModel.AvgRSCell = rSCellAsInt.ToString();
                    viewModel.AvgGSCell = gSCellAsInt.ToString();
                    viewModel.AvgBSCell = bSCellAsInt.ToString();

                    viewModel.AvgBrightnessSCell = brightnessSCell.ToString();
                    viewModel.AvgHueSCell = hueSCell.ToString();
                    viewModel.AvgSaturationSCell = saturationSCell.ToString();

                    viewModel.AvgWidthAll = widthAll.ToString();
                    viewModel.AvgHeightAll = heightAll.ToString();
                    viewModel.AvgSizeAll = sizeAll.ToString();

                    viewModel.AvgWidthNone = widthNone.ToString();
                    viewModel.AvgHeightNone = heightNone.ToString();
                    viewModel.AvgSizeNone = sizeNone.ToString();

                    viewModel.AvgWidthLCell = widthLCell.ToString();
                    viewModel.AvgHeightLCell = heightLCell.ToString();
                    viewModel.AvgSizeLCell = sizeLCell.ToString();

                    viewModel.AvgWidthSCell = widthSCell.ToString();
                    viewModel.AvgHeightSCell = heightSCell.ToString();
                    viewModel.AvgSizeSCell = sizeSCell.ToString();

                    statisticsProgressPanel.Visibility = Visibility.Collapsed;
                    statisticsPanel.Visibility = Visibility.Visible;
                    btn_exportData.IsEnabled = true;
                });
            }
        }

        private (System.Drawing.Color, int, int, int, int, float, float, float) GetAvg(Bitmap bmp)
        {
            int a = 0;
            int r = 0;
            int g = 0;
            int b = 0;

            float brightness = 0.0f;
            float hue = 0.0f;
            float saturation = 0.0f;
            int total = 0;

            for(int x = 0; x < bmp.Width; x++)
            {
                for(int y = 0; y < bmp.Height; y++)
                {
                    System.Drawing.Color clr = bmp.GetPixel(x, y);
                    a += clr.A;
                    r += clr.R;
                    g += clr.G;
                    b += clr.B;
                    brightness += clr.GetBrightness();
                    hue += clr.GetHue();
                    saturation += clr.GetSaturation();

                    total++;
                }
            }

            a /= total;
            r /= total;
            g /= total;
            b /= total;

            brightness /= total;
            hue /= total;
            saturation /= total;

            return (System.Drawing.Color.FromArgb(a, r, g, b), a, r, g, b, brightness, hue, saturation);
        }

        private int GetAvg(float target, float total)
        {
            if (target == 0f || total == 0f) return 0;

            var result = target / total;
            var resultAsInt = Convert.ToInt32(result);

            resultAsInt = resultAsInt > 255 ? 255 : resultAsInt;
            resultAsInt = resultAsInt < 0 ? 0 : resultAsInt;

            return resultAsInt;
        }

        private void DeleteBBox()
        {
            foreach (var child in canvas.Children)
            {
                if (child.GetType() == typeof(Microsoft.UI.Xaml.Shapes.Rectangle))
                {
                    canvas.Children.Remove(child);
                }
            }
        }

        private void CreateBBox(string x, string y, string width, string height, string className)
        {
            DeleteBBox();

            Microsoft.UI.Xaml.Shapes.Rectangle bBox = new()
            {
                Width = int.Parse(width),
                Height = int.Parse(height),
                Stroke = new SolidColorBrush(showBBoxColor ? ToMediaColor(LabelingHelper.GetBoundingBoxColor(className)) : Windows.UI.Color.FromArgb(255, 219, 66, 66))
            };

            canvas.Children.Add(bBox);
            bBox.SetValue(Canvas.LeftProperty, Double.Parse(x));
            bBox.SetValue(Canvas.TopProperty, Double.Parse(y));
        }

        private Windows.UI.Color ToMediaColor(System.Drawing.Color color)
        {
            return Windows.UI.Color.FromArgb(color.A, color.R, color.G, color.B);
        }

        private void AppBarToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            if (btn_hideBBox.IsChecked == true)
            {
                if(canvas != null)
                {
                    DeleteBBox();
                }

                btn_toggleBBoxColor.IsEnabled = false;
            }

            else
            {
                btn_toggleBBoxColor.IsEnabled = true;

                if (listView.SelectedIndex > -1)
                {
                    int idx = listView.SelectedIndex;
                    LabelingDataModel dataModel = Datas[idx];

                    CreateBBox(dataModel.x, dataModel.y, dataModel.width, dataModel.height, dataModel.classId);
                }
            }
        }

        private void ToggleBBoxColor(object sender, RoutedEventArgs e)
        {
            if (canvas != null)
            {
                DeleteBBox();
            }

            showBBoxColor = btn_toggleBBoxColor.IsChecked == true;

            if (listView != null && listView.SelectedIndex > -1)
            {
                int idx = listView.SelectedIndex;
                LabelingDataModel dataModel = Datas[idx];

                CreateBBox(dataModel.x, dataModel.y, dataModel.width, dataModel.height, dataModel.classId);
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
                    ShowAlert("Done", $"Analyze data was exported to {folder.Path}.");

                    btn_exportData.Visibility = Visibility.Visible;
                    appBarProgress.Visibility = Visibility.Collapsed;
                }
                catch (Exception ex)
                {
                    ShowAlert("Error", $"An error occurred while saving the file.\nPlease check if the file already exists or re-run the software.\nError: {ex.Message}");
                    btn_exportData.Visibility = Visibility.Visible;
                    appBarProgress.Visibility = Visibility.Collapsed;
                }
            }
        }

        private async Task Export()
        {
            AnalyzeByClassDataModel[] data = [
                new("All", viewModel.AvgAAll, viewModel.AvgRAll, viewModel.AvgGAll, viewModel.AvgBAll, viewModel.AvgHueAll, viewModel.AvgSaturationAll, viewModel.AvgBrightnessAll, viewModel.AvgWidthAll, viewModel.AvgHeightAll, viewModel.AvgSizeAll),
                new("None", viewModel.AvgANone, viewModel.AvgRNone, viewModel.AvgGNone, viewModel.AvgBNone, viewModel.AvgHueNone, viewModel.AvgSaturationNone, viewModel.AvgBrightnessNone, viewModel.AvgWidthNone, viewModel.AvgHeightNone, viewModel.AvgSizeNone),
                new("Large Cell", viewModel.AvgALCell, viewModel.AvgRLCell, viewModel.AvgGLCell, viewModel.AvgBLCell, viewModel.AvgHueLCell, viewModel.AvgSaturationLCell, viewModel.AvgBrightnessLCell, viewModel.AvgWidthLCell, viewModel.AvgHeightLCell, viewModel.AvgSizeLCell),
                new("Small Cell", viewModel.AvgASCell, viewModel.AvgRSCell, viewModel.AvgGSCell, viewModel.AvgBSCell, viewModel.AvgHueSCell, viewModel.AvgSaturationSCell, viewModel.AvgBrightnessSCell, viewModel.AvgWidthSCell, viewModel.AvgHeightSCell, viewModel.AvgSizeSCell)
            ];

            List<AnalyzeDataModel> allData = new();

            var tasks = Datas.Select(async d =>
            {
                return await Task.Run(async () =>
                {
                    using (Bitmap _bmp = new(imagePath))
                    {
                        Bitmap bmp = new(_bmp, new Size(512, 512));
                        Rectangle cropRect = new(int.Parse(d.x), int.Parse(d.y), int.Parse(d.width), int.Parse(d.height));

                        if (int.Parse(d.width) != 0 && int.Parse(d.height) != 0)
                        {
                            using (Bitmap croppedImage = bmp.Clone(cropRect, bmp.PixelFormat))
                            {
                                var avgData = GetAvg(croppedImage);

                                return new AnalyzeDataModel(d.x, d.y, d.width, d.height, new(d.classId, avgData.Item2.ToString(), avgData.Item3.ToString(), avgData.Item4.ToString(), avgData.Item5.ToString(), avgData.Item7.ToString(), avgData.Item8.ToString(), avgData.Item6.ToString(), avgOfSize: (int.Parse(d.width) * int.Parse(d.height)).ToString()));
                            }
                        }
                        else
                        {
                            return new AnalyzeDataModel(d.x, d.y, d.width, d.height, new(d.classId, "", "", "", "", "", "", "", avgOfSize: "0"));
                        }
                    }

                });
            });

            allData.AddRange(await Task.WhenAll(tasks));

            await Task.Run(() => helper.CreateCSVFile(csvPath, path, data, allData));
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
