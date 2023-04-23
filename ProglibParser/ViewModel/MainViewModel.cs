using ParserLib;
using ProglibParser.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;
using static System.Net.WebRequestMethods;

namespace ProglibParser.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged
    {
        #region Properties
        public ObservableCollection<Vacancy> Vacancies { get; set; }

        private string getPagesRuntimeMeasurement;
        public string GetPagesRuntimeTemperature 
        { 
            get { return getPagesRuntimeMeasurement; }
            set { getPagesRuntimeMeasurement = value; OnPropertyChanged(); }
        }

        private string parseRuntimeMeasurement;
        public string ParseRuntimeMeasurement
        {
            get { return parseRuntimeMeasurement; }
            set { parseRuntimeMeasurement = value; OnPropertyChanged(); }
        }
        #endregion

        public MainViewModel() 
        {
            Task.Run(() => Loaded());
        }

        public async Task Loaded()
        {
            try
            {
                Stopwatch stopwatch = Stopwatch.StartNew();
                Vacancies = new ObservableCollection<Vacancy>(await Selector.GetVacanciesFromAllPagesAsync());
                stopwatch.Stop();
                ParseRuntimeMeasurement = $"Parsing the data took {stopwatch.ElapsedMilliseconds} ms";
            }
            catch { }
            OnPropertyChanged(nameof(Vacancies));
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string property = "")
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(property));
        }
    }
}
