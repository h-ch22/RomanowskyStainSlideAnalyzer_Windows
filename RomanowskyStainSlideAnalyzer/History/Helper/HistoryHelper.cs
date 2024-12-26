using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Shapes;
using RomanowskyStainSlideAnalyzer.Analyze.Helper;
using RomanowskyStainSlideAnalyzer.Analyze.Models;
using RomanowskyStainSlideAnalyzer.History.Models;
using RomanowskyStainSlideAnalyzer.Labeling.Helper;
using RomanowskyStainSlideAnalyzer.Labeling.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RomanowskyStainSlideAnalyzer.History.Helper
{
    public class HistoryHelper
    {
        private string GetImage(string path)
        {
            var jpgFiles = Directory.GetFiles($@"{path}\", "*.jpg");
            var jpegFiles = Directory.GetFiles($@"{path}\", "*.jpeg");
            var pngFiles = Directory.GetFiles($@"{path}\", "*.png");

            var file = "";

            if (jpgFiles.Length == 1 && !jpgFiles[0].Contains(@"\input.jpg"))
            {
                file = jpgFiles[0];
            }
            else if (jpgFiles.Length == 2)
            {
                file = jpgFiles[0].Contains(@"\input.jpg") ? jpgFiles[1] : jpgFiles[0];
            }
            else if (jpegFiles.Length == 1 && !jpegFiles[0].Contains(@"\input.jpeg")) {
                file = jpegFiles[0];
            }
            else if (jpegFiles.Length == 2)
            {
                file = jpegFiles[0].Contains(@"\input.jpeg") ? jpegFiles[1] : jpegFiles[0];
            }

            else if (pngFiles.Length == 1 && !pngFiles[0].Contains(@"\input.png"))
            {
                file = pngFiles[0];
            }
            else if (pngFiles.Length == 2)
            {
                file = pngFiles[0].Contains(@"\input.png") ? pngFiles[1] : pngFiles[0];
            }

            return file;
        }

        public bool IsMaskAvailable(string path, string inputFileName)
        {
            var isMaskDirectoryExists = Directory.Exists($@"{path}\Masks\{inputFileName}");

            if(isMaskDirectoryExists)
            {
                return Directory.GetFiles($@"{path}\Masks\{inputFileName}", "*.csv").Length > 0;
            } else
            {
                return false;
            }
        }

        public bool IsBBoxAvailable(string path, string inputFileName)
        {
            return File.Exists($@"{path}\{inputFileName}.txt");
        }

        public async Task<ObservableCollection<HistoryDataModel>> GetHistory(string date)
        {
            return await Task.Run(() =>
            {
                var history = new ObservableCollection<HistoryDataModel>();
                var path = @"C:\RomanowskyStainSlideAnalyzer";

                if (!Directory.Exists(path)) return history;

                var directories = Directory.GetDirectories(@"C:\RomanowskyStainSlideAnalyzer");

                foreach (var dir in directories)
                {
                    if (dir.Contains(date))
                    {
                        var file = GetImage(dir);
                        var csvFiles = Directory.GetFiles($@"{dir}\", "*.csv");

                        var fullDate = dir.Split(@"\");
                        var fullLog = "";

                        using (var reader = new StreamReader($@"{dir}\log.txt", Encoding.UTF8))
                        {
                            while (!reader.EndOfStream)
                            {
                                var line = reader.ReadLine();
                                fullLog += $"{line}\n";
                            }
                        }

                        var fileSplit = file.Split(@"\");
                        var maskPath = $@"{dir}\Masks\{fileSplit[fileSplit.Length - 1]}\";
                        bool isMaskExported = false;
                        bool isMaskLabeled = false;

                        if (Directory.Exists(maskPath))
                        {
                            isMaskExported = Directory.GetFiles($@"{maskPath}", "mask_*.csv").Count(file => System.IO.Path.GetFileName(file).StartsWith("mask_")) > 0;
                            isMaskLabeled = Directory.GetFiles($@"{maskPath}", "Mask_Labeled_*.csv").Count(file => System.IO.Path.GetFileName(file).StartsWith("Mask_Labeled_")) > 0;
                        }

                        var canItLabeled = CanItLabeling(file);
                        Symbol symbol;
                        string statusText;

                        if (isMaskExported && csvFiles.Length > 0)
                        {
                            symbol = Symbol.Accept;
                            statusText = "Labeling Data & Mask Data Included";
                        }
                        else if (isMaskExported)
                        {
                            symbol = Symbol.Accept;
                            statusText = "Mask Data Included";
                        }
                        else if (csvFiles.Length > 0)
                        {
                            symbol = Symbol.Accept;
                            statusText = "Labeling Data Included";
                        }
                        else if (canItLabeled)
                        {
                            symbol = Symbol.ImportAll;
                            statusText = "Labeling data can be imported";
                        }
                        else if (fullLog.Contains("Use Automatic Segmentation: False"))
                        {
                            symbol = Symbol.TouchPointer;
                            statusText = "Manually Segmented";
                        }
                        else if (fullLog.Contains("Use Post Process: True"))
                        {
                            symbol = Symbol.Find;
                            statusText = "Post Processed";
                        }
                        else
                        {
                            symbol = Symbol.Cancel;
                            statusText = "Not Labeled";
                        }

                        history.Add(
                            new HistoryDataModel(
                                fullDate[fullDate.Length - 1],
                                dir,
                                csvFiles.Length > 0 ? csvFiles[0] : "",
                                file,
                                fullLog,
                                labeledMaskPropertiesVisibility: isMaskLabeled ? Visibility.Visible : Visibility.Collapsed,
                                labeledBBoxPropertiesVisibility: csvFiles.Length > 0 ? Visibility.Visible : Visibility.Collapsed,
                                maskPropertiesVisibility: isMaskExported ? Visibility.Visible : Visibility.Collapsed,
                                bBoxPropertiesVisibility: File.Exists($@"{file}.txt") ? Visibility.Visible : Visibility.Collapsed,
                                symbol: symbol,
                                statusText: statusText,
                                canItLabeled: canItLabeled,
                                imgFile: file
                            )
                        );
                    }
                }

                return new ObservableCollection<HistoryDataModel>(history.OrderByDescending(x => x.date));
            });
        }

        public bool ValidateLabeledData(string file)
        {
            try
            {
                StreamReader sr = new(file);
                int id = 0;
                List<BoundingBoxDataModel> boundingBoxes = new();
                var line = sr.ReadLine();
                var lineSplited = line.Split(",");

                if (lineSplited.Length != 5) return false;
                else if (
                    (lineSplited[0] != "Class" && lineSplited[0] != " Class") ||
                    (lineSplited[1] != "X" && lineSplited[1] != " X") ||
                    (lineSplited[2] != "Y" && lineSplited[2] != " Y") ||
                    (lineSplited[3] != "W" && lineSplited[3] != " W") ||
                    (lineSplited[4] != "H" && lineSplited[4] != " H")
                ) return false;

                return true;
            }

            catch(Exception ex)
            {
                Debug.WriteLine(ex);
                return false;
            }
        }

        private bool CanItLabeling(string path)
        {
            return File.Exists($@"{path}.txt");
        }

        public void Copy(string from, string to)
        {
            var splitPath = from.Split(@"\");
            var fileName = splitPath[splitPath.Length - 1];

            try
            {
                File.Copy($"{from.Split(".txt")[0]}", $@"{to}\{fileName.Split(@".txt")[0]}", true);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public bool IsSingleLabeled(string dir)
        {
            var allFiles = Directory.GetFiles($@"{dir}\", "*.csv");
            var csvFiles = allFiles.Where(file => file.Contains("Labeled")).ToArray();

            return csvFiles.Length == 1;
        }

        public async Task<string> CopyMaskLabelingData(string from, string to, bool isLabeledOnly = true)
        {
            return await Task.Run(() =>
            {
                var allFiles = Directory.GetFiles($@"{from}\", "*.csv");
                var csvFiles = allFiles.Where(file => file.Contains("Labeled")).ToArray();
                var nonLabeledFiles = allFiles.Where(file => !file.Contains("Labeled")).ToArray();

                try
                {
                    foreach (var f in isLabeledOnly ? csvFiles : nonLabeledFiles)
                    {
                        var fSplit = f.Split(@"\");
                        Debug.WriteLine($"from {f} to {to}/{fSplit[fSplit.Length - 1]}");
                        File.Copy(f, $@"{to}\{fSplit[fSplit.Length - 1]}", true);
                    }

                    return "";
                }
                catch (Exception e)
                {
                    throw e;
                }
            });
        }

        public void ChangeMaskData(string from, string to, string fileName)
        {
            try
            {
                if (!Directory.Exists($@"{to}\Masks"))
                {
                    Directory.CreateDirectory($@"{to}\Masks");
                }

                if (!Directory.Exists($@"{to}\Masks\{fileName}"))
                {
                    Directory.CreateDirectory($@"{to}\Masks\{fileName}");
                }

                var fileNameSplit = from.Split(@"\");
                var name = fileNameSplit[fileNameSplit.Length - 1];

                if (name == $"Mask_Labeled.csv")
                {
                    File.Copy(from, $@"{to}\Masks\{fileName}\Mask_Labeled.csv", true);
                }
            }

            catch(Exception e)
            {
                throw e;
            }
        }

        public async Task ChangeMaskData(string from, string to, string fileName, bool isLabeled)
        {
            await Task.Run(() =>
            {
                try
                {
                    var origFiles = Directory.GetFiles($@"{from}\", "*.csv");
                    var files = origFiles.OrderBy(file =>
                    {
                        var name = System.IO.Path.GetFileNameWithoutExtension(file);

                        if (isLabeled && name.Contains("Labeled"))
                        {
                            var parts = name.Split('_');

                            if (parts.Length > 1 && int.TryParse(parts[^1], out int number))
                            {
                                return number;
                            }

                            return int.MaxValue;
                        }

                        else if (!isLabeled && !name.Contains("Labeled"))
                        {
                            var parts = name.Split('_');

                            if (parts.Length > 1 && int.TryParse(parts[^1], out int number))
                            {
                                return number;
                            }

                            return int.MaxValue;
                        }

                        else return int.MaxValue;


                    }).ToArray();

                    if (!Directory.Exists($@"{to}\Masks"))
                    {
                        Directory.CreateDirectory($@"{to}\Masks");
                    }

                    if (!Directory.Exists($@"{to}\Masks\{fileName}"))
                    {
                        Directory.CreateDirectory($@"{to}\Masks\{fileName}");
                    }

                    for (var i = 0; i < files.Length; i++)
                    {
                        var fileNameSplit = files[i].Split(@"\");
                        var name = fileNameSplit[fileNameSplit.Length - 1];

                        if (name == $"mask_{i}.csv" && !isLabeled)
                        {
                            File.Copy(files[i], $@"{to}\Masks\{fileName}\mask_{i}.csv", true);
                        }

                        else if (name == $"Mask_Labeled_{i}.csv" && isLabeled)
                        {
                            File.Copy(files[i], $@"{to}\Masks\{fileName}\Mask_Labeled_{i}.csv", true);
                        }
                    }
                }

                catch (Exception ex)
                {
                    throw ex;
                }
            });
        }

        public async Task<Bitmap?> ExportWithMasks(Bitmap bmp, string csvPath, bool exportWithClasses)
        {
            return await Task.Run(() =>
            {
                try
                {
                    var dataCount = Directory.GetFiles($@"{csvPath}", exportWithClasses ? "Mask_Labeled_*.csv" : "mask_*.csv").Count(s => exportWithClasses ? s.Contains("Labeled") : !s.Contains("Labeled"));

                    for (var i = 0; i < dataCount; i++)
                    {
                        int[,] data;
                        string className = "";

                        if(exportWithClasses)
                        {
                            var maskAndClasses = AnalyzeHelper.GetMaskWithClass(csvPath, i.ToString());
                            data = maskAndClasses.Item2;
                            className = maskAndClasses.Item1;
                            var classType = data[0, 0];
                        }

                        else
                        {
                            data = AnalyzeHelper.GetMask(csvPath, i.ToString());
                        }

                        var originalW = data.GetLength(1);
                        var originalH = exportWithClasses ? data.GetLength(0) - 1 : data.GetLength(0);
                        var targetW = bmp.Width;
                        var targetH = bmp.Height;

                        var resizedData = AnalyzeHelper.ResizeData(data, originalW, originalH, targetW, targetH);

                        int rows = resizedData.GetLength(0);
                        int cols = resizedData.GetLength(1);
                        int pMinX = int.MaxValue, pMinY = int.MaxValue;
                        int pMaxX = int.MinValue, pMaxY = int.MinValue;
                        int idx = 0;

                        var points = new List<Point>();

                        for (int y = exportWithClasses ? 1 : 0; y < rows; y++)
                        {
                            for (int x = 0; x < cols; x++)
                            {
                                if (resizedData[y, x] == 1)
                                {
                                    var point = new Point(x, y);
                                    points.Add(point);
                                }
                            }
                        }

                        Point[] pCol = new Point[points.Count];

                        foreach (var point in points)
                        {
                            if (point.X > pMaxX) pMaxX = (int)point.X;
                            if (point.Y > pMaxY) pMaxY = (int)point.Y;
                            if (point.X < pMinX) pMinX = (int)point.X;
                            if (point.Y < pMinY) pMinY = (int)point.Y;

                            pCol[idx] = new(point.X, point.Y);
                            idx++;
                        }

                        var width = pMaxX - pMinX;
                        var height = pMaxY - pMinY;

                        if (points.Count == 0 || width == 0 || height == 0)
                        {
                            continue;
                        }

                        Color bBoxColor = Color.FromArgb(255, 235, 64, 52);

                        if (exportWithClasses)
                        {
                            bBoxColor = LabelingHelper.GetBoundingBoxColor(className.Split(";")[0]);
                        }

                        using (Graphics g = Graphics.FromImage(bmp))
                        {
                            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                            using (Pen pen = new(Color.FromArgb(80, bBoxColor.R, bBoxColor.G, bBoxColor.B), 1))
                            {
                                g.DrawPolygon(pen, pCol);
                            }
                        }
                    }

                    return bmp;
                }

                catch (Exception e)
                {
                    Debug.WriteLine(e);
                    return null;
                }
            });

        }

        public async Task<string> ExportWithBBoxes(Bitmap bmp, string csvFile, bool exportWithClasses, string dir, string fileName, bool isCSV)
        {
            try
            {
                await Task.Run(async () =>
                {
                    var labelingHelper = new LabelingHelper();
                    var datas = labelingHelper.GetData(csvFile, isCSV);

                    foreach (var d in datas)
                    {
                        using (Graphics g = Graphics.FromImage(bmp))
                        {
                            Color bBoxColor = Color.FromArgb(255, 235, 64, 52);

                            if (exportWithClasses)
                            {
                                bBoxColor = LabelingHelper.GetBoundingBoxColor(d.classId);
                            }

                            using (Pen pen = new(bBoxColor, 2))
                            {
                                System.Drawing.Rectangle rect = new(Convert.ToInt32(float.Parse(d.x)) * 4, Convert.ToInt32(float.Parse(d.y)) * 4, Convert.ToInt32(float.Parse(d.width)) * 4, Convert.ToInt32(float.Parse(d.height)) * 4);
                                g.DrawRectangle(pen, rect);
                            }
                        }
                    }

                    bmp.Save(@$"{dir}\{fileName}.png");
                    bmp.Dispose();
                });

                return "";
            }

            catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return ex.Message;
            }
        }
    }
}
