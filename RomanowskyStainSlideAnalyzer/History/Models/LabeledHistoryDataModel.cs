using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RomanowskyStainSlideAnalyzer.History.Models
{
    public class LabeledHistoryDataModel
    {
        public string ImagePath;
        public string LabeledDataPath;
        public string Id;

        public LabeledHistoryDataModel(string ImagePath, string LabeledDataPath, string Id)
        {
            this.ImagePath = ImagePath;
            this.LabeledDataPath = LabeledDataPath;
            this.Id = Id;
        }
    }
}
