using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RomanowskyStainSlideAnalyzer.History.Models
{
    public class LogDataModel
    {
        public string header { get; set; }
        public string contents { get; set; }

        public LogDataModel(string header, string contents)
        {
            this.header = header;
            this.contents = contents;
        }   
    }
}
