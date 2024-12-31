using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RomanowskyStainSlideAnalyzer.Home.Models
{
    public class SegmentParameterDataModel
    {
        public string filePath { get; set; }
        public string destination { get; set; }
        public string prefix { get; set; }
        public string postfix { get; set; }
        public string ext { get; set; }
        public bool useAutomaticSegmentation { get; set; }
        public bool extractBoundingBoxes { get; set; }
        public bool extractMasks { get; set; }
        public List<double> inputCoords { get; set; }
        public List<int> inputLabels { get; set; }
        public bool usePostProcess { get; set; }
        public string pointsPerSide { get; set; }
        public string pointsPerBatch { get; set; }
        public string predIoUThresh { get; set; }
        public string stabilityScoreThresh { get; set; }
        public string stabilityScoreOffset { get; set; }
        public string maskThreshold { get; set; }
        public string boxNMSThresh { get; set; }
        public string cropNLayers { get; set; }
        public string cropNMSThresh { get; set; }
        public string cropOverlapRatio { get; set; }
        public string cropNPointsDownscaleFactor { get; set; }
        public string minMaskRegionArea { get; set; }

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
            string minMaskRegionArea = "-1"
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
            this.pointsPerSide = pointsPerSide;
            this.pointsPerBatch = pointsPerBatch;
            this.predIoUThresh = predIoUThresh;
            this.stabilityScoreThresh = stabilityScoreThresh;
            this.stabilityScoreOffset = stabilityScoreOffset;
            this.maskThreshold = maskThreshold;
            this.boxNMSThresh = boxNMSThresh;
            this.cropNLayers = cropNLayers;
            this.cropNMSThresh = cropNMSThresh;
            this.cropOverlapRatio = cropOverlapRatio;
            this.cropNPointsDownscaleFactor = cropNPointsDownscaleFactor;
            this.minMaskRegionArea = minMaskRegionArea;
        }
    }
}
