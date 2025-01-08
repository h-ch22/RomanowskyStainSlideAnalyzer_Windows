using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using RomanowskyStainSlideAnalyzer.Home.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RomanowskyStainSlideAnalyzer.Home.Models
{
    public class SegmentParameterDataModel: INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string _filePath = "";
        public string filePath
        {
            get => _filePath;
            set
            {
                if(value != _filePath)
                {
                    _filePath = value;
                    OnPropertyChanged(nameof(filePath));

                    if(File.Exists(value)) source = new(new Uri(value));
                }
            }
        }

        private string _destination = "";
        public string destination
        {
            get => _destination;
            set
            {
                if (value != _destination)
                {
                    _destination = value;
                    OnPropertyChanged(nameof(destination));

                    if(File.Exists(value)) destinationSource = new(new Uri(value));
                }
            }
        }

        private BitmapImage _source;
        public BitmapImage source
        {
            get => _source;
            set
            {
                _source = value;
                OnPropertyChanged(nameof(source));
            }
        }

        private BitmapImage _destinationSource;

        public BitmapImage destinationSource
        {
            get => _destinationSource;
            set
            {
                _destinationSource = value;
                OnPropertyChanged(nameof(destinationSource));
            }
        }

        private string _prefix;
        public string prefix
        {
            get => _prefix;
            set
            {
                if(value != _prefix)
                {
                    _prefix = value;
                    OnPropertyChanged(nameof(prefix));
                }
            }
        }

        private string _postfix;
        public string postfix
        {
            get => _postfix;
            set
            {
                if(value != _postfix)
                {
                    _postfix = value;
                    OnPropertyChanged(nameof(postfix));
                }
            }
        }

        private string _newName = "";
        public string newName
        {
            get => _newName;
            set
            {
                if(value != _newName)
                {
                    _newName = value;
                    OnPropertyChanged(nameof(newName));
                }
            }
        }

        private string _ext;
        public string ext
        {
            get => _ext;
            set
            {
                if(value != _ext)
                {
                    _ext = value;
                    OnPropertyChanged(nameof(ext));
                }
            }
        }

        private bool _useAutomaticSegmentation;
        public bool useAutomaticSegmentation
        {
            get => _useAutomaticSegmentation;
            set
            {
                _useAutomaticSegmentation = value;
                OnPropertyChanged(nameof(useAutomaticSegmentation));
            }
        }

        private bool _extractBoundingBoxes;
        public bool extractBoundingBoxes
        {
            get => _extractBoundingBoxes;
            set
            {
                _extractBoundingBoxes = value;
                OnPropertyChanged(nameof(extractBoundingBoxes));
            }
        }

        private bool _extractMasks;
        public bool extractMasks
        {
            get => _extractMasks;
            set
            {
                _extractMasks = value;
                OnPropertyChanged(nameof(extractMasks));
            }
        }

        private List<double> _inputCoords;
        public List<double> inputCoords
        {
            get => _inputCoords;
            set
            {
                _inputCoords = value;
                OnPropertyChanged(nameof(inputCoords));
            }
        }

        private List<int> _inputLabels;
        public List<int> inputLabels
        {
            get => _inputLabels;
            set
            {
                _inputLabels = value;
                OnPropertyChanged(nameof(inputLabels));
            }
        }

        private bool _usePostProcess;
        public bool usePostProcess
        {
            get => _usePostProcess;
            set
            {
                _usePostProcess = value;
                OnPropertyChanged(nameof(usePostProcess));
            }
        }

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

        private string _Device = "Auto";
        public string Device
        {
            get => _Device;
            set
            {
                _Device = value;
                OnPropertyChanged(nameof(Device));
            }
        }

        private string _Model = "";
        public string Model
        {
            get => _Model;
            set
            {
                _Model = value;
                OnPropertyChanged(nameof(Model));
            }
        }

        public List<PointDataModel> Points = new();

        public SegmentParameterDataModel(
            string filePath,
            string destination,
            string prefix,
            string postfix,
            string ext,
            List<double> inputCoords,
            List<int> inputLabels,
            bool useAutomaticSegmentation = true,
            bool extractBoundingBoxes = true,
            bool extractMasks = true,
            string pointsPerSide = "128",
            string pointsPerBatch = "32",
            string predIoUThresh = "0.0",
            string stabilityScoreThresh = "1.0",
            string stabilityScoreOffset = "1.0",
            string maskThreshold = "0.0",
            string boxNMSThresh = "1.0",
            string cropNLayers = "2",
            string cropNMSThresh = "0.7",
            string cropOverlapRatio = "0.3413",
            string cropNPointsDownscaleFactor = "1",
            string minMaskRegionArea = "-1",
            string model = ""
        )
        {
            this.filePath = filePath;
            this.destination = destination;
            this.prefix = prefix;
            this.postfix = postfix;
            this.ext = ext;
            this.inputCoords = inputCoords;
            this.inputLabels = inputLabels;
            this.useAutomaticSegmentation = useAutomaticSegmentation;
            this.extractBoundingBoxes = extractBoundingBoxes;
            this.extractMasks = extractMasks;
            PointsPerSide.Value = pointsPerSide;
            PointsPerBatch.Value = pointsPerBatch;
            PredIoUThresh.Value = predIoUThresh;
            StabilityScoreThresh.Value = stabilityScoreThresh;
            StabilityScoreOffset.Value = stabilityScoreOffset;
            MaskThreshold.Value = maskThreshold;
            BoxNMSThresh.Value = boxNMSThresh;
            CropNLayers.Value = cropNLayers;
            CropNMSThresh.Value = cropNMSThresh;
            CropOverlapRatio.Value = cropOverlapRatio;
            CropNPointsDownScaleFactor.Value = cropNPointsDownscaleFactor;
            MinMaskRegionArea.Value = minMaskRegionArea;
            Model = model;
        }

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

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
