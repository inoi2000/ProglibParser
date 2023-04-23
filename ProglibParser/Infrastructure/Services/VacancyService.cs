using ProglibParser.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ProglibParser.Infrastructure.Services
{
    public static class VacancyService
    {
        public static IEnumerable<Vacancy> GetVacancies(IEnumerable<string> HttpPages)
        {
            List<Vacancy> vacanciesList = new List<Vacancy>();
            string vacancyPattern = @"preview-card__content.*?<a href=""(.+?)"".*?<div itemprop=""description"">(.+?)<\/div>\s*?<div itemprop=""datePosted"">(.+?)<\/div>";
            Regex regex = new Regex(vacancyPattern, RegexOptions.Multiline | RegexOptions.Singleline);
            HttpPages.ToList().ForEach(httpPage =>
            {
                MatchCollection matches = regex.Matches(httpPage);
                matches.ToList().ForEach(item => vacanciesList.Add(new Vacancy 
                { 
                    Reference = $"https://proglib.io/{item.Groups[1].Value}", 
                    Name = item.Groups[2].Value, 
                    PostData = DateTime.Parse(item.Groups[3].Value)
                }));
            });
            return vacanciesList;
        }
    }
}
