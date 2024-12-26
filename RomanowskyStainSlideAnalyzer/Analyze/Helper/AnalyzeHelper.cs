using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using RomanowskyStainSlideAnalyzer.Analyze.Models;
using RomanowskyStainSlideAnalyzer.Labeling.Helper;
using RomanowskyStainSlideAnalyzer.Labeling.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;

namespace RomanowskyStainSlideAnalyzer.Analyze.Helper
{
    public class AnalyzeHelper
    {
        public static int[,] GetMask(string path, string idx)
        {
            int Height = 512, Width = 512;

            int[,] data = new int[Height, Width];
            string[] lines = File.ReadAllLines($@"{path}\mask_{idx}.csv");

            for (int y = 0; y < Height; y++)
            {
                var values = lines[y].Split(',');
                for (int x = 0; x < Width; x++)
                {
                    data[y, x] = int.Parse(values[x]);
                }
            }
            return data;
        }

        public static (string, int[,]) GetMaskWithClass(string path, string idx)
        {
            int Height = 512, Width = 512;

            int[,] data = new int[Height, Width];
            string[] lines = File.ReadAllLines($@"{path}\Mask_Labeled_{idx}.csv");

            for (int y = 1; y < Height; y++)
            {
                var values = lines[y].Split(',');
                for (int x = 0; x < Width; x++)
                {
                    data[y, x] = int.Parse(values[x]);
                }
            }

            return (lines[0], data);
        }

        public static int GetMaskCount(string path)
        {
            var files = Directory.GetFiles(path);

            return files.Count(s => !s.Contains("Labeled"));
        }

        public static List<Tuple<int, int>>? GetMaskData(string path, string idx, bool isLabeled = false)
        {
            var csvFile = isLabeled ? $@"{path}\Mask_Labeled_{idx}.csv" : $@"{path}\mask_{idx}.csv";
            int[,] data = new int[512, 512];
            List<Tuple<int, int>> coordinates = new List<Tuple<int, int>>();

            try
            {
                string[] lines = File.ReadAllLines(csvFile);

                for (int y = 1; y < 511; y++)
                {
                    var values = lines[y].Split(',');

                    for (int x = 1; x < 511; x++)
                    {
                        data[y, x] = int.Parse(values[x]);

                        if (data[y, x] == 1)
                        {
                            coordinates.Add(new Tuple<int, int>(y, x));
                        }
                    }
                }

                return coordinates;
            }
            catch (Exception ex)
            {
                Debug.Write(ex.Message);
                return null;
            }
        }

        public static (int, int, int, int) CalculateMaskSize(List<Tuple<int, int>> coordinates)
        {
            int minX = coordinates.Min(c => c.Item2);
            int maxX = coordinates.Max(c => c.Item2);
            int minY = coordinates.Min(c => c.Item1);
            int maxY = coordinates.Max(c => c.Item1);

            int width = maxX - minX + 1;
            int height = maxY - minY + 1;

            return (width, height, minX ,minY);
        }

        public static int[,] ResizeData(int[,] data, int originalWidth, int originalHeight, int targetWidth, int targetHeight)
        {
            int[,] resizedData = new int[targetHeight, targetWidth];
            float xRatio = (float)originalWidth / targetWidth;
            float yRatio = (float)originalHeight / targetHeight;

            for (int y = 0; y < targetHeight; y++)
            {
                for (int x = 0; x < targetWidth; x++)
                {
                    int originalX = (int)(x * xRatio);
                    int originalY = (int)(y * yRatio);
                    resizedData[y, x] = data[originalY, originalX];
                }
            }
            return resizedData;
        }

        private string GetClass(string path, string idx)
        {
            var csvFile = $@"{path}\Mask_Labeled_{idx}.csv";
            return File.ReadLines(csvFile).First().Split(";")[0];
        }

        public ObservableCollection<LabelingDataModel> GetData(string maskPath)
        {
            var csvFiles = Directory.GetFiles(maskPath, "Mask_Labeled_*.csv");
            ObservableCollection<LabelingDataModel> datas = new();

            try
            {
                for(var i = 0; i < csvFiles.Length; i++)
                {
                    var maskData = AnalyzeHelper.GetMaskData(maskPath, i.ToString());
                    var maskSize = AnalyzeHelper.CalculateMaskSize(maskData);

                    datas.Add(
                        new(
                            (i+1).ToString(),
                            LabelingHelper.convertClassIdAsClass(GetClass(maskPath, i.ToString())),
                            maskSize.Item3.ToString(),
                            maskSize.Item4.ToString(),
                            maskSize.Item1.ToString(),
                            maskSize.Item2.ToString()
                        )
                    );
                }

                return datas;
            }

            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return datas;
            }
        }

