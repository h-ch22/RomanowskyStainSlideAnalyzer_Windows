using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RomanowskyStainSlideAnalyzer.Frameworks.Models
{
    public class EnvironmentDataModel
    {
        public string Title = "";
        public bool IsActivated = false;
        public string StatusText
        {
            get => IsActivated ? "Activated" : "Not Activated";
        }

        public Symbol SymbolIcon
        {
            get => IsActivated ? Symbol.Accept : Symbol.Cancel;
        }

        public Visibility FixButtonVisibility
        {
            get => IsActivated ? Visibility.Collapsed : Visibility.Visible;
        }

        public EnvironmentDataModel(string Title, bool IsActivated)
        {
            this.Title = Title;
            this.IsActivated = IsActivated;
        }
    }
}
