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
                new("Framework", "- Saving a single file now displays the File Save Dialog\ninstead of the Folder Picker, allowing you to customize the file's name and extension.")    
            );

            ReleaseNotes.Add(
                new("Segmentation", "- Newly designed HomeView.\n- You can now add files by dragging.\n- You can now segment multiple images.\n- You can now choose which GPU to use for inference.\n- Parallel GPU option is now available.\n- Shortcuts have been applied to some functions.\n- The ability to cancel segmentation is now available.\n- You can now apply the currently set options to all files.\n- Model Selection is now available.")
            );

            ReleaseNotes.Add(
                new("Customize Parameters", "- Parameter Preset is now available.\n- Number Box is now available.")    
            );

            ReleaseNotes.Add(
                new("History", "- The History list is now displayed by file name.\n- Shortcuts have been applied to some functions and menus.\n- Image Viewer is now available.\n- Calendar Date Picker is now available.")
            );

            ReleaseNotes.Add(
                new("Labeling", "- Now you can check the mask and bounding box at the same time.\n- Shortcuts have been applied to some functions.\n- Fixed the issue where Bounding Box indexes were not displayed completely.")
            );

            ReleaseNotes.Add(
                new("Analyze", "- You can now check mask data and bounding box data at the same time.\n- Shortcuts have been applied to some functions.")
            );

            ReleaseNotes.Add(
                new("Settings", "- You can now delete the Preset.")
            );
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
