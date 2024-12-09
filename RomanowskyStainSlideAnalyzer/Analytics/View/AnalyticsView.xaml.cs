using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using RomanowskyStainSlideAnalyzer.History.Helper;
using System.Collections.ObjectModel;
using RomanowskyStainSlideAnalyzer.History.Models;
using System.ComponentModel;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace RomanowskyStainSlideAnalyzer.Analytics.View
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AnalyticsView : Page, INotifyPropertyChanged
    {
        private HistoryHelper historyHelper = new();
        private ObservableCollection<LabeledHistoryDataModel> Datas = new();
        private Visibility _ShowProgress = Visibility.Visible;
        public Visibility ShowProgress
        {
            get => _ShowProgress;
            set
            {
                _ShowProgress = value;
                OnPropertyChanged(nameof(ShowProgress));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public AnalyticsView()
        {
            this.InitializeComponent();

            Datas = historyHelper.GetAllLabeledHistory();
            ShowProgress = Visibility.Collapsed;
            listView.Visibility = Visibility.Visible;
            listView.ItemsSource = Datas;
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
