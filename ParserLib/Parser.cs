using AngleSharp;
using AngleSharp.Dom;
using ProglibParser.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proglib
{
    public class Parser // upgrade Selector
    {
        private IConfiguration _config;
        private IBrowsingContext _context;

        public Parser() 
        {
            _config = Configuration.Default.WithDefaultLoader();
            _context = BrowsingContext.New(_config);
        }


        public async Task<IEnumerable<Vacancy>> GetVacanciesFromPageAsync(string pageUri)
        {
            List<Vacancy> vacancies = new();
            string selector = "div.feed__items > div > article";

            IDocument document = await _context.OpenAsync(pageUri);

            var articles = document.QuerySelectorAll(selector);

            foreach (var article in articles)
            {
                var vacancyName = article.QuerySelector("div.flex.align-between > h2")?.TextContent;
                var vacancyDate = article.QuerySelector("div.preview-card__publish > div.publish-info")?.TextContent;
                var vacancyUri = article.QuerySelector("div > div.preview-card__content > a")?.GetAttribute("href");

                if(!DateTime.TryParse(vacancyDate, out var vacancyPostDate)) { throw new Exception(); }
                string vacancyReference = "https://proglib.io" + vacancyUri;

                vacancies.Add(new Vacancy { Name = vacancyName, PostData = vacancyPostDate, Reference = vacancyReference });
            }
            return vacancies;
        }


        public async Task<IEnumerable<Vacancy>> GetVacanciesFromAllPagesParallel()
        {
            var pagesUri = new List<string>();
            var vacancies = new ConcurrentBag<Vacancy>();

            string totalPagesSelector = "main > div.feed-pagination.flex.align-center";
            IDocument document = await _context.OpenAsync("https://proglib.io/vacancies/all");
            var total = document.QuerySelector(totalPagesSelector)?.GetAttribute("data-total");
            
            if(!int.TryParse(total, out var totalPages)) { throw new Exception(); }

            for (int i =1; i <= totalPages; i++)
            {
                pagesUri.Add(@"https://proglib.io/vacancies/all?workType=all&workPlace=all&experience=&salaryFrom=&page=" + i);
            }

            await Parallel.ForEachAsync(pagesUri, async (page, token) =>
            {
                foreach(var vacancy in await GetVacanciesFromPageAsync(page)) { vacancies.Add(vacancy); }
            });

            return vacancies;
        }
    }
}
