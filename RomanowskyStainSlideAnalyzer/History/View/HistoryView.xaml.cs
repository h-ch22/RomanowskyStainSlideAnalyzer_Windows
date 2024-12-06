using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Interop;
using Microsoft.UI.Xaml.Controls;
using RomanowskyStainSlideAnalyzer.History.Helper;
using RomanowskyStainSlideAnalyzer.History.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading;

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

        private ObservableCollection<HistoryDataModel> _Datas = new();

        public ObservableCollection<HistoryDataModel> Datas
        {
            get => _Datas;
            set
            {
                _Datas = value;
                OnPropertyChanged(nameof(Datas));
            }
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

            if (propertyName == nameof(Date)) GetHistory();
        }

        public HistoryView()
        {
            this.InitializeComponent();
            DataContext = this;
            GetHistory();
        }

        private void GetHistory()
        {
            if(Datas != null && Datas.Count > 0) Datas.Clear();

            ShowProgress = Visibility.Visible;

            var month = Date.Month < 10 ? $"0{Date.Month}" : Date.Month.ToString();
            var day = Date.Day < 10 ? $"0{Date.Day}" : Date.Day.ToString();
            var year = Date.Year;

            var date = $"{month}_{day}_{year}";

            Thread thread = new(() => GetHistory(date));
            thread.Start();

            DispatcherQueue.TryEnqueue(async () =>
            {
                ShowProgress = Visibility.Collapsed;

                if (Datas.Count == 0)
                {
                    emptyPanel.Visibility = Visibility.Visible;
                    listView.Visibility = Visibility.Collapsed;
                }
                else
                {
                    emptyPanel.Visibility = Visibility.Collapsed;
                    listView.Visibility = Visibility.Visible;
                }
            });
        }

        private void GetHistory(string date)
        {
            Datas = helper.GetHistory(date);
        }
    }
}
