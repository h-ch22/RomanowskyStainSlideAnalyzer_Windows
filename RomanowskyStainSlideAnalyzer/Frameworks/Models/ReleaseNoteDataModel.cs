using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RomanowskyStainSlideAnalyzer.Frameworks.Models
{
    public class ReleaseNoteDataModel
    {
        public string Key { get; set; }
        public string Values { get; set; }

        public ReleaseNoteDataModel(string Key, string Values)
        {
            this.Key = Key;
            this.Values = Values;
        }
    }
}
