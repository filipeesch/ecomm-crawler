using HtmlAgilityPack;

namespace Crawler
{
    public class DownloadResult
    {
        public DownloadResult(DownloadStatus status, HtmlDocument document)
        {
            Status = status;
            Document = document;
        }

        public DownloadStatus Status { get; }

        public HtmlDocument Document { get; }
    }
}