        public async Task<(AvgDataModel?, Bitmap?)> Analyze(int[,] mask, string imagePath)
        {
            int rows = mask.GetLength(0);
            int cols = mask.GetLength(1);

            int pMinX = int.MaxValue, pMinY = int.MaxValue;
            int pMaxX = int.MinValue, pMaxY = int.MinValue;
            int i = 0;

            var points = new List<Point>();

            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < cols; x++)
                {
                    if (mask[y, x] == 1)
                    {
                        var point = new Point(x, y);
                        points.Add(point);
                    }
                }
            }

            Point[] pCol = new Point[points.Count];

            foreach(var point in points)
            {
                if (point.X > pMaxX) pMaxX = (int)point.X;
                if (point.Y > pMaxY) pMaxY = (int)point.Y;
                if (point.X < pMinX) pMinX = (int)point.X;
                if (point.Y < pMinY) pMinY = (int)point.Y;

                pCol[i] = new(point.X, point.Y);
                i++;
            }

            return await Task.Run(async () =>
            {
                var width = pMaxX - pMinX;
                var height = pMaxY - pMinY;

                if (points.Count == 0 || width == 0 || height == 0)
                {
                    return (null, null);
                }

                using (Bitmap originalImage = new(imagePath))
                {
                    using (Bitmap bmp = new(originalImage, new Size(512, 512)))
                    {
                        var brush = new TextureBrush(bmp);
                        var bmpWrk = new Bitmap(512, 512);

                        using(Graphics g = Graphics.FromImage(bmpWrk))
                        {
                            g.FillPolygon(brush, pCol);
                        }

                        System.Drawing.Rectangle cropRect = new(pMinX, pMinY, width, height);
                        var bmpClone = bmpWrk.Clone(cropRect, bmpWrk.PixelFormat);

                        var avgData = await GetAvg(bmpClone);

                        return (avgData, bmpClone);
                    }
                }
            });
        }

        public async Task<(AvgDataModel, Bitmap)> Analyze(string x, string y, string width, string height, string imagePath)
        {
            return await Task.Run(async () =>
            {
                using (Bitmap _bmp = new(imagePath))
                {
                    using(Bitmap bmp = new(_bmp, new Size(512, 512)))
                    {
                        Rectangle cropRect = new(int.Parse(x), int.Parse(y), int.Parse(width), int.Parse(height));
                        var croppedImage = bmp.Clone(cropRect, bmp.PixelFormat);

                        var croppedBmp = bmp.Clone(cropRect, bmp.PixelFormat);
                        var avgData = await GetAvg(croppedImage);

                        return (avgData, croppedBmp);
                    }
                }
            });
        }

        public async Task<AllAvgDataModel> Analyze(string imagePath, List<LabelingDataModel> Datas, string csvPath)
        {
            var dataCount = Directory.GetFiles($@"{csvPath}", "mask_*.csv").Count(file => Path.GetFileName(file).StartsWith("mask_")); ;

            return await Task.Run(async () =>
            {
                var avgAll = new AvgDataModel(Color.Red, 0, 0, 0, 0, 0f, 0f, 0f, 0f, 0, 0, 0f);
                var avgNone = new AvgDataModel(Color.Red, 0, 0, 0, 0, 0f, 0f, 0f, 0f, 0, 0, 0f);
                var avgLCell = new AvgDataModel(Color.Red, 0, 0, 0, 0, 0f, 0f, 0f, 0f, 0, 0, 0f);
                var avgSCell = new AvgDataModel(Color.Red, 0, 0, 0, 0, 0f, 0f, 0f, 0f, 0, 0, 0f);

                for (var i = 0; i < dataCount; i++)
                {
                    var mask = AnalyzeHelper.GetMask(csvPath, i.ToString());

                    var analyzedData = await Analyze(mask, imagePath);

                    if (analyzedData.Item1 == null || analyzedData.Item2 == null) continue;

                    var data = analyzedData.Item1;
                    var bitmap = analyzedData.Item2;

                    var x = int.Parse(Datas[i].x);
                    var y = int.Parse(Datas[i].y);
                    var width = int.Parse(Datas[i].width);
                    var height = int.Parse(Datas[i].height);

                    avgAll.width += float.Parse(Datas[i].width);
                    avgAll.height += float.Parse(Datas[i].height);
                    avgAll.size += float.Parse(Datas[i].width) * float.Parse(Datas[i].height);

                    if (width == 0 || height == 0) continue;

                    using(Bitmap croppedImage = bitmap)
                    {
                        for(int _x = 0; _x < bitmap.Width; _x++)
                        {
                            for(int _y = 0; _y < bitmap.Height; _y++)
                            {
                                Color clr = bitmap.GetPixel(_x, _y);
                                avgAll.a += clr.A;
                                avgAll.r += clr.R;
                                avgAll.g += clr.G;
                                avgAll.b += clr.B;
                                avgAll.brightness += clr.GetBrightness();
                                avgAll.hue += clr.GetHue();
                                avgAll.saturation += clr.GetSaturation();

                                avgAll.total++;

                                switch(Datas[i].classId)
                                {
                                    case "None":
                                        avgNone.a += clr.A;
                                        avgNone.r += clr.R;
                                        avgNone.g += clr.G;
                                        avgNone.b += clr.B;
                                        avgNone.brightness += clr.GetBrightness();
                                        avgNone.hue += clr.GetHue();
                                        avgNone.saturation += clr.GetSaturation();

                                        avgNone.width += float.Parse(Datas[i].width);
                                        avgNone.height += float.Parse(Datas[i].height);
                                        avgNone.size += (float.Parse(Datas[i].width) * float.Parse(Datas[i].height));

                                        avgNone.total++;

                                        break;

                                    case "Large Cell":
                                        avgLCell.a += clr.A;
                                        avgLCell.r += clr.R;
                                        avgLCell.g += clr.G;
                                        avgLCell.b += clr.B;
                                        avgLCell.brightness += clr.GetBrightness();
                                        avgLCell.hue += clr.GetHue();
                                        avgLCell.saturation += clr.GetSaturation();

                                        avgLCell.width += float.Parse(Datas[i].width);
                                        avgLCell.height += float.Parse(Datas[i].height);
                                        avgLCell.size += (float.Parse(Datas[i].width) * float.Parse(Datas[i].height));

                                        avgLCell.total++;

                                        break;

                                    case "Small Cell":
                                        avgSCell.a += clr.A;
                                        avgSCell.r += clr.R;
                                        avgSCell.g += clr.G;
                                        avgSCell.b += clr.B;
                                        avgSCell.brightness += clr.GetBrightness();
                                        avgSCell.hue += clr.GetHue();
                                        avgSCell.saturation += clr.GetSaturation();

                                        avgSCell.width += float.Parse(Datas[i].width);
                                        avgSCell.height += float.Parse(Datas[i].height);
                                        avgSCell.size += (float.Parse(Datas[i].width) * float.Parse(Datas[i].height));

                                        avgSCell.total++;

                                        break;

                                    default: break;
                                }
                            }
                        }
                    }
                }

                avgAll.calculateHSB();
                avgAll.calculateSize(Datas.Count);

                avgLCell.calculateHSB();
                avgLCell.calculateSize();

                avgSCell.calculateHSB();
                avgSCell.calculateSize();

                avgNone.calculateHSB();
                avgNone.calculateSize();

                avgAll.a = GetAvg(avgAll.a, avgAll.total);
                avgAll.r = GetAvg(avgAll.r, avgAll.total);
                avgAll.g = GetAvg(avgAll.g, avgAll.total);
                avgAll.b = GetAvg(avgAll.b, avgAll.total);

                avgNone.a = GetAvg(avgNone.a, avgNone.total);
                avgNone.r = GetAvg(avgNone.r, avgNone.total);
                avgNone.g = GetAvg(avgNone.g, avgNone.total);
                avgNone.b = GetAvg(avgNone.b, avgNone.total);

                avgLCell.a = GetAvg(avgLCell.a, avgLCell.total);
                avgLCell.r = GetAvg(avgLCell.r, avgLCell.total);
                avgLCell.g = GetAvg(avgLCell.g, avgLCell.total);
                avgLCell.b = GetAvg(avgLCell.b, avgLCell.total);

                avgSCell.a = GetAvg(avgSCell.a, avgSCell.total);
                avgSCell.r = GetAvg(avgSCell.r, avgSCell.total);
                avgSCell.g = GetAvg(avgSCell.g, avgSCell.total);
                avgSCell.b = GetAvg(avgSCell.b, avgSCell.total);

                avgAll.setARGB();
                avgLCell.setARGB();
                avgSCell.setARGB();
                avgNone.setARGB();

                return new AllAvgDataModel(avgAll, avgNone, avgLCell, avgSCell);
            });
        }

        public async Task<AllAvgDataModel> Analyze(string imagePath, List<LabelingDataModel> Datas, bool isContour = false)
        {
            return await Task.Run(() =>
            {
                var avgAll = new AvgDataModel(Color.Red, 0, 0, 0, 0, 0f, 0f, 0f, 0f, 0, 0, 0f);
                var avgNone = new AvgDataModel(Color.Red, 0, 0, 0, 0, 0f, 0f, 0f, 0f, 0, 0, 0f);
                var avgLCell = new AvgDataModel(Color.Red, 0, 0, 0, 0, 0f, 0f, 0f, 0f, 0, 0, 0f);
                var avgSCell = new AvgDataModel(Color.Red, 0, 0, 0, 0, 0f, 0f, 0f, 0f, 0, 0, 0f);

                using (Bitmap _bmp = new(imagePath))
                {
                    Bitmap bmp = new(_bmp, new Size(512, 512));

                    foreach (var data in Datas)
                    {
                        var x = int.Parse(data.x);
                        var y = int.Parse(data.y);
                        var width = int.Parse(data.width);
                        var height = int.Parse(data.height);

                        avgAll.width += float.Parse(data.width);
                        avgAll.height += float.Parse(data.height);
                        avgAll.size += float.Parse(data.width) * float.Parse(data.height);

                        if (width == 0 || height == 0)
                        {
                            continue;
                        }

                        Rectangle cropRect = new(x, y, width, height);

                        using (Bitmap croppedImage = bmp.Clone(cropRect, bmp.PixelFormat))
                        {
                            for (int _x = 0; _x < bmp.Width; _x++)
                            {
                                for (int _y = 0; _y < bmp.Height; _y++)
                                {
                                    Color clr = bmp.GetPixel(_x, _y);
                                    avgAll.a += clr.A;
                                    avgAll.r += clr.R;
                                    avgAll.g += clr.G;
                                    avgAll.b += clr.B;
                                    avgAll.brightness += clr.GetBrightness();
                                    avgAll.hue += clr.GetHue();
                                    avgAll.saturation += clr.GetSaturation();

                                    avgAll.total++;

                                    switch (data.classId)
                                    {
                                        case "None":
                                            avgNone.a += clr.A;
                                            avgNone.r += clr.R;
                                            avgNone.g += clr.G;
                                            avgNone.b += clr.B;
                                            avgNone.brightness += clr.GetBrightness();
                                            avgNone.hue += clr.GetHue();
                                            avgNone.saturation += clr.GetSaturation();

                                            avgNone.width += float.Parse(data.width);
                                            avgNone.height += float.Parse(data.height);
                                            avgNone.size += (float.Parse(data.width) * float.Parse(data.height));

                                            avgNone.total++;

                                            break;

                                        case "Large Cell":
                                            avgLCell.a += clr.A;
                                            avgLCell.r += clr.R;
                                            avgLCell.g += clr.G;
                                            avgLCell.b += clr.B;
                                            avgLCell.brightness += clr.GetBrightness();
                                            avgLCell.hue += clr.GetHue();
                                            avgLCell.saturation += clr.GetSaturation();

                                            avgLCell.width += float.Parse(data.width);
                                            avgLCell.height += float.Parse(data.height);
                                            avgLCell.size += (float.Parse(data.width) * float.Parse(data.height));

                                            avgLCell.total++;

                                            break;

                                        case "Small Cell":
                                            avgSCell.a += clr.A;
                                            avgSCell.r += clr.R;
                                            avgSCell.g += clr.G;
                                            avgSCell.b += clr.B;
                                            avgSCell.brightness += clr.GetBrightness();
                                            avgSCell.hue += clr.GetHue();
                                            avgSCell.saturation += clr.GetSaturation();

                                            avgSCell.width += float.Parse(data.width);
                                            avgSCell.height += float.Parse(data.height);
                                            avgSCell.size += (float.Parse(data.width) * float.Parse(data.height));

                                            avgSCell.total++;

                                            break;

                                        default: break;
                                    }
                                }
                            }
                        }
                    }

                    avgAll.calculateHSB();
                    avgAll.calculateSize(Datas.Count);

                    avgLCell.calculateHSB();
                    avgLCell.calculateSize();

                    avgSCell.calculateHSB();
                    avgSCell.calculateSize();

                    avgNone.calculateHSB();
                    avgNone.calculateSize();

                    avgAll.a = GetAvg(avgAll.a, avgAll.total);
                    avgAll.r = GetAvg(avgAll.r, avgAll.total);
                    avgAll.g = GetAvg(avgAll.g, avgAll.total);
                    avgAll.b = GetAvg(avgAll.b, avgAll.total);

                    avgNone.a = GetAvg(avgNone.a, avgNone.total);
                    avgNone.r = GetAvg(avgNone.r, avgNone.total);
                    avgNone.g = GetAvg(avgNone.g, avgNone.total);
                    avgNone.b = GetAvg(avgNone.b, avgNone.total);

                    avgLCell.a = GetAvg(avgLCell.a, avgLCell.total);
                    avgLCell.r = GetAvg(avgLCell.r, avgLCell.total);
                    avgLCell.g = GetAvg(avgLCell.g, avgLCell.total);
                    avgLCell.b = GetAvg(avgLCell.b, avgLCell.total);

                    avgSCell.a = GetAvg(avgSCell.a, avgSCell.total);
                    avgSCell.r = GetAvg(avgSCell.r, avgSCell.total);
                    avgSCell.g = GetAvg(avgSCell.g, avgSCell.total);
                    avgSCell.b = GetAvg(avgSCell.b, avgSCell.total);

                    avgAll.setARGB();
                    avgLCell.setARGB();
                    avgSCell.setARGB();
                    avgNone.setARGB();

                    return new AllAvgDataModel(avgAll, avgNone, avgLCell, avgSCell);
                }
            });
        }

        private async Task<AvgDataModel> GetAvg(Bitmap bmp)
        {
            return await Task.Run(() =>
            {
                int a = 0, r = 0, g = 0, b = 0, total = 0;
                float brightness = 0.0f, hue = 0.0f, saturation = 0.0f;

                BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, bmp.PixelFormat);
                IntPtr ptr = bmpData.Scan0;
                int bytesPerPixel = Bitmap.GetPixelFormatSize(bmp.PixelFormat) / 8;
                int byteCount = bmpData.Stride * bmp.Height;
                byte[] pixels = new byte[byteCount];
                System.Runtime.InteropServices.Marshal.Copy(ptr, pixels, 0, byteCount);

                for (int x = 0; x < bmp.Width; x++)
                {
                    for (int y = 0; y < bmp.Height; y++)
                    {
                        int index = (y * bmpData.Stride) + (x * bytesPerPixel);
                        byte blue = pixels[index];
                        byte green = pixels[index + 1];
                        byte red = pixels[index + 2];
                        byte alpha = pixels[index + 3];

                        Color clr = Color.FromArgb(alpha, red, green, blue);

                        a += clr.A;
                        r += clr.R;
                        g += clr.G;
                        b += clr.B;
                        brightness += clr.GetBrightness();
                        hue += clr.GetHue();
                        saturation += clr.GetSaturation();

                        total++;
                    }
                }

                bmp.UnlockBits(bmpData);

                a /= total;
                r /= total;
                g /= total;
                b /= total;

                brightness /= total;
                hue /= total;
                saturation /= total;

                var size = float.Parse(bmp.Width.ToString()) * float.Parse(bmp.Height.ToString());

                return new AvgDataModel(Color.FromArgb(a, r, g, b), a, r, g, b, brightness, hue, saturation, total, bmp.Width, bmp.Height, size);
            });
        }

        private int GetAvg(float target, float total)
        {
            if (target == 0f || total == 0f) return 0;

            var result = target / total;
            var resultAsInt = Convert.ToInt32(result);

            resultAsInt = resultAsInt > 255 ? 255 : resultAsInt;
            resultAsInt = resultAsInt < 0 ? 0 : resultAsInt;

            return resultAsInt;
        }

        public async Task<(AnalyzeByClassDataModel[], List<AnalyzeDataModel>)> Export(AllAvgDataModel allData, ObservableCollection<LabelingDataModel> Datas, string imagePath)
        {
            AnalyzeByClassDataModel[] data = [
                new(
                    "All",
                    allData.avgData.a.ToString(),
                    allData.avgData.r.ToString(),
                    allData.avgData.g.ToString(),
                    allData.avgData.b.ToString(),
                    allData.avgData.hue.ToString(),
                    allData.avgData.saturation.ToString(),
                    allData.avgData.brightness.ToString(),
                    allData.avgData.width.ToString(),
                    allData.avgData.height.ToString(),
                    allData.avgData.size.ToString()
                ),

                new(
                    "None",
                    allData.avgNone.a.ToString(),
                    allData.avgNone.r.ToString(),
                    allData.avgNone.g.ToString(),
                    allData.avgNone.b.ToString(),
                    allData.avgNone.hue.ToString(),
                    allData.avgNone.saturation.ToString(),
                    allData.avgNone.brightness.ToString(),
                    allData.avgNone.width.ToString(),
                    allData.avgNone.height.ToString(),
                    allData.avgNone.size.ToString()
                ),

                new(
                    "Large Cell",
                    allData.avgLCell.a.ToString(),
                    allData.avgLCell.r.ToString(),
                    allData.avgLCell.g.ToString(),
                    allData.avgLCell.b.ToString(),
                    allData.avgLCell.hue.ToString(),
                    allData.avgLCell.saturation.ToString(),
                    allData.avgLCell.brightness.ToString(),
                    allData.avgLCell.width.ToString(),
                    allData.avgLCell.height.ToString(),
                    allData.avgLCell.size.ToString()
                ),

                new(
                    "Small Cell",
                    allData.avgSCell.a.ToString(),
                    allData.avgSCell.r.ToString(),
                    allData.avgSCell.g.ToString(),
                    allData.avgSCell.b.ToString(),
                    allData.avgSCell.hue.ToString(),
                    allData.avgSCell.saturation.ToString(),
                    allData.avgSCell.brightness.ToString(),
                    allData.avgSCell.width.ToString(),
                    allData.avgSCell.height.ToString(),
                    allData.avgSCell.size.ToString()
                )
            ];

            List<AnalyzeDataModel> dataByBBox = new();

            var tasks = Datas.Select(async d =>
            {
                return await Task.Run(async () =>
                {
                    using (Bitmap _bmp = new(imagePath))
                    {
                        Bitmap bmp = new(_bmp, new Size(512, 512));
                        Rectangle cropRect = new(int.Parse(d.x), int.Parse(d.y), int.Parse(d.width), int.Parse(d.height));

                        if (int.Parse(d.width) != 0 && int.Parse(d.height) != 0)
                        {
                            using (Bitmap croppedImage = bmp.Clone(cropRect, bmp.PixelFormat))
                            {
                                var avgData = await GetAvg(croppedImage);

                                return new AnalyzeDataModel(
                                    d.x,
                                    d.y,
                                    d.width,
                                    d.height,
                                    new(
                                        d.classId,
                                        avgData.a.ToString(),
                                        avgData.r.ToString(),
                                        avgData.g.ToString(),
                                        avgData.b.ToString(),
                                        avgData.hue.ToString(),
                                        avgData.saturation.ToString(),
                                        avgData.brightness.ToString(),
                                        avgOfSize: avgData.size.ToString()
                                    )
                                );
                            }
                        }
                        else
                        {
                            return new AnalyzeDataModel(d.x, d.y, d.width, d.height, new(d.classId, "", "", "", "", "", "", "", avgOfSize: "0"));
                        }
                    }

                });
            });

            dataByBBox.AddRange(await Task.WhenAll(tasks));

            return (data, dataByBBox);
        }
    }
}
