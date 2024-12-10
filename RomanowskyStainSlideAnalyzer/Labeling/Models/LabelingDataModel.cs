using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RomanowskyStainSlideAnalyzer.Labeling.Models
{
    public class LabelingDataModel
    {
        public string id { get; set; }
        public string classId { get; set; }
        public string x { get; set; }
        public string y { get; set; }
        public string width { get; set; }
        public string height { get; set; }

        public LabelingDataModel(string id, string classId, string x, string y, string width, string height)
        {
            this.id = id;
            this.classId = classId;
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
        }
    }
}
