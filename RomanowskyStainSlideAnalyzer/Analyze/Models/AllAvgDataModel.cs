using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RomanowskyStainSlideAnalyzer.Analyze.Models
{
    public class AllAvgDataModel
    {
        public AvgDataModel avgData { get; set; }
        public AvgDataModel avgNone { get; set; }
        public AvgDataModel avgLCell { get; set; }
        public AvgDataModel avgSCell { get; set; }

        public AllAvgDataModel(
            AvgDataModel avgData,
            AvgDataModel avgNone,
            AvgDataModel avgLCell,
            AvgDataModel avgSCell
        )
        {
            this.avgData = avgData;
            this.avgNone = avgNone;
            this.avgLCell = avgLCell;
            this.avgSCell = avgSCell;
        }
    }
}
