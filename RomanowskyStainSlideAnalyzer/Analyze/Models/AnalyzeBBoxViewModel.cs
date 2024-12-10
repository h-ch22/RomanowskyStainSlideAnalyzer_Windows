using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RomanowskyStainSlideAnalyzer.Analyze.Models
{
    class AnalyzeBBoxViewModel: INotifyPropertyChanged
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

        private Visibility _ShowAnalyzeProgress = Visibility.Collapsed;
        public Visibility ShowAnalyzeProgress
        {
            get => _ShowAnalyzeProgress;
            set
            {
                _ShowAnalyzeProgress = value;
                OnPropertyChanged(nameof(ShowAnalyzeProgress));
            }
        }

        private BitmapImage _CroppedImage;
        public BitmapImage CroppedImage
        {
            get => _CroppedImage;
            set
            {
                _CroppedImage = value;
                OnPropertyChanged(nameof(CroppedImage));
            }
        }

        private string _Average;
        public string Average
        {
            get => _Average;
            set
            {
                _Average = value;
                OnPropertyChanged(nameof(Average));
            }
        }

        private string _AvgA;
        public string AvgA
        {
            get => _AvgA;
            set
            {
                _AvgA = value;
                OnPropertyChanged(nameof(AvgA));
            }
        }

        private string _AvgR;
        public string AvgR
        {
            get => _AvgR;
            set
            {
                _AvgR = value;
                OnPropertyChanged(nameof(AvgR));
            }
        }

        private string _AvgG;
        public string AvgG
        {
            get => _AvgG;
            set
            {
                _AvgG = value;
                OnPropertyChanged(nameof(AvgG));
            }
        }

        private string _AvgB;
        public string AvgB
        {
            get => _AvgB;
            set
            {
                _AvgB = value;
                OnPropertyChanged(nameof(AvgB));
            }
        }

        private string _AvgHue;
        public string AvgHue
        {
            get => _AvgHue;
            set
            {
                _AvgHue = value;
                OnPropertyChanged(nameof(AvgHue));
            }
        }

        private string _AvgSaturation;
        public string AvgSaturation
        {
            get => _AvgSaturation;
            set
            {
                _AvgSaturation = value;
                OnPropertyChanged(nameof(AvgSaturation));
            }
        }

        private string _AvgBrightness;
        public string AvgBrightness
        {
            get => _AvgBrightness;
            set
            {
                _AvgBrightness = value;
                OnPropertyChanged(nameof(AvgBrightness));
            }
        }

        private string _Class;
        public string Class
        {
            get => _Class;
            set
            {
                _Class = value;
                OnPropertyChanged(nameof(Class));
            }
        }

        private string _Index;
        public string Index
        {
            get => _Index;
            set
            {
                _Index = value;
                OnPropertyChanged(nameof(Index));
            }
        }

        private string _X;
        public string X
        {
            get => _X;
            set
            {
                _X = value;
                OnPropertyChanged(nameof(X));
            }
        }

        private string _Y;
        public string Y
        {
            get => _Y;
            set
            {
                _Y = value;
                OnPropertyChanged(nameof(Y));
            }
        }

        private string _Width;
        public string Width
        {
            get => _Width;
            set
            {
                _Width = value;
                OnPropertyChanged(nameof(Width));
            }
        }

        private string _Height;
        public string Height
        {
            get => _Height;
            set
            {
                _Height = value;
                OnPropertyChanged(nameof(Height));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
