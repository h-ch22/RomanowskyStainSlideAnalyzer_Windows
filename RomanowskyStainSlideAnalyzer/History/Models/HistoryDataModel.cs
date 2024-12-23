using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
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
        public Symbol symbol;
        public string statusText;
        public bool canItLabeled;
        public string imgFile;
        public Visibility showChangeButton;
        public Visibility showProgress { get; set; }
        public Visibility showMaskButton;
        public bool includeAllMaskInOneFile { get; set; }
        public string checkBoxContent { get; set; }

        public HistoryDataModel(
            string date,
            string root,
            string labelingDataPath,
            string imagePath,
            string log,
            Visibility showSaveButton,
            Visibility showProgress = Visibility.Collapsed,
            Symbol symbol = Symbol.Accept,
            string statusText = "",
            bool canItLabeled = false,
            Visibility showChangeButton = Visibility.Collapsed,
            string imgFile = "",
            Visibility showMaskButton = Visibility.Collapsed,
            bool includeAllMaskInOneFile = true,
            string checkBoxContent = "Include all masks in one file"

        )
        {
            this.date = date;
            this.root = root;
            this.labelingDataPath = labelingDataPath;
            this.imagePath = imagePath;
            this.log = log;
            this.showSaveButton = showSaveButton;
            this.showProgress = showProgress;
            this.symbol = symbol;
            this.statusText = statusText;
            this.canItLabeled = canItLabeled;
            this.showChangeButton = showChangeButton;
            this.imgFile = imgFile;
            this.showMaskButton = showMaskButton;
            this.includeAllMaskInOneFile = includeAllMaskInOneFile;
            this.checkBoxContent = checkBoxContent;
        }
    }
}
