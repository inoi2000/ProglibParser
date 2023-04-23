using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ParserLib
{
    public static class ProglibHttp
    {
        private static string proglibRef { get => "https://proglib.io/vacancies/all?workType=all&workPlace=all&experience=&salaryFrom=&page="; }

        public static async Task<IEnumerable<string>> GetHttpPagesParallel()
        {
            List<string> PagesRef = new List<string>();
            List<string> HttpPages = new List<string>();
            string mainHtmlBody = await new HttpClient().GetStringAsync(proglibRef);
            string PageTotalCountPattern = @"data-total=""(\d+?)""";
            if (int.TryParse(Regex.Match(mainHtmlBody, PageTotalCountPattern).Groups[1].Value, out var totalPages))
            {
                for (int i = 1; i <= totalPages; i++) { PagesRef.Add($"{proglibRef}{i}"); }
                await Parallel.ForEachAsync(PagesRef, async (httpBody, token) => { HttpPages.Add(await new HttpClient().GetStringAsync(httpBody)); });
                //await Task.Run(()=> Parallel.ForEach(PagesRef, httpBody => HttpPages.Add(new HttpClient().GetStringAsync(httpBody).Result)));                
            }
            return HttpPages;
        }
    }
}
