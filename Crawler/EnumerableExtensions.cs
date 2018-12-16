using System.Collections.Generic;

namespace Crawler
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<IEnumerable<T>> SplitInPages<T>(this IEnumerable<T> list, int pageSize)
        {
            using (var item = list.GetEnumerator())
            {
                while (item.MoveNext())
                    yield return SplitInPagesIterator(item, pageSize);
            }
        }

        private static IEnumerable<T> SplitInPagesIterator<T>(IEnumerator<T> item, int pageSize)
        {
            do
            {
                yield return item.Current;
            } while (--pageSize > 0 && item.MoveNext());
        }
    }
}
