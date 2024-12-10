using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RomanowskyStainSlideAnalyzer.Labeling.Models
{
    public class LabelingDataPreviewViewModel : INotifyPropertyChanged
    {
        private string _ImagePath = "";
        public string ImagePath
        {
            get => _ImagePath;
            set
            {
                _ImagePath = value;
                OnPropertyChanged(nameof(ImagePath));
            }
        }

        private string _CSVPath = "";
        public string CSVPath
        {
            get => _CSVPath;
            set
            {
                _CSVPath = value;
                OnPropertyChanged(nameof(CSVPath));
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

        private ScrollingZoomMode _ZoomMode = ScrollingZoomMode.Enabled;
        public ScrollingZoomMode ZoomMode
        {
            get => IsZoomModeEnabled ? ScrollingZoomMode.Enabled : ScrollingZoomMode.Disabled;
            set
            {
                _ZoomMode = value;
            }
        }

        private Visibility _ShowProgress = Visibility.Visible;
        public Visibility ShowProgress
        {
            get => _ShowProgress;
            set
            {
                _ShowProgress = value;
                OnPropertyChanged(nameof(ShowProgress));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
