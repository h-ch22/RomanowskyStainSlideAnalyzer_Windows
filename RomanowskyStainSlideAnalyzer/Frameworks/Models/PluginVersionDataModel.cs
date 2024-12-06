using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RomanowskyStainSlideAnalyzer.Frameworks.Models
{
    public class PluginVersionDataModel: INotifyPropertyChanged
    {
        public int id = 0;
        public string Title = "";
        private string _Version = "1.0.0.0";
        public string Version
        {
            get => _Version;
            set
            {
                _Version = value;
                OnPropertyChanged(nameof(Version));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public PluginVersionDataModel(int Id, string Title, string Version)
        {
            this.id = Id;
            this.Title = Title;
            this.Version = Version;
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
