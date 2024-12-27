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
                new("Segmentation", "- Extract Masks is now available.\n- An option is available to save the image immediately after segmentation is complete.")
            );

            ReleaseNotes.Add(
                new("History", "- A completely new design for History View is now available.\n- Re-labeling is now available.\n- Save Mask Labeling Data is now available.\n- Label Mask Data is now available.\n- Save the masked image is available.")
            );

            ReleaseNotes.Add(
                new("Labeling", "- Toggle Bounding Box / Mask option is now available.\n- Now when you click the back and next buttons, if there is any saved data it will be displayed.")
            );

            ReleaseNotes.Add(
                new("Analyze", "- The ability to analyze segmentation mask data is now available.\n- Toggle Bounding Box / Mask option is now available.\n- Fixed the issue where the average A value was not displayed properly when exporting data.")
            );
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
