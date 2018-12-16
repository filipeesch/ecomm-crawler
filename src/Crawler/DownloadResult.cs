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

    public class DownloadResult<T>
    {
        public DownloadResult(DownloadStatus status, T document)
        {
            Status = status;
            Document = document;
        }

        public DownloadStatus Status { get; }

        public T Document { get; }
    }
}