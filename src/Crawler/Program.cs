using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Crawler
{
    internal class Program
    {
        private static async Task Main()
        {
            var sites = new Dictionary<string, string>
            {
                ["Worten"] = "https://www.worten.pt/robots.txt",
                ["Fnac"] = "https://www.fnac.pt/robots.txt"
            };

            var tasks = sites.Select(site =>
            {
                var crawler = new RobotsCrawler(site.Key, new Uri(site.Value));

                return crawler.Start();
            });

            await Task.WhenAll(tasks);

            Console.ReadLine();
        }
    }
}
