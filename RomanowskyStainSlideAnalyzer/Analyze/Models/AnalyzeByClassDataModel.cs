using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RomanowskyStainSlideAnalyzer.Analyze.Models
{
    public class AnalyzeByClassDataModel
    {
        public string className;
        public string avgOfA;
        public string avgOfR;
        public string avgOfG;
        public string avgOfB;
        public string avgOfHue;
        public string avgOfSaturation;
        public string avgOfBrightness;

        public string avgOfWidth;
        public string avgOfHeight;
        public string avgOfSize;

        public AnalyzeByClassDataModel(string className, string avgOfA, string avgOfR, string avgOfG, string avgOfB, string avgOfHue, string avgOfSaturation, string avgOfBrightness, string avgOfWidth = "NaN", string avgOfHeight = "NaN", string avgOfSize = "NaN")
        {
            this.className = className;
            this.avgOfA = avgOfA;
            this.avgOfR = avgOfR;
            this.avgOfG = avgOfG;
            this.avgOfB = avgOfB;
            this.avgOfHue = avgOfHue;
            this.avgOfSaturation = avgOfSaturation;
            this.avgOfBrightness = avgOfBrightness;
            this.avgOfWidth = avgOfWidth;
            this.avgOfHeight = avgOfHeight;
            this.avgOfSize = avgOfSize;
        }
    }
}
