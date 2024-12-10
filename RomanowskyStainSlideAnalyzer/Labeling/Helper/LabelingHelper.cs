using Microsoft.UI.Xaml.Shapes;
using RomanowskyStainSlideAnalyzer.History.Models;
using RomanowskyStainSlideAnalyzer.Labeling.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
                var header = string.Format("{0}, {1}, {2}, {3}, {4}", "Class", "X", "Y", "W", "H");
                csv.AppendLine(header);

                File.WriteAllText(filePath, csv.ToString());
            }
            catch (Exception ex) {
                throw ex;
            }
        }

        public void AppendText(int classId, string X, string Y, string W, string H)
        {
            try
            {
                var filePath = $"{path.Split(".txt")[0]}.csv";
                var csv = new StringBuilder();
                var content = string.Format("{0}, {1}, {2}, {3}, {4}", classId.ToString(), X, Y, W, H);
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
                var content = string.Format("{0}, {1}, {2}, {3}, {4}", classId.ToString(), X, Y, W, H);

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

        public ObservableCollection<LabelingDataModel> GetData(string csvPath)
        {
            ObservableCollection<LabelingDataModel> datas = new();

            try
            {
                StreamReader sr = new(csvPath);
                int id = 0;

                while (!sr.EndOfStream)
                {
                    string line = sr.ReadLine();

                    if (id > 0)
                    {
                        string[] data = line.Split(',');

                        string classId = convertClassIdAsClass(data[0]);
                        string X = data[1];
                        string Y = data[2];
                        string W = data[3];
                        string H = data[4];

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
    }
}
