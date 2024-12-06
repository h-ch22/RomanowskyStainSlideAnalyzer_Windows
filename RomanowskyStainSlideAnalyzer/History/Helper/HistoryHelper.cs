using RomanowskyStainSlideAnalyzer.History.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RomanowskyStainSlideAnalyzer.History.Helper
{
    public class HistoryHelper
    {
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
                    var jpgFiles = Directory.GetFiles($@"{dir}\", "*.jpg");
                    var jpegFiles = Directory.GetFiles($@"{dir}\", "*.jpeg");
                    var pngFiles = Directory.GetFiles($@"{dir}\", "*.png");

                    var file = "";

                    if (jpgFiles.Length > 0) file = jpgFiles[0];
                    else if(jpegFiles.Length > 0) file = jpegFiles[0];
                    else file = pngFiles[0];

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

                    history.Add(
                        new HistoryDataModel(
                            fullDate[fullDate.Length - 1], file, fullLog
                        )
                    );
                }
            }

            return new ObservableCollection<HistoryDataModel>(history.OrderByDescending(x => x.date));
        }
    }
}
