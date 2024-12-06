using RomanowskyStainSlideAnalyzer.Home.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;

namespace RomanowskyStainSlideAnalyzer.Home.Helper
{
    public class SegmentationHelper
    {
        public static string GetParameterDescription(string param)
        {
            switch(param)
            {
                case "PointsPerSide":
                    return "The number of points to be sampled along one side of the image.\nThe total number of points is points_per_side^2.";
                
                case "PointsPerBatch":
                    return "Sets the number of points run simultaneously by the model.\nHigher numbers may be faster but use more GPU memory.";

                case "PredIoUThresh":
                    return "A filtering threshold in [0,1],\nusing the model's predicted mask quality.";

                case "StabilityScoreThresh":
                    return "A filtering threshold in [0,1],\nusing the stability of the mask under changes to the cutoff used to binarize the model's\nmask predictions.";

                case "StabilityScoreOffset":
                    return "The amount to shift the cutoff when calculated the stability score.";

                case "MaskThreshold":
                    return "Threshold for binarizing the mask logits.";

                case "BoxNMSThresh":
                    return "The box IoU cutoff used by\nnon-maximal suppression to filter duplicate masks.";

                case "CropNLayers":
                    return "If >0, mask prediction will be run again on crops of the image.\nSets the number of layers to run, where each layer has 2^i_layer number of image crops.";

                case "CropNMSThresh":
                    return "The box IoU cutoff used by\nnon-maximal suppression to filter duplicate masks between different crops.";

                case "CropOverlapRatio":
                    return "Sets the degree to which crops overlap.\nIn the first crop layer, crops will overlap by this fraction of the image length.\nLater layers with more crops scale down this overlap.";

                case "CropNPointsDownScaleFactor":
                    return "The number of points-per-side sampled in layer n is\nscaled down by Crop N Points Downscale Factor^n.";

                case "MinMaskRegionArea":
                    return "If >0, postprocessing will be applied\nto remove disconnected regions and holes in masks with area smaller than\nMin Mask Region Area.";

                default: return "";
            }
        }

