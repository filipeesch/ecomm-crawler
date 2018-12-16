using System;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Crawler
{
    public class RobotsCrawler : IDisposable
    {
        private static readonly Lazy<HttpClient> HttpClientFactory = new Lazy<HttpClient>(() =>
        {
            var httpClient = new HttpClient();
            //httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "text/html");
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", @"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/70.0.3538.102 Safari/537.36");
            return httpClient;
        });

        public RobotsCrawler(string name, Uri robotsUri)
        {
            Name = name;
            RobotsUrl = robotsUri.AbsoluteUri;
        }

        public string Name { get; }

        public string RobotsUrl { get; }

        public void Dispose()
        {
            if (HttpClientFactory.IsValueCreated)
                HttpClientFactory.Value.Dispose();
        }

        public async Task Start()
        {
            try
            {
                Console.WriteLine($"Starting: {Name}");

                var responseRobots = await ReadRobotsFile();

                if (responseRobots.Status != DownloadStatus.Success)
                    return;

                var sitemap = GetSitemapUrl(responseRobots.Document);

                Console.WriteLine($"{Name}: {sitemap}");

                Console.WriteLine($"End: {Name}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{Name} Error: {ex.Message}");
            }
        }

        private static HttpClient GetHttpClient() => HttpClientFactory.Value;

        private string GetSitemapUrl(string content)
        {
            var regex = new Regex("Sitemap: (?<url>.+)", RegexOptions.Multiline);

            var match = regex.Match(content);

            if (match.Success)
            {
                return match.Groups["url"].Value;
            }

            return string.Empty;
        }

        private async Task<DownloadResult<string>> ReadRobotsFile()
        {
            var httpClient = GetHttpClient();

            using (var response = await httpClient.GetAsync(RobotsUrl))
            using (var content = response.Content)
            {
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    Console.WriteLine($"{Name} Error: {response.ReasonPhrase}");
                    return new DownloadResult<string>(DownloadStatus.Error, string.Empty);
                }

                var text = await content.ReadAsStringAsync();

                return new DownloadResult<string>(DownloadStatus.Success, text);
            }
        }
    }
}
