using AngleSharp;
using ProglibParser.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ParserLib
{
    public static class Selector
    {
        public static async Task<List<string>> GetVacancyNamesAsync(string url)
        {
            List<string> finalResult = new List<string>();
            IConfiguration config = Configuration.Default.WithDefaultLoader();
            IBrowsingContext context = BrowsingContext.New(config);
            AngleSharp.Dom.IDocument document = await context.OpenAsync(url);
            string selector = "article > div > div > a > div.flex.align-between > h2";
            AngleSharp.Dom.IHtmlCollection<AngleSharp.Dom.IElement> elements =
            document.QuerySelectorAll(selector);
            IEnumerable<string> results = elements.Select(it => it.TextContent);
            finalResult = results.ToList();
            return finalResult;
        }

        public static async Task<List<string>> GetVacancyReferencesAsync(string url)
        {
            List<string> finalResult = new List<string>();
            IConfiguration config = Configuration.Default.WithDefaultLoader();
            IBrowsingContext context = BrowsingContext.New(config);
            AngleSharp.Dom.IDocument document = await context.OpenAsync(url);
            string selector = "article > div > div > a";
            AngleSharp.Dom.IHtmlCollection<AngleSharp.Dom.IElement> elements = document.QuerySelectorAll(selector);
            IEnumerable<string> results = elements.Select(it => "https://proglib.io" + it.GetAttribute("href"));
            finalResult = results.ToList();
            return finalResult;
        }

        public static async Task<List<string>> GetPublishingDatesAsync(string url)
        {
            List<string> finalResult = new List<string>();
            IConfiguration config = Configuration.Default.WithDefaultLoader();
            IBrowsingContext context = BrowsingContext.New(config);
            AngleSharp.Dom.IDocument document = await context.OpenAsync(url);
            string selector = "article > header > div > div.preview-card__publish > div.publish-info";
            AngleSharp.Dom.IHtmlCollection<AngleSharp.Dom.IElement> elements =
            document.QuerySelectorAll(selector);
            IEnumerable<string> results = elements.Select(it => it.TextContent);
            finalResult = results.ToList();
            return finalResult;
        }

        public async static Task<int> GetTotalPagesAsync()
        {
            string url = "https://proglib.io/vacancies/all?workType=all&workPlace=all&experience=&salaryFrom=&page=1";
            IConfiguration config = Configuration.Default.WithDefaultLoader();
            IBrowsingContext context = BrowsingContext.New(config);
            AngleSharp.Dom.IDocument document = await context.OpenAsync(url);
            string selector = "body > div.basis.sheet > div.basis__h-wrapper.sheet__center > div.basis__h-content > div > div > main > div.feed-pagination.flex.align-center";
            var element = document.QuerySelector(selector);
            if (int.TryParse(element?.GetAttribute("data-total"), out var totalPages))
            { return totalPages;}
            else throw new NullReferenceException();
        }

        public static async Task<IEnumerable<Vacancy>> GetVacanciesAsync(string url)
        {
            var vacancies = new List<Vacancy>();
            try
            {
                var names = await GetVacancyNamesAsync(url);
                var refs = await GetVacancyReferencesAsync(url);
                var dates = await GetPublishingDatesAsync(url);
                for (int i = 0; i < names.Count; i++)
                {
                    var vacancy = new Vacancy
                    {
                        Name = names[i],
                        Reference = refs[i],
                        PostData = DateTime.Parse(dates[i])
                    };
                    vacancies.Add(vacancy);
                };
            }
            catch { }
            return vacancies;
        }

        public static async Task<IEnumerable<Vacancy>> GetVacanciesFromAllPagesParallel()
        {
            var vacanciesBag = new List<Vacancy>();
            int totalPages = await GetTotalPagesAsync();
            List<string> refs = new List<string>();
            for (int i = 1; i <= totalPages; i++)
            {
                refs.Add("https://proglib.io/vacancies/all?workType=all&workPlace=all&experience=&salaryFrom=&page=" + i.ToString());
            }
            var options = new ParallelOptions();
            options.MaxDegreeOfParallelism = 50;
            var token = options.CancellationToken;

            await Parallel.ForEachAsync(refs, options, async (currentUrl, token) =>
            {
                var vacancies = await GetVacanciesAsync(currentUrl);
                Parallel.ForEach(vacancies, v => { vacanciesBag.Add(v); });
            });
            return vacanciesBag;
        }
    }
}
