using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RomanowskyStainSlideAnalyzer.History.Models
{
    public class HistoryDataModel
    {
        public string date;
        public string root;
        public string labelingDataPath;
        public string imagePath;
        public string log;
        public Visibility showSaveButton;
        public Visibility showProgress { get; set; }

        public HistoryDataModel(string date, string root, string labelingDataPath, string imagePath, string log, Visibility showSaveButton, Visibility showProgress = Visibility.Collapsed)
        {
            this.date = date;
            this.root = root;
            this.labelingDataPath = labelingDataPath;
            this.imagePath = imagePath;
            this.log = log;
            this.showSaveButton = showSaveButton;
            this.showProgress = showProgress;
        }
    }
}
