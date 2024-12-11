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

        private string _Size;
        public string Size
        {
            get => _Size;
            set
            {
                _Size = value;
                OnPropertyChanged(nameof(Size));
            }
        }

        private string _AverageAll;
        public string AverageAll
        {
            get => _AverageAll;
            set
            {
                _AverageAll = value;
                OnPropertyChanged(nameof(AverageAll));
            }
        }

        private string _AvgAAll;
        public string AvgAAll
        {
            get => _AvgAAll;
            set
            {
                _AvgAAll = value;
                OnPropertyChanged(nameof(AvgAAll));
            }
        }

        private string _AvgRAll;
        public string AvgRAll
        {
            get => _AvgRAll;
            set
            {
                _AvgRAll = value;
                OnPropertyChanged(nameof(AvgRAll));
            }
        }

        private string _AvgGAll;
        public string AvgGAll
        {
            get => _AvgGAll;
            set
            {
                _AvgGAll = value;
                OnPropertyChanged(nameof(AvgGAll));
            }
        }

        private string _AvgBAll;
        public string AvgBAll
        {
            get => _AvgBAll;
            set
            {
                _AvgBAll = value;
                OnPropertyChanged(nameof(AvgBAll));
            }
        }

        private string _AvgHueAll;
        public string AvgHueAll
        {
            get => _AvgHueAll;
            set
            {
                _AvgHueAll = value;
                OnPropertyChanged(nameof(AvgHueAll));
            }
        }

        private string _AvgSaturationAll;
        public string AvgSaturationAll
        {
            get => _AvgSaturationAll;
            set
            {
                _AvgSaturationAll = value;
                OnPropertyChanged(nameof(AvgSaturationAll));
            }
        }

        private string _AvgBrightnessAll;
        public string AvgBrightnessAll
        {
            get => _AvgBrightnessAll;
            set
            {
                _AvgBrightnessAll = value;
                OnPropertyChanged(nameof(AvgBrightnessAll));
            }
        }

        private string _AvgWidthAll;
        public string AvgWidthAll
        {
            get => _AvgWidthAll;
            set
            {
                _AvgWidthAll = value;
                OnPropertyChanged(nameof(AvgWidthAll));
            }
        }

        private string _AvgHeightAll;
        public string AvgHeightAll
        {
            get => _AvgHeightAll;
            set
            {
                _AvgHeightAll = value;
                OnPropertyChanged(nameof(AvgHeightAll));
            }
        }

        private string _AvgSizeAll;
        public string AvgSizeAll
        {
            get => _AvgSizeAll;
            set
            {
                _AvgSizeAll = value;
                OnPropertyChanged(nameof(AvgSizeAll));
            }
        }

        private string _AverageNone;
        public string AverageNone
        {
            get => _AverageNone;
            set
            {
                _AverageNone = value;
                OnPropertyChanged(nameof(AverageNone));
            }
        }

        private string _AvgANone;
        public string AvgANone
        {
            get => _AvgANone;
            set
            {
                _AvgANone = value;
                OnPropertyChanged(nameof(AvgANone));
            }
        }

        private string _AvgRNone;
        public string AvgRNone
        {
            get => _AvgRNone;
            set
            {
                _AvgRNone = value;
                OnPropertyChanged(nameof(AvgRNone));
            }
        }

        private string _AvgGNone;
        public string AvgGNone
        {
            get => _AvgGNone;
            set
            {
                _AvgGNone = value;
                OnPropertyChanged(nameof(AvgGNone));
            }
        }

        private string _AvgBNone;
        public string AvgBNone
        {
            get => _AvgBNone;
            set
            {
                _AvgBNone = value;
                OnPropertyChanged(nameof(AvgBNone));
            }
        }

        private string _AvgHueNone;
        public string AvgHueNone
        {
            get => _AvgHueNone;
            set
            {
                _AvgHueNone = value;
                OnPropertyChanged(nameof(AvgHueNone));
            }
        }

        private string _AvgSaturationNone;
        public string AvgSaturationNone
        {
            get => _AvgSaturationNone;
            set
            {
                _AvgSaturationNone = value;
                OnPropertyChanged(nameof(AvgSaturationNone));
            }
        }

        private string _AvgBrightnessNone;
        public string AvgBrightnessNone
        {
            get => _AvgBrightnessNone;
            set
            {
                _AvgBrightnessNone = value;
                OnPropertyChanged(nameof(AvgBrightnessNone));
            }
        }

        private string _AvgWidthNone;
        public string AvgWidthNone
        {
            get => _AvgWidthNone;
            set
            {
                _AvgWidthNone = value;
                OnPropertyChanged(nameof(AvgWidthNone));
            }
        }

        private string _AvgHeightNone;
        public string AvgHeightNone
        {
            get => _AvgHeightNone;
            set
            {
                _AvgHeightNone = value;
                OnPropertyChanged(nameof(AvgHeightNone));
            }
        }

        private string _AvgSizeNone;
        public string AvgSizeNone
        {
            get => _AvgSizeNone;
            set
            {
                _AvgSizeNone = value;
                OnPropertyChanged(nameof(AvgSizeNone));
            }
        }

        private string _AverageLCell;
        public string AverageLCell
        {
            get => _AverageLCell;
            set
            {
                _AverageLCell = value;
                OnPropertyChanged(nameof(AverageLCell));
            }
        }

        private string _AvgALCell;
        public string AvgALCell
        {
            get => _AvgALCell;
            set
            {
                _AvgALCell = value;
                OnPropertyChanged(nameof(AvgALCell));
            }
        }

        private string _AvgRLCell;
        public string AvgRLCell
        {
            get => _AvgRLCell;
            set
            {
                _AvgRLCell = value;
                OnPropertyChanged(nameof(AvgRLCell));
            }
        }

        private string _AvgGLCell;
        public string AvgGLCell
        {
            get => _AvgGLCell;
            set
            {
                _AvgGLCell = value;
                OnPropertyChanged(nameof(AvgGLCell));
            }
        }

        private string _AvgBLCell;
        public string AvgBLCell
        {
            get => _AvgBLCell;
            set
            {
                _AvgBLCell = value;
                OnPropertyChanged(nameof(AvgBLCell));
            }
        }

        private string _AvgHueLCell;
        public string AvgHueLCell
        {
            get => _AvgHueLCell;
            set
            {
                _AvgHueLCell = value;
                OnPropertyChanged(nameof(AvgHueLCell));
            }
        }

        private string _AvgSaturationLCell;
        public string AvgSaturationLCell
        {
            get => _AvgSaturationLCell;
            set
            {
                _AvgSaturationLCell = value;
                OnPropertyChanged(nameof(AvgSaturationLCell));
            }
        }

        private string _AvgBrightnessLCell;
        public string AvgBrightnessLCell
        {
            get => _AvgBrightnessLCell;
            set
            {
                _AvgBrightnessLCell = value;
                OnPropertyChanged(nameof(AvgBrightnessLCell));
            }
        }

        private string _AvgWidthLCell;
        public string AvgWidthLCell
        {
            get => _AvgWidthLCell;
            set
            {
                _AvgWidthLCell = value;
                OnPropertyChanged(nameof(AvgWidthLCell));
            }
        }

        private string _AvgHeightLCell;
        public string AvgHeightLCell
        {
            get => _AvgHeightLCell;
            set
            {
                _AvgHeightLCell = value;
                OnPropertyChanged(nameof(AvgHeightLCell));
            }
        }

        private string _AvgSizeLCell;
        public string AvgSizeLCell
        {
            get => _AvgSizeLCell;
            set
            {
                _AvgSizeLCell = value;
                OnPropertyChanged(nameof(AvgSizeLCell));
            }
        }

        private string _AverageSCell;
        public string AverageSCell
        {
            get => _AverageSCell;
            set
            {
                _AverageSCell = value;
                OnPropertyChanged(nameof(AverageSCell));
            }
        }

        private string _AvgASCell;
        public string AvgASCell
        {
            get => _AvgASCell;
            set
            {
                _AvgASCell = value;
                OnPropertyChanged(nameof(AvgASCell));
            }
        }

        private string _AvgRSCell;
        public string AvgRSCell
        {
            get => _AvgRSCell;
            set
            {
                _AvgRSCell = value;
                OnPropertyChanged(nameof(AvgRSCell));
            }
        }

        private string _AvgGSCell;
        public string AvgGSCell
        {
            get => _AvgGSCell;
            set
            {
                _AvgGSCell = value;
                OnPropertyChanged(nameof(AvgGSCell));
            }
        }

        private string _AvgBSCell;
        public string AvgBSCell
        {
            get => _AvgBSCell;
            set
            {
                _AvgBSCell = value;
                OnPropertyChanged(nameof(AvgBSCell));
            }
        }

        private string _AvgHueSCell;
        public string AvgHueSCell
        {
            get => _AvgHueSCell;
            set
            {
                _AvgHueSCell = value;
                OnPropertyChanged(nameof(AvgHueSCell));
            }
        }

        private string _AvgSaturationSCell;
        public string AvgSaturationSCell
        {
            get => _AvgSaturationSCell;
            set
            {
                _AvgSaturationSCell = value;
                OnPropertyChanged(nameof(AvgSaturationSCell));
            }
        }

        private string _AvgBrightnessSCell;
        public string AvgBrightnessSCell
        {
            get => _AvgBrightnessSCell;
            set
            {
                _AvgBrightnessSCell = value;
                OnPropertyChanged(nameof(AvgBrightnessSCell));
            }
        }

        private string _AvgWidthSCell;
        public string AvgWidthSCell
        {
            get => _AvgWidthSCell;
            set
            {
                _AvgWidthSCell = value;
                OnPropertyChanged(nameof(AvgWidthSCell));
            }
        }

        private string _AvgHeightSCell;
        public string AvgHeightSCell
        {
            get => _AvgHeightSCell;
            set
            {
                _AvgHeightSCell = value;
                OnPropertyChanged(nameof(AvgHeightSCell));
            }
        }

        private string _AvgSizeSCell;
        public string AvgSizeSCell
        {
            get => _AvgSizeSCell;
            set
            {
                _AvgSizeSCell = value;
                OnPropertyChanged(nameof(AvgSizeSCell));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
