using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using RomanowskyStainSlideAnalyzer.Frameworks.Helper;
using RomanowskyStainSlideAnalyzer.Frameworks.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using Windows.Storage;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace RomanowskyStainSlideAnalyzer.Frameworks.View
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsView : Page, INotifyPropertyChanged
    {
        private string _AppName = $"Romanowsky Stain Slide Analyzer 1.0.0.0";
        public string AppName
        {
            get => _AppName;
            set
            {
                _AppName = $"Romanowsky Stain Slide Analyzer {value}";
                OnPropertyChanged(nameof(AppName));
            }
        }

        private string _IsEnvironmentSet = "Not Set";
        public string IsEnvironmentSet
        {
            get => _IsEnvironmentSet;
            set
            {
                _IsEnvironmentSet = value;
                OnPropertyChanged(nameof(IsEnvironmentSet));
            }
        }

        private Symbol _EnvironmentSymbol = Symbol.Cancel;
        public Symbol EnvironmentSymbol
        {
            get => _EnvironmentSymbol;
            set
            {
                _EnvironmentSymbol = value;
                OnPropertyChanged(nameof(EnvironmentSymbol));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private EnvironmentHelper helper = new();
        public ObservableCollection<EnvironmentDataModel> Datas
        {
            get; private set;
        }

        private List<string> Titles = ["WSL Status", "Windows Additional Features Status", "Feature Activator Status", "Linux Status", "Essential Packages Status", "Python Packages Status", "Project Status"];

        public SettingsView()
        {
            this.InitializeComponent();
            DataContext = this;

            AppName = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            Datas = new();

            foreach(var title in Titles)
            {
                Datas.Add( new EnvironmentDataModel(title, helper.GetStatus(title)) );
            }

            IsEnvironmentSet = helper.GetStatus("All Status") ? "Set" : "Not Set";
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

            if(propertyName == nameof(IsEnvironmentSet))
            {
                SolidColorBrush brush;

                switch(IsEnvironmentSet)
                {
                    case "Set":
                        EnvironmentSymbol = Symbol.Accept;
                        break;

                    default:
                        EnvironmentSymbol = Symbol.Cancel;
                        break;
                }
            }
        }
    }
}
