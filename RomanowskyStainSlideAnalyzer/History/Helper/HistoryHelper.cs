using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Shapes;
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

        public ObservableCollection<HistoryDataModel> GetHistory(string date)
        {
            var history = new ObservableCollection<HistoryDataModel>();
            var path = @"C:\RomanowskyStainSlideAnalyzer";

            if (!Directory.Exists(path)) return history;

            var directories = Directory.GetDirectories(@"C:\RomanowskyStainSlideAnalyzer");

            foreach(var dir in directories)
            {
                if(dir.Contains(date))
                {
                    var file = GetImage(dir);
                    var csvFiles = Directory.GetFiles($@"{dir}\", "*.csv");

                    var fullDate = dir.Split(@"\");
                    var fullLog = "";

                    using(var reader = new StreamReader($@"{dir}\log.txt", Encoding.UTF8))
                    {
                        while(!reader.EndOfStream)
                        {
                            var line = reader.ReadLine();
                            fullLog += $"{line}\n";
                        }
                    }

                    var canItLabeled = CanItLabeling(file);
                    Symbol symbol;
                    string statusText;

                    if(csvFiles.Length > 0)
                    {
                        symbol = Symbol.Accept;
                        statusText = "Labeling Data Included";
                    } else if(canItLabeled)
                    {
                        symbol = Symbol.ImportAll;
                        statusText = "Labeling data can be imported";
                    } else
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
                            csvFiles.Length > 0 ? Visibility.Visible : Visibility.Collapsed,
                            symbol: symbol,
                            statusText: statusText,
                            canItLabeled: canItLabeled,
                            showChangeButton: canItLabeled ? Visibility.Visible : Visibility.Collapsed,
                            imgFile: file
                        )
                    ); 
                }
            }

            return new ObservableCollection<HistoryDataModel>(history.OrderByDescending(x => x.date));
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
                File.Copy($"{from.Split(".txt")[0]}", $@"{to}\{fileName.Split(@".txt")[0]}");
            }
            catch (Exception e)
            {
                throw e;
            }
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
