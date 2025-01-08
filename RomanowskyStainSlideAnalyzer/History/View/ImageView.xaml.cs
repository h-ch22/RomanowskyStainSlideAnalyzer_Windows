using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using RomanowskyStainSlideAnalyzer.Frameworks.View;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Provider;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace RomanowskyStainSlideAnalyzer.History.View
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ImageView : Window
    {
        private string path = "";
        private string log = "";

        public ImageView(string path, string log)
        {
            this.InitializeComponent();
            this.path = path;
            this.log = log;

            IntPtr hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            var windowId = Win32Interop.GetWindowIdFromWindow(hwnd);

            AppWindow appWindow = AppWindow.GetFromWindowId(windowId);
            appWindow.Resize(new Windows.Graphics.SizeInt32(640, 640));

            ExtendsContentIntoTitleBar = true;
            SystemBackdrop = new MicaBackdrop() { Kind = Microsoft.UI.Composition.SystemBackdrops.MicaKind.BaseAlt };
            SetTitleBar(AppTitleBar);

            img_source.Source = new BitmapImage(new Uri(path));
        }

        private async void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            switch((sender as AppBarButton).Name)
            {
                case "btn_info":
                    SegmentationLogView logView = new(log);
                    logView.Activate();
                    break;

                case "btn_zoomIn":
                    scrollView.ZoomTo((float)(scrollView.ZoomFactor + 0.1), new(256, 256));
                    break;

                case "btn_zoomOut":
                    scrollView.ZoomTo((float)(scrollView.ZoomFactor - 0.1), new(256, 256));
                    break;

                case "btn_rotate":
                    img_source.CenterPoint = new(256, 256, 0);
                    img_source.Rotation += 90;
                    break;

                case "btn_save":
                    var file = await MainWindow.ShowSaveDialog(
                        new List<string> { "Portable Network Graphics (PNG) File", "Joint Photographic Experts Group (JPG) File", "Joint Photographic Experts Group (JPEG) File" },
                        new List<string> { ".png", ".jpg", ".jpeg" }
                    );

                    if(file != null)
                    {
                        File.Copy(path, file.Path, true);
                    }
                    break;
            }
        }
    }
}
