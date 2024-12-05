using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using RomanowskyStainSlideAnalyzer.Home.Helper;
using System.Collections.Generic;
using System.ComponentModel;

namespace RomanowskyStainSlideAnalyzer.Home.Models
{
    public class ParametersViewModel: INotifyPropertyChanged
    {
        private ParameterDataModel _PointsPerSide = new("128", SegmentationHelper.GetParameterDescription(nameof(PointsPerSide)));
        public ParameterDataModel PointsPerSide
        {
            get => _PointsPerSide;
            set
            {
                _PointsPerSide = value;
                OnPropertyChanged(nameof(PointsPerSide));
            }
        }

        private ParameterDataModel _PointsPerBatch = new("32", SegmentationHelper.GetParameterDescription(nameof(PointsPerBatch)));
        public ParameterDataModel PointsPerBatch
        {
            get => _PointsPerBatch;
            set
            {
                _PointsPerBatch = value;
                OnPropertyChanged(nameof(PointsPerBatch));
            }
        }

        private ParameterDataModel _PredIoUThresh = new("0.0", SegmentationHelper.GetParameterDescription(nameof(PredIoUThresh)));
        public ParameterDataModel PredIoUThresh
        {
            get => _PredIoUThresh;
            set
            {
                _PredIoUThresh = value;
                OnPropertyChanged(nameof(PredIoUThresh));
            }
        }

        private ParameterDataModel _StabilityScoreThresh = new("1.0", SegmentationHelper.GetParameterDescription(nameof(StabilityScoreThresh)));
        public ParameterDataModel StabilityScoreThresh
        {
            get => _StabilityScoreThresh;
            set
            {
                _StabilityScoreThresh = value;
                OnPropertyChanged(nameof(StabilityScoreThresh));
            }
        }

        private ParameterDataModel _StabilityScoreOffset = new("1.0", SegmentationHelper.GetParameterDescription(nameof(StabilityScoreOffset)));
        public ParameterDataModel StabilityScoreOffset
        {
            get => _StabilityScoreOffset;
            set
            {
                _StabilityScoreOffset = value;
                OnPropertyChanged(nameof(StabilityScoreOffset));
            }
        }

        private ParameterDataModel _MaskThreshold = new("0.0", SegmentationHelper.GetParameterDescription(nameof(MaskThreshold)));
        public ParameterDataModel MaskThreshold
        {
            get => _MaskThreshold;
            set
            {
                _MaskThreshold = value;
                OnPropertyChanged(nameof(MaskThreshold));
            }
        }

        private ParameterDataModel _BoxNMSThresh = new("1.0", SegmentationHelper.GetParameterDescription(nameof(BoxNMSThresh)));
        public ParameterDataModel BoxNMSThresh
        {
            get => _BoxNMSThresh;
            set
            {
                _BoxNMSThresh = value;
                OnPropertyChanged(nameof(BoxNMSThresh));
            }
        }

        private ParameterDataModel _CropNLayers = new("2", SegmentationHelper.GetParameterDescription(nameof(CropNLayers)));
        public ParameterDataModel CropNLayers
        {
            get => _CropNLayers;
            set
            {
                _CropNLayers = value;
                OnPropertyChanged(nameof(CropNLayers));
            }
        }

        private ParameterDataModel _CropNMSThresh = new("0.7", SegmentationHelper.GetParameterDescription(nameof(CropNMSThresh)));
        public ParameterDataModel CropNMSThresh
        {
            get => _CropNMSThresh;
            set
            {
                _CropNMSThresh = value;
                OnPropertyChanged(nameof(CropNMSThresh));
            }
        }

        private ParameterDataModel _CropOverlapRatio = new("0.3413", SegmentationHelper.GetParameterDescription(nameof(CropOverlapRatio)));
        public ParameterDataModel CropOverlapRatio
        {
            get => _CropOverlapRatio;
            set
            {
                _CropOverlapRatio = value;
                OnPropertyChanged(nameof(CropOverlapRatio));
            }
        }

        private ParameterDataModel _CropNPointsDownScaleFactor = new("1", SegmentationHelper.GetParameterDescription(nameof(CropNPointsDownScaleFactor)));
        public ParameterDataModel CropNPointsDownScaleFactor
        {
            get => _CropNPointsDownScaleFactor;
            set
            {
                _CropNPointsDownScaleFactor = value;
                OnPropertyChanged(nameof(CropNPointsDownScaleFactor));
            }
        }

        private ParameterDataModel _MinMaskRegionArea = new("-1", SegmentationHelper.GetParameterDescription(nameof(MinMaskRegionArea)));
        public ParameterDataModel MinMaskRegionArea
        {
            get => _MinMaskRegionArea;
            set
            {
                _MinMaskRegionArea = value;
                OnPropertyChanged(nameof(MinMaskRegionArea));
            }
        }

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

        public List<PointDataModel> Points = new();

        public event PropertyChangedEventHandler PropertyChanged;
        public void ResetParams()
        {
            PointsPerSide = new("128", SegmentationHelper.GetParameterDescription(nameof(PointsPerSide)));
            PointsPerBatch = new("32", SegmentationHelper.GetParameterDescription(nameof(PointsPerBatch)));
            PredIoUThresh = new("0.0", SegmentationHelper.GetParameterDescription(nameof(PredIoUThresh)));
            StabilityScoreThresh = new("1.0", SegmentationHelper.GetParameterDescription(nameof(StabilityScoreThresh)));
            StabilityScoreOffset = new("1.0", SegmentationHelper.GetParameterDescription(nameof(StabilityScoreOffset)));
            MaskThreshold = new("0.0", SegmentationHelper.GetParameterDescription(nameof(MaskThreshold)));
            BoxNMSThresh = new("1.0", SegmentationHelper.GetParameterDescription(nameof(BoxNMSThresh)));
            CropNLayers = new("2", SegmentationHelper.GetParameterDescription(nameof(CropNLayers)));
            CropNMSThresh = new("0.7", SegmentationHelper.GetParameterDescription(nameof(CropNMSThresh)));
            CropOverlapRatio = new("0.3413", SegmentationHelper.GetParameterDescription(nameof(CropOverlapRatio)));
            CropNPointsDownScaleFactor = new("1", SegmentationHelper.GetParameterDescription(nameof(CropNPointsDownScaleFactor)));
            MinMaskRegionArea = new("-1", SegmentationHelper.GetParameterDescription(nameof(MinMaskRegionArea)));
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