        public bool segment(
            string filePath,
            string destination,
            string prefix,
            string postfix,
            string ext,
            bool useAutomaticSegmentation,
            bool extractBoundingBoxes,
            List<double> inputCoords,
            List<int> inputLabels,
            bool usePostProcess,
            string pointsPerSide,
            string pointsPerBatch,
            string predIoUThresh,
            string stabilityScoreThresh,
            string stabilityScoreOffset,
            string maskThreshold,
            string boxNMSThresh,
            string cropNLayers,
            string cropNMSThresh,
            string cropOverlapRatio,
            string cropNPointsDownscaleFactor,
            string minMaskRegionArea
        )
        {
            var path = filePath.Replace(@"\", "/").Replace("C:", "");
            var cli = $"cd ~/RomanowskyStainSlideAnalyzer && source RomanowskyStainSlideAnalyzer_venv/bin/activate && python main.py -f /mnt/c{path}";

            if(prefix != "")
            {
                cli += $" -prefix {prefix}";
            } else if(postfix != "")
            {
                cli += $" -postfix {postfix}";
            } else
            {
                cli += $" -d {destination} -e {ext}";
            }

            if(useAutomaticSegmentation)
            {
                cli += " -a y";

                if (!extractBoundingBoxes) cli += " -b n";
            }
            else
            {
                cli += " -a n -ip ";

                foreach(var point in inputCoords)
                {
                    cli += $"{point.ToString()} ";
                }

                cli += "-il ";

                foreach(var label in inputLabels)
                {
                    cli += $"{label.ToString()} ";
                }
            }

            cli += $" -p {usePostProcess} -pps {pointsPerSide} -ppb {pointsPerBatch} -pit {predIoUThresh} -sst {stabilityScoreThresh} -sso {stabilityScoreOffset} -mt {maskThreshold} -bnt {boxNMSThresh} -cnl {cropNLayers} -cnt {cropNMSThresh} -cor {cropOverlapRatio} -cnp {cropNPointsDownscaleFactor} -mmr {minMaskRegionArea}";
            Debug.WriteLine(cli);
            try
            {
                Process process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "ubuntu",
                        Arguments = $"run {cli}",
                        UseShellExecute = true,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                process.WaitForExit();

                return process.ExitCode == 0;
            }
            catch (Exception e)
            {
                Debug.Write(e.Message);
                return false;
            }
        }

        public bool CreateHistory(
            string targetPath,
            string outputFileName,
            bool usePostProcess,
            bool useAutomaticSegmentation,
            bool extractBoundingBoxes,
            List<double> inputCoords,
            List<int> inputLabels,
            string pointsPerSide,
            string pointsPerBatch,
            string predIoUThresh,
            string stabilityScoreThresh,
            string stabilityScoreOffset,
            string maskThreshold,
            string boxNMSThresh,
            string cropNLayers,
            string cropNMSThresh,
            string cropOverlapRatio,
            string cropNPointsDownscaleFactor,
            string minMaskRegionArea
        )
        {
            try
            {
                var rssaFolder = @"C:\RomanowskyStainSlideAnalyzer";

                if (!Directory.Exists(rssaFolder))
                {
                    Directory.CreateDirectory(rssaFolder);
                }

                var finalPath = Path.Combine(rssaFolder, targetPath);

                if (!Directory.Exists(finalPath))
                {
                    Directory.CreateDirectory(finalPath);
                }

                var linuxPath = finalPath.Replace(@"\", "/").Replace("C:", "");

                var cli = $"cd ~/RomanowskyStainSlideAnalyzer/outputs && cp ./{outputFileName} /mnt/c{linuxPath}/";

                if(extractBoundingBoxes)
                {
                    cli += $" && cp ./{outputFileName}.txt /mnt/c{linuxPath}/";
                }

                Debug.WriteLine(cli);

                string points = "";
                string labels = "";

                for(var i = 0; i < inputCoords.Count; i+=2)
                {
                    points += $"{inputCoords[i].ToString()}, {inputCoords[i+1].ToString()}\n";
                }

                foreach (var label in inputLabels)
                {
                    labels += $"{label.ToString()}, ";
                }

                string[] lines =
                {
                    $"Use Automatic Segmentation: {useAutomaticSegmentation}",
                    $"Points: {points}",
                    $"Labels: {labels}",
                    $"Points Per Side: {pointsPerSide}",
                    $"Points Per Batch: {pointsPerBatch}",
                    $"Pred IoU Thresh: {predIoUThresh}",
                    $"Stability Score Thresh: {stabilityScoreThresh}",
                    $"Stability Score Offset: {stabilityScoreOffset}",
                    $"Mask Threshold: {maskThreshold}",
                    $"Box NMS Thresh: {boxNMSThresh}",
                    $"Crop N Layers: {cropNLayers}",
                    $"Crop NMS Thresh: {cropNMSThresh}",
                    $"Crop Overlap Ratio: {cropOverlapRatio}",
                    $"Crop N Points Downscale Factor: {cropNPointsDownscaleFactor}",
                    $"Min Mask Region Area: {minMaskRegionArea}",
                    $"Use Post Process: {usePostProcess}",
                    $"Output File Name: {outputFileName}",
                };

                string docPath = $@"{finalPath}\log.txt";

                using (StreamWriter txtFile = new StreamWriter(docPath))
                {
                    foreach(string line in lines)
                    {
                        txtFile.WriteLine(line);
                    }
                }

                Process process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "ubuntu",
                        Arguments = $"run {cli}",
                        UseShellExecute = true,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                process.WaitForExit();

                return process.ExitCode == 0;
            }
            catch (Exception e)
            {
                Debug.Write(e.Message);
                return false;
            }
        }
    }
}
