using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using RomanowskyStainSlideAnalyzer.Frameworks.Models;
using RomanowskyStainSlideAnalyzer.History.Helper;
using RomanowskyStainSlideAnalyzer.History.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace RomanowskyStainSlideAnalyzer.History.View
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class HistoryView : Page, INotifyPropertyChanged
    {
        private HistoryHelper helper = new();

        public ObservableCollection<HistoryDataModel> Datas
        {
            get; private set;
        }

        private DateTimeOffset _Date = DateTimeOffset.Now;
        public DateTimeOffset Date
        {
            get => _Date;
            set
            {
                _Date = value;
                OnPropertyChanged(nameof(Date));
            }
        }

        private Visibility _ShowProgress = Visibility.Visible;
        public Visibility ShowProgress
        {
            get => _ShowProgress;
            set
            {
                if(value == Visibility.Visible)
                {
                    emptyPanel.Visibility = Visibility.Collapsed;
                    listView.Visibility = Visibility.Collapsed;
                }

                _ShowProgress = value;
                OnPropertyChanged(nameof(ShowProgress));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

            if (propertyName == "Date") GetHistory();
        }

        public HistoryView()
        {
            this.InitializeComponent();
            DataContext = this;
            GetHistory();
        }

        private void GetHistory()
        {
            var month = Date.Month < 10 ? $"0{Date.Month}" : Date.Month.ToString();
            var day = Date.Day < 10 ? $"0{Date.Day}" : Date.Day.ToString();
            var year = Date.Year;

            var date = $"{month}_{day}_{year}";

            Datas = helper.GetHistory(date);

            ShowProgress = Visibility.Collapsed;

            if(Datas.Count == 0)
            {
                emptyPanel.Visibility = Visibility.Visible;
                listView.Visibility = Visibility.Collapsed;
            }
            else
            {
                emptyPanel.Visibility = Visibility.Collapsed;
                listView.Visibility = Visibility.Visible;
            }
        }
    }
}
