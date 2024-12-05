using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RomanowskyStainSlideAnalyzer.Home.Models
{
    public class ParameterDataModel
    {
        private string _Value;
        public string Value
        {
            get => _Value;
            set => _Value = value;
        }

        private string _Description;
        public string Description
        {
            get => _Description;
            set => _Description = value;
        }

        public ParameterDataModel(string Value, string Description)
        {
            this.Value = Value;
            this.Description = Description;
        }
    }
}
