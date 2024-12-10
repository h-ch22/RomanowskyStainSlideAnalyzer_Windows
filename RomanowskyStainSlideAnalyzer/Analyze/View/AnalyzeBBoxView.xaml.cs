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

        public AnalyzeBBoxView(string ImagePath, string CSVPath)
        {
            this.InitializeComponent();

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
            var csvPath = viewModel.CSVPath;

            Thread thread = new(() =>
            {
                var data = helper.GetData(csvPath);

                DispatcherQueue.TryEnqueue(() =>
                {
                    viewModel.ShowProgress = Visibility.Collapsed;
                    scrollView.Visibility = Visibility.Visible;
                    Datas = data;
                    listView.ItemsSource = Datas;
                });
            });

            thread.Start();
        }

        private void listView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int idx = (sender as ListView).SelectedIndex;
            LabelingDataModel dataModel = Datas[idx];

            viewModel.Index = dataModel.id;
            viewModel.Class = dataModel.classId;
            viewModel.X = dataModel.x;
            viewModel.Y = dataModel.y;
            viewModel.Width = dataModel.width;
            viewModel.Height = dataModel.height;

            CreateBBox(dataModel.x, dataModel.y, dataModel.width, dataModel.height);
            Analyze(dataModel.x, dataModel.y, dataModel.width, dataModel.height);

            if (viewModel.IsZoomModeEnabled && img_scrollView.ZoomFactor <= 1F)
            {
                img_scrollView.ZoomTo(3F, new(float.Parse(dataModel.x), float.Parse(dataModel.y)));
            }
            else
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

        private (Color, int, int, int, int, float, float, float) GetAvg(Bitmap bmp)
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
                    Color clr = bmp.GetPixel(x, y);
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

            return (Color.FromArgb(r, g, b), a, r, g, b, brightness, hue, saturation);
        }

        private void CreateBBox(string x, string y, string width, string height)
        {
            foreach (var child in canvas.Children)
            {
                if (child.GetType() == typeof(Microsoft.UI.Xaml.Shapes.Rectangle))
                {
                    canvas.Children.Remove(child);
                }
            }

            Microsoft.UI.Xaml.Shapes.Rectangle bBox = new()
            {
                Width = int.Parse(width),
                Height = int.Parse(height),
                Stroke = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 219, 66, 66))
            };

            canvas.Children.Add(bBox);
            bBox.SetValue(Canvas.LeftProperty, Double.Parse(x));
            bBox.SetValue(Canvas.TopProperty, Double.Parse(y));
        }
    }
}
