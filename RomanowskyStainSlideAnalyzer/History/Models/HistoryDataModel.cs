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
        public string imagePath;
        public string log;

        public HistoryDataModel(string date, string imagePath, string log)
        {
            this.date = date;
            this.imagePath = imagePath;
            this.log = log;
        }
    }
}
