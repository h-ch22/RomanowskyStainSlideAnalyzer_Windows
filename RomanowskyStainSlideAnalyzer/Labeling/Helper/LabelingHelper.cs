using Microsoft.UI.Xaml.Shapes;
using RomanowskyStainSlideAnalyzer.History.Models;
using RomanowskyStainSlideAnalyzer.Labeling.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RomanowskyStainSlideAnalyzer.Labeling.Helper
{
    public class LabelingHelper
    {
        private string path;

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

        public string GetSource()
        {
            var jpgFiles = Directory.GetFiles($@"{path}\", "*.jpg");
            var jpegFiles = Directory.GetFiles($@"{path}\", "*.jpeg");
            var pngFiles = Directory.GetFiles($@"{path}\", "*.png");

            var file = "";

            if (jpgFiles.Length > 0) file = jpgFiles[0];
            else if (jpegFiles.Length > 0) file = jpegFiles[0];
            else file = pngFiles[0];

            return file;
        }
    }
}
