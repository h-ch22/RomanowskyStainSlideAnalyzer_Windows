using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RomanowskyStainSlideAnalyzer.Frameworks.Models
{
    public class EnvironmentDataModel: INotifyPropertyChanged
    {
        public int id = 0;
        public string Title = "";

        private bool _IsActivated = false;
        public bool IsActivated
        {
            get => _IsActivated;
            set
            {
                _IsActivated = value;
                OnPropertyChanged(nameof(IsActivated));
                OnPropertyChanged(nameof(StatusText));
            }
        }
        public string StatusText
        {
            get => IsActivated ? "Activated" : "Not Activated";
        }

        public Symbol SymbolIcon
        {
            get => IsActivated ? Symbol.Accept : Symbol.Cancel;
        }

        private Visibility _ProgressBarVisibility = Visibility.Collapsed;

        public Visibility ProgressBarVisibility
        {
            get => _ProgressBarVisibility;
            set
            {
                _ProgressBarVisibility = value;
                OnPropertyChanged(nameof(ProgressBarVisibility));
            }
        }

        private Visibility _RefreshButtonVisibility = Visibility.Visible;
        public Visibility RefreshButtonVisibility
        {
            get => _RefreshButtonVisibility;
            set
            {
                _RefreshButtonVisibility = value;
                OnPropertyChanged(nameof(RefreshButtonVisibility));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public EnvironmentDataModel(int Id, string Title, bool IsActivated)
        {
            this.id = Id;
            this.Title = Title;
            this.IsActivated = IsActivated;
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
