using RomanowskyStainSlideAnalyzer.Home.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using static System.Net.Mime.MediaTypeNames;

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

        public static List<string> GetAvailableModels()
        {
            var path = @"\\wsl$\Ubuntu\root\RomanowskyStainSlideAnalyzer\checkpoints";
            var files = Directory.GetFiles(path, "*.pt");
            List<string> models = new();

            foreach(var file in files)
            {
                var fileSplit = file.Split(@"\");
                models.Add(
                    fileSplit[fileSplit.Length - 1].Split(".pt")[0]    
                );
            }

            return models;
        }

        public bool cancelSegment()
        {
            try
            {
                Process[] processes = Process.GetProcessesByName("wsl");

                foreach (Process process in processes)
                {
                    process.Kill();
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return false;
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
            bool extractMasks,
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
            string minMaskRegionArea,
            string device,
            string model
        )
        {
            var pathSplitByDrive = filePath.Split(@":\");
            var drive = pathSplitByDrive[0].ToLower();

            var path = pathSplitByDrive[1].Replace(@"\", "/").Replace(":/", "/");

            var cli = $"cd ~/RomanowskyStainSlideAnalyzer && source RomanowskyStainSlideAnalyzer_venv/bin/activate && python main.py -f /mnt/{drive}/{path} -dev {device} -model {model}";

            if (prefix != "")
            {
                cli += $" -prefix {prefix}";
            }
            else if (postfix != "")
            {
                cli += $" -postfix {postfix}";
            }
            else
            {
                cli += $" -d {destination} -e {ext}";
            }

            if (useAutomaticSegmentation)
            {
                cli += " -a y";

                if (!extractBoundingBoxes) cli += " -b n";
                if (!extractMasks) cli += " -m n";
            }
            else
            {
                cli += " -a n -ip ";

                foreach (var point in inputCoords)
                {
                    cli += $"{point.ToString()} ";
                }

                cli += "-il ";

                foreach (var label in inputLabels)
                {
                    cli += $"{label.ToString()} ";
                }
            }

            cli += $" -p {usePostProcess} -pps {pointsPerSide} -ppb {pointsPerBatch} -pit {predIoUThresh} -sst {stabilityScoreThresh} -sso {stabilityScoreOffset} -mt {maskThreshold} -bnt {boxNMSThresh} -cnl {cropNLayers} -cnt {cropNMSThresh} -cor {cropOverlapRatio} -cnp {cropNPointsDownscaleFactor} -mmr {minMaskRegionArea}";

            try
            {
                Process process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "ubuntu",
                        Arguments = $"run {cli}",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    }
                };

                process.ErrorDataReceived += (sender, e) => Debug.WriteLine($"STDERR: {e.Data}");

                process.Start();
                process.BeginErrorReadLine();

                process.WaitForExit();

                return process.ExitCode == 0;
            }
            catch (Exception e)
            {
                Debug.Write(e.Message);
                return false;
            }
        }

        private bool DeleteDirectory(string path)
        {
            DirectoryInfo di = new DirectoryInfo(path);

            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete();
            }
            foreach (DirectoryInfo dir in di.GetDirectories())
            {
                dir.Delete(true);
            }

            Directory.Delete(path);

            return !Directory.Exists(path);
        }

        public bool CreateHistory(
            string filePath,
            string targetPath,
            string outputFileName,
            bool usePostProcess,
            bool useAutomaticSegmentation,
            bool extractBoundingBoxes,
            bool extractMasks,
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
            string minMaskRegionArea,
            string device,
            string model,
            bool useParallel
        )
        {
            var rssaFolder = @"C:\RomanowskyStainSlideAnalyzer";
            var filePathSplit = filePath.Split(".");
            var ext = filePathSplit[filePathSplit.Length - 1];

            var finalPath = Path.Combine(rssaFolder, targetPath);
            var maskPath = Path.Combine(finalPath, "Masks");

            try
            {
                if (!Directory.Exists(rssaFolder))
                {
                    DirectoryInfo di = Directory.CreateDirectory(rssaFolder);
                    di.Attributes = System.IO.FileAttributes.Directory | System.IO.FileAttributes.Hidden;
                }

                if (!Directory.Exists(finalPath))
                {
                    Directory.CreateDirectory(finalPath);
                }

                if(!Directory.Exists(maskPath))
                {
                    Directory.CreateDirectory(maskPath);
                }

                File.Copy(filePath, $@"{finalPath}\input.{ext}", true);

                var linuxPath = finalPath.Replace(@"\", "/").Replace("C:", "");
                var maskLinuxPath = maskPath.Replace(@"\", "/").Replace("C:", "");

                var cli = $"cd ~/RomanowskyStainSlideAnalyzer/outputs && cp ./{outputFileName} /mnt/c{linuxPath}/";

                if(extractBoundingBoxes)
                {
                    cli += $" && cp ./{outputFileName}.txt /mnt/c{linuxPath}/";
                }

                if(extractMasks)
                {
                    cli += $" && cp -r ./Masks/{outputFileName} /mnt/c{maskLinuxPath}";  
                }

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
                    $"Extract Bounding Box: {extractBoundingBoxes}",
                    $"Extract Masks: {extractMasks}",
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
                    $"Device: {device}",
                    $"Model: {model}",
                    $"Use Parallel: {useParallel}"
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
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    }
                };

                process.ErrorDataReceived += (sender, e) => Debug.WriteLine($"STDERR: {e.Data}");
                process.Start();
                process.BeginErrorReadLine();

                process.WaitForExit();

                var exitCode = process.ExitCode;

                if(exitCode != 0 && Directory.Exists(finalPath))
                {
                    DeleteDirectory(finalPath);
                }

                return exitCode == 0;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);

                if (Directory.Exists(finalPath))
                {
                    DeleteDirectory(finalPath);
                }

                return false;
            }
        }

        public bool checkDefaultPresetExists()
        {
            if(!File.Exists(@"C:\RomanowskyStainSlideAnalyzer\Presets\default.txt"))
            {
                var result = createPreset("Default", new("", "", "", "", "", new(), new()));
                return result == 0;
            }

            return true;
        }

        public static int createPreset(
            string presetName,
            SegmentParameterDataModel data,
            bool overwrite = false
        )
        {
            var directory = @"C:\RomanowskyStainSlideAnalyzer\Presets";

            try
            {
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                if (File.Exists($@"{directory}\{presetName}.txt") && !overwrite)
                {
                    return 1;
                }

                string[] lines =
                {
                    $"PPS: {data.PointsPerSide.Value}",
                    $"PPB: {data.PointsPerBatch.Value}",
                    $"PIT: {data.PredIoUThresh.Value}",
                    $"SST: {data.StabilityScoreThresh.Value}",
                    $"SSO: {data.StabilityScoreOffset.Value}",
                    $"MT: {data.MaskThreshold.Value}",
                    $"BNT: {data.BoxNMSThresh.Value}",
                    $"CNL: {data.CropNLayers.Value}",
                    $"CNT: {data.CropNMSThresh.Value}",
                    $"COR: {data.CropOverlapRatio.Value}",
                    $"CNPDF: {data.CropNPointsDownScaleFactor.Value}",
                    $"MMRA: {data.MinMaskRegionArea.Value}"
                };

                string docPath = $@"{directory}\{presetName}.txt";

                using (StreamWriter txtFile = new StreamWriter(docPath))
                {
                    foreach (string line in lines)
                    {
                        txtFile.WriteLine(line);
                    }
                }

                return 0;
            }

            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return -1;
            }
        }

        public static SegmentParameterDataModel loadPreset(string presetName)
        {
            var param = new SegmentParameterDataModel("", "", "", "", "", new(), new());
            var path = $@"C:\RomanowskyStainSlideAnalyzer\Presets\{presetName}.txt";

            if (!File.Exists(path)) return param;

            try
            {

                using (var reader = new StreamReader(path, Encoding.UTF8))
                {
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        var header = line.Split(": ")[0];
                        var value = line.Split(": ")[1];

                        switch(header)
                        {
                            case "PPS":
                                param.PointsPerSide.Value = value;
                                break;

                            case "PPB":
                                param.PointsPerBatch.Value = value;
                                break;

                            case "PIT":
                                param.PredIoUThresh.Value = value;
                                break;

                            case "SST":
                                param.StabilityScoreThresh.Value = value;
                                break;

                            case "SSO":
                                param.StabilityScoreOffset.Value = value;
                                break;

                            case "MT":
                                param.MaskThreshold.Value = value;
                                break;

                            case "BNT":
                                param.BoxNMSThresh.Value = value;
                                break;

                            case "CNL":
                                param.CropNLayers.Value = value;
                                break;

                            case "CNT":
                                param.CropNMSThresh.Value = value;
                                break;

                            case "COR":
                                param.CropOverlapRatio.Value = value;
                                break;

                            case "CNPDF":
                                param.CropNPointsDownScaleFactor.Value = value;
                                break;

                            case "MMRA":
                                param.MinMaskRegionArea.Value = value;
                                break;

                            default: break;
                        }
                    }

                    return param;
                }
            }

            catch(Exception e)
            {
                Debug.WriteLine(e.Message);

                return param;
            }
        }

        public static List<string> loadPresets()
        {
            var directory = @"C:\RomanowskyStainSlideAnalyzer\Presets";
            var result = new List<string>();

            var files = Directory.GetFiles($@"{directory}\", "*.txt");

            foreach(var file in files)
            {
                var fileSplit = file.Split(@"\");
                var fileName = fileSplit[fileSplit.Length - 1];
                result.Add(fileName.Split(".txt")[0]);
            }

            return result;
        }

        public static bool DeletePreset(string name)
        {
            var directory = @"C:\RomanowskyStainSlideAnalyzer\Presets";

            try
            {
                if (File.Exists($@"{directory}\{name}.txt"))
                {
                    File.Delete($@"{directory}\{name}.txt");
                }

                return true;
            }

            catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return false;
            }
        }
    }
}
