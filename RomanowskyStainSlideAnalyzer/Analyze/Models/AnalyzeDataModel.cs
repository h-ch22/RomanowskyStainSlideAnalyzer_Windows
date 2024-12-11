using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RomanowskyStainSlideAnalyzer.Analyze.Models
{
    public class AnalyzeDataModel
    {
        public string x;
        public string y;
        public string w;
        public string h;
        public AnalyzeByClassDataModel data;

        public AnalyzeDataModel(string x, string y, string w, string h, AnalyzeByClassDataModel data)
        {
            this.x = x;
            this.y = y;
            this.w = w;
            this.h = h;
            this.data = data;
        }
    }
}
