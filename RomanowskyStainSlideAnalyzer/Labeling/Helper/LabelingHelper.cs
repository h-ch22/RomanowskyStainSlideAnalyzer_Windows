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
            var filePath = $"{path.Split(".txt")[0]}.csv";
            var csv = new StringBuilder();
            var header = string.Format("{0}, {1}, {2}, {3}, {4}", "Class", "X", "Y", "W", "H");
            csv.AppendLine(header);

            File.WriteAllText(filePath, csv.ToString());
        }

        public void AppendText(int classId, string X, string Y, string W, string H)
        {
            var filePath = $"{path.Split(".txt")[0]}.csv";
            var csv = new StringBuilder();
            var content = string.Format("{0}, {1}, {2}, {3}, {4}", classId.ToString(), X, Y, W, H);
            csv.AppendLine(content);

            File.AppendAllText(filePath, csv.ToString());
        }

        public void ChangeLine(int classId, string X, string Y, string W, string H, int line)
        {
            var filePath = $"{path.Split(".txt")[0]}.csv";
            var csv = new StringBuilder();
            var content = string.Format("{0}, {1}, {2}, {3}, {4}", classId.ToString(), X, Y, W, H);

            string[] lineArr = File.ReadAllLines(filePath);
            lineArr[line] = csv.ToString();
            
            File.WriteAllLines(filePath, lineArr);
        }
    }
}
