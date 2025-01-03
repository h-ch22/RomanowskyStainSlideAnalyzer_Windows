using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RomanowskyStainSlideAnalyzer.Labeling.Models
{
    public class LabelingViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private BitmapImage _Source;
        public BitmapImage Source
        {
            get => _Source;
            set
            {
                _Source = value;
                OnPropertyChanged(nameof(Source));
            }
        }

        private bool _IsZoomModeEnabled = true;
        public bool IsZoomModeEnabled
        {
            get => _IsZoomModeEnabled;
            set
            {
                _IsZoomModeEnabled = value;
                OnPropertyChanged(nameof(IsZoomModeEnabled));
                OnPropertyChanged(nameof(ZoomMode));
            }
        }

        private ScrollingZoomMode _ZoomMode;
        public ScrollingZoomMode ZoomMode
        {
            get => IsZoomModeEnabled ? ScrollingZoomMode.Enabled : ScrollingZoomMode.Disabled;
            set
            {
                _ZoomMode = value;
            }
        }

        private string _CurrentBoundingBox;
        public string CurrentBoundingBox
        {
            get => _CurrentBoundingBox;
            set
            {
                _CurrentBoundingBox = value;
                OnPropertyChanged(nameof(CurrentBoundingBox));
            }
        }

        private int _AllIndex = 0;
        public int AllIndex
        {
            get => _AllIndex;
            set
            {
                _AllIndex = value;
                OnPropertyChanged(nameof(AllIndex));
                OnPropertyChanged(nameof(CurrentBoundingBox));
            }
        }

        private int _CurrentIndex = 1;
        public int CurrentIndex
        {
            get => _CurrentIndex;
            set
            {
                _CurrentIndex = value;
                OnPropertyChanged(nameof(CurrentIndex));
            }
        }

        private bool _UseBBoxAsTarget = false;
        public bool UseBBoxAsTarget
        {
            get => _UseBBoxAsTarget;
            set
            {
                _UseBBoxAsTarget = value;
                OnPropertyChanged(nameof(UseBBoxAsTarget));
            }
        }

        private bool _IsAppBarEnabled = true;
        public bool IsAppBarEnabled
        {
            get => _IsAppBarEnabled;
            set
            {
                _IsAppBarEnabled = value;
                OnPropertyChanged(nameof(IsAppBarEnabled));
            }
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
