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
        public Symbol symbol;
        public string statusText;
        public bool canItLabeled;
        public string imgFile;
        public Visibility LabeledMaskPropertiesVisibility { get; set; }
        public Visibility LabeledBBoxPropertiesVisibility { get; set; }
        public Visibility BBoxPropertiesVisibility { get; set; }
        public Visibility MaskPropertiesVisibility { get; set; }
        public Visibility AnalyzeBtnVisibility { get; set; }
        public Visibility ReLabelingVisibility { get; set; }
        public Visibility SaveBBoxImageVisibility { get; set; }
        public bool includeAllMaskInOneFile { get; set; }
        public string checkBoxContent { get; set; }
        public Visibility saveMenuVisibility { get; set; }

        public HistoryDataModel(
            string date,
            string root,
            string labelingDataPath,
            string imagePath,
            string log,
            Visibility labeledMaskPropertiesVisibility,
            Visibility labeledBBoxPropertiesVisibility,
            Visibility maskPropertiesVisibility,
            Visibility bBoxPropertiesVisibility,
            Symbol symbol = Symbol.Accept,
            string statusText = "",
            bool canItLabeled = false,
            string imgFile = ""
        )
        {
            this.date = date;
            this.root = root;
            this.labelingDataPath = labelingDataPath;
            this.imagePath = imagePath;
            this.log = log;
            this.symbol = symbol;
            this.statusText = statusText;
            this.canItLabeled = canItLabeled;
            this.imgFile = imgFile;
            LabeledBBoxPropertiesVisibility = labeledBBoxPropertiesVisibility;
            LabeledMaskPropertiesVisibility = labeledMaskPropertiesVisibility;
            MaskPropertiesVisibility = maskPropertiesVisibility;
            BBoxPropertiesVisibility = bBoxPropertiesVisibility;

            AnalyzeBtnVisibility = (LabeledBBoxPropertiesVisibility == Visibility.Visible || LabeledMaskPropertiesVisibility == Visibility.Visible) ? Visibility.Visible : Visibility.Collapsed;
            ReLabelingVisibility = (BBoxPropertiesVisibility == Visibility.Visible || MaskPropertiesVisibility == Visibility.Visible) ? Visibility.Visible : Visibility.Collapsed;
            SaveBBoxImageVisibility = (BBoxPropertiesVisibility == Visibility.Visible || LabeledBBoxPropertiesVisibility == Visibility.Visible) ? Visibility.Visible : Visibility.Collapsed;
            saveMenuVisibility = (LabeledBBoxPropertiesVisibility == Visibility.Visible || LabeledMaskPropertiesVisibility == Visibility.Visible || MaskPropertiesVisibility == Visibility.Visible) ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
