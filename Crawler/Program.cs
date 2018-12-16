using HtmlAgilityPack;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Timers;

namespace Crawler
{
    internal class Program
    {
        private const string BaseUrl = "https://mediamarkt.pt";
        private static readonly ConcurrentDictionary<string, object> VisitedUris = new ConcurrentDictionary<string, object>();
        private static readonly ConcurrentDictionary<string, object> InvalidUris = new ConcurrentDictionary<string, object>();
        private static readonly ConcurrentDictionary<string, object> ProductPages = new ConcurrentDictionary<string, object>();

        private static readonly Lazy<HttpClient> HttpClientFactory = new Lazy<HttpClient>(() =>
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "text/html");
            //http.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Encoding", "gzip");
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", @"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/70.0.3538.102 Safari/537.36");
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept-Language", "pt-BR");

            return httpClient;
        });

        private static async Task Main()
        {
            var countTimer = new Timer(5000);
            countTimer.Elapsed += (e, args) => Console.WriteLine("Total Pages: {0}\t PDPs: {1}" , VisitedUris.Count, ProductPages.Count);
            countTimer.Start();

            await DownloadPage(new Uri(BaseUrl), 7);

            Console.WriteLine("\n\nFounded {0} Product pages", ProductPages.Count);
            Console.ReadLine();
        }

        private static async Task DownloadPage(Uri url, int depth)
        {
            if (depth <= 0)
                return;

            VisitedUris.TryAdd(url.ToString(), null);

            var downloadResult = await DownloadHtml(url);

            if (downloadResult.Status != DownloadStatus.Success)
                return;

            var document = downloadResult.Document;

            var links = document.DocumentNode
                .SelectNodes("//a")
                .Select(x => x.Attributes["href"]?.Value)
                .Where(IsSiteUrl)
                .Distinct();

            var productNode = document.DocumentNode
                .SelectSingleNode("//div[contains(@itemtype, 'http://schema.org/Product')]");

            if (productNode != null)
            {
                ProductPages.TryAdd(url.ToString(), null);
                Console.WriteLine("Found Product Page: " + url);
            }

            downloadResult = null;
            productNode = null;
            document = null;

            var linksTasks = links.Select(link =>
            {
                var textUrl = PrepareLink(BaseUrl, link);

                if (VisitedUris.ContainsKey(textUrl) || InvalidUris.ContainsKey(textUrl))
                    return Task.CompletedTask;

                if (!Uri.TryCreate(textUrl, UriKind.Absolute, out var uri))
                {
                    InvalidUris.TryAdd(textUrl, null);

                    Console.WriteLine("Invalid URI: " + link);
                    return Task.CompletedTask;
                }

                return DownloadPage(uri, depth - 1);
            });

            foreach (var tasks in linksTasks.SplitInPages(1))
            {
                await Task.WhenAll(tasks);
            }
        }

        private static bool IsSiteUrl(string url)
        {
            if (url == null)
                return false;

            if ((url.StartsWith("http://") || url.StartsWith("https://")) &&
                !url.StartsWith(BaseUrl))
            {
                return false;
            }

            return true;
        }

        private static string PrepareLink(string baseUrl, string link)
        {
            link = link ?? string.Empty;

            if (link.StartsWith(baseUrl))
                return link;

            return baseUrl + '/' + link
               .TrimStart('/')
               .TrimEnd('/', '?', '#', '&');
        }

        private static async Task<DownloadResult> DownloadHtml(Uri url)
        {
            try
            {
                var httpClient = GetHttpClient();

                using (var response = await httpClient.GetAsync(url))
                using (var content = response.Content)
                {
                    if (response.StatusCode != HttpStatusCode.OK)
                        return new DownloadResult(DownloadStatus.Error, null);

                    if (!IsHtmlDocument(content.Headers.ContentType.MediaType))
                        return new DownloadResult(DownloadStatus.NotHtml, null);

                    var document = new HtmlDocument();

                    using (var contentStream = await content.ReadAsStreamAsync())
                        document.Load(contentStream);

                    return new DownloadResult(DownloadStatus.Success, document);
                }
            }
            catch
            {
                return new DownloadResult(DownloadStatus.Error, null);
            }
        }

        private static HttpClient GetHttpClient() => HttpClientFactory.Value;

        private static bool IsHtmlDocument(string contentType)
        {
            return string.Compare(
                contentType,
                "text/html",
                StringComparison.InvariantCultureIgnoreCase) == 0;
        }
    }
}
