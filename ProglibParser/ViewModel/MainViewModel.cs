using ProglibParser.Infrastructure.Services;
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
            Load().GetAwaiter();
        }

        private async Task Load()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            #region Без распаралеливания
            //Vacancies = new ObservableCollection<Vacancy>(await VacancyService.Instance.GetAllVacancies());
            //stopwatch.Stop();
            //ParseRuntimeMeasurement = $"Parsing the data took {stopwatch.ElapsedMilliseconds} ms";
            //OnPropertyChanged(nameof(Vacancies));
            #endregion

            #region С распаралеливанием
            IEnumerable<string> pages = await VacancyService.Instance.GetHttpPagesAsync();
            stopwatch.Stop();
            GetPagesRuntimeTemperature = $"Downloading the data took {stopwatch.ElapsedMilliseconds} ms";
            stopwatch.Restart();
            Vacancies = new ObservableCollection<Vacancy>(await VacancyService.Instance.GetVacanciesAsync(pages));
            stopwatch.Stop();
            ParseRuntimeMeasurement = $"Parsing the data took {stopwatch.ElapsedMilliseconds} ms";
            OnPropertyChanged(nameof(Vacancies));
            #endregion

        }

        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string property = "")
        {
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs(property));
        }
    }
}
