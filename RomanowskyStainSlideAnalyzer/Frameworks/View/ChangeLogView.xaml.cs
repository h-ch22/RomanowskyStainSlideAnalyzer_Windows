using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System.ComponentModel;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using System.Collections.ObjectModel;
using RomanowskyStainSlideAnalyzer.Frameworks.Models;
using System.Reflection;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace RomanowskyStainSlideAnalyzer.Frameworks.View
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ChangeLogView : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private ObservableCollection<ReleaseNoteDataModel> _ReleaseNotes = new();
        public ObservableCollection<ReleaseNoteDataModel> ReleaseNotes
        {
            get => _ReleaseNotes;
            set
            {
                _ReleaseNotes = value;
                OnPropertyChanged(nameof(ReleaseNotes));
            }
        }

        private string _Version = $"Romanowsky Stain Slide Analyzer {Assembly.GetExecutingAssembly().GetName().Version.ToString()} Release Note";
        public string Version
        {
            get => _Version;
        }

        public ChangeLogView()
        {
            this.InitializeComponent();
            ExtendsContentIntoTitleBar = true;

            IntPtr hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            var windowId = Win32Interop.GetWindowIdFromWindow(hwnd);

            AppWindow appWindow = AppWindow.GetFromWindowId(windowId);
            appWindow.Resize(new Windows.Graphics.SizeInt32(800, 600));
            SystemBackdrop = new MicaBackdrop() { Kind = Microsoft.UI.Composition.SystemBackdrops.MicaKind.BaseAlt };
            SetTitleBar(AppTitleBar);

            gridView.DataContext = this;

            ReleaseNotes.Add(
                new("Select Points", "- Fixed an issue where points were not displayed properly in Manually Segmentation mode")
            );

            ReleaseNotes.Add(
                new("History", "- Labeling Data Change is now available.\n- Analyze Bounding Box is now available.\n- Export Image with Bounding Boxes is now available.")    
            );

            ReleaseNotes.Add(
                new("Labeling", "- Toggle Bounding Box Show/Hide is now available.\n- Fixed an issue where blank spaces were appearing at the beginning of each piece of data.\n- Automatic Zoom & Scroll is now available.\n- Fixed an issue where labeling would be reset even if the Cancel button was pressed in the dialog that appears\nwhen there are already labeled files.")    
            );

            ReleaseNotes.Add(
                new("Analyze", "- The Analyze feature is now available to view and export Color, Hue, Saturation, Brightness by Bounding Box and\nthe overall Color, Hue, Saturation, Brightness average.")
            );
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
