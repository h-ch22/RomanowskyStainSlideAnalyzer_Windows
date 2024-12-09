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
                new("Segmentation", "- Manually Segmentation is now available.\n- Extract Bounding Boxes option is now available for extracting bounding box coordinates.\n- Labeling is now available.")
            );

            ReleaseNotes.Add(
                new("Select Points", "- You can segment only that part by clicking on the part you want to segment with the mouse.\n- You can specify the class of the area to be segmented.")
            );

            ReleaseNotes.Add(
                new("Labeling", "- You can specify a class for each bounding box area.\n- You can export the labeled file to a csv file.")
            );

            ReleaseNotes.Add(
                new("History", "- You can save the result image file.\n- You can re-save the labeled csv file.\n- Fixed an issue where records would not display properly when the date was changed.\n- Records are now displayed in most recent order.")
            );

            ReleaseNotes.Add(
                new("Settings", "- You can check and update each library version.")
            );
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
