using Microsoft.UI.Xaml.Shapes;
using RomanowskyStainSlideAnalyzer.Analyze.Models;
using RomanowskyStainSlideAnalyzer.History.Models;
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

namespace RomanowskyStainSlideAnalyzer.Labeling.Helper
{
    public class LabelingHelper
    {
        private string path;

        public LabelingHelper()
        {

        }

        public LabelingHelper(string path)
        {
            this.path = path;
        }

        public static Color GetBoundingBoxColor(string className)
        {
            switch (className)
            {
                case "None":
                default:
                    return Color.FromArgb(255, 235, 64, 52);

                case "Large Cell":
                    return Color.FromArgb(255, 43, 207, 98);

                case "Small Cell":
                    return Color.FromArgb(255, 43, 120, 207);
            }
        }

        public List<BoundingBoxDataModel> GetBoundingBox()
        {
            List<BoundingBoxDataModel> boundingBoxes = new();

            if(!File.Exists(path))
            {
                return boundingBoxes;
            }

            using (var reader = new StreamReader(path, Encoding.UTF8))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var lineSplit = line.Split("[")[1].Split("]")[0];
                    var X = lineSplit.Split(", ")[0];
                    var Y = lineSplit.Split(", ")[1];
                    var W = lineSplit.Split(", ")[2];
                    var H = lineSplit.Split(", ")[3];

                    boundingBoxes.Add(
                        new(Double.Parse(X), Double.Parse(Y), Double.Parse(W), Double.Parse(H))
                    );
                }
            }

            return boundingBoxes;
        }

        public bool GetFileAlreadyExists()
        {
            var filePath = $"{path.Split(".txt")[0]}.csv";

            return File.Exists(filePath);
        }

        public void CreateCSVFile()
        {
            try
            {
                var filePath = $"{path.Split(".txt")[0]}.csv";
                var csv = new StringBuilder();
                var header = string.Format("{0},{1},{2},{3},{4}", "Class", "X", "Y", "W", "H");
                csv.AppendLine(header);

                File.WriteAllText(filePath, csv.ToString());
            }
            catch (Exception ex) {
                throw ex;
            }
        }

        public void CreateCSVFile(string csvFilePath, string to, AnalyzeByClassDataModel[] classData, List<AnalyzeDataModel> data)
        {
            try
            {
                var csvFilePathSplit = csvFilePath.Split(@"\");
                var fileName = csvFilePathSplit[csvFilePathSplit.Length - 1].Split(".csv")[0];
                var allLabelHeader = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12}", "Class", "X", "Y", "W", "H", "Size", "A", "R", "G", "B", "Hue", "Saturation", "Brightness");
                var header = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10}", "Class", "A", "R", "G", "B", "Hue", "Saturation", "Brightness", "Width", "Height", "Size");

                var allFilePath = $@"{to}\{fileName}_Average_of_All.csv";
                var filePath = $@"{to}\{fileName}_Average_by_Class.csv";

                Debug.WriteLine($"allFilePath: {allFilePath}, filePath: {filePath}, csvPath: {csvFilePath}, fileName: {fileName}");

                var allCsv = new StringBuilder();
                var csv = new StringBuilder();

                allCsv.AppendLine(allLabelHeader);
                csv.AppendLine(header);

                foreach(var d in classData)
                {
                    csv.AppendLine(
                        string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10}", convertClassAsClassId(d.className), d.avgOfA, d.avgOfR, d.avgOfG, d.avgOfB, d.avgOfHue, d.avgOfSaturation, d.avgOfBrightness, d.avgOfWidth, d.avgOfHeight, d.avgOfSize)
                    );
                }

                foreach(var d in data)
                {
                    allCsv.AppendLine(
                       string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12}", convertClassAsClassId(d.data.className), d.x, d.y, d.w, d.h, d.data.avgOfSize, d.data.avgOfA, d.data.avgOfR, d.data.avgOfG, d.data.avgOfB, d.data.avgOfHue, d.data.avgOfSaturation, d.data.avgOfBrightness)
                    );
                }

                File.WriteAllText(allFilePath, allCsv.ToString());
                File.WriteAllText(filePath, csv.ToString());
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void AppendText(int classId, string X, string Y, string W, string H)
        {
            try
            {
                var filePath = $"{path.Split(".txt")[0]}.csv";
                var csv = new StringBuilder();
                var content = string.Format("{0},{1},{2},{3},{4}", classId.ToString(), X, Y, W, H);
                csv.AppendLine(content);

                File.AppendAllText(filePath, csv.ToString());
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void ChangeLine(int classId, string X, string Y, string W, string H, int line)
        {
            try
            {
                var filePath = $"{path.Split(".txt")[0]}.csv";
                var content = string.Format("{0},{1},{2},{3},{4}", classId.ToString(), X, Y, W, H);

                string[] lineArr = File.ReadAllLines(filePath);
                lineArr[line] = content.ToString();

                File.WriteAllLines(filePath, lineArr);
            }

            catch(Exception ex)
            {
                throw ex;
            }
        }

        public void Copy(string folder)
        {
            var splitPath = path.Split(@"\");
            var fileName = splitPath[splitPath.Length - 1];
            
            try
            {
                File.Copy($"{path.Split(".txt")[0]}.csv", $@"{folder}\{fileName.Split(@".txt")[0]}.csv");
            }
            catch(Exception e)
            {
                throw e;
            }
        }

        public ObservableCollection<LabelingDataModel> GetData(string csvPath, bool isCSV = true)
        {
            ObservableCollection<LabelingDataModel> datas = new();

            try
            {
                StreamReader sr = new(csvPath);
                int id = 0;
                List<BoundingBoxDataModel> boundingBoxes = new();

                while (!sr.EndOfStream)
                {
                    string line = sr.ReadLine();

                    if (id > 0)
                    {
                        string[] data = line.Split(',');
                        string classId;
                        string X;
                        string Y;
                        string W;
                        string H;

                        if(isCSV)
                        {
                            classId = convertClassIdAsClass(data[0]);
                            X = data[1];
                            Y = data[2];
                            W = data[3];
                            H = data[4];
                        }

                        else
                        {
                            var lineSplit = line.Split("[")[1].Split("]")[0];
                            X = lineSplit.Split(", ")[0];
                            Y = lineSplit.Split(", ")[1];
                            W = lineSplit.Split(", ")[2];
                            H = lineSplit.Split(", ")[3];

                            classId = "";
                        }

                        datas.Add(new(id.ToString(), classId, X, Y, W, H));
                    }

                    id += 1;
                }

                sr.Close();
                return datas;
            }

            catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return datas;
            }
        }

        private string convertClassIdAsClass(string classId)
        {
            switch(classId)
            {
                case "0": return "None";
                case "1": return "Large Cell";
                case "2": return "Small Cell";
                default: return "Unknown";
            }
        }

        private string convertClassAsClassId(string className)
        {
            switch(className)
            {
                case "None": return "0";
                case "Large Cell": return "1";
                case "Small Cell": return "2";
                default: return className;
            }
        }
    }
}
