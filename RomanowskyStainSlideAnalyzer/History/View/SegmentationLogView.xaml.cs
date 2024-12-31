using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using RomanowskyStainSlideAnalyzer.History.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace RomanowskyStainSlideAnalyzer.History.View
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SegmentationLogView : Window
    {
        private List<LogDataModel> logData;
        private string log;

        public SegmentationLogView(string log)
        {
            this.InitializeComponent();
            this.log = log;

            IntPtr hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            var windowId = Win32Interop.GetWindowIdFromWindow(hwnd);

            AppWindow appWindow = AppWindow.GetFromWindowId(windowId);
            appWindow.Resize(new Windows.Graphics.SizeInt32(800, 850));
            Init();
        }

        private void Init()
        {
            ExtendsContentIntoTitleBar = true;
            SystemBackdrop = new MicaBackdrop() { Kind = Microsoft.UI.Composition.SystemBackdrops.MicaKind.BaseAlt };
            SetTitleBar(AppTitleBar);

            SetLog();
        }

        private void SetLog()
        {
            logData = new();

            Task.Run(() =>
            {
                StringReader reader = new(log);
                string line;

                while((line = reader.ReadLine()) != null)
                {
                    var header = line.Split(":")[0].Replace(Environment.NewLine, "");
                    var contents = line.Split(":")[1].Replace(Environment.NewLine, "");
                    contents = contents.Substring(1, contents.Length - 1);

                    var data = new LogDataModel(header, contents == "" ? "N/A" : contents);
                    logData.Add(data);
                }

                DispatcherQueue.TryEnqueue(() =>
                {
                    logListView.ItemsSource = logData;
                });
                
            });
        }
    }
}
