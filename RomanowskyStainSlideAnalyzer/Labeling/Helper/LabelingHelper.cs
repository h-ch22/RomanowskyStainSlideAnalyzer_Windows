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
using Windows.ApplicationModel.DataTransfer;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
                case "0":
                default:
                    return Color.FromArgb(255, 235, 64, 52);

                case "1":
                case "Large Cell":
                    return Color.FromArgb(255, 43, 207, 98);

                case "2":
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

        public void CreateCSVFile(string csvFilePath, string to, AnalyzeByClassDataModel[] classData, List<AnalyzeDataModel> data, bool isBBox)
        {
            try
            {
                var csvFilePathSplit = csvFilePath.Split(@"\");
                var postfix = isBBox ? "BoundingBoxes" : "Masks";
                var fileName = $"{csvFilePathSplit[csvFilePathSplit.Length - 1].Split(".csv")[0]}_{postfix}";
                var allLabelHeader = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12}", "Class", "X", "Y", "W", "H", "Size", "A", "R", "G", "B", "Hue", "Saturation", "Brightness");
                var header = string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10}", "Class", "A", "R", "G", "B", "Hue", "Saturation", "Brightness", "Width", "Height", "Size");

                var allFilePath = $@"{to}\{fileName}_Average_of_All.csv";
                var filePath = $@"{to}\{fileName}_Average_by_Class.csv";

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

        public bool IsDestinationFileExists(string folder)
        {
            var splitPath = path.Split(@"\");
            var fileName = splitPath[splitPath.Length - 1];

            return File.Exists($@"{folder}\{fileName.Split(@".txt")[0]}.csv");
        }

        public void Copy(string folder)
        {
            var splitPath = path.Split(@"\");
            var fileName = splitPath[splitPath.Length - 1];
            
            try
            {
                File.Copy($"{path.Split(".txt")[0]}.csv", folder, true);
            }
            catch(Exception e)
            {
                throw e;
            }
        }

        public static void Copy(string from, string to, bool isSingleFile)
        {
            var allFiles = Directory.GetFiles($@"{from}\", "*.csv");
            var csvFiles = allFiles.Where(file => file.Contains("Labeled")).ToArray();

            try
            {
                if(isSingleFile)
                {
                    File.Copy($@"{from}\Mask_Labeled.csv", $@"{to}\Mask_Labeled.csv");
                }

                else
                {
                    foreach (var f in csvFiles)
                    {
                        var fSplit = f.Split(@"\");
                        File.Copy(f, $@"{to}\{fSplit[fSplit.Length - 1]}");
                    }
                }
            } catch(Exception e)
            {
                throw e;
            }
        }

        public async Task CreateLabelingData(List<int> labeledDatas, List<BoundingBoxDataModel> bBoxes)
        {
            await Task.Run(() =>
            {
                try
                {
                    for(var i = 0; i < labeledDatas.Count; i++)
                    {
                        var filePath = $"{path.Split(".txt")[0]}.csv";
                        var csv = new StringBuilder();
                        var content = string.Format(
                                                    "{0},{1},{2},{3},{4}",
                                                    labeledDatas[i].ToString(),
                                                    bBoxes[i].X,
                                                    bBoxes[i].Y,
                                                    bBoxes[i].Width,
                                                    bBoxes[i].Height
                                                );

                        csv.AppendLine(content);

                        File.AppendAllText(filePath, csv.ToString());
                    }
                }

                catch (Exception ex)
                {
                    throw ex;
                }
            });
        }

        public static void CreateMaskLabelingData(string path, bool exportAsOneFile, List<int> labeledDatas)
        {
            var allFiles = Directory.GetFiles($@"{path}\", "*.csv");
            var csvFiles = allFiles.Where(file => !file.Contains("Labeled")).ToArray();

            if(File.Exists($@"{path}\Mask_Labeled.csv") && exportAsOneFile)
            {
                File.Delete($@"{path}\Mask_Labeled.csv");
            }

            for (var i = 0; i < csvFiles.Length; i++)
            {
                var file = csvFiles[i];

                try
                {
                    string filePath = "";
                    List<string> mergedLines = new();

                    var lines = File.ReadAllLines(file);
                    mergedLines.Add($"{labeledDatas[i].ToString()};");
                    mergedLines.AddRange(lines[1..]);

                    if (exportAsOneFile)
                    {
                        filePath = $@"{path}\Mask_Labeled.csv";

                        if(!File.Exists(filePath))
                        {
                            File.WriteAllLines(filePath, mergedLines);
                        }

                        else
                        {
                            File.AppendAllLines(filePath, mergedLines);
                        }
                    }

                    else
                    {
                        filePath = $@"{path}\Mask_Labeled_{i}.csv";
                        File.WriteAllLines(filePath, mergedLines);
                    }
                }

                catch (Exception ex)
                {
                    throw ex;
                }
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

        public static string convertClassIdAsClass(string classId)
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

        public static void writePythonFile(bool isSingleFile, string path)
        {
            var postfix = isSingleFile ? "single" : "multiple";
            string pyPath = $@"{path}\main_{postfix}.py";
            var pythonStylePath = path.Replace(@"\", "/");

            List<string> singleCodeLines = new()
            {
                "import numpy as np\n",
                "classes = []",
                "data_rows = []",
                $"segmentation_data = '{pythonStylePath}/Mask_Labeled.csv'\n",
                "with open(segmentation_data, 'r') as file:",
                "\tfor line in file:",
                "\t\tline = line.strip()\n",
                "\t\tif ';' in line:",
                "\t\t\tclass_value = int(line.replace(';', '').strip())",
                "\t\t\tclasses.append(class_value)\n",
                "\t\telse:",
                "\t\t\tdata_row = list(map(int, line.split(',')))",
                "\t\t\tdata_rows.append(data_row)\n",
                "data_array = np.array(data_rows)"
            };

            List<string> multipleCodeLines = new()
            {
                "import numpy as np",
                "from glob import glob\n",
                "classes = []",
                "data_rows = []",
                $"segmentation_datas = '{pythonStylePath}/Mask_Labeled_*.csv'\n",
                "for file in glob(segmentation_datas):",
                "\twith open(file, 'r') as f:",
                "\t\tfor line in f:",
                "\t\t\tline = line.strip()\n",
                "\t\t\tif ';' in line:",
                "\t\t\t\tclass_value = int(line.replace(';', '').strip())",
                "\t\t\t\tclasses.append(class_value)\n",
                "\t\t\telse:",
                "\t\t\t\tdata_row = list(map(int, line.split(',')))",
                "\t\t\t\tdata_rows.append(data_row)",
                "data_array = np.array(data_rows)"
            };

            if (isSingleFile)
            {
                using (StreamWriter writer = new StreamWriter(pyPath))
                {
                    foreach (var line in singleCodeLines)
                    {
                        writer.WriteLine(line);
                    }
                }
            }

            else
            {
                using (StreamWriter writer = new StreamWriter(pyPath))
                {
                    foreach (var line in multipleCodeLines)
                    {
                        writer.WriteLine(line);
                    }
                }
            }
        }
    }
}
