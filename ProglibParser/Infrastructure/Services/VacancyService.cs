using ProglibParser.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation;

namespace ProglibParser.Infrastructure.Services
{
    class VacancyService
    {
        private readonly string proglibRef;
        public static VacancyService Instance { get => VacancyServiceCreate.instance; }

        private VacancyService()
        {
            proglibRef = "https://proglib.io/vacancies/all?workType=all&workPlace=all&experience=&salaryFrom=&page=";
        }

        private class VacancyServiceCreate
        {
            static VacancyServiceCreate() { }
            internal static VacancyService instance = new VacancyService();
        }

        public async Task<IEnumerable<string>> GetHttpPagesAsync()
        {
            List<string> PagesRef = new List<string>();
            List<string> HttpPages = new List<string>();
            string mainHtmlBody = await new HttpClient().GetStringAsync(proglibRef);
            string PageTotalCountPattern = @"data-total=""(\d+?)""";
            if (int.TryParse(Regex.Match(mainHtmlBody, PageTotalCountPattern).Groups[1].Value, out var totalPages))
            {
                for (int i = 1; i <= totalPages; i++) { PagesRef.Add($"{proglibRef}{i}"); }
                await Parallel.ForEachAsync(PagesRef, async(httpBody, token) => { HttpPages.Add(await new HttpClient().GetStringAsync(httpBody)); });
                //await Task.Run(()=> Parallel.ForEach(PagesRef, httpBody => HttpPages.Add(new HttpClient().GetStringAsync(httpBody).Result)));                
            }
            return HttpPages;
        }

        public async Task<IEnumerable<Vacancy>> GetVacanciesAsync(IEnumerable<string> HttpPages)
        {
            List<Vacancy> vacanciesList = new List<Vacancy>();
            string vacancyPattern = @"preview-card__content.*?<a href=""(.+?)"".*?<div itemprop=""description"">(.+?)<\/div>\s*?<div itemprop=""datePosted"">(.+?)<\/div>";
            Regex regex = new Regex(vacancyPattern, RegexOptions.Multiline | RegexOptions.Singleline);
            HttpPages.ToList().ForEach(httpPage =>
            {
                MatchCollection matches = regex.Matches(httpPage);
                Parallel.ForEach(matches, item => vacanciesList.Add(new Vacancy { Reference = $"https://proglib.io/{item.Groups[1].Value}", Name = item.Groups[2].Value, PostData = item.Groups[3].Value }));
            });
            return vacanciesList;
        }

        public async Task<List<Vacancy>> GetAllVacancies()
        {
            List<Vacancy> vacanciesList = new List<Vacancy>();
            string htmlBody = await new HttpClient().GetStringAsync(proglibRef);
            string vacancyPattern = @"preview-card__content.*?<a href=""(.+?)"".*?<div itemprop=""description"">(.+?)<\/div>\s*?<div itemprop=""datePosted"">(.+?)<\/div>";
            string PagePattern = @"data-total=""(\d+?)""";
            Regex regex = new Regex(vacancyPattern, RegexOptions.Multiline | RegexOptions.Singleline);
            int totalPages;
            if (int.TryParse(Regex.Match(htmlBody, PagePattern).Groups[1].Value, out totalPages))
            {
                for (int i = 1; i <= totalPages; i++)
                {
                    string htmlPageBody = await new HttpClient().GetStringAsync($"{proglibRef}{i}");
                    MatchCollection matches = regex.Matches(htmlPageBody);
                    foreach (Match item in matches)
                    {
                        vacanciesList.Add(new Vacancy { Reference = $"https://proglib.io/{item.Groups[1].Value}", Name = item.Groups[2].Value, PostData = item.Groups[3].Value });
                    }
                }
            }
            return vacanciesList;
        }
    }
}
