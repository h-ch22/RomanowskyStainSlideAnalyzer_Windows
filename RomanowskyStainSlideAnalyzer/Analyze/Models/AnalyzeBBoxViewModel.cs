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

        private bool _ShowAllData = true;
        public bool ShowAllData
        {
            get => _ShowAllData;
            set
            {
                _ShowAllData = value;
                OnPropertyChanged(nameof(ShowAllData));
            }
        }

        private int _Size = 256;
        public int Size
        {
            get => _Size;
            set
            {
                _Size = value;
                OnPropertyChanged(nameof(Size));
            }
        }

        private int _Center = 128;
        public int Center
        {
            get => _Center;
            set
            {
                _Center = value;
                OnPropertyChanged(nameof(Center));
            }
        }

        private bool _UseBoundingBoxAsTarget = true;
        public bool UseBoundingBoxAsTarget
        {
            get => _UseBoundingBoxAsTarget;
            set
            {
                _UseBoundingBoxAsTarget = value;
                OnPropertyChanged(nameof(UseBoundingBoxAsTarget));
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

        private Visibility _AllDataVisibility = Visibility.Visible;
        public Visibility AllDataVisibility
        {
            get => _AllDataVisibility;
            set
            {
                _AllDataVisibility = value;
                OnPropertyChanged(nameof(AllDataVisibility));
            }
        }

        private Visibility _MaskDataVisibility = Visibility.Visible;
        public Visibility MaskDataVisibility
        {
            get => _MaskDataVisibility;
            set
            {
                _MaskDataVisibility = value;
                OnPropertyChanged(nameof(MaskDataVisibility));
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

        private BitmapImage? _CroppedImage = null;
        public BitmapImage? CroppedImage
        {
            get => _CroppedImage;
            set
            {
                _CroppedImage = value;
                OnPropertyChanged(nameof(CroppedImage));
            }
        }

        private BitmapImage? _CroppedMaskImage = null;
        public BitmapImage? CroppedMaskImage
        {
            get => _CroppedMaskImage;
            set
            {
                _CroppedMaskImage = value;
                OnPropertyChanged(nameof(CroppedMaskImage));
            }
        }

        private string _Class = "";
        public string Class
        {
            get => _Class;
            set
            {
                _Class = value;
                OnPropertyChanged(nameof(Class));
            }
        }

        private string _Index = "";
        public string Index
        {
            get => _Index;
            set
            {
                _Index = value;
                OnPropertyChanged(nameof(Index));
            }
        }

        private string _X = "";
        public string X
        {
            get => _X;
            set
            {
                _X = value;
                OnPropertyChanged(nameof(X));
            }
        }

        private string _Y = "";
        public string Y
        {
            get => _Y;
            set
            {
                _Y = value;
                OnPropertyChanged(nameof(Y));
            }
        }

        private AvgDataModel _Average = new();
        public AvgDataModel Average
        {
            get => _Average;
            set
            {
                _Average = value;
                OnPropertyChanged(nameof(Average));
            }
        }

        private AllAvgDataModel _AllAvg = new(new(), new(), new(), new());
        public AllAvgDataModel AllAvg
        {
            get => _AllAvg;
            set
            {
                _AllAvg = value;
                OnPropertyChanged(nameof(AllAvg));
            }
        }

        private bool _IsMaskAvailable = false;
        public bool IsMaskAvailable
        {
            get => _IsMaskAvailable;
            set
            {
                _IsMaskAvailable = value;
                OnPropertyChanged(nameof(IsMaskAvailable));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
